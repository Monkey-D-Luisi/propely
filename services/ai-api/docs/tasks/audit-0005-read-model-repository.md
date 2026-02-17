# Audit Action: audit-0005-read-model-repository

## Metadata
- ID: audit-0005
- Type: AuditAction
- Status: DONE
- Owner: Agent
- Created: 2026-01-30
- Priority: P1
- Source:
  - Executive summary: `docs/audits/2026-01-30-executive-summary.md`
  - Action plan item: "Introduce read-model repository and update query handlers"
- Dependencies:
  - Security work (P0/P1) completed
- Related docs:
  - Walkthrough: `docs/walkthroughs/audit-0005-read-model-repository.md`
  - Clean Architecture audit: `docs/audits/2026-01-30-clean-architecture.md` (Finding 4)

## Goal
Introduce a read repository backed by the read model (`WorkItemsRead`) and update query handlers to use it, completing the CQRS read/write separation.

## Context
The Clean Architecture audit identified that `GetWorkItemByIdQueryHandler` reads from the write model (`IWorkItemRepository` → `WorkItems`) instead of the read model (`WorkItemsRead`). This undermines CQRS benefits and may cause read-side scaling issues.

## Scope
### In scope
- Create `IWorkItemReadRepository` interface in Application layer
- Create `WorkItemReadRepository` implementation in Infrastructure layer
- Create `WorkItemReadDto` in Application layer for read model projection
- Update `GetWorkItemByIdQueryHandler` to use read repository
- Register new repository in DI

### Out of scope
- Refactoring WorkItemProjectorService (separate P2 action)
- Adding new query handlers

## Requirements
- R1: Read repository interface in Application layer (clean architecture)
- R2: Read repository implementation in Infrastructure layer
- R3: Query handler uses read model, not write model
- R4: Cache behavior preserved

## Acceptance Criteria
- [x] `IWorkItemReadRepository` interface created
- [x] `WorkItemReadRepository` implementation created (with fallback for eventual consistency)
- [x] Query handler updated to use read repository
- [x] DI registration added
- [x] Build passes
- [x] All tests pass (79 tests)

## Constraints
- C1: Must maintain clean architecture layers
- C2: Must not break existing caching behavior

## Implementation Steps
1. Create `IWorkItemReadRepository` interface in Application layer
2. Create `WorkItemReadRepository` in Infrastructure layer
3. Update `GetWorkItemByIdQueryHandler` to use read repository
4. Register repository in Program.cs
5. Run build and tests

## Testing Plan
- Unit tests: Update existing handler tests if needed
- Integration tests: Existing tests should pass
- Manual checks: Verify query returns data from read model

## Definition of Done
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests pass (79 tests)
- [x] Walkthrough updated
