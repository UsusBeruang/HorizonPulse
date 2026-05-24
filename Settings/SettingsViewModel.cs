using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using HorizonPulse.Automation.Models;
using HorizonPulse.Services.Hotkeys;
using Serilog;

namespace HorizonPulse.Settings;

/// <summary>
/// ViewModel for the Settings view.
/// Implements MVVM pattern with INotifyPropertyChanged.
/// </summary>
public sealed class SettingsViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly Services.SettingsService _settingsService;
    private readonly HotkeyService _hotkeyService;
    private readonly AutomationService _automationService;
    private bool _disposed;
    private string? _validationMessage;
    private HotkeyBinding? _recordingBinding;

    /// <summary>
    /// Gets the collection of hotkey bindings.
    /// </summary>
    public ObservableCollection<HotkeyBinding> HotkeyBindings { get; }

    /// <summary>
    /// Gets or sets whether automation is enabled.
    /// </summary>
    public bool IsAutomationEnabled
    {
        get => _settingsService.Settings.IsEnabled;
        set
        {
            if (_settingsService.Settings.IsEnabled != value)
            {
                _settingsService.UpdateSettings(s => s.IsEnabled = value);
                _automationService.IsEnabled = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AutomationStateText));
            }
        }
    }

    /// <summary>
    /// Gets or sets whether auto-steering is enabled.
    /// </summary>
    public bool IsAutoSteeringEnabled
    {
        get => _settingsService.Settings.IsSteeringAssistEnabled;
        set
        {
            if (_settingsService.Settings.IsSteeringAssistEnabled != value)
            {
                _settingsService.UpdateSettings(s => s.IsSteeringAssistEnabled = value);
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the steering randomness value (0-1).
    /// </summary>
    public double SteeringRandomness
    {
        get => _settingsService.Settings.SteeringRandomness;
        set
        {
            if (Math.Abs(_settingsService.Settings.SteeringRandomness - value) > 0.001)
            {
                var clamped = Math.Max(0, Math.Min(1, value));
                _settingsService.UpdateSettings(s => s.SteeringRandomness = (float)clamped);
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the maximum throttle percentage (0-100).
    /// </summary>
    public double MaxThrottlePercent
    {
        get => _settingsService.Settings.MaxThrottlePercent;
        set
        {
            if (Math.Abs(_settingsService.Settings.MaxThrottlePercent - value) > 0.001)
            {
                var clamped = Math.Max(0, Math.Min(100, value));
                _settingsService.UpdateSettings(s => s.MaxThrottlePercent = clamped);
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets a display text for automation state.
    /// </summary>
    public string AutomationStateText => IsAutomationEnabled ? "ACTIVE" : "INACTIVE";

    /// <summary>
    /// Gets or sets validation/error messages.
    /// </summary>
    public string? ValidationMessage
    {
        get => _validationMessage;
        set
        {
            _validationMessage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasValidationMessage));
        }
    }

    /// <summary>
    /// Gets whether there is a validation message.
    /// </summary>
    public bool HasValidationMessage => !string.IsNullOrEmpty(ValidationMessage);

    /// <summary>
    /// Gets whether a binding is currently being recorded.
    /// </summary>
    public bool IsRecording => _recordingBinding != null;

    /// <summary>
    /// Command to start recording a hotkey binding.
    /// </summary>
    public ICommand RecordHotkeyCommand { get; }

    /// <summary>
    /// Command to clear a hotkey binding.
    /// </summary>
    public ICommand ClearHotkeyCommand { get; }

    /// <summary>
    /// Command to restore default hotkey bindings.
    /// </summary>
    public ICommand RestoreDefaultsCommand { get; }

    /// <summary>
    /// Command to toggle automation.
    /// </summary>
    public ICommand ToggleAutomationCommand { get; }

    /// <summary>
    /// Event for property changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Creates a new SettingsViewModel instance.
    /// </summary>
    public SettingsViewModel(
        Services.SettingsService settingsService,
        HotkeyService hotkeyService,
        AutomationService automationService)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _hotkeyService = hotkeyService ?? throw new ArgumentNullException(nameof(hotkeyService));
        _automationService = automationService ?? throw new ArgumentNullException(nameof(automationService));

        HotkeyBindings = new ObservableCollection<HotkeyBinding>();

        RecordHotkeyCommand = new RelayCommand<HotkeyBinding>(StartRecording);
        ClearHotkeyCommand = new RelayCommand<HotkeyBinding>(ClearBinding);
        RestoreDefaultsCommand = new RelayCommand(_ => RestoreDefaults());
        ToggleAutomationCommand = new RelayCommand(_ => IsAutomationEnabled = !IsAutomationEnabled);

        LoadBindings();
        RegisterHotkeys();

        Log.Information("SettingsViewModel initialized");
    }

    /// <summary>
    /// Loads hotkey bindings from settings into the observable collection.
    /// </summary>
    private void LoadBindings()
    {
        HotkeyBindings.Clear();
        foreach (var binding in _settingsService.Settings.HotkeyBindings)
        {
            binding.OnPropertyChanged += () => 
            {
                // Refresh UI when binding changes
                var index = HotkeyBindings.IndexOf(binding);
                if (index >= 0)
                {
                    OnPropertyChanged(nameof(HotkeyBindings));
                }
            };
            HotkeyBindings.Add(binding);
        }
    }

    /// <summary>
    /// Registers all hotkeys with the HotkeyService.
    /// </summary>
    private void RegisterHotkeys()
    {
        _hotkeyService.UnregisterAll();

        foreach (var binding in _settingsService.Settings.HotkeyBindings)
        {
            if (!binding.IsValid) continue;

            var action = GetActionForBinding(binding.Id);
            if (action != null)
            {
                _hotkeyService.RegisterHotkey(binding, action);
            }
        }

        Log.Information("All hotkeys registered");
    }

    /// <summary>
    /// Gets the action to execute for a given binding ID.
    /// </summary>
    private Action? GetActionForBinding(string bindingId)
    {
        return bindingId switch
        {
            "ToggleAutomation" => () =>
            {
                IsAutomationEnabled = !IsAutomationEnabled;
                Log.Information("ToggleAutomation hotkey triggered. New state: {State}", IsAutomationEnabled);
            },
            "ToggleAutoSteering" => () =>
            {
                IsAutoSteeringEnabled = !IsAutoSteeringEnabled;
                Log.Information("ToggleAutoSteering hotkey triggered. New state: {State}", IsAutoSteeringEnabled);
            },
            "EmergencyStop" => () =>
            {
                IsAutomationEnabled = false;
                _automationService.EmergencyStop();
                Log.Warning("EmergencyStop hotkey triggered. Automation disabled.");
            },
            "IncreaseSteeringRandomness" => () =>
            {
                SteeringRandomness = Math.Min(1.0, SteeringRandomness + 0.1);
                Log.Information("SteeringRandomness increased to {Value}", SteeringRandomness);
            },
            "DecreaseSteeringRandomness" => () =>
            {
                SteeringRandomness = Math.Max(0.0, SteeringRandomness - 0.1);
                Log.Information("SteeringRandomness decreased to {Value}", SteeringRandomness);
            },
            "IncreaseMaxThrottle" => () =>
            {
                MaxThrottlePercent = Math.Min(100.0, MaxThrottlePercent + 5);
                Log.Information("MaxThrottlePercent increased to {Value}", MaxThrottlePercent);
            },
            "DecreaseMaxThrottle" => () =>
            {
                MaxThrottlePercent = Math.Max(0.0, MaxThrottlePercent - 5);
                Log.Information("MaxThrottlePercent decreased to {Value}", MaxThrottlePercent);
            },
            _ => null
        };
    }

    /// <summary>
    /// Starts recording a new key for the specified binding.
    /// </summary>
    private void StartRecording(HotkeyBinding? binding)
    {
        if (binding == null) return;

        _recordingBinding = binding;
        binding.IsRecording = true;
        ValidationMessage = $"Press any key for \"{binding.DisplayName}\" (Esc to cancel)";
        
        OnPropertyChanged(nameof(IsRecording));
        Log.Debug("Started recording for binding: {BindingId}", binding.Id);
    }

    /// <summary>
    /// Completes recording with the captured key.
    /// Called from the view when a key is pressed.
    /// </summary>
    public void CompleteRecording(Key key, ModifierKeys modifiers)
    {
        if (_recordingBinding == null) return;

        // Check for Esc to cancel
        if (key == Key.Escape)
        {
            CancelRecording();
            return;
        }

        // Check for conflicts
        if (_hotkeyService.HasConflict(key, modifiers, _recordingBinding.Id))
        {
            ValidationMessage = "Error: This key combination is already assigned to another feature!";
            _recordingBinding.IsRecording = false;
            _recordingBinding = null;
            OnPropertyChanged(nameof(IsRecording));
            return;
        }

        // Update binding
        _recordingBinding.Key = key;
        _recordingBinding.Modifiers = modifiers;
        _recordingBinding.IsRecording = false;

        // Save to settings
        _settingsService.Save();

        // Re-register hotkeys
        RegisterHotkeys();

        ValidationMessage = $"Successfully bound \"{_recordingBinding.DisplayName}\" to {_recordingBinding.GetDisplayString()}";
        _recordingBinding = null;
        OnPropertyChanged(nameof(IsRecording));
        
        Log.Information("Hotkey binding updated: {BindingId} -> {DisplayString}", 
            _recordingBinding?.Id, _recordingBinding?.GetDisplayString());
    }

    /// <summary>
    /// Cancels the current recording operation.
    /// </summary>
    private void CancelRecording()
    {
        if (_recordingBinding == null) return;

        _recordingBinding.IsRecording = false;
        _recordingBinding = null;
        ValidationMessage = "Recording cancelled";
        OnPropertyChanged(nameof(IsRecording));
        Log.Debug("Recording cancelled");
    }

    /// <summary>
    /// Clears a hotkey binding.
    /// </summary>
    private void ClearBinding(HotkeyBinding? binding)
    {
        if (binding == null) return;

        binding.Clear();
        _settingsService.Save();
        RegisterHotkeys();
        
        ValidationMessage = $"Cleared \"{binding.DisplayName}\" binding";
        Log.Information("Hotkey binding cleared: {BindingId}", binding.Id);
    }

    /// <summary>
    /// Restores all hotkey bindings to defaults.
    /// </summary>
    private void RestoreDefaults()
    {
        _settingsService.ResetToDefaults();
        HotkeyBindings.Clear();
        foreach (var binding in _settingsService.Settings.HotkeyBindings)
        {
            HotkeyBindings.Add(binding);
        }
        RegisterHotkeys();
        ValidationMessage = "All settings restored to defaults";
        Log.Information("All settings restored to defaults");
    }

    /// <summary>
    /// Processes a key press during recording mode.
    /// </summary>
    public void ProcessKeyPreview(Key key, ModifierKeys modifiers)
    {
        if (IsRecording)
        {
            CompleteRecording(key, modifiers);
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        _settingsService.Dispose();
        _hotkeyService.Dispose();
        _disposed = true;
        Log.Information("SettingsViewModel disposed");
    }
}

/// <summary>
/// Generic relay command implementation.
/// </summary>
public sealed class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;

    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    public void Execute(object? parameter) => _execute(parameter);
}

/// <summary>
/// Generic relay command with type parameter.
/// </summary>
public sealed class RelayCommand<T> : ICommand
{
    private readonly Action<T?> _execute;
    private readonly Func<T?, bool>? _canExecute;

    public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter)
    {
        if (_canExecute == null) return true;
        if (parameter is T typedParam) return _canExecute(typedParam);
        return _canExecute(default);
    }

    public void Execute(object? parameter)
    {
        if (parameter is T typedParam)
            _execute(typedParam);
        else
            _execute(default);
    }
}
