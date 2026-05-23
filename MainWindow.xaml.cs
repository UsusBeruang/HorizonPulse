using System.Windows;
using HorizonPulse.Services;
using HorizonPulse.Models;

namespace HorizonPulse
{
    public partial class MainWindow : Window
    {
        private readonly TelemetryReceiver _telemetryReceiver;
        private readonly TelemetryStateService _telemetryStateService;

        public MainWindow()
        {
            InitializeComponent();
            
            _telemetryReceiver = new TelemetryReceiver();
            _telemetryStateService = new TelemetryStateService();
            
            _telemetryReceiver.PacketReceived += OnTelemetryReceived;
            _telemetryReceiver.Start(54321);
            
            // Initialize UI with default values
            UpdateUI();
        }

        private void OnTelemetryReceived(ForzaTelemetry telemetry)
        {
            _telemetryStateService.UpdateTelemetry(telemetry);
            
            // Update UI on the UI thread
            Dispatcher.Invoke(() =>
            {
                UpdateUI();
            });
        }

        private void UpdateUI()
        {
            var state = _telemetryStateService;
            
            SpeedKphText.Text = $"{state.SpeedKph:F1}";
            RpmText.Text = $"{state.Rpm:F0}";
            GearText.Text = $"{state.Gear}";
            LapNumberText.Text = $"{state.LapNumber}";
            ThrottleText.Text = $"{state.Throttle:P1}";
            BrakeTextInput.Text = $"{state.BrakeInput:P1}";
            SteerText.Text = $"{state.CurrentTelemetry.Steer}";
            RaceTimeText.Text = $"{state.CurrentRaceTime:F1}s";
            CurrentLapTimeText.Text = $"{state.CurrentTelemetry.CurrentLap:F1}s";
            
            // Update status
            StatusText.Text = state.IsRaceOn ? "Racing" : "Not Racing";
            StatusText.Foreground = state.IsRaceOn ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green) 
                                                   : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
        }

        protected override void OnClosed(EventArgs e)
        {
            _telemetryReceiver.Stop();
            base.OnClosed(e);
        }
    }
}
