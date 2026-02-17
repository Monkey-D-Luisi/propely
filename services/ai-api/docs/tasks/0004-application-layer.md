# Task: 0004-application-layer

## Metadata
- ID: 0004
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-01-28
- Related docs:
  - Walkthrough: `docs/walkthroughs/0004-application-layer.md`
  - Epic: `docs/backlog/work-item-management-epic.md`

## Goal
Define use cases and interfaces in the Application layer using the CQRS pattern for WorkItem management.

## Context
Task 0003 established the Domain layer with the WorkItem entity, WorkItemStatus enum, WorkItemCreatedV1 event, and validation exceptions. The Application layer now needs to define the use cases (commands and queries) and the interfaces that will be implemented by the Infrastructure layer.

## Scope
### In scope
- IWorkItemRepository interface
- IOutboxRepository interface
- IUnitOfWork interface
- CreateWorkItemCommand and handler
- GetWorkItemByIdQuery and handler
- DTOs (WorkItemDto, CreateWorkItemResult)
- Unit tests with mocked repositories

### Out of scope
- Infrastructure implementations (Task 0005)
- API endpoints (Task 0006)
- Update/delete commands
- List/filter queries

## Requirements
- R1: Create `IWorkItemRepository` with `AddAsync` and `GetByIdAsync` methods
- R2: Create `IOutboxRepository` with `AddAsync` method
- R3: Create `IUnitOfWork` interface for transaction management
- R4: Create `CreateWorkItemCommand` with Title and Description properties
- R5: Create `CreateWorkItemCommandHandler` that creates entity, persists via repository, adds event to outbox
- R6: Create `GetWorkItemByIdQuery` with Id parameter
- R7: Create `GetWorkItemByIdQueryHandler` that returns WorkItemDto or null
- R8: All methods must use CancellationToken
- R9: Query handler returns DTO, not domain entity

## Acceptance Criteria
- AC1: Interfaces defined without infrastructure dependencies
- AC2: Command handler creates entity and outbox message in a single unit of work
- AC3: Query handler returns DTO, not entity
- AC4: All methods use CancellationToken
- AC5: Unit tests with mocked repositories achieve >90% coverage

## Constraints (non-negotiable)
- Clean Architecture layers respected (Application only depends on Domain)
- English-only repo content
- No secrets in repo
- Update walkthrough

## Proposed Approach (high-level)
1. Create interface files in appropriate folders
2. Create command with its handler
3. Create query with its handler
4. Create DTO classes
5. Create OutboxMessage model in Application layer
6. Write unit tests with mocked repositories

## Implementation Steps
1. Create folder structure: `WorkItems/Interfaces`, `WorkItems/Commands`, `WorkItems/Queries`, `WorkItems/Dtos`, `Common/Interfaces`
2. Create `IWorkItemRepository.cs`
3. Create `IOutboxRepository.cs` and `OutboxMessage.cs` model
4. Create `IUnitOfWork.cs`
5. Create `CreateWorkItemCommand.cs`
6. Create `CreateWorkItemCommandHandler.cs`
7. Create `GetWorkItemByIdQuery.cs`
8. Create `GetWorkItemByIdQueryHandler.cs`
9. Create `WorkItemDto.cs`
10. Create unit tests for command handler
11. Create unit tests for query handler
12. Run build and tests

## Files to Create / Modify
- `src/SaasTemplate.AiApi.Application/SaasTemplate.AiApi.Application.csproj` (create)
- `src/SaasTemplate.AiApi.Application/WorkItems/Interfaces/IWorkItemRepository.cs` (create)
- `src/SaasTemplate.AiApi.Application/Common/Interfaces/IOutboxRepository.cs` (create)
- `src/SaasTemplate.AiApi.Application/Common/Interfaces/IUnitOfWork.cs` (create)
- `src/SaasTemplate.AiApi.Application/Common/Models/OutboxMessage.cs` (create)
- `src/SaasTemplate.AiApi.Application/WorkItems/Commands/CreateWorkItemCommand.cs` (create)
- `src/SaasTemplate.AiApi.Application/WorkItems/Commands/CreateWorkItemCommandHandler.cs` (create)
- `src/SaasTemplate.AiApi.Application/WorkItems/Queries/GetWorkItemByIdQuery.cs` (create)
- `src/SaasTemplate.AiApi.Application/WorkItems/Queries/GetWorkItemByIdQueryHandler.cs` (create)
- `src/SaasTemplate.AiApi.Application/WorkItems/Dtos/WorkItemDto.cs` (create)
- `tests/SaasTemplate.AiApi.UnitTests/Application/WorkItems/CreateWorkItemCommandHandlerTests.cs` (create)
- `tests/SaasTemplate.AiApi.UnitTests/Application/WorkItems/GetWorkItemByIdQueryHandlerTests.cs` (create)

## Testing Plan
- Unit tests: CreateWorkItemCommandHandler with mock repository (valid input, validation failure)
- Unit tests: GetWorkItemByIdQueryHandler with mock repository (found, not found)
- Unit tests: Verify outbox message is created on command execution

## Security & Privacy
- No direct security concerns at Application layer
- Input validation deferred to Presentation layer (FluentValidation)
- Domain validation handles business rules

## Observability
- Logs: Not added at this layer (deferred to Infrastructure)
- Metrics: Not added at this layer
- Traces: Not added at this layer

## Rollback Plan
Revert commit and remove created files if issues arise.

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
