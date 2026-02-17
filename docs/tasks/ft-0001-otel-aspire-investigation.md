# ft-0001: Investigate OpenTelemetry/Aspire Dashboard Issue

## Status: COMPLETED

## Goal

Investigate and fix the issue where orgs-api is not sending telemetry data to the Aspire Dashboard despite having OpenTelemetry configured.

## Context

- `orgs-api` has OpenTelemetry packages installed and configured
- `run-orgs-api.ps1` maps `ORGSAPI_OTEL_ENDPOINT` to `OpenTelemetry__OtlpEndpoint`
- Aspire Dashboard container is running on ports 4317 (OTLP gRPC) and 18888 (UI)
- No traces/logs appear in the dashboard after making API requests

## Scope

### In Scope
- Diagnose why OTLP export is not working
- Verify environment variable is being read correctly
- Fix the configuration issue
- Add startup logging to confirm OTEL config

### Out of Scope
- Adding new telemetry instrumentation
- Frontend telemetry

## Acceptance Criteria

- [x] Identify root cause of missing telemetry
- [x] Fix the issue so traces appear in Aspire Dashboard
- [x] Document the solution

## Investigation Steps

1. Verify environment variable reaches the app
2. Check TelemetryConfiguration.cs reads the value
3. Test OTLP endpoint connectivity
4. Add debug logging to confirm exporter setup
