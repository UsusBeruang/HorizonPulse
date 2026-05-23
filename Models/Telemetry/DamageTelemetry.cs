namespace ForzaHorizon6.Models.Telemetry;

/// <summary>
/// Damage telemetry data (FH6 exclusive smashable collision data).
/// </summary>
public readonly struct DamageTelemetry
{
    /// <summary>
    /// Velocity loss from collision (m/s) - FH6 exclusive
    /// </summary>
    public float SmashableVelDiff { get; }

    /// <summary>
    /// Mass of hit object (kg) - FH6 exclusive
    /// </summary>
    public float SmashableMass { get; }

    public DamageTelemetry(float smashableVelDiff, float smashableMass)
    {
        SmashableVelDiff = smashableVelDiff;
        SmashableMass = smashableMass;
    }
}
