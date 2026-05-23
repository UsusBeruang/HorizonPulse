namespace HorizonPulse.Telemetry.Runtime;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Runtime representation of engine telemetry for UI binding.
/// Provides computed properties and formatted values.
/// </summary>
public sealed class EngineTelemetryState
{
    private float _currentRpm;
    private float _maxRpm;
    private float _idleRpm;

    public float CurrentRpm
    {
        get => _currentRpm;
        set => _currentRpm = value;
    }

    public float MaxRpm
    {
        get => _maxRpm;
        set => _maxRpm = value;
    }

    public float IdleRpm
    {
        get => _idleRpm;
        set => _idleRpm = value;
    }

    public float RpmPercent => _maxRpm > 0 ? _currentRpm / _maxRpm : 0f;

    public void Update(ForzaHorizon6.Models.Telemetry.EngineTelemetry telemetry)
    {
        _currentRpm = telemetry.CurrentEngineRpm;
        _maxRpm = telemetry.EngineMaxRpm;
        _idleRpm = telemetry.EngineIdleRpm;
    }
}

/// <summary>
/// Runtime representation of input telemetry for UI binding.
/// </summary>
public sealed class InputTelemetryState
{
    private byte _accel;
    private byte _brake;
    private byte _clutch;
    private byte _handBrake;
    private byte _gear;
    private sbyte _steer;
    private sbyte _normalizedDrivingLine;
    private sbyte _normalizedAIBrakeDifference;

    public byte Accel
    {
        get => _accel;
        set => _accel = value;
    }

    public byte Brake
    {
        get => _brake;
        set => _brake = value;
    }

    public byte Clutch
    {
        get => _clutch;
        set => _clutch = value;
    }

    public byte HandBrake
    {
        get => _handBrake;
        set => _handBrake = value;
    }

    public byte Gear
    {
        get => _gear;
        set => _gear = value;
    }

    public sbyte Steer
    {
        get => _steer;
        set => _steer = value;
    }

    public sbyte NormalizedDrivingLine
    {
        get => _normalizedDrivingLine;
        set => _normalizedDrivingLine = value;
    }

    public sbyte NormalizedAIBrakeDifference
    {
        get => _normalizedAIBrakeDifference;
        set => _normalizedAIBrakeDifference = value;
    }

    // Computed normalized values (0-1 range)
    public float ThrottlePercent => _accel / 255f;
    public float BrakePercent => _brake / 255f;
    public float ClutchPercent => _clutch / 255f;
    public float HandBrakePercent => _handBrake / 255f;
    public float SteerNormalized => _steer / 127f;

    public void Update(ForzaHorizon6.Models.Telemetry.InputTelemetry telemetry)
    {
        _accel = telemetry.Accel;
        _brake = telemetry.Brake;
        _clutch = telemetry.Clutch;
        _handBrake = telemetry.HandBrake;
        _gear = telemetry.Gear;
        _steer = telemetry.Steer;
        _normalizedDrivingLine = telemetry.NormalizedDrivingLine;
        _normalizedAIBrakeDifference = telemetry.NormalizedAIBrakeDifference;
    }
}

/// <summary>
/// Runtime representation of motion telemetry for UI binding.
/// </summary>
public sealed class MotionTelemetryState
{
    private float _accelX, _accelY, _accelZ;
    private float _velX, _velY, _velZ;
    private float _angVelX, _angVelY, _angVelZ;
    private float _yaw, _pitch, _roll;

    public float AccelerationX { get => _accelX; set => _accelX = value; }
    public float AccelerationY { get => _accelY; set => _accelY = value; }
    public float AccelerationZ { get => _accelZ; set => _accelZ = value; }

    public float VelocityX { get => _velX; set => _velX = value; }
    public float VelocityY { get => _velY; set => _velY = value; }
    public float VelocityZ { get => _velZ; set => _velZ = value; }

