# Task 0013: Local Observability Dashboard

## Metadata
- **ID**: 0013
- **Type**: Task
- **Status**: IN_PROGRESS
- **Owner**: Agent
- **Created**: 2026-02-02
- **Related**: [Epic](../backlog/agent-ready-epic.md)

## Goal
Provide a local UI to visualize OpenTelemetry traces and metrics without external dependencies, improving the developer experience (DX) for debugging and performance monitoring.

## Context
Currently, we have OpenTelemetry instrumentation (Task 0010), but visualizing the data requires setting up external tools or checking logs. We want a "zero-config" dashboard that runs with `docker compose`.

## Requirements
- Update `docker-compose.yml` to include **Aspire Dashboard**.
  - Image: `mcr.microsoft.com/dotnet/nightly/aspire-dashboard:latest` (or stable if available).
  - Ports: Expose OTLP receiver (usually 4317/4318) and UI (18888).
  - No auth or simple auth for local dev.
- Configure `OTEL_EXPORTER_OTLP_ENDPOINT` in `src/SaasTemplate.AiApi.Api` to point to the dashboard.
- Update `QUICKSTART.md` with instructions:
  - How to view traces/metrics at `http://localhost:18888`.

## Detailed Implementation
1.  **Docker Compose**: Add `aspire-dashboard` service.
2.  **API Config**: Ensure `appsettings.json` or `docker-compose.override.yml` points OTLP exporter to the dashboard.
3.  **Documentation**: Add a "Observability" section to Quickstart.

## Definition of Done
- [x] `docker-compose.yml` includes functioning dashboard service
- [x] API successfully sends traces/metrics to dashboard
- [x] Dashboard is accessible at `http://localhost:18888`
- [x] `QUICKSTART.md` updated with access instructions
- [x] Walkthrough updated
- [x] No secrets committed
