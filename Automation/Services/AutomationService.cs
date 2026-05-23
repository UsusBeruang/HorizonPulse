using System;
using System.Diagnostics;
using System.Threading;
using HorizonPulse.Automation.Interfaces;
using HorizonPulse.Automation.Models;
using HorizonPulse.Telemetry.Runtime;

namespace HorizonPulse.Automation.Services;

/// <summary>
/// Main automation service that coordinates throttle and steering assist.
/// Reads telemetry, calculates outputs, and sends controller inputs via ViGEm.
/// Thread-safe, runs on background thread, and integrates with existing telemetry pipeline.
/// </summary>
public sealed class AutomationService : IAutomationService
{
    private readonly object _lock = new();
    private readonly VigemControllerService _vigemService;
    private readonly ThrottleAssistService _throttleService;
    private readonly SteeringAssistService _steeringService;
    private readonly AutomationState _state;
    private AutomationSettings _settings;

    private Thread? _automationThread;
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isDisposed;
    private bool _isRunning;

    // Process tracking
    private readonly Stopwatch _processCheckTimer = new();
    private bool _isTargetGameRunning = false;
    private bool _wasTargetGameRunning = false;
    private const string TargetProcessName = "forzahorizon6"; // .exe is automatically implied by GetProcessesByName

    // Conversion constants
    private const float SteeringCenter = 128f;
    private const float SteeringMaxRaw = 127f;
    private const float TriggerMaxRaw = 255f;

    /// <inheritdoc/>
    public bool IsEnabled => _state.IsEnabled;

    /// <inheritdoc/>
    public bool IsThrottleAssistEnabled => _state.IsThrottleAssistEnabled;

    /// <inheritdoc/>
    public bool IsSteeringAssistEnabled => _state.IsSteeringAssistEnabled;

    /// <inheritdoc/>
    public float CurrentThrottleOutput => _state.CurrentThrottleOutput;

    /// <inheritdoc/>
    public float CurrentBrakeInput => _state.CurrentBrakeInput;

    /// <inheritdoc/>
    public float CurrentSteeringCorrection => _state.CurrentSteeringCorrection;

    /// <inheritdoc/>
    public bool IsControllerConnected => _vigemService.IsConnected;

    /// <summary>
    /// Initializes a new instance of the automation service.
    /// </summary>
    /// <param name="settings">Initial automation settings.</param>
    public AutomationService(AutomationSettings? settings = null)
    {
        _settings = settings?.Clone() ?? new AutomationSettings();
        _vigemService = new VigemControllerService();
        _throttleService = new ThrottleAssistService(_settings.ThrottleSmoothing);
        _steeringService = new SteeringAssistService(
            _settings.SteeringRandomness,
            _settings.MaxSteeringCorrection,
            _settings.SteeringUpdateIntervalMs);
        
        _state = new AutomationState();
        
        // Start process timer
        _processCheckTimer.Start();
        
        // Apply initial settings
        ApplySettingsToServices();
    }

    /// <inheritdoc/>
    public void SetEnabled(bool enabled)
    {
        lock (_lock)
        {
            _settings.IsEnabled = enabled;
            _state.IsEnabled = enabled;
            
            if (!enabled)
            {
                // Immediately reset outputs when disabled
                _throttleService.Reset();
                _steeringService.Reset();
                _vigemService.Reset();
                _vigemService.SendReport();
            }
        }
    }

    /// <inheritdoc/>
    public void SetThrottleAssistEnabled(bool enabled)
    {
        lock (_lock)
        {
            _settings.IsThrottleAssistEnabled = enabled;
            _state.IsThrottleAssistEnabled = enabled;
            _throttleService.IsEnabled = enabled;
            
            if (!enabled)
            {
                _throttleService.Reset();
            }
        }
    }

    /// <inheritdoc/>
    public void SetSteeringAssistEnabled(bool enabled)
    {
        lock (_lock)
        {
            _settings.IsSteeringAssistEnabled = enabled;
            _state.IsSteeringAssistEnabled = enabled;
            _steeringService.IsEnabled = enabled;
            
            if (!enabled)
            {
                _steeringService.Reset();
            }
        }
    }

    /// <inheritdoc/>
    public void SetSteeringRandomness(float intensity)
    {
        lock (_lock)
        {
            _settings.SteeringRandomness = intensity;
            _steeringService.RandomnessIntensity = intensity;
        }
    }

    /// <inheritdoc/>
    public void SetThrottleSmoothing(float factor)
    {
        lock (_lock)
        {
            _settings.ThrottleSmoothing = factor;
            _throttleService.SmoothingFactor = factor;
        }
    }

    /// <inheritdoc/>
    public void Start()
    {
        lock (_lock)
        {
            if (_isRunning)
                return;

            _cancellationTokenSource = new CancellationTokenSource();
            _automationThread = new Thread(AutomationLoop)
            {
                Name = "AutomationLoop",
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal
            };
            
            _state.IsRunning = true;
            _isRunning = true;
            _automationThread.Start(_cancellationTokenSource.Token);
        }
    }

    /// <inheritdoc/>
    public void Stop()
    {
        lock (_lock)
        {
            if (!_isRunning)
                return;

            _cancellationTokenSource?.Cancel();
            
            // Wait for thread to exit (with timeout)
            _automationThread?.Join(2000);
            
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            _automationThread = null;
            _isRunning = false;
            _state.IsRunning = false;
            
            // Reset outputs
            _throttleService.Reset();
            _steeringService.Reset();
            _vigemService.Reset();
            _vigemService.SendReport();
        }
    }

