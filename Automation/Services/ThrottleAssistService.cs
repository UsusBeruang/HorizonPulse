namespace HorizonPulse.Automation.Services;

using HorizonPulse.Telemetry.Runtime;

/// <summary>
/// Throttle assist service that modulates throttle based on brake telemetry.
/// Implements inverse mapping: when brake is high, throttle is low and vice versa.
/// Thread-safe with configurable smoothing for smooth transitions.
/// </summary>
public sealed class ThrottleAssistService : IDisposable
{
    private readonly object _lock = new();
    private bool _isEnabled;
    private float _smoothingFactor;
    private float _lastOutputValue;
    private bool _isDisposed;

    /// <summary>
    /// Gets or sets whether throttle assist is enabled.
    /// </summary>
    public bool IsEnabled
    {
        get { lock (_lock) return _isEnabled; }
        set { lock (_lock) _isEnabled = value; }
    }

    /// <summary>
    /// Gets or sets the smoothing factor (0-1).
    /// Higher values produce smoother but slower transitions.
    /// </summary>
    public float SmoothingFactor
    {
        get { lock (_lock) return _smoothingFactor; }
        set { lock (_lock) _smoothingFactor = Clamp(value); }
    }

    /// <summary>
    /// Gets the current throttle output value (0-100).
    /// </summary>
    public float CurrentOutput
    {
        get { lock (_lock) return _lastOutputValue; }
    }

    /// <summary>
    /// Initializes a new instance of the throttle assist service.
    /// </summary>
    /// <param name="smoothingFactor">Initial smoothing factor (default 0.7f).</param>
    public ThrottleAssistService(float smoothingFactor = 0.7f)
    {
        _smoothingFactor = smoothingFactor;
        _lastOutputValue = 0f;
    }

    /// <summary>
    /// Calculates throttle output based on brake input.
    /// Uses inverse mapping: throttle = 100 - brake.
    /// Applies smoothing to prevent sudden jumps.
    /// </summary>
    /// <param name="brakeInput">Current brake input (0-100).</param>
    /// <returns>Calculated throttle output (0-100).</returns>
    public float CalculateThrottle(float brakeInput)
    {
        lock (_lock)
        {
            if (!_isEnabled)
            {
                // When disabled, smoothly return to 0
                _lastOutputValue = ApplySmoothing(_lastOutputValue, 0f);
                return _lastOutputValue;
            }

            // Inverse mapping: brake 100 -> throttle 0, brake 0 -> throttle 100
            float targetThrottle = 100f - Clamp(brakeInput);

            // Apply smoothing to prevent sudden jumps
            _lastOutputValue = ApplySmoothing(_lastOutputValue, targetThrottle);

            return _lastOutputValue;
        }
    }

    /// <summary>
    /// Updates throttle assist from telemetry state.
    /// </summary>
    /// <param name="telemetryState">Current telemetry state.</param>
    /// <returns>Calculated throttle output (0-100).</returns>
    public float UpdateFromTelemetry(RuntimeTelemetryState telemetryState)
    {
        // BrakeInput is already normalized 0-1, convert to 0-100
        float brakePercent = telemetryState.BrakeInput * 100f;
        return CalculateThrottle(brakePercent);
    }

    /// <summary>
    /// Resets the throttle output to zero.
    /// </summary>
    public void Reset()
    {
        lock (_lock)
        {
            _lastOutputValue = 0f;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        lock (_lock)
        {
            if (_isDisposed)
                return;
            
            Reset();
            _isDisposed = true;
        }
    }

    private static float Clamp(float value) => Math.Max(0f, Math.Min(100f, value));

    private float ApplySmoothing(float current, float target)
    {
        if (_smoothingFactor <= 0f)
            return target;

        // Linear interpolation for smooth transitions
        return current + (target - current) * (1f - _smoothingFactor);
    }
}
