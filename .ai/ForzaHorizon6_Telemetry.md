# Forza Horizon 6 â€” UDP Telemetry Reference

> C# WPF Project Reference  
> Packet size: **324 bytes** | Protocol: **UDP (one-way, outbound only)**

---

## Configuration

In-game: **Settings â†’ HUD and Gameplay**

| Setting | Description |
|---|---|
| **Data Out** | Toggle telemetry on/off. Starts sending when player begins driving. |
| **Data Out IP Address** | Target IP (supports `127.0.0.1` for localhost). |
| **Data Out IP Port** | Target port. **Avoid ports 5200â€“5300** (reserved by the game). |

---

## Data Types

| Notation | C# Equivalent | Description |
|---|---|---|
| `S8` | `sbyte` | Signed 8-bit integer (âˆ’128 to 127) |
| `U8` | `byte` | Unsigned 8-bit integer (0 to 255) |
| `U16` | `ushort` | Unsigned 16-bit integer |
| `S32` | `int` | Signed 32-bit integer |
| `U32` | `uint` | Unsigned 32-bit integer |
| `F32` | `float` | 32-bit floating point |

---

## Packet Structure

Read fields in order using `BinaryReader` with `LittleEndian` byte order.

### Race Status

| Field | Type | C# Type | Notes |
|---|---|---|---|
| `IsRaceOn` | S32 | `int` | `1` = racing; `0` = menus/stopped |
| `TimestampMS` | U32 | `uint` | Millisecond timestamp; can overflow to 0 |

### Engine

| Field | Type | C# Type | Notes |
|---|---|---|---|
| `EngineMaxRpm` | F32 | `float` | |
| `EngineIdleRpm` | F32 | `float` | |
| `CurrentEngineRpm` | F32 | `float` | |

### Motion â€” Car Local Space

> Axes: **X = right, Y = up, Z = forward**

| Field | Type | C# Type | Notes |
|---|---|---|---|
| `AccelerationX` | F32 | `float` | |
| `AccelerationY` | F32 | `float` | |
| `AccelerationZ` | F32 | `float` | |
| `VelocityX` | F32 | `float` | |
| `VelocityY` | F32 | `float` | |
| `VelocityZ` | F32 | `float` | |
| `AngularVelocityX` | F32 | `float` | Pitch (rad/s) |
| `AngularVelocityY` | F32 | `float` | Yaw (rad/s) |
| `AngularVelocityZ` | F32 | `float` | Roll (rad/s) |

### Orientation

| Field | Type | C# Type | Notes |
|---|---|---|---|
| `Yaw` | F32 | `float` | Radians |
| `Pitch` | F32 | `float` | Radians |
| `Roll` | F32 | `float` | Radians |

### Suspension

> `0.0` = max stretch; `1.0` = max compression

| Field | Type | C# Type |
|---|---|---|
| `NormalizedSuspensionTravelFrontLeft` | F32 | `float` |
| `NormalizedSuspensionTravelFrontRight` | F32 | `float` |
| `NormalizedSuspensionTravelRearLeft` | F32 | `float` |
| `NormalizedSuspensionTravelRearRight` | F32 | `float` |

### Tire Slip Ratio

> `0` = 100% grip; `|ratio| > 1.0` = loss of grip

| Field | Type | C# Type |
|---|---|---|
| `TireSlipRatioFrontLeft` | F32 | `float` |
| `TireSlipRatioFrontRight` | F32 | `float` |
| `TireSlipRatioRearLeft` | F32 | `float` |
| `TireSlipRatioRearRight` | F32 | `float` |

### Wheel Rotation Speed (rad/s)

| Field | Type | C# Type |
|---|---|---|
| `WheelRotationSpeedFrontLeft` | F32 | `float` |
| `WheelRotationSpeedFrontRight` | F32 | `float` |
| `WheelRotationSpeedRearLeft` | F32 | `float` |
| `WheelRotationSpeedRearRight` | F32 | `float` |

### Wheel Flags

> `1` = true; `0` = false

