using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using HorizonPulse.Services;
using HorizonPulse.Telemetry.Runtime;
using HorizonPulse.Automation.Services;

namespace HorizonPulse
{
    public partial class MainWindow : Window
    {
        private readonly TelemetryReceiver _telemetryReceiver;
        private readonly RuntimeTelemetryState _runtimeState;
        private readonly AutomationService _automationService;
        
        // UI throttling optimized for 200 FPS telemetry
        private const int UiUpdateIntervalMs = 16; // ~60 FPS for UI
        private readonly DispatcherTimer _uiUpdateTimer;
        private bool _pendingUiUpdate;
        private bool _pendingCollisionEvent;
        
        // Cached UI element references to avoid FindName calls
        private Border _throttleBar;
        private Border _brakeBar;
        private Border _steerBar;
        private Border _throttleBarParent;
        private Border _brakeBarParent;
        private Border _virtualThrottleBar;
        private Border _virtualThrottleBarParent;
        private Border _automationBrakeBar;
        private Border _automationBrakeBarParent;
        
        // Steering wheel visual transform
        private RotateTransform _steeringWheelTransform;
        
        // Cached brushes to avoid allocation on every update
        private readonly SolidColorBrush _greenBrush;
        private readonly SolidColorBrush _redBrush;
        private readonly SolidColorBrush _amberBrush;

