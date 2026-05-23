namespace HorizonPulse.Telemetry.Runtime;

/// <summary>
/// Aggregated runtime telemetry state for UI binding.
/// Contains all telemetry categories as strongly-typed properties.
/// Thread-safe for updates from background telemetry ingestion.
/// </summary>
public sealed class RuntimeTelemetryState
{
    private readonly object _lock = new();

    public EngineTelemetryState Engine { get; } = new();
    public InputTelemetryState Input { get; } = new();
    public MotionTelemetryState Motion { get; } = new();
    public SuspensionTelemetryState Suspension { get; } = new();
    public WheelTelemetryState Wheel { get; } = new();
    public SurfaceTelemetryState Surface { get; } = new();
    public RaceStatusState RaceStatus { get; } = new();
    public RaceTimingState RaceTiming { get; } = new();
    public PositionTelemetryState Position { get; } = new();
    public EnvironmentTelemetryState Environment { get; } = new();
    public DamageTelemetryState Damage { get; } = new();
    public MiscTelemetryState Misc { get; } = new();

    /// <summary>
    /// Gets a value indicating whether the car is currently racing.
    /// </summary>
    public bool IsRaceOn => RaceStatus.IsRaceOn;

    /// <summary>
    /// Gets the current speed in km/h.
    /// </summary>
    public float SpeedKph => Position.SpeedKph;

    /// <summary>
    /// Gets the current engine RPM.
    /// </summary>
    public float Rpm => Engine.CurrentRpm;

    /// <summary>
    /// Gets the current gear.
    /// </summary>
    public byte Gear => Input.Gear;

    /// <summary>
    /// Gets the current lap number.
    /// </summary>
    public ushort LapNumber => RaceTiming.LapNumber;

    /// <summary>
    /// Gets the current race time in seconds.
    /// </summary>
    public float CurrentRaceTime => RaceTiming.CurrentRaceTime;

    /// <summary>
    /// Gets the throttle input as a normalized 0-1 value.
    /// </summary>
    public float Throttle => Input.ThrottlePercent;

    /// <summary>
    /// Gets the brake input as a normalized 0-1 value.
    /// </summary>
    public float BrakeInput => Input.BrakePercent;

    /// <summary>
    /// Gets the current G-force magnitude.
    /// </summary>
    public float GForce => Motion.GForceMagnitude;

    /// <summary>
    /// Gets the average tire temperature.
    /// </summary>
    public float AverageTireTemp => Surface.AverageTireTemp;

    /// <summary>
    /// Updates all telemetry state from parsed telemetry data.
    /// Thread-safe operation.
    /// </summary>
    /// <param name="telemetry">Parsed telemetry data.</param>
    public void Update(ForzaHorizon6.Models.Runtime.TelemetryData telemetry)
    {
        lock (_lock)
        {
            Engine.Update(telemetry.Engine);
            Input.Update(telemetry.Input);
            Motion.Update(telemetry.Motion);
            Suspension.Update(telemetry.Suspension);
            Wheel.Update(telemetry.Wheel);
            Surface.Update(telemetry.Surface);
            RaceStatus.Update(telemetry.Race);
            RaceTiming.Update(telemetry.RaceTiming);
            Position.Update(telemetry.Position);
            Environment.Update(telemetry.Environment);
            Damage.Update(telemetry.Damage);
            Misc.Update(telemetry.Misc);
        }
    }

    /// <summary>
    /// Attempts to update state without locking (for same-thread scenarios).
    /// Use only when thread safety is guaranteed by caller.
    /// </summary>
    public void UpdateUnsafe(ForzaHorizon6.Models.Runtime.TelemetryData telemetry)
    {
        Engine.Update(telemetry.Engine);
        Input.Update(telemetry.Input);
        Motion.Update(telemetry.Motion);
        Suspension.Update(telemetry.Suspension);
        Wheel.Update(telemetry.Wheel);
        Surface.Update(telemetry.Surface);
        RaceStatus.Update(telemetry.Race);
        RaceTiming.Update(telemetry.RaceTiming);
        Position.Update(telemetry.Position);
        Environment.Update(telemetry.Environment);
        Damage.Update(telemetry.Damage);
        Misc.Update(telemetry.Misc);
    }
}