    public float AngularVelocityX { get => _angVelX; set => _angVelX = value; }
    public float AngularVelocityY { get => _angVelY; set => _angVelY = value; }
    public float AngularVelocityZ { get => _angVelZ; set => _angVelZ = value; }

    public float Yaw { get => _yaw; set => _yaw = value; }
    public float Pitch { get => _pitch; set => _pitch = value; }
    public float Roll { get => _roll; set => _roll = value; }

    // Computed G-force magnitude
    public float GForceMagnitude => MathF.Sqrt(_accelX * _accelX + _accelY * _accelY + _accelZ * _accelZ) / 9.81f;

    public void Update(ForzaHorizon6.Models.Telemetry.MotionTelemetry telemetry)
    {
        _accelX = telemetry.AccelerationX;
        _accelY = telemetry.AccelerationY;
        _accelZ = telemetry.AccelerationZ;
        _velX = telemetry.VelocityX;
        _velY = telemetry.VelocityY;
        _velZ = telemetry.VelocityZ;
        _angVelX = telemetry.AngularVelocityX;
        _angVelY = telemetry.AngularVelocityY;
        _angVelZ = telemetry.AngularVelocityZ;
        _yaw = telemetry.Yaw;
        _pitch = telemetry.Pitch;
        _roll = telemetry.Roll;
    }
}

/// <summary>
/// Runtime representation of suspension telemetry for UI binding.
/// </summary>
public sealed class SuspensionTelemetryState
{
    private float _normFL, _normFR, _normRL, _normRR;
    private float _metersFL, _metersFR, _metersRL, _metersRR;

    public float NormalizedFrontLeft { get => _normFL; set => _normFL = value; }
    public float NormalizedFrontRight { get => _normFR; set => _normFR = value; }
    public float NormalizedRearLeft { get => _normRL; set => _normRL = value; }
    public float NormalizedRearRight { get => _normRR; set => _normRR = value; }

    public float MetersFrontLeft { get => _metersFL; set => _metersFL = value; }
    public float MetersFrontRight { get => _metersFR; set => _metersFR = value; }
    public float MetersRearLeft { get => _metersRL; set => _metersRL = value; }
    public float MetersRearRight { get => _metersRR; set => _metersRR = value; }

    public void Update(ForzaHorizon6.Models.Telemetry.SuspensionTelemetry telemetry)
    {
        _normFL = telemetry.NormalizedSuspensionTravelFrontLeft;
        _normFR = telemetry.NormalizedSuspensionTravelFrontRight;
        _normRL = telemetry.NormalizedSuspensionTravelRearLeft;
        _normRR = telemetry.NormalizedSuspensionTravelRearRight;
        _metersFL = telemetry.SuspensionTravelMetersFrontLeft;
        _metersFR = telemetry.SuspensionTravelMetersFrontRight;
        _metersRL = telemetry.SuspensionTravelMetersRearLeft;
        _metersRR = telemetry.SuspensionTravelMetersRearRight;
    }
}

/// <summary>
/// Runtime representation of wheel telemetry for UI binding.
/// </summary>
public sealed class WheelTelemetryState
{
    private float _slipRatioFL, _slipRatioFR, _slipRatioRL, _slipRatioRR;
    private float _rotSpeedFL, _rotSpeedFR, _rotSpeedRL, _rotSpeedRR;
    private int _rumbleFL, _rumbleFR, _rumbleRL, _rumbleRR;
    private int _puddleFL, _puddleFR, _puddleRL, _puddleRR;
    private float _rumbleForceFL, _rumbleForceFR, _rumbleForceRL, _rumbleForceRR;
    private float _slipAngleFL, _slipAngleFR, _slipAngleRL, _slipAngleRR;
    private float _combinedSlipFL, _combinedSlipFR, _combinedSlipRL, _combinedSlipRR;