        public MainWindow()
        {
            InitializeComponent();
            
            _telemetryReceiver = new TelemetryReceiver();
            _runtimeState = new RuntimeTelemetryState();
            _automationService = new AutomationService();
            
            // Initialize cached brushes
            _greenBrush = new SolidColorBrush(Colors.Green);
            _redBrush = new SolidColorBrush(Colors.Red);
            _amberBrush = new SolidColorBrush(Color.FromRgb(255, 179, 0));
            _greenBrush.Freeze();
            _redBrush.Freeze();
            _amberBrush.Freeze();
            
            // Cache UI element references
            _throttleBar = FindName("_ThrottleBar") as Border;
            _brakeBar = FindName("_BrakeBar") as Border;
            _steerBar = FindName("_SteerBar") as Border;
            _virtualThrottleBar = FindName("VirtualThrottleBar") as Border;
            _automationBrakeBar = FindName("AutomationBrakeBar") as Border;
            
            if (_throttleBar?.Parent is Border tbParent) _throttleBarParent = tbParent;
            if (_brakeBar?.Parent is Border bbParent) _brakeBarParent = bbParent;
            if (_virtualThrottleBar?.Parent is Border vtbParent) _virtualThrottleBarParent = vtbParent;
            if (_automationBrakeBar?.Parent is Border abbParent) _automationBrakeBarParent = abbParent;
            
            // Cache Transform for the new steering wheel
            _steeringWheelTransform = FindName("SteeringWheelTransform") as RotateTransform;
            
            // Initialize automation UI state
            UpdateAutomationUI();
            
            // Setup throttled UI update timer
            _uiUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(UiUpdateIntervalMs)
            };
            _uiUpdateTimer.Tick += (s, e) =>
            {
                if (_pendingCollisionEvent)
                {
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
            
            // Subscribe to parsed telemetry events
            _telemetryReceiver.Ingestion.TelemetryReceived += OnTelemetryReceived;
            _telemetryReceiver.Start(54321);
            
            UpdateUI();
        }

        private void OnTelemetryReceived(ForzaHorizon6.Models.Runtime.TelemetryData telemetry)
        {
            _runtimeState.Update(telemetry);
            
            // Update automation with telemetry
            _automationService.UpdateFromTelemetry(_runtimeState);
            
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
            
            // Inputs Card - Bars and Text
            ThrottleText.Text = $"{state.Throttle:P1}";
            BrakeTextInput.Text = $"{state.BrakeInput:P1}";
            
            // Update Throttle/Brake
            if (_throttleBar != null && _throttleBarParent != null)
                _throttleBar.Width = _throttleBarParent.ActualWidth * state.Throttle;
                
            if (_brakeBar != null && _brakeBarParent != null)
                _brakeBar.Width = _brakeBarParent.ActualWidth * state.BrakeInput;
            
            // Update horizontal Steer Bar
            if (_steerBar != null && _steerBar.Parent is Grid steerGrid)
            {
                var steerNormalized = state.Input.Steer / 127f;
                var gridWidth = steerGrid.ActualWidth;
                var barWidth = _steerBar.Width;
                
                var centerOffset = (gridWidth - barWidth) / 2;
                var newMargin = new Thickness(centerOffset * steerNormalized + centerOffset - centerOffset, 0, 0, 0);
                _steerBar.Margin = newMargin;
                _steerBar.HorizontalAlignment = HorizontalAlignment.Left;
            }
            
            // ── NEW STEERING WHEEL LOGIC ──
            var rawSteer = state.Input.Steer; 
            
            // Map the raw -127 to 127 input into a visual angle (-180 to 180 degrees)
            double visualAngle = (rawSteer / 127.0) * 180.0;
            
            // Update text readouts
            if (RawSteerText != null) RawSteerText.Text = $"{rawSteer}";
            if (SteerAngleText != null) SteerAngleText.Text = $"{visualAngle:F1}°";
            
            // Rotate the graphic
            if (_steeringWheelTransform != null)
                _steeringWheelTransform.Angle = visualAngle;
            
            // Timing Card
            RaceTimeText.Text = FormatTimeHMS(state.CurrentRaceTime);
            CurrentLapTimeText.Text = FormatTimeHMS(state.RaceTiming.CurrentLap);
            
            // Cues and Status
            UpdateShiftCues(state);
            StatusText.Text = state.IsRaceOn ? "Racing" : "Not Racing";
            StatusText.Foreground = state.IsRaceOn ? _greenBrush : _redBrush;
            
            // Update Automation UI
            UpdateAutomationDisplay();
            
            UpdateInspectorPanel(state);
        }

        private void UpdateShiftCues(RuntimeTelemetryState state)
        {
            var rpm = state.Rpm;
            var maxRpm = state.Engine.MaxRpm > 0 ? state.Engine.MaxRpm : 8000f;
            var idleRpm = state.Engine.IdleRpm > 0 ? state.Engine.IdleRpm : 1000f;
            var gear = state.Gear;
            var speed = state.SpeedKph;
            
            var shiftUpThreshold = maxRpm * 0.92f;
            var shouldShiftUp = gear > 0 && rpm >= shiftUpThreshold;
            
            var rpmRange = maxRpm - idleRpm;
            var shiftDownThreshold = idleRpm + rpmRange * 0.35f;
            var shouldShiftDown = gear > 1 && rpm <= shiftDownThreshold && speed > 5f;
            
            ShiftUpCue.Visibility = shouldShiftUp ? Visibility.Visible : Visibility.Collapsed;
            ShiftDownCue.Visibility = shouldShiftDown ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateInspectorPanel(RuntimeTelemetryState state)
        {
            var panel = InspectorPanel;
            if (panel == null) return;
            
            panel.Children.Clear();
            
            AddInspectorRow(panel, "Susp FL", $"{state.Suspension.NormalizedFrontLeft:P0}");
            AddInspectorRow(panel, "Susp FR", $"{state.Suspension.NormalizedFrontRight:P0}");
            AddInspectorRow(panel, "Susp RL", $"{state.Suspension.NormalizedRearLeft:P0}");
            AddInspectorRow(panel, "Susp RR", $"{state.Suspension.NormalizedRearRight:P0}");
            AddInspectorDivider(panel);
            
            AddInspectorRow(panel, "Slip FL", $"{state.Wheel.SlipRatioFrontLeft:F2}");
            AddInspectorRow(panel, "Slip FR", $"{state.Wheel.SlipRatioFrontRight:F2}");
            AddInspectorRow(panel, "Slip RL", $"{state.Wheel.SlipRatioRearLeft:F2}");
            AddInspectorRow(panel, "Slip RR", $"{state.Wheel.SlipRatioRearRight:F2}");
            AddInspectorDivider(panel);
            
            AddInspectorRow(panel, "G-Force", $"{state.GForce:F2} G");
            
            AddInspectorRow(panel, "Clutch", $"{state.Input.ClutchPercent:P0}");
            AddInspectorRow(panel, "Handbrake", $"{state.Input.HandBrakePercent:P0}");
            AddInspectorDivider(panel);
            
            AddInspectorRow(panel, "Drivetrain", state.Misc.DrivetrainName);
            AddInspectorRow(panel, "Class", state.Misc.CarClassName);
            AddInspectorRow(panel, "PI", $"{state.Misc.CarPerformanceIndex}");
            AddInspectorDivider(panel);
            
            AddInspectorRow(panel, "Position", $"P{state.RaceTiming.RacePosition}");
            AddInspectorRow(panel, "Best Lap", FormatLapTime(state.RaceTiming.BestLap));
            
            AddInspectorRow(panel, "Fuel", $"{state.Environment.FuelPercent:P0}");
            AddInspectorRow(panel, "Boost", $"{state.Environment.Boost:F1} PSI");
            
            if (state.Damage.HasCollision)
            {
                AddInspectorDivider(panel);
                AddInspectorRow(panel, "COLLISION!", $"{state.Damage.SmashableVelocityDiff:F1} m/s", "#FF3D3D");
            }
        }

        private void AddInspectorRow(StackPanel panel, string label, string value, string colorOverride = null)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            
            var labelBlock = new TextBlock
            {
                Text = label,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 10,
                Foreground = Brushes.Gray,
                VerticalAlignment = VerticalAlignment.Center
            };
            
            var valueBlock = new TextBlock
            {
                Text = value,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Foreground = colorOverride != null 
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorOverride))
                    : Brushes.White,
                VerticalAlignment = VerticalAlignment.Center
            };
            