    /// <inheritdoc/>
    public bool ConnectController()
    {
        lock (_lock)
        {
            if (_vigemService.IsConnected)
                return true;

            bool connected = _vigemService.Connect();
            _state.IsControllerConnected = connected;
            return connected;
        }
    }

    /// <inheritdoc/>
    public void DisconnectController()
    {
        lock (_lock)
        {
            _vigemService.Disconnect();
            _state.IsControllerConnected = false;
        }
    }

    /// <summary>
    /// Updates automation state from telemetry.
    /// Called by the automation loop - reads from existing telemetry pipeline.
    /// </summary>
    /// <param name="telemetryState">Current runtime telemetry state.</param>
    public void UpdateFromTelemetry(RuntimeTelemetryState telemetryState)
    {
        if (!_settings.IsEnabled)
            return;

        // Read brake input from telemetry (already normalized 0-1)
        float brakePercent = telemetryState.BrakeInput * 100f;
        
        // Update state
        _state.CurrentBrakeInput = brakePercent;

        // Calculate throttle output if enabled
        if (_settings.IsThrottleAssistEnabled)
        {
            float throttleOutput = _throttleService.UpdateFromTelemetry(telemetryState);
            _state.CurrentThrottleOutput = throttleOutput;
        }
        else
        {
            _state.CurrentThrottleOutput = 0f;
        }

        // Calculate steering correction if enabled
        if (_settings.IsSteeringAssistEnabled)
        {
            float steeringCorrection = _steeringService.Update();
            _state.CurrentSteeringCorrection = steeringCorrection;
        }
        else
        {
            _state.CurrentSteeringCorrection = 0f;
        }

        // Send controller inputs
        SendControllerInputs();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        lock (_lock)
        {
            if (_isDisposed)
                return;

            Stop();
            _throttleService.Dispose();
            _steeringService.Dispose();
            _vigemService.Dispose();
            
            _isDisposed = true;
        }
    }

    /// <summary>
    /// Gets the current automation state snapshot for UI binding.
    /// </summary>
    public AutomationState GetStateSnapshot() => _state.Snapshot();

    private void AutomationLoop(object? obj)
    {
        var cancellationToken = (CancellationToken)obj!;
        const int updateIntervalMs = 16; // ~60 Hz update rate
        
        var lastUpdate = Stopwatch.GetTimestamp();
        
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Rate-limit the loop
                var now = Stopwatch.GetTimestamp();
                var elapsedMs = (now - lastUpdate) * 1000.0 / Stopwatch.Frequency;
                
                if (elapsedMs >= updateIntervalMs)
                {
                    lastUpdate = now;
                    
                    // Note: Telemetry updates come from external subscription
                    // This loop primarily handles controller output timing
                    if (_settings.IsEnabled && _vigemService.IsConnected)
                    {
                        // Ensure controller state is sent regularly
                        _vigemService.SendReport();
                    }
                }
                
                Thread.Sleep(1); // Small sleep to prevent CPU spinning
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                // Ignore unexpected errors in background loop
            }
        }
    }

    private void SendControllerInputs()
    {
        if (!_vigemService.IsConnected)
            return;

        // 1. Throttled Process Check (Every 2000ms to save CPU)
        if (_processCheckTimer.ElapsedMilliseconds > 2000)
        {
            _isTargetGameRunning = Process.GetProcessesByName(TargetProcessName).Length > 0;
            _processCheckTimer.Restart();
        }

        // 2. State Handling based on Process
        if (!_isTargetGameRunning)
        {
            // If the game just closed, we must reset the controller so it doesn't 
            // leave a stuck throttle/steering input broadcasting to Windows.
            if (_wasTargetGameRunning)
            {
                _vigemService.Reset();
                _wasTargetGameRunning = false;
            }
            return; // Abort sending new inputs
        }
        
        _wasTargetGameRunning = true;

        // 3. Normal Execution (Game is running, even in background)
        try
        {
            // Convert throttle (0-100) to trigger value (0-255)
            byte triggerValue = (byte)Math.Clamp(
                (_state.CurrentThrottleOutput / 100f) * TriggerMaxRaw,
                0f, 255f);

            // Convert steering correction (-5 to +5) to stick X offset
            float steeringOffset = _state.CurrentSteeringCorrection * (SteeringMaxRaw / 5f) * 0.15f;
            byte stickX = (byte)Math.Clamp(SteeringCenter + steeringOffset, 0f, 255f);

            // Explicitly cast SteeringCenter to (byte) to fix the float-to-byte parameter error on the Y axis
            byte stickY = (byte)SteeringCenter;

            // Apply inputs (both are now guaranteed to be bytes)
            _vigemService.SetRightTrigger(triggerValue);
            _vigemService.SetLeftStick(stickX, stickY); 
    
            // Send the report
            _vigemService.SendReport();
        }
        catch
        {
            // Handle ViGEm communication errors gracefully
            _state.IsControllerConnected = false;
        }
    }

    private void ApplySettingsToServices()
    {
        _throttleService.IsEnabled = _settings.IsThrottleAssistEnabled;
        _throttleService.SmoothingFactor = _settings.ThrottleSmoothing;
        
        _steeringService.IsEnabled = _settings.IsSteeringAssistEnabled;
        _steeringService.RandomnessIntensity = _settings.SteeringRandomness;
        _steeringService.MaxCorrection = _settings.MaxSteeringCorrection;
        _steeringService.UpdateIntervalMs = _settings.SteeringUpdateIntervalMs;
        
        _state.IsEnabled = _settings.IsEnabled;
        _state.IsThrottleAssistEnabled = _settings.IsThrottleAssistEnabled;
        _state.IsSteeringAssistEnabled = _settings.IsSteeringAssistEnabled;
    }
}