    public float SlipRatioFrontLeft { get => _slipRatioFL; set => _slipRatioFL = value; }
    public float SlipRatioFrontRight { get => _slipRatioFR; set => _slipRatioFR = value; }
    public float SlipRatioRearLeft { get => _slipRatioRL; set => _slipRatioRL = value; }
    public float SlipRatioRearRight { get => _slipRatioRR; set => _slipRatioRR = value; }

    public float RotationSpeedFrontLeft { get => _rotSpeedFL; set => _rotSpeedFL = value; }
    public float RotationSpeedFrontRight { get => _rotSpeedFR; set => _rotSpeedFR = value; }
    public float RotationSpeedRearLeft { get => _rotSpeedRL; set => _rotSpeedRL = value; }
    public float RotationSpeedRearRight { get => _rotSpeedRR; set => _rotSpeedRR = value; }

    public bool OnRumbleStripFrontLeft => _rumbleFL != 0;
    public bool OnRumbleStripFrontRight => _rumbleFR != 0;
    public bool OnRumbleStripRearLeft => _rumbleRL != 0;
    public bool OnRumbleStripRearRight => _rumbleRR != 0;

    public bool InPuddleFrontLeft => _puddleFL != 0;
    public bool InPuddleFrontRight => _puddleFR != 0;
    public bool InPuddleRearLeft => _puddleRL != 0;
    public bool InPuddleRearRight => _puddleRR != 0;

    public float SurfaceRumbleFrontLeft { get => _rumbleForceFL; set => _rumbleForceFL = value; }
    public float SurfaceRumbleFrontRight { get => _rumbleForceFR; set => _rumbleForceFR = value; }
    public float SurfaceRumbleRearLeft { get => _rumbleForceRL; set => _rumbleForceRL = value; }
    public float SurfaceRumbleRearRight { get => _rumbleForceRR; set => _rumbleForceRR = value; }

    public float SlipAngleFrontLeft { get => _slipAngleFL; set => _slipAngleFL = value; }
    public float SlipAngleFrontRight { get => _slipAngleFR; set => _slipAngleFR = value; }
    public float SlipAngleRearLeft { get => _slipAngleRL; set => _slipAngleRL = value; }
    public float SlipAngleRearRight { get => _slipAngleRR; set => _slipAngleRR = value; }

    public float CombinedSlipFrontLeft { get => _combinedSlipFL; set => _combinedSlipFL = value; }
    public float CombinedSlipFrontRight { get => _combinedSlipFR; set => _combinedSlipFR = value; }
    public float CombinedSlipRearLeft { get => _combinedSlipRL; set => _combinedSlipRL = value; }
    public float CombinedSlipRearRight { get => _combinedSlipRR; set => _combinedSlipRR = value; }

    public void Update(ForzaHorizon6.Models.Telemetry.WheelTelemetry telemetry)
    {
        _slipRatioFL = telemetry.TireSlipRatioFrontLeft;
        _slipRatioFR = telemetry.TireSlipRatioFrontRight;
        _slipRatioRL = telemetry.TireSlipRatioRearLeft;
        _slipRatioRR = telemetry.TireSlipRatioRearRight;
        _rotSpeedFL = telemetry.WheelRotationSpeedFrontLeft;
        _rotSpeedFR = telemetry.WheelRotationSpeedFrontRight;
        _rotSpeedRL = telemetry.WheelRotationSpeedRearLeft;
        _rotSpeedRR = telemetry.WheelRotationSpeedRearRight;
        _rumbleFL = telemetry.WheelOnRumbleStripFrontLeft;
        _rumbleFR = telemetry.WheelOnRumbleStripFrontRight;
        _rumbleRL = telemetry.WheelOnRumbleStripRearLeft;
        _rumbleRR = telemetry.WheelOnRumbleStripRearRight;
        _puddleFL = telemetry.WheelInPuddleFrontLeft;
        _puddleFR = telemetry.WheelInPuddleFrontRight;
        _puddleRL = telemetry.WheelInPuddleRearLeft;
        _puddleRR = telemetry.WheelInPuddleRearRight;
        _rumbleForceFL = telemetry.SurfaceRumbleFrontLeft;
        _rumbleForceFR = telemetry.SurfaceRumbleFrontRight;
        _rumbleForceRL = telemetry.SurfaceRumbleRearLeft;
        _rumbleForceRR = telemetry.SurfaceRumbleRearRight;
        _slipAngleFL = telemetry.TireSlipAngleFrontLeft;
        _slipAngleFR = telemetry.TireSlipAngleFrontRight;
        _slipAngleRL = telemetry.TireSlipAngleRearLeft;
        _slipAngleRR = telemetry.TireSlipAngleRearRight;
        _combinedSlipFL = telemetry.TireCombinedSlipFrontLeft;
        _combinedSlipFR = telemetry.TireCombinedSlipFrontRight;
        _combinedSlipRL = telemetry.TireCombinedSlipRearLeft;
        _combinedSlipRR = telemetry.TireCombinedSlipRearRight;
    }
}

