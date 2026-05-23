namespace HorizonPulse.Automation.Services;

using System;
using System.Reflection;
using HorizonPulse.Automation.Interfaces;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

// Create an alias to fix the ambiguous "Xbox360Button" error
using VigemXboxButton = Nefarius.ViGEm.Client.Targets.Xbox360.Xbox360Button;

/// <summary>
/// ViGEm virtual controller service implementation.
/// Provides Xbox 360 controller emulation for input simulation.
/// Thread-safe and handles connection failures gracefully.
/// </summary>
public sealed class VigemControllerService : IVigemControllerService
{
    private readonly object _lock = new();
    private ViGEmClient? _client;
    private IXbox360Controller? _controller;
    private bool _isDisposed;
    private bool _isConnected;

    // Current controller state for incremental updates
    private byte _leftTrigger;
    private byte _rightTrigger;
    private byte _leftStickX;
    private byte _leftStickY;
    private byte _rightStickX;
    private byte _rightStickY;

    /// <inheritdoc/>
    public bool IsConnected
    {
        // ViGEm's IXbox360Controller doesn't expose an IsConnected property, 
        // so we rely solely on our internal tracking flag.
        get { lock (_lock) return _isConnected; }
    }

    public VigemControllerService()
    {
        ResetState();
    }

    /// <inheritdoc/>
    public bool Connect()
    {
        lock (_lock)
        {
            if (_isDisposed) return false;
            if (_isConnected) return true;

            try
            {
                _client = new ViGEmClient();
                _controller = _client.CreateXbox360Controller();
                _controller.Connect();
                
                _isConnected = true;
                ResetState();
                return true;
            }
            catch (Exception ex) when (
                ex is DllNotFoundException || 
                ex is TargetInvocationException ||
                ex is InvalidOperationException)
            {
                _isConnected = false;
                Cleanup();
                return false;
            }
            catch
            {
                _isConnected = false;
                Cleanup();
                return false;
            }
        }
    }

    /// <inheritdoc/>
    public void Disconnect()
    {
        lock (_lock)
        {
            if (!_isConnected) return;

            try
            {
                Reset();
            }
            catch
            {
                // Ignore errors during disconnect
            }

            Cleanup();
            _isConnected = false;
        }
    }

    /// <inheritdoc/>
    public void SetLeftTrigger(byte value)
    {
        lock (_lock)
        {
            if (!_isConnected || _controller == null) return;
            
            _leftTrigger = value;
            _controller.SetSliderValue(Xbox360Slider.LeftTrigger, value);
        }
    }

    /// <inheritdoc/>
    public void SetRightTrigger(byte value)
    {
        lock (_lock)
        {
            if (!_isConnected || _controller == null) return;
            
            _rightTrigger = value;
            _controller.SetSliderValue(Xbox360Slider.RightTrigger, value);
        }
    }

    /// <inheritdoc/>
    public void SetLeftStick(byte x, byte y)
    {
        lock (_lock)
        {
            if (!_isConnected || _controller == null) return;
            
            _leftStickX = x;
            _leftStickY = y;
            _controller.SetAxisValue(Xbox360Axis.LeftThumbX, MapByteToShort(x));
            _controller.SetAxisValue(Xbox360Axis.LeftThumbY, MapByteToShort(y));
        }
    }

    /// <inheritdoc/>
    public void SetRightStick(byte x, byte y)
    {
        lock (_lock)
        {
            if (!_isConnected || _controller == null) return;
            
            _rightStickX = x;
            _rightStickY = y;
            _controller.SetAxisValue(Xbox360Axis.RightThumbX, MapByteToShort(x));
            _controller.SetAxisValue(Xbox360Axis.RightThumbY, MapByteToShort(y));
        }
    }

    /// <inheritdoc/>
    public void SetButton(Interfaces.Xbox360Button button, bool isPressed)
    {
        lock (_lock)
        {
            if (!_isConnected || _controller == null) return;

            // Map HorizonPulse Interfaces enum to ViGEm enum
            var vigemButton = button switch
            {
                Interfaces.Xbox360Button.Up => VigemXboxButton.Up,
                Interfaces.Xbox360Button.Down => VigemXboxButton.Down,
                Interfaces.Xbox360Button.Left => VigemXboxButton.Left,
                Interfaces.Xbox360Button.Right => VigemXboxButton.Right,
                Interfaces.Xbox360Button.Start => VigemXboxButton.Start,
                Interfaces.Xbox360Button.Back => VigemXboxButton.Back,
                Interfaces.Xbox360Button.LeftThumb => VigemXboxButton.LeftThumb,
                Interfaces.Xbox360Button.RightThumb => VigemXboxButton.RightThumb,
                Interfaces.Xbox360Button.LeftShoulder => VigemXboxButton.LeftShoulder,
                Interfaces.Xbox360Button.RightShoulder => VigemXboxButton.RightShoulder,
                Interfaces.Xbox360Button.A => VigemXboxButton.A,
                Interfaces.Xbox360Button.B => VigemXboxButton.B,
                Interfaces.Xbox360Button.X => VigemXboxButton.X,
                Interfaces.Xbox360Button.Y => VigemXboxButton.Y,
                Interfaces.Xbox360Button.Guide => VigemXboxButton.Guide,
                _ => throw new ArgumentOutOfRangeException(nameof(button))
            };

            // ViGEm expects the button AND a boolean for Pressed (true) or Released (false)
            _controller.SetButtonState(vigemButton, isPressed);
        }
    }

    /// <inheritdoc/>
    public void SendReport()
    {
        // ViGEmClient 1.21.256 automatically updates the controller state instantly 
        // when SetSliderValue, SetAxisValue, or SetButtonState is called. 
        // There is no manual SendReport() necessary in this API version.
    }

    /// <inheritdoc/>
    public void Reset()
    {
        lock (_lock)
        {
            ResetState();
            
            if (!_isConnected || _controller == null) return;

            try
            {
                // Reset Triggers
                _controller.SetSliderValue(Xbox360Slider.LeftTrigger, 0);
                _controller.SetSliderValue(Xbox360Slider.RightTrigger, 0);
                
                // 0 is perfectly centered in a 'short' data type
                _controller.SetAxisValue(Xbox360Axis.LeftThumbX, 0);
                _controller.SetAxisValue(Xbox360Axis.LeftThumbY, 0);
                _controller.SetAxisValue(Xbox360Axis.RightThumbX, 0);
                _controller.SetAxisValue(Xbox360Axis.RightThumbY, 0);
                
                // Clear all buttons
                foreach (VigemXboxButton b in Enum.GetValues(typeof(VigemXboxButton)))
                {
                    _controller.SetButtonState(b, false);
                }
            }
            catch
            {
                // Ignore errors during reset
            }
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        lock (_lock)
        {
            if (_isDisposed) return;
            Disconnect();
            _isDisposed = true;
        }
    }

    private void Cleanup()
    {
        try
        {
            if (_controller != null)
            {
                _controller.Disconnect();
                _controller = null;
            }

            if (_client != null)
            {
                _client.Dispose();
                _client = null;
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    private void ResetState()
    {
        _leftTrigger = 0;
        _rightTrigger = 0;
        _leftStickX = 128; // 128 represents center in 0-255 byte math
        _leftStickY = 128;
        _rightStickX = 128;
        _rightStickY = 128;
    }

    /// <summary>
    /// Helper to mathematically map a 0-255 byte into a -32768 to 32767 short.
    /// ViGEm thumbsticks expect shorts, but your application UI works with bytes.
    /// </summary>
    private static short MapByteToShort(byte value)
    {
        return (short)((value * 257) - 32768);
    }
}