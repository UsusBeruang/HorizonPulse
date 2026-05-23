namespace HorizonPulse.Telemetry.Mapping;

using ForzaHorizon6.Models.Runtime;
using HorizonPulse.Telemetry.Runtime;

/// <summary>
/// Maps parsed telemetry data to runtime state models for UI binding.
/// Centralized mapping logic to keep ViewModels clean.
/// </summary>
public sealed class DashboardTelemetryMapper
{
    /// <summary>
    /// Maps raw telemetry data to all runtime state models.
    /// </summary>
    /// <param name="telemetry">Parsed telemetry data from the parser.</param>
    /// <param name="state">Target runtime state object to update.</param>
    public void MapToState(TelemetryData telemetry, RuntimeTelemetryState state)
    {
        // Engine
        state.Engine.Update(telemetry.Engine);

        // Inputs
        state.Input.Update(telemetry.Input);

        // Motion
        state.Motion.Update(telemetry.Motion);

        // Suspension
        state.Suspension.Update(telemetry.Suspension);

        // Wheel
        state.Wheel.Update(telemetry.Wheel);

        // Surface/Temperatures
        state.Surface.Update(telemetry.Surface);

        // Race status
        state.RaceStatus.Update(telemetry.Race);

        // Race timing
        state.RaceTiming.Update(telemetry.RaceTiming);

        // Position/dynamics
        state.Position.Update(telemetry.Position);

        // Environment
        state.Environment.Update(telemetry.Environment);

        // Damage
        state.Damage.Update(telemetry.Damage);

        // Misc car info
        state.Misc.Update(telemetry.Misc);
    }
}