/// <summary>
/// Runtime representation of surface/temperature telemetry for UI binding.
/// </summary>
public sealed class SurfaceTelemetryState
{
    private float _tempFL, _tempFR, _tempRL, _tempRR;

    public float TireTempFrontLeft { get => _tempFL; set => _tempFL = value; }
    public float TireTempFrontRight { get => _tempFR; set => _tempFR = value; }
    public float TireTempRearLeft { get => _tempRL; set => _tempRL = value; }
    public float TireTempRearRight { get => _tempRR; set => _tempRR = value; }

    public float AverageTireTemp => (_tempFL + _tempFR + _tempRL + _tempRR) / 4f;

    public void Update(ForzaHorizon6.Models.Telemetry.SurfaceTelemetry telemetry)
    {
        _tempFL = telemetry.TireTempFrontLeft;
        _tempFR = telemetry.TireTempFrontRight;
        _tempRL = telemetry.TireTempRearLeft;
        _tempRR = telemetry.TireTempRearRight;
    }
}

/// <summary>
/// Runtime representation of race status telemetry for UI binding.
/// </summary>
public sealed class RaceStatusState
{
    private int _isRaceOn;
    private uint _timestampMS;

    public bool IsRaceOn => _isRaceOn != 0;
    public uint TimestampMS { get => _timestampMS; set => _timestampMS = value; }

    public void Update(ForzaHorizon6.Models.Telemetry.RaceTelemetry telemetry)
    {
        _isRaceOn = telemetry.IsRaceOn;
        _timestampMS = telemetry.TimestampMS;
    }
}

/// <summary>
/// Runtime representation of race timing telemetry for UI binding.
/// </summary>
public sealed class RaceTimingState
{
    private float _bestLap, _lastLap, _currentLap, _currentRaceTime;
    private ushort _lapNumber;
    private byte _racePosition;

    public float BestLap { get => _bestLap; set => _bestLap = value; }
    public float LastLap { get => _lastLap; set => _lastLap = value; }
    public float CurrentLap { get => _currentLap; set => _currentLap = value; }
    public float CurrentRaceTime { get => _currentRaceTime; set => _currentRaceTime = value; }
    public ushort LapNumber { get => _lapNumber; set => _lapNumber = value; }
    public byte RacePosition { get => _racePosition; set => _racePosition = value; }

    public void Update(ForzaHorizon6.Models.Telemetry.RaceTelemetryData telemetry)
    {
        _bestLap = telemetry.BestLap;
        _lastLap = telemetry.LastLap;
        _currentLap = telemetry.CurrentLap;
        _currentRaceTime = telemetry.CurrentRaceTime;
        _lapNumber = telemetry.LapNumber;
        _racePosition = telemetry.RacePosition;
    }
}

/// <summary>
/// Runtime representation of position/dynamics telemetry for UI binding.
/// </summary>
public sealed class PositionTelemetryState
{
    private float _x, _y, _z, _speed, _power, _torque;

