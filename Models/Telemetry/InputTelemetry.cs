namespace ForzaHorizon6.Models.Telemetry;

/// <summary>
/// Input telemetry data (player inputs).
/// </summary>
public readonly struct InputTelemetry
{
    /// <summary>
    /// Acceleration input (0-255)
    /// </summary>
    public byte Accel { get; }

    /// <summary>
    /// Brake input (0-255)
    /// </summary>
    public byte Brake { get; }

    /// <summary>
    /// Clutch input (0-255)
    /// </summary>
    public byte Clutch { get; }

    /// <summary>
    /// Hand brake input (0-255)
    /// </summary>
    public byte HandBrake { get; }

    /// <summary>
    /// Current gear
    /// </summary>
    public byte Gear { get; }

    /// <summary>
    /// Steering input (-127 = full left, 127 = full right)
    /// </summary>
    public sbyte Steer { get; }

    /// <summary>
    /// Normalized driving line (-127 to 127)
    /// </summary>
    public sbyte NormalizedDrivingLine { get; }

    /// <summary>
    /// Normalized AI brake difference (-127 to 127)
    /// </summary>
    public sbyte NormalizedAIBrakeDifference { get; }

    public InputTelemetry(
        byte accel, byte brake, byte clutch, byte handBrake,
        byte gear, sbyte steer,
        sbyte normalizedDrivingLine, sbyte normalizedAIBrakeDifference)
    {
        Accel = accel;
        Brake = brake;
        Clutch = clutch;
        HandBrake = handBrake;
        Gear = gear;
        Steer = steer;
        NormalizedDrivingLine = normalizedDrivingLine;
        NormalizedAIBrakeDifference = normalizedAIBrakeDifference;
    }
}
