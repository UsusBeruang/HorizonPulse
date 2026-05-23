namespace HorizonPulse.Automation.Interfaces;

/// <summary>
/// Interface for ViGEm virtual controller service.
/// Provides Xbox controller emulation capabilities.
/// </summary>
public interface IVigemControllerService : IDisposable
{
    /// <summary>
    /// Gets a value indicating whether the virtual controller is connected.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Connects to the ViGEm bus and initializes the virtual controller.
    /// </summary>
    /// <returns>True if connection was successful.</returns>
    bool Connect();

    /// <summary>
    /// Disconnects from the ViGEm bus and releases resources.
    /// </summary>
    void Disconnect();

    /// <summary>
    /// Sets the left trigger (RT) value.
    /// </summary>
    /// <param name="value">Value between 0 and 255.</param>
    void SetLeftTrigger(byte value);

    /// <summary>
    /// Sets the right trigger (LT) value.
    /// </summary>
    /// <param name="value">Value between 0 and 255.</param>
    void SetRightTrigger(byte value);

    /// <summary>
    /// Sets the left analog stick position.
    /// </summary>
    /// <param name="x">X axis value (0-255, 128 = center).</param>
    /// <param name="y">Y axis value (0-255, 128 = center).</param>
    void SetLeftStick(byte x, byte y);

    /// <summary>
    /// Sets the right analog stick position.
    /// </summary>
    /// <param name="x">X axis value (0-255, 128 = center).</param>
    /// <param name="y">Y axis value (0-255, 128 = center).</param>
    void SetRightStick(byte x, byte y);

    /// <summary>
    /// Sets a button state on the virtual controller.
    /// </summary>
    /// <param name="button">Button identifier.</param>
    /// <param name="isPressed">Whether the button is pressed.</param>
    void SetButton(Xbox360Button button, bool isPressed);

    /// <summary>
    /// Sends the current controller state to the ViGEm driver.
    /// </summary>
    void SendReport();

    /// <summary>
    /// Resets all inputs to neutral/default state.
    /// </summary>
    void Reset();
}

/// <summary>
/// Xbox 360 controller button identifiers.
/// </summary>
public enum Xbox360Button
{
    Up,
    Down,
    Left,
    Right,
    Start,
    Back,
    LeftThumb,
    RightThumb,
    LeftShoulder,
    RightShoulder,
    A,
    B,
    X,
    Y,
    Guide
}