| Field | Type | C# Type | Notes |
|---|---|---|---|
| `WheelOnRumbleStripFrontLeft` | S32 | `int` | |
| `WheelOnRumbleStripFrontRight` | S32 | `int` | |
| `WheelOnRumbleStripRearLeft` | S32 | `int` | |
| `WheelOnRumbleStripRearRight` | S32 | `int` | |
| `WheelInPuddleFrontLeft` | S32 | `int` | |
| `WheelInPuddleFrontRight` | S32 | `int` | |
| `WheelInPuddleRearLeft` | S32 | `int` | |
| `WheelInPuddleRearRight` | S32 | `int` | |

### Surface Rumble (Force Feedback)

| Field | Type | C# Type |
|---|---|---|
| `SurfaceRumbleFrontLeft` | F32 | `float` |
| `SurfaceRumbleFrontRight` | F32 | `float` |
| `SurfaceRumbleRearLeft` | F32 | `float` |
| `SurfaceRumbleRearRight` | F32 | `float` |

### Tire Slip Angle

> `0` = 100% grip; `|angle| > 1.0` = loss of grip

| Field | Type | C# Type |
|---|---|---|
| `TireSlipAngleFrontLeft` | F32 | `float` |
| `TireSlipAngleFrontRight` | F32 | `float` |
| `TireSlipAngleRearLeft` | F32 | `float` |
| `TireSlipAngleRearRight` | F32 | `float` |

### Tire Combined Slip

> `0` = 100% grip; `|slip| > 1.0` = loss of grip

| Field | Type | C# Type |
|---|---|---|
| `TireCombinedSlipFrontLeft` | F32 | `float` |
| `TireCombinedSlipFrontRight` | F32 | `float` |
| `TireCombinedSlipRearLeft` | F32 | `float` |
| `TireCombinedSlipRearRight` | F32 | `float` |

### Suspension Travel (meters)

| Field | Type | C# Type |
|---|---|---|
| `SuspensionTravelMetersFrontLeft` | F32 | `float` |
| `SuspensionTravelMetersFrontRight` | F32 | `float` |
| `SuspensionTravelMetersRearLeft` | F32 | `float` |
| `SuspensionTravelMetersRearRight` | F32 | `float` |

### Car Info

| Field | Type | C# Type | Notes |
|---|---|---|---|
| `CarOrdinal` | S32 | `int` | Unique car make/model ID |
| `CarClass` | S32 | `int` | `0` (D) to `7` (X class) |
| `CarPerformanceIndex` | S32 | `int` | `100` (worst) to `999` (best) |
| `DrivetrainType` | S32 | `int` | `0` = FWD, `1` = RWD, `2` = AWD |
| `NumCylinders` | S32 | `int` | Engine cylinder count |

### âš ï¸ FH6-Exclusive Fields

> These fields are **not present in Forza Motorsport**. They appear here, after `NumCylinders` and before `PositionX`.

| Field | Type | C# Type | Notes |
|---|---|---|---|
| `CarGroup` | U32 | `uint` | Car group identifier |
| `SmashableVelDiff` | F32 | `float` | Velocity loss from collision (m/s) |
| `SmashableMass` | F32 | `float` | Mass of hit object (kg) |

### Position & Dynamics

| Field | Type | C# Type | Notes |
|---|---|---|---|
| `PositionX` | F32 | `float` | World space (meters) |
| `PositionY` | F32 | `float` | World space (meters) |
| `PositionZ` | F32 | `float` | World space (meters) |
| `Speed` | F32 | `float` | m/s |
| `Power` | F32 | `float` | Watts |
| `Torque` | F32 | `float` | Newton-meters |

### Tire Temperature

| Field | Type | C# Type |
|---|---|---|
| `TireTempFrontLeft` | F32 | `float` |
| `TireTempFrontRight` | F32 | `float` |
| `TireTempRearLeft` | F32 | `float` |
| `TireTempRearRight` | F32 | `float` |

### Vehicle Systems

