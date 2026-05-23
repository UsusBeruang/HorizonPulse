using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using HorizonPulse.Automation.Interfaces;
using HorizonPulse.Telemetry.Runtime;

namespace HorizonPulse.Automation.Services;

/// <summary>
/// Main automation service that coordinates throttle and steering assist.
/// High-performance, lock-free design optimized for ~200 PPS telemetry updates.
/// Uses atomic state updates and independent controller output loop.
/// </summary>
public sealed class AutomationService : IAutomationService
{
    private readonly VigemControllerService _vigemService;
    
    // Atomic state - single source of truth
    private volatile bool _isEnabled;
    private volatile bool _isControllerConnected;
    private volatile float _currentThrottleOutput;
    private volatile float _currentBrakeInput;
    private volatile float _currentSteeringCorrection;
    
    // Settings (modified infrequently, safe to read without lock)
    private float _steeringRandomness = 0.3f;
    private float _throttleSmoothing = 0.7f;
    
    // Controller output loop
    private CancellationTokenSource? _controllerLoopCts;
    private Task? _controllerLoopTask;
    
    // Steering state (updated per-telemetry call, no locking needed)
    private float _steeringTarget;
    private float _steeringCurrent;
    private readonly Random _random = new();
    private readonly Stopwatch _steeringTimer = new();
    private const int SteeringUpdateIntervalMs = 100;
    
    // Throttle state
    private float _lastThrottleOutput;
    
    // Constants
    private const byte TriggerMaxRaw = 255;
    private const float SteeringCenter = 128f;
    private const float SteeringMaxRaw = 127f;
    
    /// <inheritdoc/>
    public bool IsEnabled => _isEnabled;
    
    /// <inheritdoc/>
    public float CurrentThrottleOutput => _currentThrottleOutput;
    
    /// <inheritdoc/>
    public float CurrentBrakeInput => _currentBrakeInput;
    
    /// <inheritdoc/>
    public float CurrentSteeringCorrection => _currentSteeringCorrection;
    
    /// <inheritdoc/>
    public bool IsControllerConnected => _isControllerConnected;

    /// <summary>
    /// Initializes a new instance of the automation service.
    /// </summary>
    public AutomationService()
    {
        _vigemService = new VigemControllerService();
        _steeringTimer.Start();
    }

    /// <inheritdoc/>
    public void SetEnabled(bool enabled)
    {
        if (_isEnabled == enabled)
            return;
        
        _isEnabled = enabled;
        
        if (enabled)
        {
            // Enable: connect controller and start loops
            if (!_vigemService.Connect())
            {
                _isEnabled = false;
                return;
            }
            
            _isControllerConnected = true;
            
            // Start controller output loop
            StartControllerLoop();
        }
        else
        {
            // Disable: stop loops and disconnect
            StopControllerLoop();
            
            // Reset all outputs immediately
            ResetOutputs();
            
            // Disconnect controller
            _vigemService.Disconnect();
            _isControllerConnected = false;
        }
    }

    /// <inheritdoc/>
    public void SetSteeringRandomness(float intensity)
    {
        _steeringRandomness = Math.Clamp(intensity, 0f, 1f);
    }

    /// <inheritdoc/>
    public void SetThrottleSmoothing(float factor)
    {
        _throttleSmoothing = Math.Clamp(factor, 0f, 1f);
    }

