using HorizonPulse.Models;

namespace HorizonPulse.Services
{
    public class TelemetryStateService
    {
        private ForzaTelemetry _currentTelemetry;
        private readonly object _lock = new();

        public ForzaTelemetry CurrentTelemetry
        {
            get
            {
                lock (_lock)
                {
                    return _currentTelemetry;
                }
            }
            private set
            {
                lock (_lock)
                {
                    _currentTelemetry = value;
                }
            }
        }

        public void UpdateTelemetry(ForzaTelemetry telemetry)
        {
            CurrentTelemetry = telemetry;
        }

        public bool IsRaceOn => CurrentTelemetry.CurrentRaceTime > 0;
        public float SpeedKph => CurrentTelemetry.Speed * 3.6f;
        public float Rpm => CurrentTelemetry.RPM;
        public byte Gear => CurrentTelemetry.Gear;
        public float Throttle => CurrentTelemetry.Accel / 255f;
        public float BrakeInput => CurrentTelemetry.Brake / 255f;
        public ushort LapNumber => CurrentTelemetry.LapNumber;
        public float CurrentRaceTime => CurrentTelemetry.CurrentRaceTime;
    }
}
