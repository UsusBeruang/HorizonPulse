using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using HorizonPulse.Services;
using HorizonPulse.Telemetry.Runtime;

namespace HorizonPulse
{
    public partial class MainWindow : Window
    {
        private readonly TelemetryReceiver _telemetryReceiver;
        private readonly RuntimeTelemetryState _runtimeState;
        
        // UI throttling optimized for 200 FPS telemetry
        // At 200 FPS, packets arrive every 5ms. We update UI at 60 FPS (16.67ms)
        // This processes ~3 packets per UI update, balancing responsiveness and performance
        private const int UiUpdateIntervalMs = 16; // ~60 FPS for UI
        private readonly DispatcherTimer _uiUpdateTimer;
        private bool _pendingUiUpdate;
        
        // Track high-priority events that should trigger immediate UI updates
        private bool _pendingCollisionEvent;
        
        // Cached UI element references to avoid FindName calls
        private Border _throttleBar;
        private Border _brakeBar;
        private Border _steerBar;
        private Border _throttleBarParent;
        private Border _brakeBarParent;
        
        // Cached brushes to avoid allocation on every update
        private readonly System.Windows.Media.SolidColorBrush _greenBrush;
        private readonly System.Windows.Media.SolidColorBrush _redBrush;

        public MainWindow()
        {
            InitializeComponent();
            
            _telemetryReceiver = new TelemetryReceiver();
            _runtimeState = new RuntimeTelemetryState();
            
            // Initialize cached brushes
            _greenBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);
            _redBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
            _greenBrush.Freeze();
            _redBrush.Freeze();
            
            // Cache UI element references
            _throttleBar = FindName("_ThrottleBar") as Border;
            _brakeBar = FindName("_BrakeBar") as Border;
            _steerBar = FindName("_SteerBar") as Border;
            
            if (_throttleBar?.Parent is Border tbParent) _throttleBarParent = tbParent;
            if (_brakeBar?.Parent is Border bbParent) _brakeBarParent = bbParent;
            
            // Setup throttled UI update timer optimized for 200 FPS telemetry
            _uiUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(UiUpdateIntervalMs)
            };
            _uiUpdateTimer.Tick += (s, e) =>
            {
                if (_pendingCollisionEvent)
                {
                    // Collision events get immediate priority - process even if no regular update pending
                    _pendingCollisionEvent = false;
                    UpdateUI();
                }
                else if (_pendingUiUpdate)
                {
                    _pendingUiUpdate = false;
                    UpdateUI();
                }
            };
            _uiUpdateTimer.Start();
            
            // Subscribe to parsed telemetry events from the ingestion service
            _telemetryReceiver.Ingestion.TelemetryReceived += OnTelemetryReceived;
            _telemetryReceiver.Start(54321);
            
