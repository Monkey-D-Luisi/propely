# Task: 0005-infrastructure-layer

## Metadata
- ID: 0005
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-01-28
- Related docs:
  - Walkthrough: `docs/walkthroughs/0005-infrastructure-layer.md`
  - Epic: `docs/backlog/work-item-management-epic.md`
  - Architecture: `docs/architecture/vertical-slice.md`

## Goal
Implement persistence using EF Core with PostgreSQL, including repositories, unit of work, and centralized domain event dispatch.

## Context
Task 0004 established the Application layer with interfaces (IWorkItemRepository, IOutboxRepository, IUnitOfWork) and handlers. Now the Infrastructure layer must implement these interfaces using EF Core and PostgreSQL.

**Key architectural decision:** Domain events will be dispatched centrally in `SaveChangesAsync` rather than manually in each handler. This simplifies handlers and prevents forgetting to process events.

## Scope
### In scope
- AppDbContext with DbSets
- Entity configurations (snake_case naming)
- WorkItemRepository and OutboxRepository implementations
- UnitOfWork with centralized domain event dispatch
- EF Core migrations for work_items and outbox_messages tables
- Integration tests with Testcontainers

### Out of scope
- API endpoints (Task 0006)
- Outbox dispatcher service (Task 0007)
- Read model table (Task 0008)

## Requirements
- R1: Create AppDbContext with DbSet<WorkItem> and DbSet<OutboxMessage>
- R2: Configure snake_case naming for all database identifiers
- R3: Implement WorkItemRepository with AddAsync and GetByIdAsync
- R4: Implement OutboxRepository with AddAsync
- R5: Implement UnitOfWork that dispatches domain events to outbox on SaveChangesAsync
- R6: Create initial migration for work_items table
- R7: Create migration for outbox_messages table
- R8: Configure connection string from environment variable
- R9: Simplify CreateWorkItemCommandHandler by removing manual outbox logic

## Acceptance Criteria
- AC1: Database schema matches vertical-slice.md exactly
- AC2: snake_case naming for all DB identifiers
- AC3: Migrations can be applied to empty database
- AC4: Integration tests with Testcontainers (Postgres)
- AC5: Repository correctly persists and retrieves WorkItem
- AC6: Domain events automatically added to outbox on SaveChanges

## Constraints (non-negotiable)
- Clean Architecture layers respected
- English-only repo content
- No secrets in repo
- Update walkthrough

## Proposed Approach (high-level)
1. Create Infrastructure project with EF Core packages
2. Create entity configurations with snake_case naming
3. Implement AppDbContext with domain event dispatch in SaveChangesAsync
4. Implement repositories
5. Create migrations
6. Simplify CreateWorkItemCommandHandler
7. Create integration tests with Testcontainers
8. Verify all tests pass

## Implementation Steps
1. Create SaasTemplate.AiApi.Infrastructure project
2. Add EF Core and Npgsql packages
3. Create WorkItemConfiguration
4. Create OutboxMessageConfiguration
5. Create AppDbContext with SaveChangesAsync override
6. Create WorkItemRepository
7. Create OutboxRepository
8. Create UnitOfWork
9. Create initial migration
10. Refactor CreateWorkItemCommandHandler to remove manual outbox logic
11. Update unit tests for simplified handler
12. Create integration test project
13. Add Testcontainers package
14. Create WorkItemRepositoryTests
15. Run all tests

## Files to Create / Modify
- `src/SaasTemplate.AiApi.Infrastructure/SaasTemplate.AiApi.Infrastructure.csproj` (create)
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/AppDbContext.cs` (create)
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Configurations/WorkItemConfiguration.cs` (create)
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Configurations/OutboxMessageConfiguration.cs` (create)
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Repositories/WorkItemRepository.cs` (create)
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Repositories/OutboxRepository.cs` (create)
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/UnitOfWork.cs` (create)
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Migrations/*.cs` (create)
- `src/SaasTemplate.AiApi.Application/WorkItems/Commands/CreateWorkItemCommandHandler.cs` (modify - simplify)
- `tests/SaasTemplate.AiApi.IntegrationTests/SaasTemplate.AiApi.IntegrationTests.csproj` (create)
- `tests/SaasTemplate.AiApi.IntegrationTests/Persistence/WorkItemRepositoryTests.cs` (create)
- `tests/SaasTemplate.AiApi.UnitTests/Application/WorkItems/CreateWorkItemCommandHandlerTests.cs` (modify)

## Testing Plan
- Integration tests: Repository CRUD with real Postgres (Testcontainers)
- Integration tests: Verify snake_case column names
- Integration tests: Outbox message persistence via domain events
- Unit tests: Verify simplified handler behavior

## Security & Privacy
- Connection string from environment variable (no hardcoded secrets)
- Parameterized queries via EF Core

## Observability
- Deferred to Task 0010

## Rollback Plan
Revert commit and drop migrations if issues arise.

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
