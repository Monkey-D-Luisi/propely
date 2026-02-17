# Walkthrough: 0004-application-layer

## Task Reference
- Task: `docs/tasks/0004-application-layer.md`
- Walkthrough: `docs/walkthroughs/0004-application-layer.md`
- Branch/PR: `main`
- Date: `2026-01-28`

## Summary
Implemented the Application layer with CQRS pattern for WorkItem management. Created repository interfaces (`IWorkItemRepository`, `IOutboxRepository`), transaction management (`IUnitOfWork`), the `OutboxMessage` model, commands (`CreateWorkItemCommand` with handler), queries (`GetWorkItemByIdQuery` with handler), and DTOs (`WorkItemDto`, `CreateWorkItemResult`). All unit tests pass with mocked repositories.

## Context
- Background: Task 0003 completed the Domain layer with WorkItem entity, events, and validation
- Problem statement: Need a use case layer to orchestrate domain operations and define contracts for infrastructure
- Constraints: Application layer must only depend on Domain layer (Clean Architecture)

## Decisions & Trade-offs
- **Decision:** Use MediatR for CQRS implementation
  - Options considered: MediatR pipeline, plain handler classes
  - Why this choice: MediatR provides clean separation, pipeline behaviors support, and standard pattern
  - Consequences / risks: Additional dependency, but enables cross-cutting concerns via behaviors

- **Decision:** OutboxMessage as a model in Application layer
  - Options considered: Put in Domain, put in Application, put in Infrastructure
  - Why this choice: Application layer owns the contract, Infrastructure implements persistence
  - Consequences / risks: OutboxMessage has domain-like behavior (factory method) but is not a domain entity

- **Decision:** JSON serialization in handler using System.Text.Json
  - Options considered: Let Infrastructure serialize, serialize in handler
  - Why this choice: Application layer decides event format, keeping serialization logic close to the handler
  - Consequences / risks: Minor coupling to System.Text.Json (part of .NET BCL)

## Implementation Notes
- Key changes:
  - Created `SaasTemplate.AiApi.Application` project with reference to Domain and MediatR
  - Created interfaces for repository pattern and unit of work
  - Implemented command/query handlers using MediatR `IRequestHandler<TRequest, TResponse>`
  - Commands/queries implement `IRequest<TResponse>` for type-safe dispatch
  - Added NSubstitute for mocking in unit tests

- Edge cases handled:
  - WorkItem with null description maps correctly to DTO
  - CancellationToken is propagated through all async operations
  - Domain validation exceptions bubble up from domain layer

- Known limitations:
  - No retry logic in handlers (will be added in Infrastructure)
  - No logging in handlers (observability in Task 0010)

## Data / Schema / Migrations
- No database changes in this task (Application layer only defines interfaces)

## Commands Run
```bash
# Add Application project to solution
dotnet sln add "src\SaasTemplate.AiApi.Application\SaasTemplate.AiApi.Application.csproj"

# Build solution
dotnet build

# Run all tests
dotnet test --verbosity normal
```

## Files Changed
- `src/SaasTemplate.AiApi.Application/SaasTemplate.AiApi.Application.csproj` — New project file with reference to Domain
- `src/SaasTemplate.AiApi.Application/Common/Interfaces/IOutboxRepository.cs` — Interface for outbox persistence
- `src/SaasTemplate.AiApi.Application/Common/Interfaces/IUnitOfWork.cs` — Interface for transaction management
- `src/SaasTemplate.AiApi.Application/Common/Models/OutboxMessage.cs` — Model for outbox message with factory method
- `src/SaasTemplate.AiApi.Application/WorkItems/Interfaces/IWorkItemRepository.cs` — Interface for WorkItem persistence
- `src/SaasTemplate.AiApi.Application/WorkItems/Commands/CreateWorkItemCommand.cs` — Command and result records
- `src/SaasTemplate.AiApi.Application/WorkItems/Commands/CreateWorkItemCommandHandler.cs` — Handler that creates entity, adds to outbox, saves changes
- `src/SaasTemplate.AiApi.Application/WorkItems/Queries/GetWorkItemByIdQuery.cs` — Query record
- `src/SaasTemplate.AiApi.Application/WorkItems/Queries/GetWorkItemByIdQueryHandler.cs` — Handler that retrieves and maps to DTO
- `src/SaasTemplate.AiApi.Application/WorkItems/Dtos/WorkItemDto.cs` — DTO record for read operations
- `tests/SaasTemplate.AiApi.UnitTests/SaasTemplate.AiApi.UnitTests.csproj` — Added NSubstitute and Application reference
- `tests/SaasTemplate.AiApi.UnitTests/Application/WorkItems/CreateWorkItemCommandHandlerTests.cs` — 10 tests for command handler
- `tests/SaasTemplate.AiApi.UnitTests/Application/WorkItems/GetWorkItemByIdQueryHandlerTests.cs` — 6 tests for query handler
- `docs/tasks/0004-application-layer.md` — Task documentation
- `docs/walkthroughs/0004-application-layer.md` — This walkthrough
- `docs/backlog/work-item-management-epic.md` — Updated task status

## Tests
### Unit
- **CreateWorkItemCommandHandlerTests (10 tests)**:
  - Valid command creates work item
  - Valid command persists work item to repository
  - Valid command adds outbox message
  - Valid command saves changes via unit of work
  - Correlation ID is included in outbox message
  - Empty title throws validation exception
  - Title exceeding max length throws validation exception
  - Cancellation token is passed to repositories
  - Returned ID matches persisted work item ID

- **GetWorkItemByIdQueryHandlerTests (6 tests)**:
  - Existing work item returns DTO
  - Non-existent work item returns null
  - Repository called with correct ID
  - Cancellation token is passed to repository
  - All properties mapped to DTO
  - Work item with no description returns DTO with null description

- How to run: `dotnet test`

### Integration
- N/A for this task (no infrastructure)

### Manual
- N/A

## Observability
- N/A for Application layer (deferred to Task 0010)

## Security
- Validation: Domain-level validation through WorkItem.Create()
- AuthN/AuthZ impact: None at this layer (Presentation layer responsibility)
- Sensitive data handling: No PII in this implementation

## Performance
- Hot paths impacted: None yet (infrastructure not implemented)
- Any profiling/bench notes: N/A

## Docs Updated
- Created task documentation
- Created walkthrough

## Rollback Plan
- Revert commit and remove Application project if issues arise
- Delete test files added to UnitTests project

## Follow-ups / Backlog
- [ ] Task 0005 will implement Infrastructure layer (repositories, EF Core, outbox persistence)
- [ ] Consider adding MediatR pipeline behaviors for logging/validation
- [ ] Add logging/telemetry in handlers when implementing Task 0010

## Checklist
- [x] Task scope matches `docs/tasks/0004-application-layer.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
