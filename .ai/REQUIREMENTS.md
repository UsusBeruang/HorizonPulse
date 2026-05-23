# Requirements

# Telemetry

Implement:
- UDP listener on port 54321
- Real-time packet processing
- Parsing according to ForzaHorizon6_Telemetry.md
- Strongly-typed telemetry models
- Runtime-safe parser

Telemetry categories:
- Engine
- Inputs
- Motion
- Suspension
- Race data
- Position

# UI

Implement:
- Modern dark theme
- Live telemetry dashboard
- Automation status
- Telemetry inspector
- Collapsible telemetry categories
- Human-readable telemetry fields

# Automation

Automation should:
- Complement in-game auto-steering
- Complement in-game auto-braking
- Use ViGEm virtual Xbox controller
- Provide throttle automation
- Apply extremely small steering adjustments

Steering adjustment range:
- Approximately -5 to +5 analog values
- Out of 255 total analog range

# Race Monitoring

Implement:
- Race active detection
- Race finished detection
- Lap tracking
- Timeout monitoring
- Automation coordination