| Field | Type | C# Type | Notes |
|---|---|---|---|
| `Boost` | F32 | `float` | PSI above atmospheric |
| `Fuel` | F32 | `float` | `0.0` = empty; `1.0` = full |
| `DistanceTraveled` | F32 | `float` | Meters |

### Lap & Race Timing

| Field | Type | C# Type | Notes |
|---|---|---|---|
| `BestLap` | F32 | `float` | Seconds; `0.0` if not applicable |
| `LastLap` | F32 | `float` | Seconds; `0.0` if not applicable |
| `CurrentLap` | F32 | `float` | Seconds |
| `CurrentRaceTime` | F32 | `float` | Seconds since driving started |
| `LapNumber` | U16 | `ushort` | Completed laps |
| `RacePosition` | U8 | `byte` | Current race position |

### Player Inputs

| Field | Type | C# Type | Range |
|---|---|---|---|
| `Accel` | U8 | `byte` | 0â€“255 |
| `Brake` | U8 | `byte` | 0â€“255 |
| `Clutch` | U8 | `byte` | 0â€“255 |
| `HandBrake` | U8 | `byte` | 0â€“255 |
| `Gear` | U8 | `byte` | Current gear |
| `Steer` | S8 | `sbyte` | âˆ’127 (full left) to 127 (full right) |
| `NormalizedDrivingLine` | S8 | `sbyte` | âˆ’127 to 127 |
| `NormalizedAIBrakeDifference` | S8 | `sbyte` | âˆ’127 to 127 |

---

## C# Implementation

### Data Model

```csharp
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ForzaTelemetry
{
    // Race Status
    public int    IsRaceOn;
    public uint   TimestampMS;

    // Engine
    public float  EngineMaxRpm;
    public float  EngineIdleRpm;
    public float  CurrentEngineRpm;

    // Acceleration (car local space)
    public float  AccelerationX;
    public float  AccelerationY;
    public float  AccelerationZ;

    // Velocity (car local space)
    public float  VelocityX;
    public float  VelocityY;
    public float  VelocityZ;

    // Angular Velocity (rad/s)
    public float  AngularVelocityX;
    public float  AngularVelocityY;
    public float  AngularVelocityZ;

    // Orientation (radians)
    public float  Yaw;
    public float  Pitch;
    public float  Roll;

    // Normalized Suspension Travel (0=stretch, 1=compression)
    public float  NormalizedSuspensionTravelFrontLeft;
    public float  NormalizedSuspensionTravelFrontRight;
    public float  NormalizedSuspensionTravelRearLeft;
    public float  NormalizedSuspensionTravelRearRight;

    // Tire Slip Ratio
    public float  TireSlipRatioFrontLeft;
    public float  TireSlipRatioFrontRight;
    public float  TireSlipRatioRearLeft;
    public float  TireSlipRatioRearRight;

    // Wheel Rotation Speed (rad/s)
    public float  WheelRotationSpeedFrontLeft;
    public float  WheelRotationSpeedFrontRight;
    public float  WheelRotationSpeedRearLeft;
    public float  WheelRotationSpeedRearRight;

    // Wheel on Rumble Strip
    public int    WheelOnRumbleStripFrontLeft;
    public int    WheelOnRumbleStripFrontRight;
    public int    WheelOnRumbleStripRearLeft;
    public int    WheelOnRumbleStripRearRight;

    // Wheel in Puddle
    public int    WheelInPuddleFrontLeft;
    public int    WheelInPuddleFrontRight;
    public int    WheelInPuddleRearLeft;
    public int    WheelInPuddleRearRight;

    // Surface Rumble
    public float  SurfaceRumbleFrontLeft;
    public float  SurfaceRumbleFrontRight;
    public float  SurfaceRumbleRearLeft;
    public float  SurfaceRumbleRearRight;

    // Tire Slip Angle
    public float  TireSlipAngleFrontLeft;
    public float  TireSlipAngleFrontRight;
    public float  TireSlipAngleRearLeft;
    public float  TireSlipAngleRearRight;

    // Tire Combined Slip
    public float  TireCombinedSlipFrontLeft;
    public float  TireCombinedSlipFrontRight;
    public float  TireCombinedSlipRearLeft;
    public float  TireCombinedSlipRearRight;

    // Suspension Travel (meters)
    public float  SuspensionTravelMetersFrontLeft;
    public float  SuspensionTravelMetersFrontRight;
    public float  SuspensionTravelMetersRearLeft;
    public float  SuspensionTravelMetersRearRight;

    // Car Info
    public int    CarOrdinal;
    public int    CarClass;
    public int    CarPerformanceIndex;
    public int    DrivetrainType;
    public int    NumCylinders;

    // FH6-exclusive (not in Forza Motorsport)
    public uint   CarGroup;
    public float  SmashableVelDiff;
    public float  SmashableMass;

    // World Position
    public float  PositionX;
    public float  PositionY;
    public float  PositionZ;

    // Dynamics
    public float  Speed;
    public float  Power;
    public float  Torque;

    // Tire Temperature
    public float  TireTempFrontLeft;
    public float  TireTempFrontRight;
    public float  TireTempRearLeft;
    public float  TireTempRearRight;

    // Systems
    public float  Boost;
    public float  Fuel;
    public float  DistanceTraveled;

    // Timing
    public float  BestLap;
    public float  LastLap;
    public float  CurrentLap;
    public float  CurrentRaceTime;
    public ushort LapNumber;
    public byte   RacePosition;

    // Inputs
    public byte   Accel;
    public byte   Brake;
    public byte   Clutch;
    public byte   HandBrake;
    public byte   Gear;
    public sbyte  Steer;
    public sbyte  NormalizedDrivingLine;
    public sbyte  NormalizedAIBrakeDifference;
}
```

