using System.Net.Sockets;
using System.Runtime.InteropServices;
using HorizonPulse.Telemetry.Services;

namespace HorizonPulse.Services
{
    /// <summary>
    /// UDP telemetry receiver that forwards raw packets to the ingestion service.
    /// Updated to work with the new TelemetryIngestionService.
    /// </summary>
    public class TelemetryReceiver
    {
        private UdpClient _udpClient;
        private const int PacketSize = 324;
        private bool _isRunning;
        private readonly TelemetryIngestionService _ingestionService;

        /// <summary>
        /// Gets the telemetry ingestion service for accessing parsed state.
        /// </summary>
        public TelemetryIngestionService Ingestion => _ingestionService;

        /// <summary>
        /// Legacy event for backward compatibility. Prefer using Ingestion.TelemetryReceived.
        /// </summary>
        [Obsolete("Use Ingestion.TelemetryReceived instead")]
        public event Action<Models.ForzaTelemetry>? PacketReceived;

        public TelemetryReceiver()
        {
            _ingestionService = new TelemetryIngestionService();
        }

        public void Start(int port = 54321)
        {
            if (_isRunning) return;

            _udpClient = new UdpClient(port);
            _isRunning = true;
            
            Task.Run(ListenLoop);
        }

        public void Stop()
        {
            _isRunning = false;
            _udpClient?.Close();
            _ingestionService?.Dispose();
        }

        private async Task ListenLoop()
        {
            while (_isRunning)
            {
                try
                {
                    var result = await _udpClient.ReceiveAsync();
                    var data = result.Buffer;

                    if (data.Length < PacketSize) continue;

                    // Process through new ingestion service
                    _ingestionService.ProcessPacket(data);

                    // Legacy support: convert struct for old subscribers
#pragma warning disable CS0618
                    if (PacketReceived != null)
                    {
                        var legacyTelemetry = BytesToStruct<Models.ForzaTelemetry>(data);
                        PacketReceived?.Invoke(legacyTelemetry);
                    }
#pragma warning restore CS0618
                }
                catch (ObjectDisposedException) { break; }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Telemetry receiver error: {ex.Message}");
                }
            }
        }

        private static T BytesToStruct<T>(byte[] data) where T : struct
        {
            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                return Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }
        }
    }
}
