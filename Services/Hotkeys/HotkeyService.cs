using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;
using HorizonPulse.Services.Hotkeys;
using Serilog;

namespace HorizonPulse.Services.Hotkeys;

/// <summary>
/// Service for managing global hotkeys using Win32 RegisterHotKey API.
/// Ensures hotkeys work even when other applications (like Forza Horizon 6) have focus.
/// </summary>
public sealed class HotkeyService : IDisposable
{
    private readonly Dictionary<int, HotkeyBinding> _registeredHotkeys = new();
    private readonly Dictionary<int, Action> _hotkeyActions = new();
    private int _nextHotkeyId = 1;
    private bool _disposed;

    // Win32 P/Invoke declarations
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    // Modifier key constants
    private const uint MOD_NONE = 0;
    private const uint MOD_ALT = 1;
    private const uint MOD_CONTROL = 2;
    private const uint MOD_SHIFT = 4;

    /// <summary>
    /// Event raised when a registered hotkey is pressed.
    /// </summary>
    public event Action<string>? HotkeyPressed;

    /// <summary>
    /// Gets the window handle for registering hotkeys.
    /// </summary>
    public IntPtr WindowHandle { get; set; }

    /// <summary>
    /// Registers a hotkey binding with an associated action.
    /// Returns false if registration fails or conflicts exist.
    /// </summary>
    public bool RegisterHotkey(HotkeyBinding binding, Action action)
    {
        if (_disposed)
        {
            Log.Warning("HotkeyService is disposed, cannot register hotkey");
            return false;
        }

        if (!binding.IsValid)
        {
            Log.Warning("Cannot register invalid hotkey binding: {BindingId}", binding.Id);
            return false;
        }

        // Check for conflicts with existing bindings (excluding self)
        foreach (var existing in _registeredHotkeys.Values)
        {
            if (existing.Id != binding.Id && 
                existing.Key == binding.Key && 
                existing.Modifiers == binding.Modifiers)
            {
                Log.Warning("Hotkey conflict detected: {NewBinding} conflicts with {ExistingBinding}", 
                    binding.DisplayName, existing.DisplayName);
                return false;
            }
        }

        // Unregister existing hotkey with same ID if present
        var existingId = _registeredHotkeys.FirstOrDefault(x => x.Value.Id == binding.Id).Key;
        if (existingId != 0)
        {
            UnregisterById(existingId);
        }

        // Allocate new ID
        var hotkeyId = _nextHotkeyId++;

        // Convert WPF Key to virtual key code
        var vkCode = (uint)KeyInterop.VirtualKeyFromKey(binding.Key);
        var modifiers = ConvertModifiers(binding.Modifiers);

        // Register with Win32
        if (WindowHandle != IntPtr.Zero)
        {
            if (!RegisterHotKey(WindowHandle, hotkeyId, modifiers, vkCode))
            {
                var error = Marshal.GetLastWin32Error();
                Log.Error(error, "Failed to register hotkey {HotkeyId}: {BindingId}. Win32 Error: {Error}", 
                    hotkeyId, binding.Id, error);
                return false;
            }
        }

        _registeredHotkeys[hotkeyId] = binding;
        _hotkeyActions[hotkeyId] = action;

        Log.Information("Registered hotkey {HotkeyId}: {BindingId} -> {DisplayString}", 
            hotkeyId, binding.Id, binding.GetDisplayString());

        return true;
    }

    /// <summary>
    /// Unregisters a hotkey by its binding ID.
    /// </summary>
    public void Unregister(string bindingId)
    {
        var entry = _registeredHotkeys.FirstOrDefault(x => x.Value.Id == bindingId);
        if (entry.Key != 0)
        {
            UnregisterById(entry.Key);
        }
    }

    /// <summary>
    /// Unregisters all hotkeys.
    /// </summary>
    public void UnregisterAll()
    {
        var ids = _registeredHotkeys.Keys.ToList();
        foreach (var id in ids)
        {
            UnregisterById(id);
        }
    }

    /// <summary>
    /// Processes a WM_HOTKEY message and invokes the associated action.
    /// Call this from your WndProc handler.
    /// </summary>
    public void ProcessHotkeyMessage(int wParam)
    {
        if (_registeredHotkeys.TryGetValue(wParam, out var binding) &&
            _hotkeyActions.TryGetValue(wParam, out var action))
        {
            Log.Debug("Hotkey triggered: {BindingId}", binding.Id);
            HotkeyPressed?.Invoke(binding.Id);
            
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error executing hotkey action for {BindingId}", binding.Id);
            }
        }
    }

    /// <summary>
    /// Re-registers all hotkeys (useful after settings change).
    /// </summary>
    public void ReregisterAll(IEnumerable<(HotkeyBinding Binding, Action Action)> hotkeys)
    {
        UnregisterAll();
        
        foreach (var (binding, action) in hotkeys)
        {
            if (binding.IsValid)
            {
                RegisterHotkey(binding, action);
            }
        }
    }

    /// <summary>
    /// Checks if a key combination would conflict with existing bindings.
    /// </summary>
    public bool HasConflict(Key key, ModifierKeys modifiers, string excludeId = null)
    {
        return _registeredHotkeys.Values.Any(b =>
            b.Id != excludeId && b.Key == key && b.Modifiers == modifiers);
    }

    private uint ConvertModifiers(ModifierKeys modifiers)
    {
        uint result = MOD_NONE;
        if ((modifiers & ModifierKeys.Alt) != 0) result |= MOD_ALT;
        if ((modifiers & ModifierKeys.Control) != 0) result |= MOD_CONTROL;
        if ((modifiers & ModifierKeys.Shift) != 0) result |= MOD_SHIFT;
        return result;
    }

    private void UnregisterById(int id)
    {
        if (WindowHandle != IntPtr.Zero)
        {
            UnregisterHotKey(WindowHandle, id);
        }
        _registeredHotkeys.Remove(id);
        _hotkeyActions.Remove(id);
        Log.Debug("Unregistered hotkey ID {HotkeyId}", id);
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        UnregisterAll();
        _disposed = true;
        Log.Information("HotkeyService disposed");
    }
}
