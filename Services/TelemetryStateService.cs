using HorizonPulse.Telemetry.Runtime;

namespace HorizonPulse.Services
{
    /// <summary>
    /// Legacy telemetry state service for backward compatibility.
    /// Wraps RuntimeTelemetryState to provide the old API.
    /// </summary>
    public class TelemetryStateService
    {
        private readonly RuntimeTelemetryState _runtimeState = new();

        /// <summary>
        /// Gets the underlying runtime telemetry state.
        /// </summary>
        public RuntimeTelemetryState RuntimeState => _runtimeState;

        /// <summary>
        /// Updates telemetry from parsed data.
        /// </summary>
        public void UpdateTelemetry(ForzaHorizon6.Models.Runtime.TelemetryData telemetry)
        {
            _runtimeState.Update(telemetry);
        }

        // Legacy properties for backward compatibility
        public bool IsRaceOn => _runtimeState.IsRaceOn;
        public float SpeedKph => _runtimeState.SpeedKph;
        public float Rpm => _runtimeState.Rpm;
        public byte Gear => _runtimeState.Gear;
        public float Throttle => _runtimeState.Throttle;
        public float BrakeInput => _runtimeState.BrakeInput;
        public ushort LapNumber => _runtimeState.LapNumber;
        public float CurrentRaceTime => _runtimeState.CurrentRaceTime;
        
        /// <summary>
        /// Gets the current runtime telemetry state for direct access.
        /// </summary>
        public RuntimeTelemetryState CurrentState => _runtimeState;
    }
}
