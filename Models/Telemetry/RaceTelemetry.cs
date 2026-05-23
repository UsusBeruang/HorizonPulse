namespace ForzaHorizon6.Models.Telemetry;

/// <summary>
/// Race status telemetry data.
/// </summary>
public readonly struct RaceTelemetry
{
    /// <summary>
    /// 1 = racing; 0 = menus/stopped
    /// </summary>
    public int IsRaceOn { get; }

    /// <summary>
    /// Millisecond timestamp; can overflow to 0
    /// </summary>
    public uint TimestampMS { get; }

    public RaceTelemetry(int isRaceOn, uint timestampMS)
    {
        IsRaceOn = isRaceOn;
        TimestampMS = timestampMS;
    }
}
