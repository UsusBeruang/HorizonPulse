using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using HorizonPulse.Models;

namespace HorizonPulse.Services
{
    public class TelemetryReceiver
    {
        private UdpClient _udpClient;
        private const int PacketSize = 324;
        private bool _isRunning;

        public event Action<ForzaTelemetry>? PacketReceived;

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

                    var telemetry = BytesToStruct<ForzaTelemetry>(data);
                    PacketReceived?.Invoke(telemetry);
                }
                catch (ObjectDisposedException) { break; }
                catch (Exception ex)
                {
                    // Log exception
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
