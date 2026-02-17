# Walkthrough: 0005-infrastructure-layer

## Task Reference
- Task: `docs/tasks/0005-infrastructure-layer.md`
- Walkthrough: `docs/walkthroughs/0005-infrastructure-layer.md`
- Branch/PR: `main`
- Date: `2026-01-28`

## Summary
Implemented the Infrastructure layer with EF Core and PostgreSQL persistence. Key feature: centralized domain event dispatch in `SaveChangesAsync` which automatically adds domain events to the outbox, simplifying command handlers.

## Context
- Background: Task 0004 defined interfaces, this task implements them
- Problem statement: Need persistence layer with automatic domain event handling
- Constraints: snake_case DB naming, Testcontainers for integration tests

## Decisions & Trade-offs
- **Decision:** Centralize domain event dispatch in `SaveChangesAsync`
  - Options considered: Manual dispatch in handlers, centralized in DbContext
  - Why this choice: DRY, impossible to forget, cleaner handlers
  - Consequences: Domain events always dispatched on save (desired behavior)

- **Decision:** Use `EnsureCreatedAsync` instead of migrations for tests
  - Options considered: Apply migrations, use EnsureCreated
  - Why this choice: Simpler for integration tests, migrations not yet generated
  - Consequences: Schema created from model, not migrations

- **Decision:** EF Core 10.0.0 (not 10.0.1) for Npgsql compatibility
  - Npgsql.EntityFrameworkCore.PostgreSQL 10.0.0 requires EF Core 10.0.0

## Implementation Notes
- Key changes:
  - Created `SaasTemplate.AiApi.Infrastructure` project with EF Core and Npgsql
  - `AppDbContext.SaveChangesAsync` collects domain events from tracked entities and adds to outbox
  - Entity configurations use snake_case naming and JSONB for payload
  - Simplified `CreateWorkItemCommandHandler` - removed manual outbox logic
  - Created integration tests with Testcontainers for PostgreSQL

- Edge cases handled:
  - Entities without domain events are skipped
  - Domain events cleared only after successful save (prevents data loss on failure)

## Data / Schema / Migrations
- `work_items` table: id, title, description, status, created_at_utc, updated_at_utc, version
- `outbox_messages` table: id, event_type, payload (JSONB), occurred_at_utc, processed_at_utc, correlation_id, causation_id
- Index on status for work_items
- Partial index on unprocessed outbox messages

## Commands Run
```bash
# Add Infrastructure project to solution
dotnet sln add "src\SaasTemplate.AiApi.Infrastructure\SaasTemplate.AiApi.Infrastructure.csproj"

# Add Integration Tests project to solution
dotnet sln add "tests\SaasTemplate.AiApi.IntegrationTests\SaasTemplate.AiApi.IntegrationTests.csproj"

# Build and test
dotnet build
dotnet test
```

## Files Changed
- `src/SaasTemplate.AiApi.Infrastructure/SaasTemplate.AiApi.Infrastructure.csproj` — New project with EF Core and Npgsql
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/AppDbContext.cs` — DbContext with domain event dispatch
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Configurations/WorkItemConfiguration.cs` — snake_case mapping
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Configurations/OutboxMessageConfiguration.cs` — JSONB mapping
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Repositories/WorkItemRepository.cs` — IWorkItemRepository implementation
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Repositories/OutboxRepository.cs` — IOutboxRepository implementation
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/UnitOfWork.cs` — IUnitOfWork implementation
- `src/SaasTemplate.AiApi.Application/WorkItems/Commands/CreateWorkItemCommandHandler.cs` — Simplified (removed outbox logic)
- `tests/SaasTemplate.AiApi.IntegrationTests/SaasTemplate.AiApi.IntegrationTests.csproj` — New test project
- `tests/SaasTemplate.AiApi.IntegrationTests/Fixtures/PostgresFixture.cs` — Testcontainers setup
- `tests/SaasTemplate.AiApi.IntegrationTests/Persistence/WorkItemRepositoryTests.cs` — 6 integration tests
- `tests/SaasTemplate.AiApi.UnitTests/Application/WorkItems/CreateWorkItemCommandHandlerTests.cs` — Updated for simplified handler

## Tests
### Unit (37 tests)
- Domain tests (22)
- Application handler tests (15) - updated for simplified handler

### Integration (6 tests)
- `AddAsync_ShouldPersistWorkItem`
- `GetByIdAsync_WhenExists_ShouldReturnWorkItem`
- `GetByIdAsync_WhenNotExists_ShouldReturnNull`
- `SaveChangesAsync_ShouldDispatchDomainEventsToOutbox`
- `WorkItemTable_ShouldUseSnakeCaseNaming`
- `OutboxTable_ShouldUseSnakeCaseNaming`

### Manual
- N/A

## Observability
- Deferred to Task 0010

## Security
- Connection string from environment variable (not hardcoded)
- Parameterized queries via EF Core

## Performance
- Partial index for unprocessed outbox messages (efficient polling)

## Docs Updated
- Task documentation
- Walkthrough
- Epic status

## Rollback Plan
- Revert commit if issues
- Drop tables if needed

## Follow-ups / Backlog
- [ ] Task 0006: Presentation Layer (API endpoints)
- [ ] Generate actual EF Core migrations for production
- [ ] Consider adding retry logic for transient DB failures

## Checklist
- [x] Task scope matches `docs/tasks/0005-infrastructure-layer.md`
- [x] Tests updated and passing (43 total)
- [x] Docs updated where relevant
- [x] No secrets committed