            Grid.SetColumn(labelBlock, 0);
            Grid.SetColumn(valueBlock, 1);
            
            grid.Children.Add(labelBlock);
            grid.Children.Add(valueBlock);
            grid.Margin = new Thickness(0, 0, 0, 8);
            
            panel.Children.Add(grid);
        }

        private void AddInspectorDivider(StackPanel panel)
        {
            var line = new System.Windows.Shapes.Rectangle
            {
                Fill = Brushes.DarkGray,
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

        // Updated time formatter based on your provided logic
        private string FormatTimeHMS(float totalSeconds)
        {
            if (totalSeconds <= 0 || float.IsNaN(totalSeconds) || float.IsInfinity(totalSeconds))
                return "----:--:--.---";

            var time = TimeSpan.FromSeconds(totalSeconds);

            return $"{(int)time.TotalHours:D4}:{time.Minutes:D2}:{time.Seconds:D2}.{time.Milliseconds:D3}";
        }

        protected override void OnClosed(EventArgs e)
        {
            _uiUpdateTimer?.Stop();
            _telemetryReceiver.Stop();
            _automationService.Dispose();
            base.OnClosed(e);
        }
    
        private void InspectorToggle_Click(object sender, RoutedEventArgs e)
        {
            if (InspectorToggle.IsChecked == true)
            {
                // Expand the Inspector
                LeftColumn.Width = new GridLength(2, GridUnitType.Star);
                CenterColumn.Width = new GridLength(2, GridUnitType.Star);
        
                // Show Spacer and Inspector Column
                InspectorSpacer.Width = new GridLength(14);
                InspectorColumn.Width = new GridLength(3, GridUnitType.Star);
        
                InspectorContainer.Visibility = Visibility.Visible;
            }
            else
            {
                // Collapse the Inspector (Left and Center split the remaining space 50/50)
                LeftColumn.Width = new GridLength(1, GridUnitType.Star);
                CenterColumn.Width = new GridLength(1, GridUnitType.Star);
        
                // Hide Spacer and Inspector Column
                InspectorSpacer.Width = new GridLength(0);
                InspectorColumn.Width = new GridLength(0);
        
                InspectorContainer.Visibility = Visibility.Collapsed;
            }
        }

        // ═══════════════════════════════════════════
        // AUTOMATION UI METHODS
        // ═══════════════════════════════════════════

        private void UpdateAutomationUI()
        {
            // Sync toggle state with service
            AutomationEnabledToggle.IsChecked = _automationService.IsEnabled;
            
            // Sync slider values
            ThrottleSmoothingSlider.Value = 0.7; // Default
            SteeringRandomnessSlider.Value = 0.3; // Default
            
            // Update controller status
            UpdateControllerStatus();
        }

        private void UpdateAutomationDisplay()
        {
            var throttleOutput = _automationService.CurrentThrottleOutput;
            var brakeInput = _automationService.CurrentBrakeInput;
            var steeringCorrection = _automationService.CurrentSteeringCorrection;
            
            // Update virtual throttle display
            VirtualThrottleText.Text = $"{throttleOutput:F0}%";
            if (_virtualThrottleBar != null && _virtualThrottleBarParent != null)
            {
                var maxWidth = _virtualThrottleBarParent.ActualWidth > 0 
                    ? _virtualThrottleBarParent.ActualWidth 
                    : 100;
                _virtualThrottleBar.Width = maxWidth * (throttleOutput / 100f);
            }
            
            // Update brake display
            AutomationBrakeText.Text = $"{brakeInput:P0}";
            if (_automationBrakeBar != null && _automationBrakeBarParent != null)
            {
                var maxWidth = _automationBrakeBarParent.ActualWidth > 0 
                    ? _automationBrakeBarParent.ActualWidth 
                    : 100;
                _automationBrakeBar.Width = maxWidth * brakeInput;
            }
            
            // Update steering correction display
            SteeringCorrectionText.Text = $"{steeringCorrection:+0.0;-0.0;0.0}°";
        }

        private void UpdateControllerStatus()
        {
            bool isConnected = _automationService.IsControllerConnected;
            
            ControllerStatusText.Text = isConnected ? "Connected" : "Disconnected";
            ControllerStatusText.Foreground = isConnected ? _greenBrush : _redBrush;
        }

        private void AutomationEnabledToggle_Click(object sender, RoutedEventArgs e)
        {
            bool isEnabled = AutomationEnabledToggle.IsChecked == true;
            _automationService.SetEnabled(isEnabled);
        }

        private void ThrottleSmoothingSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_automationService == null) return;
            
            float value = (float)e.NewValue;
            ThrottleSmoothingValueText.Text = $"{value:F2}";
            _automationService.SetThrottleSmoothing(value);
        }

        private void SteeringRandomnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_automationService == null) return;
            
            float value = (float)e.NewValue;
            SteeringRandomnessValueText.Text = $"{value:F2}";
            _automationService.SetSteeringRandomness(value);
        }
    }
}