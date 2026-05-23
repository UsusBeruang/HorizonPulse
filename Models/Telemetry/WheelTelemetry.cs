namespace ForzaHorizon6.Models.Telemetry;

/// <summary>
/// Wheel telemetry data.
/// </summary>
public readonly struct WheelTelemetry
{
    /// <summary>
    /// Tire slip ratio front left (0 = 100% grip; |ratio| > 1.0 = loss of grip)
    /// </summary>
    public float TireSlipRatioFrontLeft { get; }

    /// <summary>
    /// Tire slip ratio front right (0 = 100% grip; |ratio| > 1.0 = loss of grip)
    /// </summary>
    public float TireSlipRatioFrontRight { get; }

    /// <summary>
    /// Tire slip ratio rear left (0 = 100% grip; |ratio| > 1.0 = loss of grip)
    /// </summary>
    public float TireSlipRatioRearLeft { get; }

    /// <summary>
    /// Tire slip ratio rear right (0 = 100% grip; |ratio| > 1.0 = loss of grip)
    /// </summary>
    public float TireSlipRatioRearRight { get; }

    /// <summary>
    /// Wheel rotation speed front left (rad/s)
    /// </summary>
    public float WheelRotationSpeedFrontLeft { get; }

    /// <summary>
    /// Wheel rotation speed front right (rad/s)
    /// </summary>
    public float WheelRotationSpeedFrontRight { get; }

    /// <summary>
    /// Wheel rotation speed rear left (rad/s)
    /// </summary>
    public float WheelRotationSpeedRearLeft { get; }

    /// <summary>
    /// Wheel rotation speed rear right (rad/s)
    /// </summary>
    public float WheelRotationSpeedRearRight { get; }

    /// <summary>
    /// Wheel on rumble strip front left (1 = true, 0 = false)
    /// </summary>
    public int WheelOnRumbleStripFrontLeft { get; }

    /// <summary>
    /// Wheel on rumble strip front right (1 = true, 0 = false)
    /// </summary>
    public int WheelOnRumbleStripFrontRight { get; }

    /// <summary>
    /// Wheel on rumble strip rear left (1 = true, 0 = false)
    /// </summary>
    public int WheelOnRumbleStripRearLeft { get; }

    /// <summary>
    /// Wheel on rumble strip rear right (1 = true, 0 = false)
    /// </summary>
    public int WheelOnRumbleStripRearRight { get; }

    /// <summary>
    /// Wheel in puddle front left (1 = true, 0 = false)
    /// </summary>
    public int WheelInPuddleFrontLeft { get; }

    /// <summary>
    /// Wheel in puddle front right (1 = true, 0 = false)
    /// </summary>
    public int WheelInPuddleFrontRight { get; }

    /// <summary>
    /// Wheel in puddle rear left (1 = true, 0 = false)
    /// </summary>
    public int WheelInPuddleRearLeft { get; }

    /// <summary>
    /// Wheel in puddle rear right (1 = true, 0 = false)
    /// </summary>
    public int WheelInPuddleRearRight { get; }

    /// <summary>
    /// Surface rumble front left (force feedback)
    /// </summary>
    public float SurfaceRumbleFrontLeft { get; }

    /// <summary>
    /// Surface rumble front right (force feedback)
    /// </summary>
    public float SurfaceRumbleFrontRight { get; }

    /// <summary>
    /// Surface rumble rear left (force feedback)
    /// </summary>
    public float SurfaceRumbleRearLeft { get; }

    /// <summary>
    /// Surface rumble rear right (force feedback)
    /// </summary>
    public float SurfaceRumbleRearRight { get; }

    /// <summary>
    /// Tire slip angle front left (0 = 100% grip; |angle| > 1.0 = loss of grip)
    /// </summary>
    public float TireSlipAngleFrontLeft { get; }

    /// <summary>
    /// Tire slip angle front right (0 = 100% grip; |angle| > 1.0 = loss of grip)
    /// </summary>
    public float TireSlipAngleFrontRight { get; }

    /// <summary>
    /// Tire slip angle rear left (0 = 100% grip; |angle| > 1.0 = loss of grip)
    /// </summary>
    public float TireSlipAngleRearLeft { get; }

    /// <summary>
    /// Tire slip angle rear right (0 = 100% grip; |angle| > 1.0 = loss of grip)
    /// </summary>
    public float TireSlipAngleRearRight { get; }

    /// <summary>
    /// Tire combined slip front left (0 = 100% grip; |slip| > 1.0 = loss of grip)
    /// </summary>
    public float TireCombinedSlipFrontLeft { get; }

    /// <summary>
    /// Tire combined slip front right (0 = 100% grip; |slip| > 1.0 = loss of grip)
    /// </summary>
    public float TireCombinedSlipFrontRight { get; }

    /// <summary>
    /// Tire combined slip rear left (0 = 100% grip; |slip| > 1.0 = loss of grip)
    /// </summary>
    public float TireCombinedSlipRearLeft { get; }

    /// <summary>
    /// Tire combined slip rear right (0 = 100% grip; |slip| > 1.0 = loss of grip)
    /// </summary>
    public float TireCombinedSlipRearRight { get; }

    public WheelTelemetry(
        float slipRatioFL, float slipRatioFR, float slipRatioRL, float slipRatioRR,
        float rotationSpeedFL, float rotationSpeedFR, float rotationSpeedRL, float rotationSpeedRR,
        int rumbleStripFL, int rumbleStripFR, int rumbleStripRL, int rumbleStripRR,
        int puddleFL, int puddleFR, int puddleRL, int puddleRR,
        float surfaceRumbleFL, float surfaceRumbleFR, float surfaceRumbleRL, float surfaceRumbleRR,
        float slipAngleFL, float slipAngleFR, float slipAngleRL, float slipAngleRR,
        float combinedSlipFL, float combinedSlipFR, float combinedSlipRL, float combinedSlipRR)
    {
        TireSlipRatioFrontLeft = slipRatioFL;
        TireSlipRatioFrontRight = slipRatioFR;
        TireSlipRatioRearLeft = slipRatioRL;
        TireSlipRatioRearRight = slipRatioRR;
        WheelRotationSpeedFrontLeft = rotationSpeedFL;
        WheelRotationSpeedFrontRight = rotationSpeedFR;
        WheelRotationSpeedRearLeft = rotationSpeedRL;
        WheelRotationSpeedRearRight = rotationSpeedRR;
        WheelOnRumbleStripFrontLeft = rumbleStripFL;
        WheelOnRumbleStripFrontRight = rumbleStripFR;
        WheelOnRumbleStripRearLeft = rumbleStripRL;
        WheelOnRumbleStripRearRight = rumbleStripRR;
        WheelInPuddleFrontLeft = puddleFL;
        WheelInPuddleFrontRight = puddleFR;
        WheelInPuddleRearLeft = puddleRL;
        WheelInPuddleRearRight = puddleRR;
        SurfaceRumbleFrontLeft = surfaceRumbleFL;
        SurfaceRumbleFrontRight = surfaceRumbleFR;
        SurfaceRumbleRearLeft = surfaceRumbleRL;
        SurfaceRumbleRearRight = surfaceRumbleRR;
        TireSlipAngleFrontLeft = slipAngleFL;
        TireSlipAngleFrontRight = slipAngleFR;
        TireSlipAngleRearLeft = slipAngleRL;
        TireSlipAngleRearRight = slipAngleRR;
        TireCombinedSlipFrontLeft = combinedSlipFL;
        TireCombinedSlipFrontRight = combinedSlipFR;
        TireCombinedSlipRearLeft = combinedSlipRL;
        TireCombinedSlipRearRight = combinedSlipRR;
    }
}