            // Initialize UI with default values
            UpdateUI();
        }

        private void OnTelemetryReceived(ForzaHorizon6.Models.Runtime.TelemetryData telemetry)
        {
            // Update runtime state (thread-safe)
            _runtimeState.Update(telemetry);
            
            // Mark UI update as pending - will be processed by timer
            _pendingUiUpdate = true;
        }

        private void UpdateUI()
        {
            var state = _runtimeState;
            
            // Vehicle Status Card
            SpeedKphText.Text = $"{state.SpeedKph:F1}";
            RpmText.Text = $"{state.Rpm:F0}";
            GearText.Text = $"{state.Gear}";
            LapNumberText.Text = $"{state.LapNumber}";
            
            // Inputs Card - bars and text
            ThrottleText.Text = $"{state.Throttle:P1}";
            BrakeTextInput.Text = $"{state.BrakeInput:P1}";
            SteerText.Text = $"{state.Input.Steer}";
            
            // Update throttle bar width using cached reference
            if (_throttleBar != null && _throttleBarParent != null)
            {
                _throttleBar.Width = _throttleBarParent.ActualWidth * state.Throttle;
            }
            
            // Update brake bar width using cached reference
            if (_brakeBar != null && _brakeBarParent != null)
            {
                _brakeBar.Width = _brakeBarParent.ActualWidth * state.BrakeInput;
            }
            
            // Update steer bar position using cached reference
            if (_steerBar != null)
            {
                var steerNormalized = state.Input.Steer / 127f;
                // Center is 0, full left is -1, full right is 1
                _steerBar.HorizontalAlignment = steerNormalized switch
                {
                    < -0.1f => HorizontalAlignment.Left,
                    > 0.1f => HorizontalAlignment.Right,
                    _ => HorizontalAlignment.Center
                };
            }
            
            // Timing Card
            RaceTimeText.Text = $"{state.CurrentRaceTime:F1}s";
            CurrentLapTimeText.Text = $"{state.RaceTiming.CurrentLap:F1}s";
            
            // Update status using cached brushes
            StatusText.Text = state.IsRaceOn ? "Racing" : "Not Racing";
            StatusText.Foreground = state.IsRaceOn ? _greenBrush : _redBrush;
            
            // Populate inspector panel with extended telemetry
            UpdateInspectorPanel(state);
        }

        private void UpdateInspectorPanel(RuntimeTelemetryState state)
        {
            var panel = InspectorPanel;
            if (panel == null) return;
            
            // Skip full rebuild if no changes - inspector data changes less frequently
            // Only rebuild when there's significant change (e.g., collision, gear change)
            // For now, we keep the rebuild but it will be throttled by the UI timer
            
            panel.Children.Clear();
            
            // Add tire temperatures
            AddInspectorRow(panel, "Tire Temp FL", $"{state.Surface.TireTempFrontLeft:F1}°C");
            AddInspectorRow(panel, "Tire Temp FR", $"{state.Surface.TireTempFrontRight:F1}°C");
            AddInspectorRow(panel, "Tire Temp RL", $"{state.Surface.TireTempRearLeft:F1}°C");
            AddInspectorRow(panel, "Tire Temp RR", $"{state.Surface.TireTempRearRight:F1}°C");
            AddInspectorDivider(panel);
            
            // Add suspension travel
            AddInspectorRow(panel, "Susp FL", $"{state.Suspension.NormalizedFrontLeft:P0}");
            AddInspectorRow(panel, "Susp FR", $"{state.Suspension.NormalizedFrontRight:P0}");
            AddInspectorRow(panel, "Susp RL", $"{state.Suspension.NormalizedRearLeft:P0}");
            AddInspectorRow(panel, "Susp RR", $"{state.Suspension.NormalizedRearRight:P0}");
            AddInspectorDivider(panel);
            
            // Add wheel slip
            AddInspectorRow(panel, "Slip FL", $"{state.Wheel.SlipRatioFrontLeft:F2}");
            AddInspectorRow(panel, "Slip FR", $"{state.Wheel.SlipRatioFrontRight:F2}");
            AddInspectorRow(panel, "Slip RL", $"{state.Wheel.SlipRatioRearLeft:F2}");
            AddInspectorRow(panel, "Slip RR", $"{state.Wheel.SlipRatioRearRight:F2}");
            AddInspectorDivider(panel);
            
            // Add G-force
            AddInspectorRow(panel, "G-Force", $"{state.GForce:F2} G");
            
            // Add brake/clutch/handbrake
            AddInspectorRow(panel, "Clutch", $"{state.Input.ClutchPercent:P0}");
            AddInspectorRow(panel, "Handbrake", $"{state.Input.HandBrakePercent:P0}");
            AddInspectorDivider(panel);
            
            // Add drivetrain info
            AddInspectorRow(panel, "Drivetrain", state.Misc.DrivetrainName);
            AddInspectorRow(panel, "Class", state.Misc.CarClassName);
            AddInspectorRow(panel, "PI", $"{state.Misc.CarPerformanceIndex}");
            AddInspectorDivider(panel);
            
            // Add race position
            AddInspectorRow(panel, "Position", $"P{state.RaceTiming.RacePosition}");
            AddInspectorRow(panel, "Best Lap", FormatLapTime(state.RaceTiming.BestLap));
            
            // Add fuel/boost
            AddInspectorRow(panel, "Fuel", $"{state.Environment.FuelPercent:P0}");
            AddInspectorRow(panel, "Boost", $"{state.Environment.Boost:F1} PSI");
            
            // Add damage indicator
            if (state.Damage.HasCollision)
            {
                AddInspectorDivider(panel);
                AddInspectorRow(panel, "COLLISION!", $"{state.Damage.SmashableVelocityDiff:F1} m/s", "#FF3D3D");
            }
        }

        private void AddInspectorRow(System.Windows.Controls.StackPanel panel, string label, string value, string colorOverride = null)
        {
            var grid = new System.Windows.Controls.Grid();
            grid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = System.Windows.GridLength.Auto });
            
            var labelBlock = new System.Windows.Controls.TextBlock
            {
                Text = label,
                FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                FontSize = 10,
                Foreground = System.Windows.Media.Brushes.Gray,
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };
            
            var valueBlock = new System.Windows.Controls.TextBlock
            {
                Text = value,
                FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                FontSize = 13,
                FontWeight = System.Windows.FontWeights.Bold,
                Foreground = colorOverride != null 
                    ? new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(colorOverride))
                    : System.Windows.Media.Brushes.White,
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };
            
            System.Windows.Controls.Grid.SetColumn(labelBlock, 0);
            System.Windows.Controls.Grid.SetColumn(valueBlock, 1);
            
            grid.Children.Add(labelBlock);
            grid.Children.Add(valueBlock);
            grid.Margin = new Thickness(0, 0, 0, 8);
            
            panel.Children.Add(grid);
        }

        private void AddInspectorDivider(System.Windows.Controls.StackPanel panel)
        {
            var line = new System.Windows.Shapes.Rectangle
            {
                Fill = System.Windows.Media.Brushes.DarkGray,
                Height = 1,
                Margin = new Thickness(0, 8, 0, 8)
            };
            panel.Children.Add(line);
        }

        private string FormatLapTime(float seconds)
        {
            if (seconds <= 0) return "--:--.---";
            var mins = (int)(seconds / 60);
            var secs = seconds % 60;
            return $"{mins}:{secs:F3}";
        }

        protected override void OnClosed(EventArgs e)
        {
            _uiUpdateTimer?.Stop();
            _telemetryReceiver.Stop();
            base.OnClosed(e);
        }
    }
}
