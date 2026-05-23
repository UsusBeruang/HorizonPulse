namespace HorizonPulse.Automation.Models;

/// <summary>
/// Runtime state of the automation system.
/// Provides current values for UI display and monitoring.
/// Thread-safe for concurrent access.
/// </summary>
public sealed class AutomationState
{
    private readonly object _lock = new();
    private bool _isEnabled;
    private bool _isThrottleAssistEnabled;
    private bool _isSteeringAssistEnabled;
    private float _currentThrottleOutput;
    private float _currentBrakeInput;
    private float _currentSteeringCorrection;
    private bool _isControllerConnected;
    private bool _isRunning;

    /// <summary>
    /// Gets or sets whether automation is currently enabled.
    /// </summary>
    public bool IsEnabled
    {
        get { lock (_lock) return _isEnabled; }
        set { lock (_lock) _isEnabled = value; }
    }

    /// <summary>
    /// Gets or sets whether throttle assist is active.
    /// </summary>
    public bool IsThrottleAssistEnabled
    {
        get { lock (_lock) return _isThrottleAssistEnabled; }
        set { lock (_lock) _isThrottleAssistEnabled = value; }
    }

    /// <summary>
    /// Gets or sets whether steering assist is active.
    /// </summary>
    public bool IsSteeringAssistEnabled
    {
        get { lock (_lock) return _isSteeringAssistEnabled; }
        set { lock (_lock) _isSteeringAssistEnabled = value; }
    }

    /// <summary>
    /// Gets or sets the current virtual throttle output (0-100).
    /// </summary>
    public float CurrentThrottleOutput
    {
        get { lock (_lock) return _currentThrottleOutput; }
        set { lock (_lock) _currentThrottleOutput = Clamp(value); }
    }

    /// <summary>
    /// Gets or sets the current brake input from telemetry (0-100).
    /// </summary>
    public float CurrentBrakeInput
    {
        get { lock (_lock) return _currentBrakeInput; }
        set { lock (_lock) _currentBrakeInput = Clamp(value); }
    }

    /// <summary>
    /// Gets or sets the current steering correction (-5 to +5).
    /// </summary>
    public float CurrentSteeringCorrection
    {
        get { lock (_lock) return _currentSteeringCorrection; }
        set { lock (_lock) _currentSteeringCorrection = ClampSteering(value); }
    }

    /// <summary>
    /// Gets or sets whether the ViGEm controller is connected.
    /// </summary>
    public bool IsControllerConnected
    {
        get { lock (_lock) return _isControllerConnected; }
        set { lock (_lock) _isControllerConnected = value; }
    }

    /// <summary>
    /// Gets or sets whether the automation loop is running.
    /// </summary>
    public bool IsRunning
    {
        get { lock (_lock) return _isRunning; }
        set { lock (_lock) _isRunning = value; }
    }

    /// <summary>
    /// Creates a snapshot of the current state for UI binding.
    /// </summary>
    public AutomationState Snapshot()
    {
        lock (_lock)
        {
            return new AutomationState
            {
                _isEnabled = _isEnabled,
                _isThrottleAssistEnabled = _isThrottleAssistEnabled,
                _isSteeringAssistEnabled = _isSteeringAssistEnabled,
                _currentThrottleOutput = _currentThrottleOutput,
                _currentBrakeInput = _currentBrakeInput,
                _currentSteeringCorrection = _currentSteeringCorrection,
                _isControllerConnected = _isControllerConnected,
                _isRunning = _isRunning
            };
        }
    }

    /// <summary>
    /// Resets all runtime values to defaults.
    /// </summary>
    public void Reset()
    {
        lock (_lock)
        {
            _currentThrottleOutput = 0f;
            _currentBrakeInput = 0f;
            _currentSteeringCorrection = 0f;
        }
    }

    private static float Clamp(float value) => Math.Max(0f, Math.Min(100f, value));
    private static float ClampSteering(float value) => Math.Max(-5f, Math.Min(5f, value));
}
