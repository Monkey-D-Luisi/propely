# Task: 0010-opentelemetry-integration

## Metadata
- ID: 0010
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-01-29
- Related docs:
  - Walkthrough: `docs/walkthroughs/0010-opentelemetry-integration.md`

## Goal
Add distributed tracing, metrics, and health endpoints to enable full observability of the Work Item management API.

## Context
The vertical slice for Work Item management (Tasks 0002–0009) is complete. The system has persistence (PostgreSQL), messaging (RabbitMQ), and caching (Redis), but lacks observability instrumentation. This task adds OpenTelemetry integration for tracing and metrics, plus health check endpoints for dependency monitoring.

## Scope
### In scope
- OpenTelemetry tracing for API requests, database queries, RabbitMQ operations
- Custom application metrics (counters, histograms, gauges)
- Health check endpoints (`/health/live`, `/health/ready`)
- Structured logging with correlation ID propagation

### Out of scope
- External collector/dashboard deployment
- UI dashboards configuration
- Alerting rules setup

## Requirements
- R1: Configure OpenTelemetry for traces (API, EF Core, RabbitMQ)
- R2: Add custom metrics:
  - `workitems_created_total` (counter)
  - `workitems_query_duration_seconds` (histogram)
  - `outbox_messages_pending` (gauge)
- R3: Propagate correlation IDs across all operations
- R4: Add health endpoints:
  - `/health/live` — Process liveness
  - `/health/ready` — Dependencies (DB, RabbitMQ, Redis)
- R5: Configure structured logging with correlation IDs

## Acceptance Criteria
- AC1: Traces visible in OpenTelemetry collector/console exporter
- AC2: Custom metrics exported correctly
- AC3: Correlation ID propagated across services
- AC4: `/health/live` returns 200 when process is alive
- AC5: `/health/ready` returns 200 when all dependencies are healthy
- AC6: Logs include correlation ID

## Constraints (non-negotiable)
- Clean Architecture layers respected
- English-only repo content
- No secrets in repo
- Update walkthrough

## Proposed Approach (high-level)
1. Add OpenTelemetry NuGet packages for tracing (ASP.NET Core, EF Core, HTTP instrumentation)
2. Create `TelemetryConfiguration.cs` for centralized OTel setup
3. Create `WorkItemMetrics.cs` for custom metrics definitions
4. Create `HealthChecksConfiguration.cs` for health check setup
5. Add health check packages for PostgreSQL, RabbitMQ, Redis
6. Update Program.cs to wire in telemetry and health checks
7. Add integration tests for health endpoints
8. Create unit tests for metrics recording

## Implementation Steps
1. Add required NuGet packages to `Api.csproj` and `Infrastructure.csproj`
2. Create `TelemetryConfiguration.cs` in API layer
3. Create `HealthChecksConfiguration.cs` in API layer
4. Create `WorkItemMetrics.cs` in Infrastructure/Telemetry layer
5. Update `CreateWorkItemCommandHandler` to record metrics
6. Update `GetWorkItemByIdQueryHandler` to record query duration
7. Update `OutboxDispatcherService` to record pending message gauge
8. Update `Program.cs` to add telemetry and health checks
9. Create integration tests for health endpoints
10. Verify all tests pass

## Files to Create / Modify
- `src/SaasTemplate.AiApi.Api/SaasTemplate.AiApi.Api.csproj` — Add OTel and HealthChecks packages
- `src/SaasTemplate.AiApi.Infrastructure/SaasTemplate.AiApi.Infrastructure.csproj` — Add OTel instrumentation packages
- `src/SaasTemplate.AiApi.Api/Configuration/TelemetryConfiguration.cs` — [NEW]
- `src/SaasTemplate.AiApi.Api/Configuration/HealthChecksConfiguration.cs` — [NEW]
- `src/SaasTemplate.AiApi.Application/Common/Telemetry/WorkItemMetrics.cs` — [NEW]
- `src/SaasTemplate.AiApi.Application/WorkItems/Commands/CreateWorkItemCommandHandler.cs` — Add metrics
- `src/SaasTemplate.AiApi.Application/WorkItems/Queries/GetWorkItemByIdQueryHandler.cs` — Add metrics
- `src/SaasTemplate.AiApi.Infrastructure/Messaging/OutboxDispatcherService.cs` — Add metrics
- `src/SaasTemplate.AiApi.Api/Program.cs` — Wire telemetry and health checks
- `tests/SaasTemplate.AiApi.IntegrationTests/Health/HealthEndpointTests.cs` — [NEW]

## Testing Plan
- Unit tests: Metrics recording via WorkItemMetrics
- Integration tests: Health endpoints return correct status
- Manual verification: Run app locally and check `/health/live`, `/health/ready`

## Security & Privacy
- No sensitive data in telemetry spans
- CorrelationId is a UUID, not PII
- Health endpoints are unauthenticated (standard practice for orchestrators)

## Observability
- Logs: All operations log with correlation ID
- Metrics: `workitems_created_total`, `workitems_query_duration_seconds`, `outbox_messages_pending`
- Traces: Full request flow from API to DB/RabbitMQ

## Rollback Plan
- Remove OTel packages and configuration calls from Program.cs
- Health endpoints removal is safe (no side effects)

## Definition of Done Checklist
- [ ] Acceptance criteria met
- [ ] Build passes
- [ ] Tests added/updated and pass
- [ ] Formatting/analyzers pass
- [ ] No secrets committed
- [ ] Walkthrough updated