    /// <inheritdoc/>
    public void UpdateFromTelemetry(RuntimeTelemetryState telemetryState)
    {
        // Fast path: if disabled, do nothing
        if (!_isEnabled)
            return;
        
        // Read brake input from telemetry (already normalized 0-1)
        float brakeInput = telemetryState.BrakeInput;
        _currentBrakeInput = brakeInput;
        
        // Calculate throttle output: brake 1.0 => RT 0, brake 0.0 => RT 255
        float targetThrottle = 1f - brakeInput;
        
        // Apply smoothing
        if (_throttleSmoothing > 0f)
        {
            _lastThrottleOutput = Lerp(_lastThrottleOutput, targetThrottle, 1f - _throttleSmoothing);
        }
        else
        {
            _lastThrottleOutput = targetThrottle;
        }
        
        _currentThrottleOutput = _lastThrottleOutput * 100f; // Store as percentage for UI
        
        // Calculate steering correction (random jitter between -5 and +5)
        if (_steeringTimer.ElapsedMilliseconds >= SteeringUpdateIntervalMs)
        {
            _steeringTimer.Restart();
            
            // Generate new random correction in range [-5, +5] scaled by randomness
            float range = 5f * _steeringRandomness;
            float correction = (float)(_random.NextDouble() * 2f - 1f) * range;
            _steeringTarget = Math.Clamp(correction, -5f, 5f);
        }
        
        // Smoothly interpolate toward target
        _steeringCurrent = Lerp(_steeringCurrent, _steeringTarget, 0.1f);
        _currentSteeringCorrection = _steeringCurrent;
        
        // Note: Controller output is sent independently by the controller loop
        // This method only updates shared state - no blocking operations
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        SetEnabled(false);
        _steeringTimer.Stop();
        _vigemService.Dispose();
    }

    private void StartControllerLoop()
    {
        _controllerLoopCts = new CancellationTokenSource();
        var token = _controllerLoopCts.Token;
        
        _controllerLoopTask = Task.Run(async () => await ControllerOutputLoop(token), token);
    }

    private void StopControllerLoop()
    {
        if (_controllerLoopCts == null)
            return;
        
        _controllerLoopCts.Cancel();
        
        try
        {
            _controllerLoopTask?.Wait(TimeSpan.FromSeconds(2));
        }
        catch
        {
            // Ignore timeout
        }
        
        _controllerLoopCts?.Dispose();
        _controllerLoopCts = null;
        _controllerLoopTask = null;
    }

    private async Task ControllerOutputLoop(CancellationToken cancellationToken)
    {
        // Target 200 Hz (5ms interval), minimum 120 Hz (8.33ms)
        const int targetIntervalMs = 5;
        var sw = Stopwatch.StartNew();
        TimeSpan lastSendTime = TimeSpan.Zero;
        
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Check if it's time to send
                if (sw.Elapsed - lastSendTime >= TimeSpan.FromMilliseconds(targetIntervalMs))
                {
                    lastSendTime = sw.Elapsed;
                    
                    // Send controller report with current state
                    SendControllerReport();
                }
                
                // Small delay to prevent CPU spinning
                // At 200 Hz target, we need tight loop but not 100% CPU
                await Task.Delay(1, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                // Continue loop on non-fatal errors
                _ = ex; // Suppress unused warning
            }
        }
    }

    private void SendControllerReport()
    {
        if (!_isControllerConnected)
            return;
        
        try
        {
            // Convert throttle (0-1) to RT value (0-255)
            // brake 1.0 => RT 0, brake 0.0 => RT 255
            byte rtValue = (byte)Math.Clamp(
                (int)(_currentThrottleOutput / 100f * TriggerMaxRaw),
                0, 255);
            
            // LT must remain 0 (requirement)
            byte ltValue = 0;
            
            // Convert steering correction (-5 to +5) to left stick X
            // Range: -5 => 0, 0 => 128, +5 => 255
            float steeringOffset = _currentSteeringCorrection * (SteeringMaxRaw / 5f);
            byte stickX = (byte)Math.Clamp(SteeringCenter + steeringOffset, 0, 255);
            byte stickY = 128; // Centered
            
            // Apply inputs
            _vigemService.SetLeftTrigger(ltValue);
            _vigemService.SetRightTrigger(rtValue);
            _vigemService.SetLeftStick(stickX, stickY);
            
            // ViGEm 1.21.256 sends immediately on Set* calls
            // No explicit SendReport() needed
        }
        catch (Exception ex)
        {
            _isControllerConnected = false;
            _ = ex; // Suppress unused warning
        }
    }

    private void ResetOutputs()
    {
        _currentThrottleOutput = 0f;
        _currentBrakeInput = 0f;
        _currentSteeringCorrection = 0f;
        _lastThrottleOutput = 0f;
        _steeringCurrent = 0f;
        _steeringTarget = 0f;
        
        // Reset controller hardware
        _vigemService.Reset();
    }

    private static float Lerp(float a, float b, float t) => a + (b - a) * t;
}