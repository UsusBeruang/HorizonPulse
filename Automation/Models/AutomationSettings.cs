namespace HorizonPulse.Automation.Models;

/// <summary>
/// Configuration settings for the automation system.
/// All settings are toggleable and can be changed at runtime.
/// </summary>
public sealed class AutomationSettings
{
    /// <summary>
    /// Gets or sets whether the automation system is enabled.
    /// Default: false (disabled by default for safety).
    /// </summary>
    public bool IsEnabled { get; set; } = false;

    /// <summary>
    /// Gets or sets whether throttle assist is enabled.
    /// When enabled, modulates throttle based on brake telemetry.
    /// Default: false.
    /// </summary>
    public bool IsThrottleAssistEnabled { get; set; } = false;

    /// <summary>
    /// Gets or sets whether steering assist is enabled.
    /// When enabled, applies subtle steering corrections.
    /// Default: false.
    /// </summary>
    public bool IsSteeringAssistEnabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the steering randomness intensity (0-1).
    /// 0 = no randomness, 1 = maximum randomness within ±5 range.
    /// Default: 0.3f (subtle human-like variation).
    /// </summary>
    public float SteeringRandomness { get; set; } = 0.3f;

    /// <summary>
    /// Gets or sets the throttle smoothing factor (0-1).
    /// Higher values produce smoother but slower throttle transitions.
    /// Default: 0.7f (balanced smoothness).
    /// </summary>
    public float ThrottleSmoothing { get; set; } = 0.7f;

    /// <summary>
    /// Gets or sets the update interval for steering corrections in milliseconds.
    /// Prevents rapid oscillation and feels more natural.
    /// Default: 100ms.
    /// </summary>
    public int SteeringUpdateIntervalMs { get; set; } = 100;

    /// <summary>
    /// Gets or sets the maximum steering correction magnitude.
    /// Range: 0 to 5 (clamped internally).
    /// Default: 5 (maximum allowed per requirements).
    /// </summary>
    public float MaxSteeringCorrection { get; set; } = 5.0f;

    /// <summary>
    /// Gets or sets whether to automatically connect the ViGEm controller on start.
    /// Default: true.
    /// </summary>
    public bool AutoConnectController { get; set; } = true;

    /// <summary>
    /// Creates a deep copy of these settings.
    /// </summary>
    public AutomationSettings Clone() => new()
    {
        IsEnabled = IsEnabled,
        IsThrottleAssistEnabled = IsThrottleAssistEnabled,
        IsSteeringAssistEnabled = IsSteeringAssistEnabled,
        SteeringRandomness = SteeringRandomness,
        ThrottleSmoothing = ThrottleSmoothing,
        SteeringUpdateIntervalMs = SteeringUpdateIntervalMs,
        MaxSteeringCorrection = MaxSteeringCorrection,
        AutoConnectController = AutoConnectController
    };
}
