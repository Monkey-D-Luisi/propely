# Task: 0022 - Migration Automation and Improved Health Checks

## Metadata
- ID: 0022
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-04
- Related docs:
  - Epic: `docs/backlog/epic-001-professional-saas-refinement.md`
  - Walkthrough: `docs/walkthroughs/0022-migration-health-checks.md`

## Goal
Auto-apply EF Core migrations on service startup (with distributed locking to prevent race conditions) and add comprehensive health check endpoints that verify all dependencies.

## Context
Currently migrations require manual `dotnet ef database update` commands. In a containerized deployment, services should auto-migrate on startup. Health checks currently exist but need to verify all dependencies (DB, Redis, RabbitMQ) and distinguish between liveness and readiness.

## Scope
### In scope
- Auto-apply migrations on startup for both .NET services
- Distributed lock via PostgreSQL advisory locks to prevent concurrent migrations
- Liveness endpoint: `/health/live` - service is running
- Readiness endpoint: `/health/ready` - service can handle requests (all dependencies up)
- Health checks for: PostgreSQL, Redis, RabbitMQ
- Configure Docker Compose health checks to use these endpoints

### Out of scope
- Migration rollback automation
- Health check dashboard UI
- Custom health check for email service

## Requirements
- R1: Migrations apply automatically on first startup
- R2: Concurrent service instances don't conflict during migration
- R3: `/health/live` returns 200 if process is running
- R4: `/health/ready` returns 200 only when DB + Redis + RabbitMQ are reachable
- R5: Health check response includes individual dependency status

## Acceptance Criteria
- AC1: Starting a service with pending migrations applies them automatically
- AC2: `/health/live` returns 200
- AC3: `/health/ready` returns 200 when all dependencies are up
- AC4: `/health/ready` returns 503 when any dependency is down
- AC5: `dotnet build` succeeds for both services
- AC6: `dotnet test` passes for both services

## Constraints (non-negotiable)
- Clean Architecture layers respected.
- English-only repo content.
- No secrets in repo.
- Update walkthrough.

## Implementation Steps
1. Add migration startup code in Program.cs (both services):
   ```csharp
   using var scope = app.Services.CreateScope();
   var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
   await db.Database.MigrateAsync();
   ```
2. Add advisory lock wrapper for migration
3. Add AspNetCore.HealthChecks.NpgSql, .Redis, .RabbitMQ packages
4. Configure health checks in DependencyInjection.cs
5. Map `/health/live` and `/health/ready` endpoints
6. Update docker-compose health checks
7. Test

## Files to Create / Modify
- `Api/Program.cs` (modify - both services)
- `Api/DependencyInjection.cs` (modify - health checks)
- `docker-compose.yml` (modify - health check config)
- `docker-compose.production.yml` (modify - health check config)

## Testing Plan
- Integration tests: Verify health endpoints return correct status
- Manual verification: Start services, verify migration runs, check health endpoints

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
