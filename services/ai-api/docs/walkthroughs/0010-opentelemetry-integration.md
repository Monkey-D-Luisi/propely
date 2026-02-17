# Walkthrough: 0010-opentelemetry-integration

## Task Reference
- Task: `docs/tasks/0010-opentelemetry-integration.md`

## Summary
Implemented OpenTelemetry integration for distributed tracing and metrics, plus health check endpoints for dependency monitoring.

## Implementation

### Files Created
- `src/SaasTemplate.AiApi.Api/Configuration/TelemetryConfiguration.cs` — OpenTelemetry setup with:
  - ASP.NET Core instrumentation
  - HTTP client instrumentation
  - EF Core instrumentation
  - Console exporter (dev) and OTLP exporter (prod)
  - Custom `WorkItemMetrics` class with counters, histograms, and gauges

- `src/SaasTemplate.AiApi.Api/Configuration/HealthChecksConfiguration.cs` — Health endpoints:
  - `/health/live` — Liveness probe (self check)
  - `/health/ready` — Readiness probe (PostgreSQL, Redis)

### Files Modified
- `src/SaasTemplate.AiApi.Api/SaasTemplate.AiApi.Api.csproj` — Added:
  - OpenTelemetry packages (Extensions.Hosting, Instrumentation.*, Exporters)
  - AspNetCore.HealthChecks.NpgSql
  - AspNetCore.HealthChecks.Redis

- `src/SaasTemplate.AiApi.Infrastructure/SaasTemplate.AiApi.Infrastructure.csproj` — Added:
  - OpenTelemetry.Instrumentation.EntityFrameworkCore

- `src/SaasTemplate.AiApi.Api/Program.cs` — Wired:
  - `AddTelemetry()` for OpenTelemetry
  - `AddApplicationHealthChecks()` for health checks
  - `UseHealthCheckEndpoints()` in middleware

## Custom Metrics Defined
| Metric | Type | Description |
|--------|------|-------------|
| `workitems_created_total` | Counter | Total work items created |
| `workitems_query_duration_seconds` | Histogram | Query execution time |
| `outbox_messages_pending` | Gauge | Pending outbox messages |

## Configuration
```json
// appsettings.json
{
  "OpenTelemetry": {
    "UseConsoleExporter": true,  // Dev only
    "OtlpEndpoint": "http://otel-collector:4317"  // Prod
  }
}
```

## Health Endpoints
- `GET /health/live` — Returns 200 if process is alive
- `GET /health/ready` — Returns 200 if PostgreSQL and Redis are healthy

## Testing Notes
> **Note**: Integration tests for health endpoints were not included in this PR due to pre-existing test infrastructure issues discovered during implementation. These will be addressed in a separate fast track task.

## How to Verify
```bash
# Start the API
dotnet run --project src/SaasTemplate.AiApi.Api

# Check liveness
curl http://localhost:5000/health/live

# Check readiness
curl http://localhost:5000/health/ready
```

## Build Verification
```bash
dotnet build  # ✅ Passes
```
