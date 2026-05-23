namespace ForzaHorizon6.Models.Telemetry;

/// <summary>
/// Position and dynamics telemetry data.
/// </summary>
public readonly struct PositionTelemetry
{
    /// <summary>
    /// World space position X (meters)
    /// </summary>
    public float PositionX { get; }

    /// <summary>
    /// World space position Y (meters)
    /// </summary>
    public float PositionY { get; }

    /// <summary>
    /// World space position Z (meters)
    /// </summary>
    public float PositionZ { get; }

    /// <summary>
    /// Speed in m/s
    /// </summary>
    public float Speed { get; }

    /// <summary>
    /// Power in Watts
    /// </summary>
    public float Power { get; }

    /// <summary>
    /// Torque in Newton-meters
    /// </summary>
    public float Torque { get; }

    public PositionTelemetry(float x, float y, float z, float speed, float power, float torque)
    {
        PositionX = x;
        PositionY = y;
        PositionZ = z;
        Speed = speed;
        Power = power;
        Torque = torque;
    }
}
