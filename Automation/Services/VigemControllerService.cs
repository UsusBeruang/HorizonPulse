namespace HorizonPulse.Automation.Services;

using System.Runtime.InteropServices;
using HorizonPulse.Automation.Interfaces;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

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
    private ushort _buttons;

    /// <inheritdoc/>
    public bool IsConnected
    {
        get { lock (_lock) return _isConnected && _controller?.IsConnected == true; }
    }

    /// <summary>
    /// Initializes a new instance of the ViGEm controller service.
    /// Does not connect automatically - call Connect() explicitly.
    /// </summary>
    public VigemControllerService()
    {
        ResetState();
    }

    /// <inheritdoc/>
    public bool Connect()
    {
        lock (_lock)
        {
            if (_isDisposed)
                return false;

            if (_isConnected && _controller?.IsConnected == true)
                return true;

            try
            {
                _client = new ViGEmClient();
                _controller = _client.CreateXbox360Controller();
                _controller.Connect();
                
                // Verify connection
                if (_controller.IsConnected)
                {
                    _isConnected = true;
                    ResetState();
                    return true;
                }
            }
            catch (Exception ex) when (
                ex is DllNotFoundException || 
                ex is TargetInvocationException ||
                ex is InvalidOperationException)
            {
                // ViGEm driver not installed or unavailable
                _isConnected = false;
                Cleanup();
                return false;
            }
            catch
            {
                // Unknown error - clean up and report failure
                _isConnected = false;
                Cleanup();
                return false;
            }

            _isConnected = false;
            return false;
        }
    }

    /// <inheritdoc/>
    public void Disconnect()
    {
        lock (_lock)
        {
            if (!_isConnected)
                return;

            try
            {
                Reset();
                SendReport();
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
            if (!_isConnected || _controller == null)
                return;
            
            _leftTrigger = value;
            _controller.SetButtonState(Xbox360SpecialButton.LeftTrigger, value);
        }
    }

    /// <inheritdoc/>
    public void SetRightTrigger(byte value)
    {
        lock (_lock)
        {
            if (!_isConnected || _controller == null)
                return;
            
            _rightTrigger = value;
            _controller.SetButtonState(Xbox360SpecialButton.RightTrigger, value);
        }
    }

    /// <inheritdoc/>
    public void SetLeftStick(byte x, byte y)
    {
        lock (_lock)
        {
            if (!_isConnected || _controller == null)
                return;
            
            _leftStickX = x;
            _leftStickY = y;
            _controller.SetAxisValue(IXbox360Controller.AxisType.LeftThumbX, x);
            _controller.SetAxisValue(IXbox360Controller.AxisType.LeftThumbY, y);
        }
    }

    /// <inheritdoc/>
    public void SetRightStick(byte x, byte y)
    {
        lock (_lock)
        {
            if (!_isConnected || _controller == null)
                return;
            
            _rightStickX = x;
            _rightStickY = y;
            _controller.SetAxisValue(IXbox360Controller.AxisType.RightThumbX, x);
            _controller.SetAxisValue(IXbox360Controller.AxisType.RightThumbY, y);
        }
    }

    /// <inheritdoc/>
    public void SetButton(Xbox360Button button, bool isPressed)
    {
        lock (_lock)
        {
            if (!_isConnected || _controller == null)
                return;

            var xboxButton = button switch
            {
                Xbox360Button.Up => Xbox360Button.Up,
                Xbox360Button.Down => Xbox360Button.Down,
                Xbox360Button.Left => Xbox360Button.Left,
                Xbox360Button.Right => Xbox360Button.Right,
                Xbox360Button.Start => Xbox360Button.Start,
                Xbox360Button.Back => Xbox360Button.Back,
                Xbox360Button.LeftThumb => Xbox360Button.LeftThumb,
                Xbox360Button.RightThumb => Xbox360Button.RightThumb,
                Xbox360Button.LeftShoulder => Xbox360Button.LeftShoulder,
                Xbox360Button.RightShoulder => Xbox360Button.RightShoulder,
                Xbox360Button.A => Xbox360Button.A,
                Xbox360Button.B => Xbox360Button.B,
                Xbox360Button.X => Xbox360Button.X,
                Xbox360Button.Y => Xbox360Button.Y,
                Xbox360Button.Guide => Xbox360Button.Guide,
                _ => throw new ArgumentOutOfRangeException(nameof(button))
            };

            if (isPressed)
                _controller.SetButtonState(xboxButton);
            else
                _controller.ClearButtonState(xboxButton);
        }
    }

    /// <inheritdoc/>
    public void SendReport()
    {
        lock (_lock)
        {
            if (!_isConnected || _controller == null)
                return;

            try
            {
                _controller.SendReport();
            }
            catch
            {
                // Connection may have been lost
                _isConnected = false;
            }
        }
    }

    /// <inheritdoc/>
    public void Reset()
    {
        lock (_lock)
        {
            ResetState();
            
            if (!_isConnected || _controller == null)
                return;

            try
            {
                // Reset all inputs to neutral
                _controller.SetButtonState(Xbox360SpecialButton.LeftTrigger, 0);
                _controller.SetButtonState(Xbox360SpecialButton.RightTrigger, 0);
                _controller.SetAxisValue(IXbox360Controller.AxisType.LeftThumbX, 128);
                _controller.SetAxisValue(IXbox360Controller.AxisType.LeftThumbY, 128);
                _controller.SetAxisValue(IXbox360Controller.AxisType.RightThumbX, 128);
                _controller.SetAxisValue(IXbox360Controller.AxisType.RightThumbY, 128);
                
                foreach (Xbox360Button b in Enum.GetValues(typeof(Xbox360Button)))
                {
                    _controller.ClearButtonState(b);
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
            if (_isDisposed)
                return;

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
                if (_controller.IsConnected)
                {
                    _controller.Disconnect();
                }
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
        _leftStickX = 128;
        _leftStickY = 128;
        _rightStickX = 128;
        _rightStickY = 128;
        _buttons = 0;
    }
}
