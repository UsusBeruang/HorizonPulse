namespace ForzaHorizon6.Models.Telemetry;

/// <summary>
/// Surface telemetry data (tire contact surface information).
/// </summary>
public readonly struct SurfaceTelemetry
{
    /// <summary>
    /// Tire temperature front left (Celsius)
    /// </summary>
    public float TireTempFrontLeft { get; }

    /// <summary>
    /// Tire temperature front right (Celsius)
    /// </summary>
    public float TireTempFrontRight { get; }

    /// <summary>
    /// Tire temperature rear left (Celsius)
    /// </summary>
    public float TireTempRearLeft { get; }

    /// <summary>
    /// Tire temperature rear right (Celsius)
    /// </summary>
    public float TireTempRearRight { get; }

    public SurfaceTelemetry(float tempFL, float tempFR, float tempRL, float tempRR)
    {
        TireTempFrontLeft = tempFL;
        TireTempFrontRight = tempFR;
        TireTempRearLeft = tempRL;
        TireTempRearRight = tempRR;
    }
}
