# Task: 0003-workitem-domain-model

## Metadata
- ID: 0003
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-01-28
- Related docs:
  - Walkthrough: `docs/walkthroughs/0003-workitem-domain-model.md`
  - Epic: `docs/backlog/work-item-management-epic.md`
  - Architecture: `docs/architecture/vertical-slice.md`

## Goal
Define the WorkItem entity and related domain types in the Domain layer, following Clean Architecture principles.

## Context
This is the first code implementation task. We need to establish the .NET solution structure and implement the core domain model that will be used throughout the vertical slice.

## Scope
### In scope
- .NET solution and project structure
- WorkItem entity with validation
- WorkItemStatus enum
- WorkItemCreatedV1 domain event
- Domain exceptions
- Unit tests

### Out of scope
- Application layer (commands, queries)
- Infrastructure (persistence, messaging)
- API endpoints

## Requirements
- R1: WorkItem entity with Id, Title, Description, Status, CreatedAtUtc, UpdatedAtUtc, Version
- R2: Title validation: required, 1-200 characters
- R3: Description validation: optional, max 2000 characters
- R4: WorkItemStatus enum with Draft, Active, Completed values
- R5: WorkItemCreatedV1 event following schema from vertical-slice.md
- R6: Domain layer has zero external framework dependencies
- R7: Domain events raised when entity state changes

## Acceptance Criteria
- AC1: WorkItem entity enforces invariants (Title validation)
- AC2: WorkItemStatus enum has exactly 3 values
- AC3: WorkItemCreatedV1 follows event schema from vertical-slice.md
- AC4: Domain layer has zero framework dependencies (no EF, no ASP.NET)
- AC5: Unit tests cover validation logic with >90% coverage

## Constraints (non-negotiable)
- Clean Architecture layers respected
- English-only repo content
- No secrets in repo
- Update walkthrough

## Implementation Steps
1. Create .NET solution and project structure
2. Create WorkItemStatus enum
3. Create base domain event types
4. Create WorkItemCreatedV1 event
5. Create WorkItemValidationException
6. Create WorkItem entity with validation
7. Create unit test project
8. Write comprehensive unit tests

## Files to Create / Modify
- `SaasTemplate.AiApi.sln` (create)
- `src/SaasTemplate.AiApi.Domain/SaasTemplate.AiApi.Domain.csproj`
- `src/SaasTemplate.AiApi.Domain/WorkItems/WorkItem.cs`
- `src/SaasTemplate.AiApi.Domain/WorkItems/WorkItemStatus.cs`
- `src/SaasTemplate.AiApi.Domain/WorkItems/Events/WorkItemCreatedV1.cs`
- `src/SaasTemplate.AiApi.Domain/WorkItems/Exceptions/WorkItemValidationException.cs`
- `src/SaasTemplate.AiApi.Domain/Common/IDomainEvent.cs`
- `src/SaasTemplate.AiApi.Domain/Common/Entity.cs`
- `tests/SaasTemplate.AiApi.UnitTests/SaasTemplate.AiApi.UnitTests.csproj`
- `tests/SaasTemplate.AiApi.UnitTests/Domain/WorkItems/WorkItemTests.cs`

## Testing Plan
- Unit tests: WorkItem creation with valid title
- Unit tests: WorkItem creation with empty title (throws)
- Unit tests: WorkItem creation with title > 200 chars (throws)
- Unit tests: WorkItem creation with description > 2000 chars (throws)
- Unit tests: WorkItemCreatedV1 event construction
- Unit tests: WorkItemStatus enum values

## Security & Privacy
- No external input at this layer
- Validation logic prevents invalid data entering domain

## Observability
- N/A for domain layer (no I/O)

## Rollback Plan
Delete src/ and tests/ directories, remove solution file.

## Definition of Done Checklist
- [ ] Acceptance criteria met
- [ ] Build passes
- [ ] Tests added/updated and pass
- [ ] Formatting/analyzers pass
- [ ] No secrets committed
- [ ] Walkthrough updated
