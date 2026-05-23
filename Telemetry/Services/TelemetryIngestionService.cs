namespace HorizonPulse.Telemetry.Services;

using System.Buffers;
using ForzaHorizon6.Models.Runtime;
using ForzaHorizon6.Services;
using HorizonPulse.Telemetry.Mapping;
using HorizonPulse.Telemetry.Runtime;

/// <summary>
/// High-frequency telemetry ingestion service.
/// Receives raw UDP packets, parses them, and updates runtime state.
/// Designed for thread-safe operation from background threads.
/// </summary>
public sealed class TelemetryIngestionService : IDisposable
{
    private readonly TelemetryParser _parser = new();
    private readonly DashboardTelemetryMapper _mapper = new();
    private readonly RuntimeTelemetryState _state = new();
    private bool _disposed;

    /// <summary>
    /// Gets the current runtime telemetry state.
    /// Thread-safe to read from any thread.
    /// </summary>
    public RuntimeTelemetryState State => _state;

    /// <summary>
    /// Event raised when new telemetry is received and parsed.
    /// Includes both raw bytes and parsed data for debugging/inspection.
    /// </summary>
    public event Action<TelemetryData>? TelemetryReceived;

    /// <summary>
    /// Processes a raw UDP packet byte array.
    /// Parses the packet and updates internal state.
    /// Thread-safe - can be called from any thread.
    /// </summary>
    /// <param name="packet">Raw telemetry packet bytes.</param>
    public void ProcessPacket(ReadOnlySpan<byte> packet)
    {
        if (_disposed) return;

        try
        {
            // Parse the packet using the parser
            var telemetry = _parser.Parse(packet);

            // Update runtime state (thread-safe)
            _state.Update(telemetry);

            // Raise event for any subscribers
            TelemetryReceived?.Invoke(telemetry);
        }
        catch (ArgumentException ex)
        {
            // Invalid packet size - log but don't crash
            System.Diagnostics.Debug.WriteLine($"[TelemetryIngestion] Invalid packet: {ex.Message}");
        }
        catch (Exception ex)
        {
            // Unexpected error during parsing - log but continue
            System.Diagnostics.Debug.WriteLine($"[TelemetryIngestion] Parse error: {ex.Message}");
        }
    }

    /// <summary>
    /// Tries to process a packet, returning false if it fails.
    /// Non-throwing variant for scenarios where exceptions are undesirable.
    /// </summary>
    public bool TryProcessPacket(ReadOnlySpan<byte> packet, out TelemetryData telemetry)
    {
        telemetry = default;

        if (_disposed) return false;

        try
        {
            telemetry = _parser.Parse(packet);
            _state.Update(telemetry);
            TelemetryReceived?.Invoke(telemetry);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }
}
