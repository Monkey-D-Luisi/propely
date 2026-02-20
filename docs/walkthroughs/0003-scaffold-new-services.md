# Walkthrough: 0003-scaffold-new-services

## Task Reference
- Task: `docs/tasks/0003-scaffold-new-services.md`
- Walkthrough: `docs/walkthroughs/0003-scaffold-new-services.md`
- Branch/PR: `feat/0003-scaffold-new-services` / TBD
- Date: `2026-02-20`

## Summary
Scaffolded 4 new .NET 10 microservice solutions (properties-api, publishing-api, contacts-api, appointments-api) following the Clean Architecture + CQRS pattern established by ai-api. Each service has a complete skeleton with health endpoints, OpenTelemetry, tenant-scoped infrastructure, and architecture tests.

## Context
- Background: Propely requires 6 backend microservices. orgs-api and ai-api already exist. 4 new services need to be created.
- Problem statement: Need compilable empty shells for properties, publishing, contacts, and appointments services.
- Constraints: Must follow existing patterns (ai-api for tenant-scoped services), no domain logic yet.

## Decisions & Trade-offs
- **Decision:** Follow ai-api pattern (not orgs-api)
  - Options considered: orgs-api pattern (auth provider), ai-api pattern (tenant consumer)
  - Why this choice: New services are tenant-scoped consumers, not the auth provider. ai-api has the correct middleware (OrgContextMiddleware, ITenantAccessor) and query filter patterns.
  - Consequences / risks: None, this is the documented pattern.

- **Decision:** Create properties-api first, then clone for others
  - Options considered: Write each service independently, clone approach
  - Why this choice: All 4 services share identical skeleton structure. Clone + find-replace is faster and ensures consistency.
  - Consequences / risks: Need to verify all replacements are correct.

## Implementation Notes
- Key changes: 4 new service directories under `services/`
- Edge cases handled: Empty shells with no domain entities still have functional outbox/messaging infrastructure ready for future use
- Known limitations: No EF Core migrations yet (empty DbContext with only outbox/processed-event tables)

## Data / Schema / Migrations
- DB changes: None yet (empty DbContexts, no migrations)
- Migration strategy: Will be created when domain entities are added
- Backward compatibility: N/A (new services)

## Commands Run
```bash
# Create properties-api template with all Clean Architecture layers (50+ files)
# Clone to publishing-api, contacts-api, appointments-api via cp -r + find-replace
# Create .sln files (classic format via --format sln)
dotnet new sln --name Propely.PropertiesApi --format sln
dotnet new sln --name Propely.PublishingApi --format sln
dotnet new sln --name Propely.ContactsApi --format sln
dotnet new sln --name Propely.AppointmentsApi --format sln

# Add projects to solutions (src + tests folders)
dotnet sln add --solution-folder src src/Propely.*.Domain/*.csproj ...
dotnet sln add --solution-folder tests tests/Propely.*.UnitTests/*.csproj ...

# Build all 4 services (0 errors each)
dotnet build services/properties-api/Propely.PropertiesApi.sln
dotnet build services/publishing-api/Propely.PublishingApi.sln
dotnet build services/contacts-api/Propely.ContactsApi.sln
dotnet build services/appointments-api/Propely.AppointmentsApi.sln

# Test all 4 services (6 tests each: 5 architecture + 1 integration)
dotnet test services/properties-api/Propely.PropertiesApi.sln
dotnet test services/publishing-api/Propely.PublishingApi.sln
dotnet test services/contacts-api/Propely.ContactsApi.sln
dotnet test services/appointments-api/Propely.AppointmentsApi.sln

# Verify no regressions in existing services
dotnet build services/orgs-api/Propely.OrgsApi.sln
dotnet build services/ai-api/Propely.AiApi.sln
```

## Files Changed
### Per service (properties-api, publishing-api, contacts-api, appointments-api):
- `src/Propely.<Service>.Domain/` - Entity base, IDomainEvent, ISoftDeletable, 6 exception classes
- `src/Propely.<Service>.Application/` - DependencyInjection, ValidationBehaviour, ITenantAccessor, IUnitOfWork, ICacheService, IMessagePublisher, IOutboxRepository, OutboxMessage, PagedResult
- `src/Propely.<Service>.Infrastructure/` - DependencyInjection, AppDbContext, UnitOfWork, DesignTimeDbContextFactory, OutboxRepository, RabbitMqPublisher, OutboxDispatcherService, RedisCacheService, configurations
- `src/Propely.<Service>.Api/` - Program.cs, DependencyInjection, appsettings (3 envs), middleware (4), services (2), configuration (4)
- `tests/Propely.<Service>.UnitTests/` - Empty shell .csproj
- `tests/Propely.<Service>.IntegrationTests/` - TestWebApplicationFactory, HealthEndpointTests
- `tests/Propely.<Service>.ArchitectureTests/` - 5 architecture tests
- `Dockerfile`, `Dockerfile.dev`, `.dockerignore`, `scripts/scaffold-module.ps1`
- `Propely.<Service>.sln` - Solution file with src/ and tests/ folders

### Other files:
- `docs/tasks/0003-scaffold-new-services.md` - Task doc (status → DONE)
- `docs/walkthroughs/0003-scaffold-new-services.md` - This walkthrough
- `docs/backlog/epic-P0-foundation.md` - Task 0.3 status → DONE

## Tests
### Unit
- Architecture tests (5 per service): layer dependency enforcement, handler naming, domain event sealing
- How to run: `dotnet test services/<service>/Propely.<Service>.sln`

### Integration
- Health endpoint test (1 per service): verifies `/health/live` returns 200
- How to run: `dotnet test services/<service>/Propely.<Service>.sln`

### Manual
- `dotnet build` each solution
- `dotnet test` each solution

## Observability
- Logs added/updated: Standard logging configuration per service
- Traces/metrics added/updated: OpenTelemetry setup per service

## Security
- Validation: DevAuthenticationHandler guarded by environment check
- AuthN/AuthZ impact: JWT Bearer + Dev scheme (same as ai-api)
- Sensitive data handling: No secrets in config files

## Follow-ups / Backlog
- [ ] Task 0.5: Docker Compose updates for new services
- [ ] Task 0.6: CI pipeline updates for new services

## Checklist
- [x] Task scope matches `docs/tasks/0003-scaffold-new-services.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
