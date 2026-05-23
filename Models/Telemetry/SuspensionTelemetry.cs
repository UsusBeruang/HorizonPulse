namespace ForzaHorizon6.Models.Telemetry;

/// <summary>
/// Suspension telemetry data.
/// 0.0 = max stretch; 1.0 = max compression
/// </summary>
public readonly struct SuspensionTelemetry
{
    /// <summary>
    /// Normalized suspension travel front left (0=stretch, 1=compression)
    /// </summary>
    public float NormalizedSuspensionTravelFrontLeft { get; }

    /// <summary>
    /// Normalized suspension travel front right (0=stretch, 1=compression)
    /// </summary>
    public float NormalizedSuspensionTravelFrontRight { get; }

    /// <summary>
    /// Normalized suspension travel rear left (0=stretch, 1=compression)
    /// </summary>
    public float NormalizedSuspensionTravelRearLeft { get; }

    /// <summary>
    /// Normalized suspension travel rear right (0=stretch, 1=compression)
    /// </summary>
    public float NormalizedSuspensionTravelRearRight { get; }

    /// <summary>
    /// Suspension travel front left (meters)
    /// </summary>
    public float SuspensionTravelMetersFrontLeft { get; }

    /// <summary>
    /// Suspension travel front right (meters)
    /// </summary>
    public float SuspensionTravelMetersFrontRight { get; }

    /// <summary>
    /// Suspension travel rear left (meters)
    /// </summary>
    public float SuspensionTravelMetersRearLeft { get; }

    /// <summary>
    /// Suspension travel rear right (meters)
    /// </summary>
    public float SuspensionTravelMetersRearRight { get; }

    public SuspensionTelemetry(
        float normalizedFrontLeft, float normalizedFrontRight,
        float normalizedRearLeft, float normalizedRearRight,
        float metersFrontLeft, float metersFrontRight,
        float metersRearLeft, float metersRearRight)
    {
        NormalizedSuspensionTravelFrontLeft = normalizedFrontLeft;
        NormalizedSuspensionTravelFrontRight = normalizedFrontRight;
        NormalizedSuspensionTravelRearLeft = normalizedRearLeft;
        NormalizedSuspensionTravelRearRight = normalizedRearRight;
        SuspensionTravelMetersFrontLeft = metersFrontLeft;
        SuspensionTravelMetersFrontRight = metersFrontRight;
        SuspensionTravelMetersRearLeft = metersRearLeft;
        SuspensionTravelMetersRearRight = metersRearRight;
    }
}
