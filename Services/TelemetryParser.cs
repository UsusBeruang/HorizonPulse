namespace ForzaHorizon6.Services;

using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using ForzaHorizon6.Models.Runtime;
using ForzaHorizon6.Models.Telemetry;

/// <summary>
/// Forza Horizon 6 UDP telemetry packet parser.
/// Parses raw byte packets into strongly-typed telemetry models.
/// 
/// Reference: .ai/ForzaHorizon6_Telemetry.md
/// Packet size: 324 bytes | Protocol: UDP (one-way, outbound only)
/// </summary>
public sealed class TelemetryParser
{
    /// <summary>
    /// Expected packet size in bytes as per FH6 telemetry specification.
    /// </summary>
    public const int ExpectedPacketSize = 324;

    /// <summary>
    /// Parses a raw UDP telemetry packet into structured telemetry data.
    /// </summary>
    /// <param name="packet">Raw byte span containing the telemetry packet.</param>
    /// <returns>Parsed telemetry data with all fields populated.</returns>
    /// <exception cref="ArgumentException">Thrown when packet size does not match expected size.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TelemetryData Parse(ReadOnlySpan<byte> packet)
    {
        // Validate packet size before parsing
        if (packet.Length != ExpectedPacketSize)
        {
            ThrowInvalidPacketSize(packet.Length);
        }

        // Parse all fields sequentially according to telemetry documentation
        // Using little-endian BinaryPrimitives for performance and no allocations

        int offset = 0;

        // === Race Status (offset 0, size 8) ===
        // Field: IsRaceOn (S32) - Offset 0
        int isRaceOn = ReadInt32(packet, ref offset);
        // Field: TimestampMS (U32) - Offset 4
        uint timestampMS = ReadUInt32(packet, ref offset);
        var race = new RaceTelemetry(isRaceOn, timestampMS);

        // === Engine (offset 8, size 12) ===
        // Field: EngineMaxRpm (F32) - Offset 8
        float engineMaxRpm = ReadFloat(packet, ref offset);
        // Field: EngineIdleRpm (F32) - Offset 12
        float engineIdleRpm = ReadFloat(packet, ref offset);
        // Field: CurrentEngineRpm (F32) - Offset 16
        float currentEngineRpm = ReadFloat(packet, ref offset);
        var engine = new EngineTelemetry(engineMaxRpm, engineIdleRpm, currentEngineRpm);

        // === Motion - Acceleration (offset 20, size 12) ===
        // Field: AccelerationX (F32) - Offset 20
        float accelerationX = ReadFloat(packet, ref offset);
        // Field: AccelerationY (F32) - Offset 24
        float accelerationY = ReadFloat(packet, ref offset);
        // Field: AccelerationZ (F32) - Offset 28
        float accelerationZ = ReadFloat(packet, ref offset);

        // === Motion - Velocity (offset 32, size 12) ===
        // Field: VelocityX (F32) - Offset 32
        float velocityX = ReadFloat(packet, ref offset);
        // Field: VelocityY (F32) - Offset 36
        float velocityY = ReadFloat(packet, ref offset);
        // Field: VelocityZ (F32) - Offset 40
        float velocityZ = ReadFloat(packet, ref offset);

        // === Motion - Angular Velocity (offset 44, size 12) ===
        // Field: AngularVelocityX (F32) - Pitch - Offset 44
        float angularVelocityX = ReadFloat(packet, ref offset);
        // Field: AngularVelocityY (F32) - Yaw - Offset 48
        float angularVelocityY = ReadFloat(packet, ref offset);
        // Field: AngularVelocityZ (F32) - Roll - Offset 52
        float angularVelocityZ = ReadFloat(packet, ref offset);

        // === Orientation (offset 56, size 12) ===
        // Field: Yaw (F32) - Offset 56
        float yaw = ReadFloat(packet, ref offset);
        // Field: Pitch (F32) - Offset 60
        float pitch = ReadFloat(packet, ref offset);
        // Field: Roll (F32) - Offset 64
        float roll = ReadFloat(packet, ref offset);

        var motion = new MotionTelemetry(
            accelerationX, accelerationY, accelerationZ,
            velocityX, velocityY, velocityZ,
            angularVelocityX, angularVelocityY, angularVelocityZ,
            yaw, pitch, roll);

        // === Suspension - Normalized Travel (offset 68, size 16) ===
        // Field: NormalizedSuspensionTravelFrontLeft (F32) - Offset 68
        float normSuspFL = ReadFloat(packet, ref offset);
        // Field: NormalizedSuspensionTravelFrontRight (F32) - Offset 72
        float normSuspFR = ReadFloat(packet, ref offset);
        // Field: NormalizedSuspensionTravelRearLeft (F32) - Offset 76
        float normSuspRL = ReadFloat(packet, ref offset);
        // Field: NormalizedSuspensionTravelRearRight (F32) - Offset 80
        float normSuspRR = ReadFloat(packet, ref offset);

        // === Tire Slip Ratio (offset 84, size 16) ===
        // Field: TireSlipRatioFrontLeft (F32) - Offset 84
        float slipRatioFL = ReadFloat(packet, ref offset);
        // Field: TireSlipRatioFrontRight (F32) - Offset 88
        float slipRatioFR = ReadFloat(packet, ref offset);
        // Field: TireSlipRatioRearLeft (F32) - Offset 92
        float slipRatioRL = ReadFloat(packet, ref offset);
        // Field: TireSlipRatioRearRight (F32) - Offset 96
        float slipRatioRR = ReadFloat(packet, ref offset);

        // === Wheel Rotation Speed (offset 100, size 16) ===
        // Field: WheelRotationSpeedFrontLeft (F32) - Offset 100
        float wheelSpeedFL = ReadFloat(packet, ref offset);
        // Field: WheelRotationSpeedFrontRight (F32) - Offset 104
        float wheelSpeedFR = ReadFloat(packet, ref offset);
        // Field: WheelRotationSpeedRearLeft (F32) - Offset 108
        float wheelSpeedRL = ReadFloat(packet, ref offset);
        // Field: WheelRotationSpeedRearRight (F32) - Offset 112
        float wheelSpeedRR = ReadFloat(packet, ref offset);

        // === Wheel Flags - Rumble Strip (offset 116, size 16) ===
        // Field: WheelOnRumbleStripFrontLeft (S32) - Offset 116
        int rumbleStripFL = ReadInt32(packet, ref offset);
        // Field: WheelOnRumbleStripFrontRight (S32) - Offset 120
        int rumbleStripFR = ReadInt32(packet, ref offset);
        // Field: WheelOnRumbleStripRearLeft (S32) - Offset 124
        int rumbleStripRL = ReadInt32(packet, ref offset);
        // Field: WheelOnRumbleStripRearRight (S32) - Offset 128
        int rumbleStripRR = ReadInt32(packet, ref offset);

        // === Wheel Flags - Puddle (offset 132, size 16) ===
        // Field: WheelInPuddleFrontLeft (S32) - Offset 132
        int puddleFL = ReadInt32(packet, ref offset);
        // Field: WheelInPuddleFrontRight (S32) - Offset 136
        int puddleFR = ReadInt32(packet, ref offset);
        // Field: WheelInPuddleRearLeft (S32) - Offset 140
        int puddleRL = ReadInt32(packet, ref offset);
        // Field: WheelInPuddleRearRight (S32) - Offset 144
        int puddleRR = ReadInt32(packet, ref offset);

        // === Surface Rumble (offset 148, size 16) ===
        // Field: SurfaceRumbleFrontLeft (F32) - Offset 148
        float surfaceRumbleFL = ReadFloat(packet, ref offset);
        // Field: SurfaceRumbleFrontRight (F32) - Offset 152
        float surfaceRumbleFR = ReadFloat(packet, ref offset);
        // Field: SurfaceRumbleRearLeft (F32) - Offset 156
        float surfaceRumbleRL = ReadFloat(packet, ref offset);
        // Field: SurfaceRumbleRearRight (F32) - Offset 160
        float surfaceRumbleRR = ReadFloat(packet, ref offset);

        // === Tire Slip Angle (offset 164, size 16) ===
        // Field: TireSlipAngleFrontLeft (F32) - Offset 164
        float slipAngleFL = ReadFloat(packet, ref offset);
        // Field: TireSlipAngleFrontRight (F32) - Offset 168
        float slipAngleFR = ReadFloat(packet, ref offset);
        // Field: TireSlipAngleRearLeft (F32) - Offset 172
        float slipAngleRL = ReadFloat(packet, ref offset);
        // Field: TireSlipAngleRearRight (F32) - Offset 176
        float slipAngleRR = ReadFloat(packet, ref offset);

        // === Tire Combined Slip (offset 180, size 16) ===
        // Field: TireCombinedSlipFrontLeft (F32) - Offset 180
        float combinedSlipFL = ReadFloat(packet, ref offset);
        // Field: TireCombinedSlipFrontRight (F32) - Offset 184
        float combinedSlipFR = ReadFloat(packet, ref offset);
        // Field: TireCombinedSlipRearLeft (F32) - Offset 188
        float combinedSlipRL = ReadFloat(packet, ref offset);
        // Field: TireCombinedSlipRearRight (F32) - Offset 192
        float combinedSlipRR = ReadFloat(packet, ref offset);

        // === Suspension Travel Meters (offset 196, size 16) ===
        // Field: SuspensionTravelMetersFrontLeft (F32) - Offset 196
        float suspMetersFL = ReadFloat(packet, ref offset);
        // Field: SuspensionTravelMetersFrontRight (F32) - Offset 200
        float suspMetersFR = ReadFloat(packet, ref offset);
        // Field: SuspensionTravelMetersRearLeft (F32) - Offset 204
        float suspMetersRL = ReadFloat(packet, ref offset);
        // Field: SuspensionTravelMetersRearRight (F32) - Offset 208
        float suspMetersRR = ReadFloat(packet, ref offset);

        var suspension = new SuspensionTelemetry(
            normSuspFL, normSuspFR, normSuspRL, normSuspRR,
            suspMetersFL, suspMetersFR, suspMetersRL, suspMetersRR);

        var wheel = new WheelTelemetry(
            slipRatioFL, slipRatioFR, slipRatioRL, slipRatioRR,
            wheelSpeedFL, wheelSpeedFR, wheelSpeedRL, wheelSpeedRR,
            rumbleStripFL, rumbleStripFR, rumbleStripRL, rumbleStripRR,
            puddleFL, puddleFR, puddleRL, puddleRR,
            surfaceRumbleFL, surfaceRumbleFR, surfaceRumbleRL, surfaceRumbleRR,
            slipAngleFL, slipAngleFR, slipAngleRL, slipAngleRR,
            combinedSlipFL, combinedSlipFR, combinedSlipRL, combinedSlipRR);

        // === Car Info (offset 212, size 20) ===
        // Field: CarOrdinal (S32) - Offset 212
        int carOrdinal = ReadInt32(packet, ref offset);
        // Field: CarClass (S32) - Offset 216
        int carClass = ReadInt32(packet, ref offset);
        // Field: CarPerformanceIndex (S32) - Offset 220
        int carPerformanceIndex = ReadInt32(packet, ref offset);
        // Field: DrivetrainType (S32) - Offset 224
        int drivetrainType = ReadInt32(packet, ref offset);
        // Field: NumCylinders (S32) - Offset 228
        int numCylinders = ReadInt32(packet, ref offset);

        // === FH6-Exclusive Fields (offset 232, size 12) ===
        // Field: CarGroup (U32) - Offset 232
        uint carGroup = ReadUInt32(packet, ref offset);
        // Field: SmashableVelDiff (F32) - Offset 236
        float smashableVelDiff = ReadFloat(packet, ref offset);
        // Field: SmashableMass (F32) - Offset 240
        float smashableMass = ReadFloat(packet, ref offset);

        var misc = new MiscTelemetry(
            carOrdinal, carClass, carPerformanceIndex,
            drivetrainType, numCylinders, carGroup);

        var damage = new DamageTelemetry(smashableVelDiff, smashableMass);

        // === Position & Dynamics (offset 244, size 24) ===
        // Field: PositionX (F32) - Offset 244
        float positionX = ReadFloat(packet, ref offset);
        // Field: PositionY (F32) - Offset 248
        float positionY = ReadFloat(packet, ref offset);
        // Field: PositionZ (F32) - Offset 252
        float positionZ = ReadFloat(packet, ref offset);
        // Field: Speed (F32) - Offset 256
        float speed = ReadFloat(packet, ref offset);
        // Field: Power (F32) - Offset 260
        float power = ReadFloat(packet, ref offset);
        // Field: Torque (F32) - Offset 264
        float torque = ReadFloat(packet, ref offset);

        var position = new PositionTelemetry(positionX, positionY, positionZ, speed, power, torque);

        // === Tire Temperature (offset 268, size 16) ===
        // Field: TireTempFrontLeft (F32) - Offset 268
        float tireTempFL = ReadFloat(packet, ref offset);
        // Field: TireTempFrontRight (F32) - Offset 272
        float tireTempFR = ReadFloat(packet, ref offset);
        // Field: TireTempRearLeft (F32) - Offset 276
        float tireTempRL = ReadFloat(packet, ref offset);
        // Field: TireTempRearRight (F32) - Offset 280
        float tireTempRR = ReadFloat(packet, ref offset);

        var surface = new SurfaceTelemetry(tireTempFL, tireTempFR, tireTempRL, tireTempRR);

        // === Vehicle Systems (offset 284, size 12) ===
        // Field: Boost (F32) - Offset 284
        float boost = ReadFloat(packet, ref offset);
        // Field: Fuel (F32) - Offset 288
        float fuel = ReadFloat(packet, ref offset);
        // Field: DistanceTraveled (F32) - Offset 292
        float distanceTraveled = ReadFloat(packet, ref offset);

        var environment = new EnvironmentTelemetry(boost, fuel, distanceTraveled);

        // === Lap & Race Timing (offset 296, size 18) ===
        // Field: BestLap (F32) - Offset 296
        float bestLap = ReadFloat(packet, ref offset);
        // Field: LastLap (F32) - Offset 300
        float lastLap = ReadFloat(packet, ref offset);
        // Field: CurrentLap (F32) - Offset 304
        float currentLap = ReadFloat(packet, ref offset);
        // Field: CurrentRaceTime (F32) - Offset 308
        float currentRaceTime = ReadFloat(packet, ref offset);
        // Field: LapNumber (U16) - Offset 312
        ushort lapNumber = ReadUInt16(packet, ref offset);
        // Field: RacePosition (U8) - Offset 314
        byte racePosition = ReadByte(packet, ref offset);

        // === Player Inputs (offset 315, size 9) ===
        // Field: Accel (U8) - Offset 315
        byte accel = ReadByte(packet, ref offset);
        // Field: Brake (U8) - Offset 316
        byte brake = ReadByte(packet, ref offset);
        // Field: Clutch (U8) - Offset 317
        byte clutch = ReadByte(packet, ref offset);
        // Field: HandBrake (U8) - Offset 318
        byte handBrake = ReadByte(packet, ref offset);
        // Field: Gear (U8) - Offset 319
        byte gear = ReadByte(packet, ref offset);
        // Field: Steer (S8) - Offset 320
        sbyte steer = ReadSByte(packet, ref offset);
        // Field: NormalizedDrivingLine (S8) - Offset 321
        sbyte normalizedDrivingLine = ReadSByte(packet, ref offset);
        // Field: NormalizedAIBrakeDifference (S8) - Offset 322
        sbyte normalizedAIBrakeDifference = ReadSByte(packet, ref offset);

        var input = new InputTelemetry(accel, brake, clutch, handBrake, gear, steer, normalizedDrivingLine, normalizedAIBrakeDifference);
        var raceTiming = new RaceTelemetryData(bestLap, lastLap, currentLap, currentRaceTime, lapNumber, racePosition);

        return new TelemetryData(race, engine, motion, suspension, wheel, surface, input, raceTiming, position, environment, damage, misc);
    }

