namespace ForzaHorizon6.Models.Telemetry;

/// <summary>
/// Race timing and lap telemetry data.
/// </summary>
public readonly struct RaceTelemetryData
{
    /// <summary>
    /// Best lap time in seconds (0.0 if not applicable)
    /// </summary>
    public float BestLap { get; }

    /// <summary>
    /// Last lap time in seconds (0.0 if not applicable)
    /// </summary>
    public float LastLap { get; }

    /// <summary>
    /// Current lap time in seconds
    /// </summary>
    public float CurrentLap { get; }

    /// <summary>
    /// Current race time in seconds since driving started
    /// </summary>
    public float CurrentRaceTime { get; }

    /// <summary>
    /// Completed laps
    /// </summary>
    public ushort LapNumber { get; }

    /// <summary>
    /// Current race position
    /// </summary>
    public byte RacePosition { get; }

    public RaceTelemetryData(
        float bestLap, float lastLap, float currentLap, float currentRaceTime,
        ushort lapNumber, byte racePosition)
    {
        BestLap = bestLap;
        LastLap = lastLap;
        CurrentLap = currentLap;
        CurrentRaceTime = currentRaceTime;
        LapNumber = lapNumber;
        RacePosition = racePosition;
    }
}
