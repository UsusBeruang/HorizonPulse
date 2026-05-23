namespace HorizonPulse.Automation.Services;

using System.Diagnostics;

/// <summary>
/// Steering assist service that applies subtle random steering corrections.
/// Provides human-like steering behavior with configurable randomness.
/// Thread-safe with controlled update intervals to prevent oscillation.
/// </summary>
public sealed class SteeringAssistService : IDisposable
{
    private readonly object _lock = new();
    private readonly Random _random;
    private readonly Stopwatch _updateTimer;
    
    private bool _isEnabled;
    private float _randomnessIntensity;
    private float _maxCorrection;
    private int _updateIntervalMs;
    private float _currentCorrection;
    private float _targetCorrection;
    private bool _isDisposed;

    /// <summary>
    /// Gets or sets whether steering assist is enabled.
    /// </summary>
    public bool IsEnabled
    {
        get { lock (_lock) return _isEnabled; }
        set 
        { 
            lock (_lock) 
            { 
                _isEnabled = value;
                if (!_isEnabled)
                    Reset();
            } 
        }
    }

    /// <summary>
    /// Gets or sets the randomness intensity (0-1).
    /// 0 = no randomness, 1 = maximum variation.
    /// </summary>
    public float RandomnessIntensity
    {
        get { lock (_lock) return _randomnessIntensity; }
        set { lock (_lock) _randomnessIntensity = Clamp(value); }
    }

    /// <summary>
    /// Gets or sets the maximum steering correction magnitude (-5 to +5).
    /// </summary>
    public float MaxCorrection
    {
        get { lock (_lock) return _maxCorrection; }
        set { lock (_lock) _maxCorrection = ClampMaxCorrection(value); }
    }

    /// <summary>
    /// Gets or sets the update interval in milliseconds.
    /// Controls how often new random values are generated.
    /// </summary>
    public int UpdateIntervalMs
    {
        get { lock (_lock) return _updateIntervalMs; }
        set { lock (_lock) _updateIntervalMs = Math.Max(50, value); } // Minimum 50ms
    }

    /// <summary>
    /// Gets the current steering correction value (-5 to +5).
    /// </summary>
    public float CurrentCorrection
    {
        get { lock (_lock) return _currentCorrection; }
    }

    /// <summary>
    /// Initializes a new instance of the steering assist service.
    /// </summary>
    /// <param name="randomnessIntensity">Initial randomness intensity (default 0.3f).</param>
    /// <param name="maxCorrection">Maximum correction magnitude (default 5.0f).</param>
    /// <param name="updateIntervalMs">Update interval in ms (default 100ms).</param>
    public SteeringAssistService(
        float randomnessIntensity = 0.3f,
        float maxCorrection = 5.0f,
        int updateIntervalMs = 100)
    {
        _random = new Random(Guid.NewGuid().GetHashCode());
        _updateTimer = new Stopwatch();
        _updateTimer.Start();
        
        _randomnessIntensity = randomnessIntensity;
        _maxCorrection = maxCorrection;
        _updateIntervalMs = updateIntervalMs;
        _currentCorrection = 0f;
        _targetCorrection = 0f;
    }

    /// <summary>
    /// Updates and returns the current steering correction.
    /// Generates new random values at controlled intervals.
    /// </summary>
    /// <returns>Current steering correction (-5 to +5).</returns>
    public float Update()
    {
        lock (_lock)
        {
            if (!_isEnabled)
            {
                _currentCorrection = 0f;
                _targetCorrection = 0f;
                return 0f;
            }

            // Check if it's time to generate a new random value
            if (_updateTimer.ElapsedMilliseconds >= _updateIntervalMs)
            {
                _updateTimer.Restart();
                GenerateNewCorrection();
            }

            // Smoothly interpolate toward target for natural feel
            _currentCorrection = Lerp(_currentCorrection, _targetCorrection, 0.1f);

            return _currentCorrection;
        }
    }

    /// <summary>
    /// Resets the steering correction to zero.
    /// </summary>
    public void Reset()
    {
        lock (_lock)
        {
            _currentCorrection = 0f;
            _targetCorrection = 0f;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        lock (_lock)
        {
            if (_isDisposed)
                return;

            _updateTimer.Stop();
            Reset();
            _isDisposed = true;
        }
    }

    private void GenerateNewCorrection()
    {
        // Generate random correction within ±maxCorrection range
        // Scaled by randomness intensity
        float range = _maxCorrection * _randomnessIntensity;
        
        // Use sum of multiple random values for more natural distribution
        // (central limit theorem gives bell curve instead of flat distribution)
        float sum = 0f;
        const int samples = 3;
        for (int i = 0; i < samples; i++)
        {
            sum += (float)_random.NextDouble() * 2f - 1f; // -1 to +1
        }
        
        // Normalize and scale
        float normalized = sum / samples;
        _targetCorrection = normalized * range;

        // Clamp to safe bounds
        _targetCorrection = ClampSteering(_targetCorrection);
    }

    private static float Clamp(float value) => Math.Max(0f, Math.Min(1f, value));
    private static float ClampMaxCorrection(float value) => Math.Max(0f, Math.Min(5f, value));
    private static float ClampSteering(float value) => Math.Max(-5f, Math.Min(5f, value));

    private static float Lerp(float a, float b, float t) => a + (b - a) * t;
}
