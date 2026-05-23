namespace HorizonPulse.Automation.Interfaces;

/// <summary>
/// Interface for the main automation service.
/// Coordinates throttle and steering assist features.
/// </summary>
public interface IAutomationService : IDisposable
{
    /// <summary>
    /// Gets a value indicating whether automation is currently enabled.
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Gets a value indicating whether throttle assist is active.
    /// </summary>
    bool IsThrottleAssistEnabled { get; }

    /// <summary>
    /// Gets a value indicating whether steering assist is active.
    /// </summary>
    bool IsSteeringAssistEnabled { get; }

    /// <summary>
    /// Gets the current virtual throttle output (0-100).
    /// </summary>
    float CurrentThrottleOutput { get; }

    /// <summary>
    /// Gets the current brake telemetry value (0-100).
    /// </summary>
    float CurrentBrakeInput { get; }

    /// <summary>
    /// Gets the current steering correction value (-5 to +5).
    /// </summary>
    float CurrentSteeringCorrection { get; }

    /// <summary>
    /// Gets a value indicating whether the ViGEm controller is connected.
    /// </summary>
    bool IsControllerConnected { get; }

    /// <summary>
    /// Enables or disables the automation system.
    /// </summary>
    /// <param name="enabled">Whether to enable automation.</param>
    void SetEnabled(bool enabled);

    /// <summary>
    /// Enables or disables throttle assist.
    /// </summary>
    /// <param name="enabled">Whether to enable throttle assist.</param>
    void SetThrottleAssistEnabled(bool enabled);

    /// <summary>
    /// Enables or disables steering assist.
    /// </summary>
    /// <param name="enabled">Whether to enable steering assist.</param>
    void SetSteeringAssistEnabled(bool enabled);

    /// <summary>
    /// Sets the steering randomness intensity (0-1 scale).
    /// </summary>
    /// <param name="intensity">Randomness intensity from 0 (none) to 1 (max).</param>
    void SetSteeringRandomness(float intensity);

    /// <summary>
    /// Sets the throttle smoothing factor (0-1 scale).
    /// Higher values = smoother but slower response.
    /// </summary>
    /// <param name="factor">Smoothing factor from 0 (no smoothing) to 1 (max smoothing).</param>
    void SetThrottleSmoothing(float factor);

    /// <summary>
    /// Starts the automation loop.
    /// </summary>
    void Start();

    /// <summary>
    /// Stops the automation loop.
    /// </summary>
    void Stop();

    /// <summary>
    /// Attempts to connect the virtual controller.
    /// </summary>
    /// <returns>True if connection was successful.</returns>
    bool ConnectController();

    /// <summary>
    /// Disconnects the virtual controller.
    /// </summary>
    void DisconnectController();
}
