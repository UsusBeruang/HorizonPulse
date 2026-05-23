# Architecture

# Principles

- Keep architecture simple
- Avoid over-engineering
- Prefer folders over many projects
- Use small focused classes
- Keep telemetry parsing isolated
- Avoid unnecessary abstractions

# Project Structure

```text
HorizonPulse/
â”‚
â”œâ”€â”€ .ai/
â”œâ”€â”€ Automation/
â”œâ”€â”€ Models/
â”œâ”€â”€ Services/
â”œâ”€â”€ UI/
â””â”€â”€ Utils/
```

# Recommended Services

## Telemetry
- TelemetryReceiver
- TelemetryParser
- TelemetryState

## Automation
- AutomationService
- ViGEmControllerService
- SteeringAssist

## Runtime
- RaceMonitorService
- RuntimeStateService

# Threading

- Networking should be async
- Avoid blocking UI thread
- Use dispatcher only when necessary
- Keep UI updates lightweight

# State Management

Use:
- central telemetry state object
- observable properties for UI
- minimal shared mutable state
