namespace ForzaHorizon6.Models.Telemetry;

/// <summary>
/// Environment telemetry data (vehicle systems and conditions).
/// </summary>
public readonly struct EnvironmentTelemetry
{
    /// <summary>
    /// Boost pressure in PSI above atmospheric
    /// </summary>
    public float Boost { get; }

    /// <summary>
    /// Fuel level (0.0 = empty, 1.0 = full)
    /// </summary>
    public float Fuel { get; }

    /// <summary>
    /// Distance traveled in meters
    /// </summary>
    public float DistanceTraveled { get; }

    public EnvironmentTelemetry(float boost, float fuel, float distanceTraveled)
    {
        Boost = boost;
        Fuel = fuel;
        DistanceTraveled = distanceTraveled;
    }
}
