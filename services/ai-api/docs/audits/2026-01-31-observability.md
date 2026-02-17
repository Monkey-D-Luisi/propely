# Observability Audit (2026-01-31)

## Scope
- OpenTelemetry traces and metrics configuration in `src/` and configuration files.
- Structured logging with correlation IDs across request and messaging boundaries.
- Health checks for liveness and readiness (PostgreSQL, RabbitMQ, Redis).
- Logging verbosity and sensitive data exposure.

## Evidence

### OpenTelemetry traces and metrics
- OpenTelemetry is wired in the API startup via `AddTelemetry`, with ASP.NET Core, HTTP client, and EF Core instrumentation, plus custom work item metrics.
- OTLP exporter and console exporter are optional and enabled via configuration keys `OpenTelemetry:OtlpEndpoint` and `OpenTelemetry:UseConsoleExporter`.
- Custom metrics are registered via `WorkItemMetrics` and exposed through `AddMeter`.

**Evidence:**
- `src/SaasTemplate.AiApi.Api/Program.cs` (AddTelemetry registration)
- `src/SaasTemplate.AiApi.Api/Configuration/TelemetryConfiguration.cs` (tracing/metrics setup)
- `src/SaasTemplate.AiApi.Application/Common/Telemetry/WorkItemMetrics.cs` (custom meter)
- `src/SaasTemplate.AiApi.Api/appsettings*.json` (no OpenTelemetry settings currently defined)

### Structured logging with correlation IDs across boundaries
- HTTP request correlation IDs are extracted from `X-Correlation-ID`, sanitized, stored in a scoped accessor, returned in response headers, and applied to the logging scope.
- Domain events and outbox records carry `CorrelationId`, and RabbitMQ publishes set `properties.CorrelationId` when available.
- Event consumption logs include `CorrelationId` in structured scopes for projector processing.

**Evidence:**
- `src/SaasTemplate.AiApi.Api/Middleware/CorrelationIdMiddleware.cs` (request scope + header handling)
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/AppDbContext.cs` (domain event -> outbox with correlation ID)
- `src/SaasTemplate.AiApi.Infrastructure/Messaging/RabbitMqPublisher.cs` (AMQP correlation ID propagation)
- `src/SaasTemplate.AiApi.Infrastructure/Messaging/WorkItemEventProjector.cs` (logging scope with correlation ID)

### Health checks (liveness + readiness)
- Liveness endpoint checks only process health via tag `live`.
- Readiness endpoint checks dependency health (PostgreSQL, Redis, RabbitMQ) when configuration is present.
- Health endpoints are exposed at `/health/live` and `/health/ready`.

**Evidence:**
- `src/SaasTemplate.AiApi.Api/Configuration/HealthChecksConfiguration.cs`
- `src/SaasTemplate.AiApi.Api/Program.cs` (health check registration and endpoints)

### Logging verbosity and sensitive data exposure
- Logging defaults: `Information` in production and `Debug` in development/test, with component overrides.
- Redis connection logging redacts passwords.
- Logs avoid dumping payload bodies or secrets; messages are largely metadata-only.

**Evidence:**
- `src/SaasTemplate.AiApi.Api/appsettings.json` and `appsettings.Development.json`
- `src/SaasTemplate.AiApi.Infrastructure/Caching/RedisCacheService.cs` (redaction helper)
- `src/SaasTemplate.AiApi.Infrastructure/Messaging/*.cs` (metadata-only logging)

## Findings

### Strengths
- OpenTelemetry instrumentation is consistently configured for traces and metrics, and the service name is set for resource identification.
- Health checks distinguish liveness and readiness and include dependency tags.
- Correlation IDs are generated/sanitized, scoped, and returned to callers.
- Logging avoids PII/secret exposure, with explicit redaction for Redis connection strings.

### Gaps & Risks
- OpenTelemetry configuration keys are not present in `appsettings*.json`, so tracing/metrics export relies on environment variables or external configuration only.
- HTTP correlation IDs are not currently passed into application commands, so correlation can be lost between HTTP requests and domain/outbox events unless callers set correlation IDs explicitly.
- Outbox dispatcher logs do not consistently include correlation IDs for published events, reducing cross-boundary traceability for async workflows.
- Readiness checks are only added when configuration values are non-empty; if config is missing, readiness may appear “healthy” while dependencies are effectively unconfigured.

## Recommended Improvements
1. **Add explicit OpenTelemetry configuration defaults** in appsettings (e.g., `OpenTelemetry:OtlpEndpoint`, `OpenTelemetry:UseConsoleExporter`) and document environment variable overrides.
2. **Propagate correlation IDs from the HTTP layer into commands** (e.g., via MediatR pipeline behavior or controller using `ICorrelationIdAccessor`).
3. **Include correlation IDs in outbox processing logs**, ensuring async paths remain traceable without inspecting payloads.
4. **Make readiness checks explicit for required dependencies** by failing startup or emitting warnings when critical configuration is missing.

## Notes
- No runtime validation was performed; findings are based on static code inspection.
