namespace HorizonPulse.Models.Telemetry;

/// <summary>
/// Input telemetry data (player inputs).
/// </summary>
public readonly struct InputTelemetry(
    byte accel,
    byte brake,
    byte clutch,
    byte handBrake,
    byte gear,
    sbyte steer,
    sbyte normalizedDrivingLine,
    sbyte normalizedAiBrakeDifference)
{
    /// <summary>
    /// Acceleration input (0-255)
    /// </summary>
    public byte Accel { get; } = accel;

    /// <summary>
    /// Brake input (0-255)
    /// </summary>
    public byte Brake { get; } = brake;

    /// <summary>
    /// Clutch input (0-255)
    /// </summary>
    public byte Clutch { get; } = clutch;

    /// <summary>
    /// Hand brake input (0-255)
    /// </summary>
    public byte HandBrake { get; } = handBrake;

    /// <summary>
    /// Current gear
    /// </summary>
    public byte Gear { get; } = gear;

    /// <summary>
    /// Steering input (-127 = full left, 127 = full right)
    /// </summary>
    public sbyte Steer { get; } = steer;

    /// <summary>
    /// Normalized driving line (-127 to 127)
    /// </summary>
    public sbyte NormalizedDrivingLine { get; } = normalizedDrivingLine;

    /// <summary>
    /// Normalized AI brake difference (-127 to 127)
    /// </summary>
    public sbyte NormalizedAIBrakeDifference { get; } = normalizedAiBrakeDifference;
}
