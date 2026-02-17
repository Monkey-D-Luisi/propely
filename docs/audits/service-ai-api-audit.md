# Service Audit: ai-api

## Audit Metadata
- **Service:** `services/ai-api/`
- **Date:** 2026-02-13
- **Auditor:** Agent
- **Status:** In Progress
- **Technology:** .NET 10 Clean Architecture + CQRS (MediatR)
- **Source files analyzed:** ~45
- **Test files analyzed:** ~20

## Scores

| Area | Score | Verdict |
|------|-------|---------|
| Clean Code | 88/100 | Strong adherence to SOLID, thin controllers, well-typed exceptions |
| Architecture | 91/100 | Exemplary Clean Architecture with automated enforcement |
| Security | 82/100 | Good posture, needs RBAC and JWT key validation |
| Performance | 87/100 | Well-optimized queries, proper caching, minor index gap |
| Test Coverage | 89/100 | Comprehensive with Testcontainers, some infra unit test gaps |
| **Overall** | **87/100** | **Production-grade microservice with minor improvements needed** |

---

## Security Findings

### CRITICAL

*None identified.*

### HIGH

*None identified.*

### MEDIUM

#### F1. Authorization Policies Mapped to RequireAuthenticatedUser Only
- **Severity:** MEDIUM
- **OWASP:** A01 Broken Access Control
- **Files:** `services/ai-api/src/SaasTemplate.AiApi.Api/DependencyInjection.cs:170-173`
- **Problem:** All authorization policies (`CanCreateWorkItem`, `CanReadWorkItem`, `CanUpdateWorkItem`, `CanDeleteWorkItem`) are mapped to `RequireAuthenticatedUser()` only. There is no role-based or claims-based authorization beyond authentication.
- **Impact:** Any authenticated user has full CRUD access. No RBAC differentiation.
- **Recommendation:** Implement proper role-based authorization policies with claims checks.

#### F2. JWT Secret Minimum Length Not Validated at Startup
- **Severity:** MEDIUM
- **OWASP:** A02 Cryptographic Failures
- **Files:** `services/ai-api/src/SaasTemplate.AiApi.Api/DependencyInjection.cs:123`
- **Problem:** The JWT secret from `Jwt:Secret` configuration is not validated for minimum length. HMAC-SHA256 requires at least 256 bits (32 bytes).
- **Impact:** If deployed with a short/weak secret, JWTs could be brute-forced.
- **Recommendation:** Add startup validation requiring `Jwt:Secret` >= 32 bytes, throwing `InvalidOperationException` if too short.

#### F3. Failed Auth Attempts Not Logged at Controller Level
- **Severity:** MEDIUM
- **OWASP:** A09 Logging Failures
- **Files:** `services/ai-api/src/SaasTemplate.AiApi.Api/Controllers/WorkItemsController.cs:73,81`
- **Problem:** The `Unauthorized()` and `Forbid()` returns do not produce log entries. Only command handlers log forbidden access attempts.
- **Impact:** Security monitoring blind spot for authentication/authorization failures.
- **Recommendation:** Add structured log entries before returning Unauthorized/Forbid responses.

### LOW

