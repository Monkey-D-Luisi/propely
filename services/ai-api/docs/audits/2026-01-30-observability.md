# Observability Audit Report

## Metadata
- **Date**: 2026-01-30
- **Scope**: API service (`src/SaasTemplate.AiApi.Api`), infrastructure services (`src/SaasTemplate.AiApi.Infrastructure`), and application telemetry usage.
- **Reviewer**: Automated audit

## Checklist
| Area | Status | Notes |
| --- | --- | --- |
| OpenTelemetry traces | Partial | Tracing configured for ASP.NET Core, HTTP client, and EF Core; RabbitMQ instrumentation is not present. |
| OpenTelemetry metrics | Partial | Metrics configured for ASP.NET Core, HTTP client, and custom WorkItem metrics; no messaging metrics observed. |
| Structured logging | Partial | Uses `ILogger` message templates (structured), but no explicit JSON/log enrichment is configured. |
| Correlation ID propagation | Gap | Correlation IDs exist in domain/outbox models, but there is no inbound correlation middleware or log enrichment; HTTP/RabbitMQ boundaries are not explicitly correlated. |
| Health checks | Partial | /health/live and /health/ready are wired; readiness checks for dependencies are registered conditionally. |
| Secrets/PII in logs | Pass | No log statements include credentials or PII; Redis connection strings are redacted before logging. |

## Findings
### 1) OpenTelemetry traces and metrics are configured in startup
**Status**: Partial
- OpenTelemetry is registered in `Program.cs` and configured in `TelemetryConfiguration` for ASP.NET Core, HTTP client, and EF Core instrumentation.
- OTLP/console exporters are wired via configuration flags but no default OpenTelemetry configuration values are present in `appsettings`.
- RabbitMQ tracing/metrics are not instrumented.

### 2) Structured logging and correlation ID propagation
**Status**: Gap
- Logging uses structured templates (`ILogger`), which supports structured log events.
- There is no middleware that reads or generates a correlation ID from HTTP headers, and no log enrichment is configured to include trace/correlation IDs in log scopes.
- Correlation IDs are supported in the domain/outbox model and RabbitMQ publisher, but no clear propagation from HTTP requests into commands/outbox exists.

### 3) Health check endpoints for liveness and readiness
**Status**: Partial
- `/health/live` checks process liveness.
- `/health/ready` includes PostgreSQL, Redis, and RabbitMQ health checks when configuration values are provided.
- When connection strings or host settings are missing, readiness checks for those dependencies are not registered.

### 4) Logs avoid secrets and PII
**Status**: Pass
- Logging statements do not include payloads, credentials, or user-identifying data.
- Redis connection strings are redacted before logging.
- RabbitMQ logging includes host/port only.

## Recommendations
1. Add inbound correlation ID middleware to read/create an ID (e.g., `X-Correlation-ID`) and store it in logging scope, propagating it to commands/outbox.
2. Enrich logs with trace IDs (OpenTelemetry or Activity) for end-to-end correlation.
3. Add RabbitMQ instrumentation (tracing/metrics) if observability across messaging is required.
4. Document OpenTelemetry configuration defaults in `appsettings.json` or `README.md` so exporters can be enabled without code changes.

## Evidence
- `src/SaasTemplate.AiApi.Api/Program.cs`
- `src/SaasTemplate.AiApi.Api/Configuration/TelemetryConfiguration.cs`
- `src/SaasTemplate.AiApi.Api/Configuration/HealthChecksConfiguration.cs`
- `src/SaasTemplate.AiApi.Infrastructure/Caching/RedisCacheService.cs`
- `src/SaasTemplate.AiApi.Infrastructure/Messaging/RabbitMqPublisher.cs`
- `src/SaasTemplate.AiApi.Infrastructure/Messaging/OutboxDispatcherService.cs`
- `src/SaasTemplate.AiApi.Infrastructure/Messaging/WorkItemProjectorService.cs`
- `src/SaasTemplate.AiApi.Application/WorkItems/Commands/CreateWorkItemCommand.cs`
- `src/SaasTemplate.AiApi.Api/Controllers/WorkItemsController.cs`