    public float PositionX { get => _x; set => _x = value; }
    public float PositionY { get => _y; set => _y = value; }
    public float PositionZ { get => _z; set => _z = value; }
    public float Speed { get => _speed; set => _speed = value; }
    public float Power { get => _power; set => _power = value; }
    public float Torque { get => _torque; set => _torque = value; }

    public float SpeedKph => _speed * 3.6f;

    public void Update(ForzaHorizon6.Models.Telemetry.PositionTelemetry telemetry)
    {
        _x = telemetry.PositionX;
        _y = telemetry.PositionY;
        _z = telemetry.PositionZ;
        _speed = telemetry.Speed;
        _power = telemetry.Power;
        _torque = telemetry.Torque;
    }
}

/// <summary>
/// Runtime representation of environment telemetry for UI binding.
/// </summary>
public sealed class EnvironmentTelemetryState
{
    private float _boost, _fuel, _distanceTraveled;

    public float Boost { get => _boost; set => _boost = value; }
    public float Fuel { get => _fuel; set => _fuel = value; }
    public float DistanceTraveled { get => _distanceTraveled; set => _distanceTraveled = value; }

    public float FuelPercent => _fuel;

    public void Update(ForzaHorizon6.Models.Telemetry.EnvironmentTelemetry telemetry)
    {
        _boost = telemetry.Boost;
        _fuel = telemetry.Fuel;
        _distanceTraveled = telemetry.DistanceTraveled;
    }
}

/// <summary>
/// Runtime representation of damage telemetry for UI binding.
/// </summary>
public sealed class DamageTelemetryState
{
    private float _smashableVelDiff;
    private float _smashableMass;

    public float SmashableVelocityDiff { get => _smashableVelDiff; set => _smashableVelDiff = value; }
    public float SmashableMass { get => _smashableMass; set => _smashableMass = value; }

    public bool HasCollision => _smashableVelDiff > 0.1f;

    public void Update(ForzaHorizon6.Models.Telemetry.DamageTelemetry telemetry)
    {
        _smashableVelDiff = telemetry.SmashableVelDiff;
        _smashableMass = telemetry.SmashableMass;
    }
}

/// <summary>
/// Runtime representation of miscellaneous car info telemetry for UI binding.
/// </summary>
public sealed class MiscTelemetryState
{
    private int _carOrdinal;
    private int _carClass;
    private int _carPerformanceIndex;
    private int _drivetrainType;
    private int _numCylinders;
    private uint _carGroup;

    public int CarOrdinal { get => _carOrdinal; set => _carOrdinal = value; }
    public int CarClass { get => _carClass; set => _carClass = value; }
    public int CarPerformanceIndex { get => _carPerformanceIndex; set => _carPerformanceIndex = value; }
    public int DrivetrainType { get => _drivetrainType; set => _drivetrainType = value; }
    public int NumCylinders { get => _numCylinders; set => _numCylinders = value; }
    public uint CarGroup { get => _carGroup; set => _carGroup = value; }

    public string DrivetrainName => _drivetrainType switch
    {
        0 => "FWD",
        1 => "RWD",
        2 => "AWD",
        _ => $"Unknown ({_drivetrainType})"
    };

    public string CarClassName => _carClass switch
    {
        0 => "D",
        1 => "C",
        2 => "B",
        3 => "A",
        4 => "S1",
        5 => "S2",
        6 => "R",
        7 => "X",
        _ => $"Unknown ({_carClass})"
    };

    public void Update(ForzaHorizon6.Models.Telemetry.MiscTelemetry telemetry)
    {
        _carOrdinal = telemetry.CarOrdinal;
        _carClass = telemetry.CarClass;
        _carPerformanceIndex = telemetry.CarPerformanceIndex;
        _drivetrainType = telemetry.DrivetrainType;
        _numCylinders = telemetry.NumCylinders;
        _carGroup = telemetry.CarGroup;
    }
}
