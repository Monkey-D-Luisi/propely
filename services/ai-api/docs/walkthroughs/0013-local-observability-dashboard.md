# Walkthrough - Task 0013: Local Observability Dashboard

## Task Info
- **Task**: [0013-local-observability-dashboard](../tasks/0013-local-observability-dashboard.md)
- **Feature**: Developer Experience / Observability

## Implementation Summary
Integrated the .NET Aspire Dashboard to provide a standalone, local visualization tool for OpenTelemetry data (traces, metrics, logs).

### Changes
- **Infrastructure**: Added `aspire-dashboard` container to `docker-compose.yml`.
- **Configuration**: Pointed OTLP exporters to the dashboard.
- **Documentation**: Updated `QUICKSTART.md` with usage instructions.

## Verification
### Automated Tests
- [ ] `dotnet test` (Should pass, infrastructure change shouldn't break logic)

### Manual Verification
- [x] Run `./scripts/dev-up.sh`
- [x] Open http://localhost:18888
- [x] Send request to API (e.g. create work item)
- [x] Verify trace appears in dashboard

## Checklist
- [x] Task scope matches docs/tasks/0013-local-observability-dashboard.md
- [x] Docs updated where relevant
- [x] No secrets committed
