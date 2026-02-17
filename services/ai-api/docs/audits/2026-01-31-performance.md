# Performance Audit — 2026-01-31

## Scope
- Reviewed async usage, data access, cache usage, connection management, queue throughput, and allocation hotspots in `src/`.

## Findings

### 1) Blocking async calls
**Status:** No blocking async calls found in `src/`.

**Evidence**
- No `.Result` or `.Wait()` usage present in the application entrypoint or services reviewed. The `WorkItemsController` and query/command handlers use `async`/`await` consistently.【F:src/SaasTemplate.AiApi.Api/Controllers/WorkItemsController.cs†L1-L115】【F:src/SaasTemplate.AiApi.Application/WorkItems/Queries/GetWorkItemByIdQueryHandler.cs†L1-L85】

**Recommendation**
- No change required. Continue to avoid sync-over-async usage when adding new features.

---

### 2) Data access (N+1 and pagination)
**Status:** No N+1 patterns identified; no list endpoints exist today.

**Evidence**
- The only read endpoint is a `GET` by ID; there are no list endpoints requiring pagination logic.【F:src/SaasTemplate.AiApi.Api/Controllers/WorkItemsController.cs†L80-L115】
- The read repository performs single-entity lookups via the read model or write model, not iterative per-entity queries.【F:src/SaasTemplate.AiApi.Infrastructure/Persistence/Repositories/WorkItemReadRepository.cs†L23-L92】

**Recommendation**
- When list endpoints are added, enforce pagination at the API boundary and avoid per-row lookup loops (use set-based queries and projection).

---

### 3) Redis usage (TTL, key conventions, cache-as-optimization)
**Status:** Meets baseline requirements; small improvement opportunity.

**Evidence**
- Cache keys are centralized with a predictable prefix (`workitem:{id}`) and a configurable global prefix (`aiapi:`).【F:src/SaasTemplate.AiApi.Application/WorkItems/WorkItemCacheKeys.cs†L1-L18】【F:src/SaasTemplate.AiApi.Infrastructure/Caching/Configuration/RedisConfiguration.cs†L29-L39】
- Cache-aside pattern is used with TTL from configuration, and cache failure falls back to DB reads (optimization-only).【F:src/SaasTemplate.AiApi.Application/WorkItems/Queries/GetWorkItemByIdQueryHandler.cs†L33-L79】【F:src/SaasTemplate.AiApi.Infrastructure/Caching/RedisCacheService.cs†L31-L118】

**Recommendation**
- Consider adding metrics for cache hit/miss rates to validate effectiveness and tune TTLs (e.g., histogram or counters). This makes optimization measurable rather than assumed.

---

### 4) Connection pooling, retry policies, and queue throughput
**Status:** Functional but potential throughput and resiliency gaps.

**Evidence**
- EF Core uses `AddDbContext` (no DbContext pooling configured).【F:src/SaasTemplate.AiApi.Api/Program.cs†L35-L47】
- Redis and RabbitMQ connections are reused via singletons, but no explicit retry/backoff policies are configured for transient failures (connections are retried only on next usage).【F:src/SaasTemplate.AiApi.Infrastructure/Caching/RedisCacheService.cs†L119-L197】【F:src/SaasTemplate.AiApi.Infrastructure/Messaging/RabbitMqPublisher.cs†L60-L183】
- Outbox dispatch is sequential per message with a `SaveChangesAsync` per message, limiting throughput under large backlogs.【F:src/SaasTemplate.AiApi.Infrastructure/Messaging/OutboxDispatcherService.cs†L86-L163】
- Projector uses a single consumer with configurable prefetch; parallelism requires additional consumers or instances to increase throughput.【F:src/SaasTemplate.AiApi.Infrastructure/Messaging/WorkItemProjectorService.cs†L91-L175】

**Recommendations**
- **DbContext pooling:** Consider `AddDbContextPool<AppDbContext>` (with pool size) for high-throughput APIs to reduce allocations and context initialization costs.
- **Retries/backoff:** Introduce bounded retries with exponential backoff for Redis/RabbitMQ operations to smooth transient failures without hammering downstream services.
- **Outbox throughput:** Batch database updates (mark processed in-memory and flush once per batch) or use a transactional bulk update to reduce per-message `SaveChangesAsync` overhead.
- **Projector scaling:** Allow multiple consumers per instance or scale out instances to increase parallelism; keep prefetch aligned with processing time.

---

### 5) Memory allocation hotspots
**Status:** No critical hotspots found; a couple of potential optimizations if throughput demands increase.

**Evidence**
- Outbox dispatch materializes batches into memory (`ToListAsync`) and serializes messages for publish; this is bounded by batch size but could be tuned if payloads are large.【F:src/SaasTemplate.AiApi.Infrastructure/Persistence/Repositories/OutboxRepository.cs†L23-L47】【F:src/SaasTemplate.AiApi.Infrastructure/Messaging/OutboxDispatcherService.cs†L86-L163】
- Redis cache serialization/deserialization is performed per request; acceptable today, but may warrant optimization if cache traffic grows significantly.【F:src/SaasTemplate.AiApi.Infrastructure/Caching/RedisCacheService.cs†L31-L118】

**Recommendations**
- Keep batch sizes aligned with payload size and memory budget.
- If cache traffic grows, consider source-generated serializers for hot DTOs to reduce allocation pressure.

## Summary of Top Risks
1. **Outbox throughput bottleneck** from sequential publish + per-message `SaveChangesAsync` — optimize batching when backlog growth is observed.【F:src/SaasTemplate.AiApi.Infrastructure/Messaging/OutboxDispatcherService.cs†L86-L163】
2. **Lack of explicit retry/backoff policies** for transient Redis/RabbitMQ issues — add bounded retries for resiliency under load.【F:src/SaasTemplate.AiApi.Infrastructure/Caching/RedisCacheService.cs†L119-L197】【F:src/SaasTemplate.AiApi.Infrastructure/Messaging/RabbitMqPublisher.cs†L60-L183】
3. **No DbContext pooling** configured — consider enabling pooling if request volume grows and profiling shows context creation overhead.【F:src/SaasTemplate.AiApi.Api/Program.cs†L35-L47】
