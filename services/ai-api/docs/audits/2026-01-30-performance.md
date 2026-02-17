# Performance Audit Report (2026-01-30)

## Summary
This audit reviewed async blocking calls, data access patterns, Redis caching, connection pooling, and RabbitMQ usage. One blocking async call was found in health check configuration (acknowledged exception); no N+1 query patterns were found. The primary scalability risks relate to missing DbContext pooling and limited RabbitMQ reconnection resilience.

## Scope
- Application and Infrastructure layers under `src/`
- Data access and list endpoints in `Presentation` and `Infrastructure`
- Redis caching configuration and usage
- RabbitMQ publisher/consumer configuration

## Methodology
- Searched for blocking async patterns (`.Result`, `.Wait()`, `.GetAwaiter().GetResult()`).
- Reviewed query handlers, repositories, and controller endpoints for list access and pagination.
- Inspected Redis caching configuration, cache-aside usage, TTL enforcement, and key conventions.
- Inspected EF Core configuration for connection pooling.
- Reviewed RabbitMQ publisher and projector services for connection reuse and recovery behavior.

## Findings
### F-01: DbContext pooling is not enabled (Moderate)
**Impact:** Additional allocations and higher CPU usage under load due to frequent DbContext creation.

**Evidence:** `Program.cs` registers DbContext via `AddDbContext` rather than `AddDbContextPool`. Npgsql does pool connections, but DbContext pooling is a separate optimization.

### F-02: RabbitMQ connections lack auto-recovery/backoff logic (Moderate)
**Impact:** Publisher/consumer may stall after network interruptions until the process restarts.

**Evidence:** `RabbitMqPublisher` lazily establishes and recreates its connection/channel via `EnsureConnectionAsync` on first and subsequent `PublishAsync` calls but does not enable client auto-recovery or backoff; `WorkItemProjectorService` opens a connection/channel once and does not attempt to reconnect after disconnects.

### F-03: No list endpoints detected; pagination not currently applicable (Info)
**Impact:** None for current API surface. If list endpoints are added, enforce pagination to prevent unbounded reads.

**Evidence:** Controller and application search did not locate list endpoints or query handlers that return collections.

### F-04: Redis cache usage follows TTL and key prefix conventions (OK)
**Impact:** Cache entries are set with TTL and a configurable prefix, aligning with repository standards.

**Evidence:** `RedisCacheService.SetAsync` requires an expiration value; `GetWorkItemByIdQueryHandler` uses `DefaultTtlMinutes`; `RedisConfiguration` provides a configurable `KeyPrefix` with a default value (not validated/enforced); `WorkItemCacheKeys` standardizes per-entity key segments.

### F-05: One blocking async call detected in health check (Low)
**Impact:** Minimal; occurs only during service startup in health check registration lambda.

**Evidence:** Pattern scan for `.Result`, `.Wait()`, and `.GetAwaiter().GetResult()` found one match: `factory.CreateConnectionAsync().GetAwaiter().GetResult()` in `src/SaasTemplate.AiApi.Api/Configuration/HealthChecksConfiguration.cs:82`. This is an acknowledged exception because the health check library API requires a synchronous factory delegate; the blocking occurs only once at startup, not in request paths.

### F-06: No obvious N+1 query patterns detected (OK)
**Impact:** None observed.

**Evidence:** Repository and query handlers use single-query access patterns; no nested per-item queries found in handlers or repositories.

## Remediation Plan
1. **Enable DbContext pooling** by switching to `AddDbContextPool<AppDbContext>` in `Program.cs`, and confirm compatibility with scoped services.
2. **Add RabbitMQ recovery/resilience** by configuring the client's built-in automatic recovery. In the `ConnectionFactory`, set `AutomaticRecoveryEnabled = true` and configure a `NetworkRecoveryInterval`. This is the recommended approach and is often sufficient without a custom reconnect loop.
3. **Maintain pagination enforcement** when list endpoints are introduced; add pagination parameters and defaults in controllers and queries.

## Evidence Log
- `rg -n "\.Result\b|\.Wait\(\)|GetAwaiter\(\)\.GetResult\(\)" src` → 1 match in `HealthChecksConfiguration.cs:82` (acknowledged exception).
- `rg -n "GetAll|List|Paged|Page|Skip\(|Take\(" src/SaasTemplate.AiApi.Api src/SaasTemplate.AiApi.Application src/SaasTemplate.AiApi.Infrastructure` → no list endpoints found.
- Redis cache TTL and key conventions verified in:
  - `src/SaasTemplate.AiApi.Application/WorkItems/Queries/GetWorkItemByIdQueryHandler.cs`
  - `src/SaasTemplate.AiApi.Application/WorkItems/WorkItemCacheKeys.cs`
  - `src/SaasTemplate.AiApi.Infrastructure/Caching/RedisCacheService.cs`
  - `src/SaasTemplate.AiApi.Infrastructure/Caching/Configuration/RedisConfiguration.cs`
- DbContext registration in `src/SaasTemplate.AiApi.Api/Program.cs`.
- RabbitMQ connection setup in:
  - `src/SaasTemplate.AiApi.Infrastructure/Messaging/RabbitMqPublisher.cs`
  - `src/SaasTemplate.AiApi.Infrastructure/Messaging/WorkItemProjectorService.cs`
