namespace ForzaHorizon6.Models.Telemetry;

/// <summary>
/// Motion telemetry data (car local space).
/// Axes: X = right, Y = up, Z = forward
/// </summary>
public readonly struct MotionTelemetry
{
    /// <summary>
    /// Acceleration along X axis (m/s²)
    /// </summary>
    public float AccelerationX { get; }

    /// <summary>
    /// Acceleration along Y axis (m/s²)
    /// </summary>
    public float AccelerationY { get; }

    /// <summary>
    /// Acceleration along Z axis (m/s²)
    /// </summary>
    public float AccelerationZ { get; }

    /// <summary>
    /// Velocity along X axis (m/s)
    /// </summary>
    public float VelocityX { get; }

    /// <summary>
    /// Velocity along Y axis (m/s)
    /// </summary>
    public float VelocityY { get; }

    /// <summary>
    /// Velocity along Z axis (m/s)
    /// </summary>
    public float VelocityZ { get; }

    /// <summary>
    /// Angular velocity around X axis - Pitch (rad/s)
    /// </summary>
    public float AngularVelocityX { get; }

    /// <summary>
    /// Angular velocity around Y axis - Yaw (rad/s)
    /// </summary>
    public float AngularVelocityY { get; }

    /// <summary>
    /// Angular velocity around Z axis - Roll (rad/s)
    /// </summary>
    public float AngularVelocityZ { get; }

    /// <summary>
    /// Yaw orientation (radians)
    /// </summary>
    public float Yaw { get; }

    /// <summary>
    /// Pitch orientation (radians)
    /// </summary>
    public float Pitch { get; }

    /// <summary>
    /// Roll orientation (radians)
    /// </summary>
    public float Roll { get; }

    public MotionTelemetry(
        float accelerationX, float accelerationY, float accelerationZ,
        float velocityX, float velocityY, float velocityZ,
        float angularVelocityX, float angularVelocityY, float angularVelocityZ,
        float yaw, float pitch, float roll)
    {
        AccelerationX = accelerationX;
        AccelerationY = accelerationY;
        AccelerationZ = accelerationZ;
        VelocityX = velocityX;
        VelocityY = velocityY;
        VelocityZ = velocityZ;
        AngularVelocityX = angularVelocityX;
        AngularVelocityY = angularVelocityY;
        AngularVelocityZ = angularVelocityZ;
        Yaw = yaw;
        Pitch = pitch;
        Roll = roll;
    }
}