### UDP Listener

```csharp
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

public class ForzaUdpListener
{
    private UdpClient _udpClient;
    private const int PacketSize = 324;

    public event Action<ForzaTelemetry>? PacketReceived;

    public void Start(int port = 7777)
    {
        _udpClient = new UdpClient(port);
        Task.Run(ListenLoop);
    }

    public void Stop() => _udpClient?.Close();

    private async Task ListenLoop()
    {
        while (true)
        {
            try
            {
                var result = await _udpClient.ReceiveAsync();
                var data = result.Buffer;

                if (data.Length < PacketSize) continue;

                var telemetry = BytesToStruct<ForzaTelemetry>(data);
                PacketReceived?.Invoke(telemetry);
            }
            catch (ObjectDisposedException) { break; }
            catch (Exception ex) { /* log ex */ }
        }
    }

    private static T BytesToStruct<T>(byte[] data) where T : struct
    {
        var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
        try
        {
            return Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
        }
        finally
        {
            handle.Free();
        }
    }
}
```

### WPF ViewModel (MVVM)

```csharp
// Wire up in your ViewModel:
var listener = new ForzaUdpListener();
listener.PacketReceived += telemetry =>
{
    Application.Current.Dispatcher.Invoke(() =>
    {
        if (telemetry.IsRaceOn == 0) return;

        SpeedKph  = telemetry.Speed * 3.6f;
        Rpm       = telemetry.CurrentEngineRpm;
        Gear      = telemetry.Gear;
        Throttle  = telemetry.Accel / 255f;
        BrakeInput = telemetry.Brake / 255f;
    });
};
listener.Start(port: 7777);
```

---

## Notes

- Data is sent **only while actively driving** â€” not during menus, pauses, replays, rewinds, or post-race.
- Traffic is **outbound UDP only**. The game does not receive data.
- The packet format is **fixed** (no format selection like Forza Motorsport's "Dash" mode).
- FH6 **does not include** `TireWear` or `TrackOrdinal` fields that exist in Forza Motorsport's Dash format.
- FH6 **adds three fields** absent in Forza Motorsport: `CarGroup`, `SmashableVelDiff`, `SmashableMass` â€” inserted after `NumCylinders`.
- Speed from `Speed` field is in **m/s**. Multiply by `3.6` for km/h or `2.237` for mph.
