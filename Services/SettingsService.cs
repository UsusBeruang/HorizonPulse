using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Input;
using HorizonPulse.Services.Hotkeys;
using Serilog;

namespace HorizonPulse.Services;

/// <summary>
/// Service for persisting and loading application settings using JSON.
/// Handles automation settings and hotkey bindings.
/// </summary>
public sealed class SettingsService : IDisposable
{
    private const string SettingsFileName = "settings.json";
    private readonly string _settingsPath;
    private readonly object _lockObj = new();
    private bool _disposed;

    /// <summary>
    /// Gets the current automation settings.
    /// </summary>
    public AutomationSettingsModel Settings { get; private set; }

    /// <summary>
    /// Event raised when settings are loaded or changed.
    /// </summary>
    public event Action? SettingsChanged;

    /// <summary>
    /// Creates a new SettingsService instance.
    /// </summary>
    /// <param name="basePath">Base directory for storing settings. Defaults to application directory.</param>
    public SettingsService(string? basePath = null)
    {
        _settingsPath = Path.Combine(basePath ?? AppDomain.CurrentDomain.BaseDirectory, SettingsFileName);
        Settings = LoadFromFile() ?? CreateDefaultSettings();
        Log.Information("SettingsService initialized. Path: {SettingsPath}", _settingsPath);
    }

    /// <summary>
    /// Loads settings from the JSON file.
    /// Returns null if file doesn't exist or is invalid.
    /// </summary>
    public AutomationSettingsModel? LoadFromFile()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                Log.Information("Settings file not found, will create with defaults");
                return null;
            }

            var json = File.ReadAllText(_settingsPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
                ReadNumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
            };

            var settings = JsonSerializer.Deserialize<AutomationSettingsModel>(json, options);
            
            if (settings != null)
            {
                Log.Information("Settings loaded successfully from {SettingsPath}", _settingsPath);
                EnsureAllHotkeysPresent(settings);
            }
            
            return settings;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load settings from {SettingsPath}. Using defaults.", _settingsPath);
            return null;
        }
    }

    /// <summary>
    /// Saves current settings to the JSON file.
    /// </summary>
    public void Save()
    {
        lock (_lockObj)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never
                };

                var json = JsonSerializer.Serialize(Settings, options);
                File.WriteAllText(_settingsPath, json);
                
                Log.Information("Settings saved to {SettingsPath}", _settingsPath);
                SettingsChanged?.Invoke();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to save settings to {SettingsPath}", _settingsPath);
            }
        }
    }

    /// <summary>
    /// Updates settings and saves automatically.
    /// </summary>
    public void UpdateSettings(Action<AutomationSettingsModel> updateAction)
    {
        updateAction(Settings);
        Save();
    }

    /// <summary>
    /// Resets all settings to defaults and saves.
    /// </summary>
    public void ResetToDefaults()
    {
        Settings = CreateDefaultSettings();
        Save();
        Log.Information("Settings reset to defaults");
    }

    /// <summary>
    /// Ensures all required hotkey bindings are present in settings.
    /// Adds missing ones with default values.
    /// </summary>
    private void EnsureAllHotkeysPresent(AutomationSettingsModel settings)
    {
        var defaultBindings = GetDefaultHotkeyBindings();
        
        foreach (var defaultBinding in defaultBindings)
        {
            var existing = settings.HotkeyBindings.FirstOrDefault(b => b.Id == defaultBinding.Id);
            if (existing == null)
            {
                settings.HotkeyBindings.Add(defaultBinding.Clone());
                Log.Information("Added missing hotkey binding: {BindingId}", defaultBinding.Id);
            }
        }
    }

    /// <summary>
    /// Creates default settings with predefined hotkey bindings.
    /// </summary>
    private static AutomationSettingsModel CreateDefaultSettings()
    {
        return new AutomationSettingsModel
        {
            IsEnabled = false,
            IsThrottleAssistEnabled = false,
            IsSteeringAssistEnabled = false,
            SteeringRandomness = 0.3f,
            ThrottleSmoothing = 0.7f,
            MaxThrottlePercent = 100.0f,
            HotkeyBindings = GetDefaultHotkeyBindings()
        };
    }

    /// <summary>
    /// Returns the default hotkey bindings configuration.
    /// </summary>
    public static List<HotkeyBinding> GetDefaultHotkeyBindings()
    {
        return new List<HotkeyBinding>
        {
            new("ToggleAutomation", "Toggle Automation ON/OFF", Key.F1),
            new("ToggleAutoSteering", "Toggle Auto-Steering", Key.F2),
            new("EmergencyStop", "Emergency Stop Automation", Key.F3, ModifierKeys.Control),
            new("IncreaseSteeringRandomness", "Increase Steering Randomness", Key.Up, ModifierKeys.Shift),
            new("DecreaseSteeringRandomness", "Decrease Steering Randomness", Key.Down, ModifierKeys.Shift),
            new("IncreaseMaxThrottle", "Increase Max Throttle %", Key.Up, ModifierKeys.Control),
            new("DecreaseMaxThrottle", "Decrease Max Throttle %", Key.Down, ModifierKeys.Control)
        };
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        // Auto-save on dispose
        Save();
        _disposed = true;
        Log.Information("SettingsService disposed");
    }
}

/// <summary>
/// Extended automation settings model including hotkey bindings.
/// </summary>
public sealed class AutomationSettingsModel
{
    /// <summary>
    /// Gets or sets whether the automation system is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = false;

    /// <summary>
    /// Gets or sets whether throttle assist is enabled.
    /// </summary>
    public bool IsThrottleAssistEnabled { get; set; } = false;

    /// <summary>
    /// Gets or sets whether steering assist is enabled.
    /// </summary>
    public bool IsSteeringAssistEnabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the steering randomness intensity (0-1).
    /// </summary>
    public float SteeringRandomness { get; set; } = 0.3f;

    /// <summary>
    /// Gets or sets the throttle smoothing factor (0-1).
    /// </summary>
    public float ThrottleSmoothing { get; set; } = 0.7f;

    /// <summary>
    /// Gets or sets the maximum throttle percentage (0-100).
    /// </summary>
    public double MaxThrottlePercent { get; set; } = 100.0;

    /// <summary>
    /// Gets or sets the list of hotkey bindings.
    /// </summary>
    public List<HotkeyBinding> HotkeyBindings { get; set; } = new();
}
