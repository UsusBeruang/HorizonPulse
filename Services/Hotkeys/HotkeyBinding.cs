using System;
using System.Windows.Input;

namespace HorizonPulse.Services.Hotkeys;

/// <summary>
/// Represents a configurable hotkey binding for automation features.
/// Supports Ctrl, Alt, Shift modifiers plus regular keys, function keys, and numpad keys.
/// </summary>
public sealed class HotkeyBinding : IEquatable<HotkeyBinding>
{
    private Key _key;
    private ModifierKeys _modifiers;

    /// <summary>
    /// Gets or sets the primary key for this binding.
    /// </summary>
    public Key Key
    {
        get => _key;
        set
        {
            if (_key != value)
            {
                _key = value;
                OnPropertyChanged?.Invoke();
            }
        }
    }

    /// <summary>
    /// Gets or sets the modifier keys (Ctrl, Alt, Shift) for this binding.
    /// </summary>
    public ModifierKeys Modifiers
    {
        get => _modifiers;
        set
        {
            if (_modifiers != value)
            {
                _modifiers = value;
                OnPropertyChanged?.Invoke();
            }
        }
    }

    /// <summary>
    /// Gets or sets the unique identifier for this binding (e.g., "ToggleAutomation").
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name for this binding (e.g., "Toggle Automation ON/OFF").
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this binding is currently being recorded.
    /// </summary>
    public bool IsRecording { get; set; }

    /// <summary>
    /// Event raised when a property changes.
    /// </summary>
    public event Action? OnPropertyChanged;

    /// <summary>
    /// Creates a new instance of HotkeyBinding.
    /// </summary>
    public HotkeyBinding() { }

    /// <summary>
    /// Creates a new instance with specified values.
    /// </summary>
    public HotkeyBinding(string id, string displayName, Key key, ModifierKeys modifiers = ModifierKeys.None)
    {
        Id = id;
        DisplayName = displayName;
        Key = key;
        Modifiers = modifiers;
    }

    /// <summary>
    /// Gets a human-readable string representation of the hotkey.
    /// </summary>
    public string GetDisplayString()
    {
        if (Key == Key.None)
            return "Not bound";

        var parts = new System.Collections.Generic.List<string>();

        if ((Modifiers & ModifierKeys.Control) != 0)
            parts.Add("Ctrl");
        if ((Modifiers & ModifierKeys.Alt) != 0)
            parts.Add("Alt");
        if ((Modifiers & ModifierKeys.Shift) != 0)
            parts.Add("Shift");

        parts.Add(Key.ToString());

        return string.Join(" + ", parts);
    }

    /// <summary>
    /// Clears the binding (sets key to None and modifiers to None).
    /// </summary>
    public void Clear()
    {
        Key = Key.None;
        Modifiers = ModifierKeys.None;
    }

    /// <summary>
    /// Checks if this binding is valid (has at least a key).
    /// </summary>
    public bool IsValid => Key != Key.None;

    /// <summary>
    /// Checks if two bindings are equal based on their key and modifiers.
    /// </summary>
    public bool Equals(HotkeyBinding? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Key == other.Key && Modifiers == other.Modifiers;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as HotkeyBinding);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Key, Modifiers);

    /// <summary>
    /// Creates a deep copy of this binding.
    /// </summary>
    public HotkeyBinding Clone()
    {
        return new HotkeyBinding(Id, DisplayName, Key, Modifiers);
    }
}