    #region Read Helpers

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte ReadByte(ReadOnlySpan<byte> packet, ref int offset)
    {
        byte value = packet[offset];
        offset += 1;
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static sbyte ReadSByte(ReadOnlySpan<byte> packet, ref int offset)
    {
        sbyte value = (sbyte)packet[offset];
        offset += 1;
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ushort ReadUInt16(ReadOnlySpan<byte> packet, ref int offset)
    {
        ushort value = BinaryPrimitives.ReadUInt16LittleEndian(packet.Slice(offset));
        offset += 2;
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ReadInt32(ReadOnlySpan<byte> packet, ref int offset)
    {
        int value = BinaryPrimitives.ReadInt32LittleEndian(packet.Slice(offset));
        offset += 4;
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint ReadUInt32(ReadOnlySpan<byte> packet, ref int offset)
    {
        uint value = BinaryPrimitives.ReadUInt32LittleEndian(packet.Slice(offset));
        offset += 4;
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float ReadFloat(ReadOnlySpan<byte> packet, ref int offset)
    {
        int bits = BinaryPrimitives.ReadInt32LittleEndian(packet.Slice(offset));
        offset += 4;
        return Unsafe.As<int, float>(ref bits);
    }

    #endregion

    #region Exception Helpers

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowInvalidPacketSize(int actualSize)
    {
        throw new ArgumentException(
            $"Invalid FH6 telemetry packet size. Expected {ExpectedPacketSize} bytes, got {actualSize} bytes.",
            nameof(actualSize));
    }

    #endregion
}
