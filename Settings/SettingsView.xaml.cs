using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HorizonPulse.Automation.Services;
using HorizonPulse.Services;
using HorizonPulse.Services.Hotkeys;

namespace HorizonPulse.Settings;

/// <summary>
/// Code-behind for the SettingsView UserControl.
/// Handles key preview for hotkey recording mode.
/// </summary>
public partial class SettingsView : UserControl
{
    private readonly SettingsViewModel _viewModel;

    public SettingsView()
    {
        // Initialize services
        var settingsService = new SettingsService();
        var hotkeyService = new HotkeyService();
        var automationService = new AutomationService();

        // Set window handle for hotkey registration
        if (Application.Current?.MainWindow != null)
        {
            hotkeyService.WindowHandle = new System.Windows.Interop.WindowInteropHelper(Application.Current.MainWindow).Handle;
        }

        // Create ViewModel
        _viewModel = new SettingsViewModel(settingsService, hotkeyService, automationService);

        InitializeComponent();
        DataContext = _viewModel;

        // Register for key preview to capture hotkey recording
        PreviewKeyDown += OnPreviewKeyDown;
    }

    /// <summary>
    /// Handles key preview for capturing hotkey bindings during recording mode.
    /// </summary>
    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (_viewModel.IsRecording)
        {
            e.Handled = true;

            var modifiers = ModifierKeys.None;
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                modifiers |= ModifierKeys.Control;
            if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
                modifiers |= ModifierKeys.Alt;
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                modifiers |= ModifierKeys.Shift;

            _viewModel.ProcessKeyPreview(e.Key, modifiers);
        }
    }

    /// <summary>
    /// Cleanup on unload.
    /// </summary>
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        _viewModel.Dispose();
    }
}
