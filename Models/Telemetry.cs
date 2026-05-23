using System.Runtime.InteropServices;

namespace HorizonPulse.Models
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ForzaTelemetry
    {
        // Vehicle Status
        public float Speed;
        public float RPM;
        public byte Gear;
        public ushort LapNumber;
        public float CurrentRaceTime;
        
        // Inputs
        public byte Accel;
        public byte Brake;
        public sbyte Steer;
        
        // Timing
        public float CurrentLap;
        
        // Additional fields from the original struct
        public float AccelerationX;
        public float AccelerationY;
        public float AccelerationZ;
        public float VelocityX;
        public float VelocityY;
        public float VelocityZ;
        public float AngularVelocityX;
        public float AngularVelocityY;
        public float AngularVelocityZ;
        public float Yaw;
        public float Pitch;
        public float Roll;
        public float TireSlipRatioFrontLeft;
        public float TireSlipRatioFrontRight;
        public float TireSlipRatioRearLeft;
        public float TireSlipRatioRearRight;
        public float WheelRotationSpeedFrontLeft;
        public float WheelRotationSpeedFrontRight;
        public float WheelRotationSpeedRearLeft;
        public float WheelRotationSpeedRearRight;
        public int WheelOnRumbleStripFrontLeft;
        public int WheelOnRumbleStripFrontRight;
        public int WheelOnRumbleStripRearLeft;
        public int WheelOnRumbleStripRearRight;
        public int WheelInPuddleFrontLeft;
        public int WheelInPuddleFrontRight;
        public int WheelInPuddleRearLeft;
        public int WheelInPuddleRearRight;
        public float SurfaceRumbleFrontLeft;
        public float SurfaceRumbleFrontRight;
        public float SurfaceRumbleRearLeft;
        public float SurfaceRumbleRearRight;
        public float TireSlipAngleFrontLeft;
        public float TireSlipAngleFrontRight;
        public float TireSlipAngleRearLeft;
        public float TireSlipAngleRearRight;
        public float TireCombinedSlipFrontLeft;
        public float TireCombinedSlipFrontRight;
        public float TireCombinedSlipRearLeft;
        public float TireCombinedSlipRearRight;
        public float SuspensionTravelMetersFrontLeft;
        public float SuspensionTravelMetersFrontRight;
        public float SuspensionTravelMetersRearLeft;
        public float SuspensionTravelMetersRearRight;
        public int CarOrdinal;
        public int CarClass;
        public int CarPerformanceIndex;
        public int DrivetrainType;
        public int NumCylinders;
        public uint CarGroup;
        public float SmashableVelDiff;
        public float SmashableMass;
        public float PositionX;
        public float PositionY;
        public float PositionZ;
        public float Power;
        public float Torque;
        public float TireTempFrontLeft;
        public float TireTempFrontRight;
        public float TireTempRearLeft;
        public float TireTempRearRight;
        public float Boost;
        public float Fuel;
        public float DistanceTraveled;
        public float BestLap;
        public float LastLap;
        public float CurrentLapTime;
        public byte RacePosition;
        public sbyte NormalizedDrivingLine;
        public sbyte NormalizedBrakeDifference;
    }
}
