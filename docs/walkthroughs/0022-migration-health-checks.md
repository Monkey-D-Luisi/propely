# Walkthrough: 0022-migration-health-checks

## Task Reference
- Task: `docs/tasks/0022-migration-health-checks.md`
- Walkthrough: `docs/walkthroughs/0022-migration-health-checks.md`
- Branch/PR: `feat/0022-migration-health-checks`
- Date: `2026-02-07`

## Summary
Added automatic EF Core migration on startup for both .NET services, using PostgreSQL advisory locks to prevent concurrent instances from conflicting. Health check endpoints (`/health/live` and `/health/ready`) were already implemented with PostgreSQL, Redis, and RabbitMQ dependency checks, so no changes were needed there.

## Context
- Background: Services required manual `dotnet ef database update` to apply migrations. In containerized deployments, services should self-migrate.
- Problem statement: Multiple service replicas starting simultaneously could attempt migrations concurrently, causing race conditions.
- Constraints: Must not interfere with test environments (tests use `EnsureCreatedAsync`). Must use existing Npgsql dependency (no new packages).

## Decisions & Trade-offs
- **PostgreSQL advisory locks over distributed lock libraries:**
  - Options considered: Npgsql advisory locks, Redis-based lock (RedLock), separate migration init container
  - Why this choice: Advisory locks use the same PostgreSQL connection — zero new dependencies. Each service already connects to its own database, so the lock naturally scopes per-service.
  - Consequences / risks: Lock is database-scoped, not cluster-scoped. If services share a database (they don't), they'd need different lock IDs. This is already handled with unique IDs per service (200001 for ai-api, 200002 for orgs-api).

- **Skip migration in Testing environment:**
  - Tests use `EnsureCreatedAsync()` which creates the schema directly from the EF Core model, bypassing migrations entirely. This is faster and avoids test-ordering dependencies on migration files.
  - The `ApplyMigrationsAsync()` extension checks `app.Environment.EnvironmentName == "Testing"` and returns early.

- **Health check endpoints unchanged:**
  - `/health/live` and `/health/ready` were already fully implemented with PostgreSQL, Redis, and RabbitMQ dependency checks. Docker Compose already uses these endpoints. No modifications needed.

## Implementation Notes
- Key changes: New `DatabaseMigrationConfiguration.cs` in each service's Api/Configuration folder with `ApplyMigrationsAsync()` extension method on `WebApplication`.
- Edge cases handled: Double-check after acquiring lock (another instance may have applied migrations while waiting), early return if no pending migrations, advisory lock cleanup in `finally` block.
- Known limitations: If PostgreSQL is unreachable at startup, migration will fail and crash the service. This is intentional — the service can't operate without its database anyway, and Docker's restart policy will retry.

## Commands Run
```bash
dotnet build services/ai-api/SaasTemplate.AiApi.sln --configuration Release
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln --configuration Release
dotnet test services/ai-api/SaasTemplate.AiApi.sln --configuration Release --no-build
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln --configuration Release --no-build
```

## Files Changed
- `services/ai-api/src/SaasTemplate.AiApi.Api/Configuration/DatabaseMigrationConfiguration.cs` — New file: auto-migration with advisory lock
- `services/ai-api/src/SaasTemplate.AiApi.Api/Program.cs` — Added `await app.ApplyMigrationsAsync()` after `builder.Build()`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Configuration/DatabaseMigrationConfiguration.cs` — New file: auto-migration with advisory lock
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Program.cs` — Added `await app.ApplyMigrationsAsync()` after `builder.Build()`
- `services/ai-api/tests/SaasTemplate.AiApi.IntegrationTests/Persistence/DatabaseMigrationTests.cs` — New: 3 integration tests
- `services/orgs-api/tests/SaasTemplate.OrgsApi.IntegrationTests/Persistence/DatabaseMigrationTests.cs` — New: 3 integration tests

## Tests
### Integration
- **DatabaseMigrationTests** (3 tests per service, 6 total):
  - `MigrateAsync_AppliesAllMigrations_ToFreshDatabase` — Verifies all migration files apply cleanly to an empty Testcontainers PostgreSQL database
  - `MigrateAsync_IsIdempotent_WhenRunTwice` — Verifies running migrations twice doesn't fail or duplicate entries
  - `AdvisoryLock_PreventsConcurrentMigrations` — Verifies `pg_advisory_lock` blocks concurrent acquisition and releases correctly
- How to run: `dotnet test services/ai-api/SaasTemplate.AiApi.sln` / `dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln`

### Results
- ai-api: 140 passed (76 unit + 5 architecture + 59 integration)
- orgs-api: 149 passed (90 unit + 5 architecture + 54 integration)

## Observability
- Logs added: Migration status logging via `ILogger<AppDbContext>` — logs pending migration count, names, lock acquisition/release, and whether migrations were applied or already handled by another instance.

## Security
- No secrets committed. Connection string obtained from EF Core context at runtime.
- Advisory lock IDs are hardcoded constants (not sensitive).

## Checklist
- [x] Task scope matches `docs/tasks/0022-migration-health-checks.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed
