namespace ForzaHorizon6.Models.Telemetry;

/// <summary>
/// Engine telemetry data.
/// </summary>
public readonly struct EngineTelemetry
{
    /// <summary>
    /// Maximum engine RPM
    /// </summary>
    public float EngineMaxRpm { get; }

    /// <summary>
    /// Idle engine RPM
    /// </summary>
    public float EngineIdleRpm { get; }

    /// <summary>
    /// Current engine RPM
    /// </summary>
    public float CurrentEngineRpm { get; }

    public EngineTelemetry(float engineMaxRpm, float engineIdleRpm, float currentEngineRpm)
    {
        EngineMaxRpm = engineMaxRpm;
        EngineIdleRpm = engineIdleRpm;
        CurrentEngineRpm = currentEngineRpm;
    }
}