#### F4. LIKE Wildcard Characters Not Escaped in Search
- **Severity:** LOW
- **OWASP:** A03 Injection
- **Files:** `services/ai-api/src/SaasTemplate.AiApi.Infrastructure/Persistence/Repositories/WorkItemReadRepository.cs:98`
- **Problem:** The `ILike` search uses `%{search}%` pattern without escaping `%` and `_` wildcard characters in user input. EF Core properly parameterizes the query, but wildcard metacharacters alter matching behavior.
- **Impact:** Users can pass `%` to match everything. Low severity since tenant filters are not bypassed.
- **Recommendation:** Escape `%`, `_`, and `\` in search input before wrapping with wildcards.

#### F5. CSP Header May Need Refinement
- **Severity:** LOW
- **OWASP:** A05 Security Misconfiguration
- **Files:** `services/ai-api/src/SaasTemplate.AiApi.Api/Middleware/SecurityHeadersMiddleware.cs:26`
- **Problem:** CSP is `default-src 'self'`. Correct for an API but may need adjustment if any static content is served.
- **Recommendation:** Document CSP policy and review if error pages or static content are added.

#### F6. Dev Auth Handler Exists (Properly Guarded)
- **Severity:** LOW
- **OWASP:** A01 Broken Access Control
- **Files:** `services/ai-api/src/SaasTemplate.AiApi.Api/Configuration/DevAuthenticationHandler.cs`
- **Problem:** Auto-authenticates all requests in dev/test environments. Protected by environment check at `DependencyInjection.cs:97-100`.
- **Recommendation:** No action needed; properly guarded. Monitor during deployments.

---

## Clean Code

### Architecture Compliance

**Score:** 91/100

Layer dependency verification:
- **Domain.csproj**: Zero project references, zero NuGet packages. PASS.
- **Application.csproj** (lines 17-18): References only Domain. Uses MediatR and FluentValidation (abstractions only). PASS.
- **Infrastructure.csproj** (lines 26-28): References Application (transitively includes Domain). PASS.
- **Api.csproj** (lines 31-33): References Infrastructure (transitively includes Application and Domain). PASS.

Architecture is verified automatically via `ArchitectureTests.cs`.

CQRS pattern strictly followed: separate read/write models, separate repositories, event-driven projection with idempotency tracking.

### Strengths
- Thin controllers delegating all logic to MediatR handlers (`WorkItemsController.cs`)
- Well-typed exception hierarchy: `DomainException` > `NotFoundException`, `ConflictException`, `ForbiddenException`, `TenantMismatchException`
- Production error responses strip internal details (`ExceptionHandlerMiddleware.cs:76-79`)
- FluentValidation at both API and Application layers
- Consistent PascalCase for public members, camelCase for locals, snake_case for DB columns

### Issues
- **CC-1 (LOW):** Repeated user ID extraction pattern in `WorkItemsController.cs:69-74, 178-179, 202-203`. The `User.FindFirst(ClaimTypes.NameIdentifier)` + `Guid.TryParse` pattern is duplicated 3 times.
- **CC-2 (LOW):** `OpenAiService.cs:47` catches generic `Exception` and wraps in another generic `Exception` -- loses specificity.
- **CC-3 (LOW):** Status string-to-enum parsing duplicated in `WorkItemReadRepository.cs:37-43` and `111-118`.

### Dead Code / Duplication
- No dead code or commented-out blocks detected.
- Minor DRY violations listed in Issues above.

---

## Performance

### Database / Data Access
- **N+1 queries:** None detected. `ListAsync` uses a single query with `Skip`/`Take`/`Where`/`OrderBy`. `AsNoTracking()` on all reads.
- **Missing indexes:** Composite index `(org_id, status, created_at_utc DESC)` missing on `work_items_read` table for the `ListAsync` query pattern (`WorkItemReadConfiguration.cs`). Trigram GIN index missing on `title` for `ILike` search (`WorkItemReadRepository.cs:98`).
- **Connection pooling:** Properly configured. EF Core uses Npgsql pooling by default. Redis `ConnectionMultiplexer` is singleton with lazy init. RabbitMQ connection is singleton with double-checked locking.
- **Query optimization:** No issues detected.

### Caching
- Cache-aside pattern in `GetWorkItemByIdQueryHandler.cs:53-71` with configurable TTL.
- Cache invalidation on all mutation events in `WorkItemEventProjector.cs:133-134, 176, 205`.
- Graceful degradation when Redis unavailable.
- `ListWorkItemsQueryHandler` does not use caching (list queries always hit DB).

### Async Patterns
- All I/O operations properly `async`/`await`.
- `CancellationToken` propagated through all handler methods.
- `HealthChecksConfiguration.cs:81` uses `.GetAwaiter().GetResult()` -- constrained by health check library's synchronous factory API.

---

## Test Coverage

### Summary

| Layer | Tests | Gaps |
|-------|-------|------|
| Unit (Domain) | ~10 | None -- ~95% coverage |
| Unit (Application) | ~15 | None -- ~90% coverage |
| Integration (Infrastructure) | ~20 | `WorkItemReadRepository` and `WorkItemEventProjector` lack dedicated unit tests |
| Integration (Api) | ~15 | `SecurityHeadersMiddleware` only tested indirectly |
| Architecture | 5 | None |

### Missing Tests
1. `WorkItemReadRepository` -- No unit tests for fallback-to-write-model logic and status filtering/search (tested only through integration tests).
2. `WorkItemEventProjector` -- No unit tests for create/update/delete projection logic and idempotency checking (integration tested only).
3. `RabbitMqPublisher` -- No unit tests for connection management, double-checked locking, reconnection, and disposal.
4. `WorkItemProjectorService` -- No unit tests for consumer background service, message ACK/NACK logic.
5. `OpenAiService` -- Only has a registration test and optional live API test. No mock-based unit test for error handling paths.

---

## What's Done Well

1. **Clean Architecture is exemplary.** Domain has zero framework dependencies. Dependency direction strictly inward-only. Architecture tests automatically enforce this invariant. (`ArchitectureTests.cs`)
2. **Multi-tenancy is production-grade.** Fail-closed design (`Guid.Empty` on missing claim), EF Core query filters for automatic tenant scoping, cross-tenant injection prevention with `TenantMismatchException`. (`AppDbContext.cs:53-58, 101-106`, `HttpTenantAccessor.cs:31-35`)
3. **Transactional Outbox Pattern.** Domain events atomically committed with entity changes. Outbox dispatcher handles batch processing, failure resilience, at-least-once delivery. (`AppDbContext.cs:71-76`, `OutboxDispatcherService.cs`)
4. **CQRS separation is clean.** Separate read/write models, event-driven projector with idempotency tracking, cache-aside on queries, fallback-to-write-model for eventual consistency. (`WorkItemRead`, `WorkItemEventProjector`)
5. **Error handling is thorough.** Typed exceptions map to HTTP status codes. Production errors strip details. Redis/RabbitMQ failures degrade gracefully. (`ExceptionHandlerMiddleware.cs`)
6. **Security posture is strong.** JWT validation, CORS, rate limiting (100 req/min), security headers (CSP, X-Frame-Options, HSTS). (`SecurityHeadersMiddleware.cs`, `DependencyInjection.cs:46-53`)
7. **Observability built-in.** OpenTelemetry tracing/metrics, structured logging with correlation IDs, health check endpoints. (`CorrelationIdMiddleware.cs`)
8. **Test infrastructure is excellent.** Testcontainers for real Postgres, Redis, RabbitMQ. Shared fixtures minimize container startup cost. (`WorkItemTenantIsolationTests.cs`)

---

## Prioritized Action Plan

> This table is consumed by the `fix service audits` workflow.
> Items are ordered by priority (P0 first) and within priority by severity.

| # | Priority | Severity | Category | Title | Description | Files | Dependencies | Status |
|---|----------|----------|----------|-------|-------------|-------|--------------|--------|
| 1 | P0 | MEDIUM | Security | Implement role-based authorization policies | Replace `RequireAuthenticatedUser` with proper claims/roles-based policies for RBAC | `Api/DependencyInjection.cs:170-173` | None | Deferred — JWT issued by orgs-api does not include `role` claim; revisit when JwtTokenService emits roles |
| 2 | P1 | MEDIUM | Security | Add JWT secret minimum length validation | Validate `Jwt:Secret` >= 32 bytes at startup, throw if too short | `Api/DependencyInjection.cs:123` | None | Done (PR #270) |
| 3 | P1 | MEDIUM | Security | Log failed auth attempts at controller level | Add structured logging before returning Unauthorized/Forbid | `Api/Controllers/WorkItemsController.cs:73,81` | None | Not started |
| 4 | P1 | MEDIUM | Performance | Add composite index for list query | Add `(org_id, status, created_at_utc DESC)` index on `work_items_read` via migration | `Infrastructure/Persistence/Configurations/WorkItemReadConfiguration.cs` | EF migration | Not started |
| 5 | P1 | MEDIUM | Test Coverage | Add WorkItemReadRepository unit tests | Mock `AppDbContext`, test fallback logic, status parsing | New test file | None | Not started |
| 6 | P2 | MEDIUM | Test Coverage | Add WorkItemEventProjector unit tests | Mock `IServiceScopeFactory`, test each projection path | New test file | None | Not started |
| 7 | P2 | LOW | Clean Code | Extract userId extraction helper | Extract `User.FindFirst(ClaimTypes.NameIdentifier)` + `Guid.TryParse` to shared method | `Api/Controllers/ClaimsPrincipalExtensions.cs` | None | Done (PR #270) |
| 8 | P2 | LOW | Clean Code | Use specific exception in OpenAiService | Replace generic `Exception` wrapping with a typed `AiServiceException` | `Infrastructure/Services/OpenAiService.cs:47` | None | Not started |
| 9 | P2 | LOW | Security | Escape LIKE wildcards in search | Escape `%`, `_`, `\` in search input before `ILike` | `Infrastructure/Persistence/Repositories/WorkItemReadRepository.cs:98` | None | Not started |
| 10 | P2 | LOW | Performance | Add trigram GIN index for text search | Add GIN index on `work_items_read.title` for `ILike` optimization | `Infrastructure/Persistence/Configurations/WorkItemReadConfiguration.cs` | EF migration | Not started |
| 11 | P3 | MEDIUM | Test Coverage | Add RabbitMqPublisher unit tests | Test connection management, double-checked locking, disposal | New test file | None | Not started |
| 12 | P3 | MEDIUM | Test Coverage | Add WorkItemProjectorService unit tests | Test consumer logic, ACK/NACK, dead-letter handling | New test file | None | Not started |
| 13 | P3 | LOW | Test Coverage | Add OpenAiService mock-based unit tests | Test error handling paths with mocked OpenAI client | New test file | None | Not started |
| 14 | P3 | LOW | Performance | Consider caching for list queries | Add cache-aside for `ListWorkItems` with invalidation strategy | `Application/WorkItems/Queries/ListWorkItems/ListWorkItemsQueryHandler.cs` | None | Not started |
| 15 | P3 | LOW | Clean Code | Extract status parsing helper | Extract string-to-enum parsing in `WorkItemReadRepository` to shared method | `Infrastructure/Persistence/Repositories/WorkItemReadRepository.cs:37-43, 111-118` | None | Not started |
| 16 | P3 | LOW | Performance | Document sync-over-async health check limitation | Add comment documenting the known `.GetAwaiter().GetResult()` limitation | `Api/Configuration/HealthChecksConfiguration.cs:81` | None | Not started |
| 17 | P3 | LOW | Security | Review CSP header policy | Document and review CSP for any future static content needs | `Api/Middleware/SecurityHeadersMiddleware.cs:26` | None | Not started |
| 18 | P3 | LOW | Security | Monitor OTel EF Core instrumentation for GA | Upgrade from pre-release beta when GA is available | `Infrastructure/SaasTemplate.AiApi.Infrastructure.csproj:23` | None | Not started |

---

## Verification Commands

```bash
# Run after all audit actions are implemented
dotnet build services/ai-api/SaasTemplate.AiApi.sln
dotnet test services/ai-api/SaasTemplate.AiApi.sln
```
