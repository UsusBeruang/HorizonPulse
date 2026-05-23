# Development Rules

# General Rules

- Avoid over-engineering
- Avoid unnecessary abstractions
- Prefer readability over cleverness
- Keep classes focused
- Keep methods small
- Use clear naming

# UI Rules

- Never block UI thread
- Keep dashboard responsive
- Avoid excessive animations
- Use human-readable formatting

# Telemetry Rules

- Use allocation-safe parsing where possible
- Validate packet sizes
- Keep parser isolated
- Avoid parser side effects

# Automation Rules

- Safety first
- Stop automation on telemetry failure
- Stop automation on race finish
- Never spam controller input
- Keep steering adjustments minimal

# Architecture Rules

Prefer:
- folders
- namespaces
- focused services

Avoid:
- excessive interfaces
- unnecessary inheritance
- enterprise patterns
- excessive projects/assemblies

# Logging Rules

Use Serilog for:
- telemetry status
- automation state
- runtime errors
- controller connection state

Avoid:
- excessive log spam
