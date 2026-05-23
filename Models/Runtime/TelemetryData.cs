namespace ForzaHorizon6.Models.Runtime;

using ForzaHorizon6.Models.Telemetry;

/// <summary>
/// Root telemetry data model containing all telemetry categories.
/// </summary>
public readonly struct TelemetryData
{
    /// <summary>
    /// Race status telemetry
    /// </summary>
    public RaceTelemetry Race { get; }

    /// <summary>
    /// Engine telemetry
    /// </summary>
    public EngineTelemetry Engine { get; }

    /// <summary>
    /// Motion telemetry (car local space)
    /// </summary>
    public MotionTelemetry Motion { get; }

    /// <summary>
    /// Suspension telemetry
    /// </summary>
    public SuspensionTelemetry Suspension { get; }

    /// <summary>
    /// Wheel telemetry
    /// </summary>
    public WheelTelemetry Wheel { get; }

    /// <summary>
    /// Surface telemetry (tire contact)
    /// </summary>
    public SurfaceTelemetry Surface { get; }

    /// <summary>
    /// Input telemetry (player inputs)
    /// </summary>
    public InputTelemetry Input { get; }

    /// <summary>
    /// Race timing and lap telemetry
    /// </summary>
    public RaceTelemetryData RaceTiming { get; }

    /// <summary>
    /// Position and dynamics telemetry
    /// </summary>
    public PositionTelemetry Position { get; }

    /// <summary>
    /// Environment telemetry (vehicle systems)
    /// </summary>
    public EnvironmentTelemetry Environment { get; }

    /// <summary>
    /// Damage telemetry (FH6 exclusive)
    /// </summary>
    public DamageTelemetry Damage { get; }

    /// <summary>
    /// Miscellaneous telemetry (car info)
    /// </summary>
    public MiscTelemetry Misc { get; }

    public TelemetryData(
        RaceTelemetry race,
        EngineTelemetry engine,
        MotionTelemetry motion,
        SuspensionTelemetry suspension,
        WheelTelemetry wheel,
        SurfaceTelemetry surface,
        InputTelemetry input,
        RaceTelemetryData raceTiming,
        PositionTelemetry position,
        EnvironmentTelemetry environment,
        DamageTelemetry damage,
        MiscTelemetry misc)
    {
        Race = race;
        Engine = engine;
        Motion = motion;
        Suspension = suspension;
        Wheel = wheel;
        Surface = surface;
        Input = input;
        RaceTiming = raceTiming;
        Position = position;
        Environment = environment;
        Damage = damage;
        Misc = misc;
    }
}
