namespace ForzaHorizon6.Models.Telemetry;

/// <summary>
/// Miscellaneous telemetry data (car info and FH6 exclusive fields).
/// </summary>
public readonly struct MiscTelemetry
{
    /// <summary>
    /// Unique car make/model ID
    /// </summary>
    public int CarOrdinal { get; }

    /// <summary>
    /// Car class: 0 (D) to 7 (X class)
    /// </summary>
    public int CarClass { get; }

    /// <summary>
    /// Car performance index: 100 (worst) to 999 (best)
    /// </summary>
    public int CarPerformanceIndex { get; }

    /// <summary>
    /// Drivetrain type: 0 = FWD, 1 = RWD, 2 = AWD
    /// </summary>
    public int DrivetrainType { get; }

    /// <summary>
    /// Engine cylinder count
    /// </summary>
    public int NumCylinders { get; }

    /// <summary>
    /// Car group identifier - FH6 exclusive
    /// </summary>
    public uint CarGroup { get; }

    public MiscTelemetry(
        int carOrdinal, int carClass, int carPerformanceIndex,
        int drivetrainType, int numCylinders, uint carGroup)
    {
        CarOrdinal = carOrdinal;
        CarClass = carClass;
        CarPerformanceIndex = carPerformanceIndex;
        DrivetrainType = drivetrainType;
        NumCylinders = numCylinders;
        CarGroup = carGroup;
    }
}
