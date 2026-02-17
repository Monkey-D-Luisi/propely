# Task: 0016 - Soft Delete Pattern

## Metadata
- ID: 0016
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-04
- Related docs:
  - Epic: `docs/backlog/epic-001-professional-saas-refinement.md`
  - Walkthrough: `docs/walkthroughs/0016-soft-delete.md`

## Goal
Implement a soft delete pattern across both .NET services. Entities are marked as deleted instead of physically removed, with a global EF Core query filter to exclude deleted records by default.

## Context
Currently all deletes are permanent. A professional SaaS needs data preservation for compliance, audit trails, and the ability to restore accidentally deleted data.

## Scope
### In scope
- Add `ISoftDeletable` interface with `IsDeleted` and `DeletedAtUtc` properties
- Apply to: Organization, Membership, Invitation, WorkItem (AI API)
- Add global query filter in EF Core: `.HasQueryFilter(e => !e.IsDeleted)`
- Create `SoftDelete()` method on entities
- Update delete operations to call `SoftDelete()` instead of `Remove()`
- Add `IgnoreQueryFilters()` capability for admin queries
- Create EF Core migration

### Out of scope
- Undelete UI (future task)
- Scheduled hard-delete of old soft-deleted records
- User entity soft delete (account deletion is a separate concern)

## Requirements
- R1: Deleted entities are not returned by default queries
- R2: `IsDeleted` is set to true and `DeletedAtUtc` is set on soft delete
- R3: Existing queries work unchanged (global filter is transparent)
- R4: Integration tests verify soft-deleted entities are excluded

## Acceptance Criteria
- AC1: Deleting an organization marks it as deleted, doesn't remove the row
- AC2: Deleted organizations don't appear in `GET /orgs/mine`
- AC3: `dotnet build` succeeds for both services
- AC4: `dotnet test` passes for both services
- AC5: EF migration applies cleanly

## Constraints (non-negotiable)
- Clean Architecture layers respected.
- English-only repo content.
- No secrets in repo.
- Update walkthrough.

## Implementation Steps
1. Create `ISoftDeletable` interface in Domain
2. Add `IsDeleted` and `DeletedAtUtc` to applicable entities
3. Add global query filter in AppDbContext
4. Update delete operations in handlers
5. Create EF Core migration
6. Update tests

## Files to Create / Modify
### orgs-api
- `Domain/Common/ISoftDeletable.cs` (create)
- `Domain/Orgs/Organization.cs` (modify)
- `Domain/Orgs/Membership.cs` (modify)
- `Domain/Orgs/Invitation.cs` (modify)
- `Infrastructure/Persistence/AppDbContext.cs` (modify - query filter)
- `Infrastructure/Persistence/Configurations/` (modify entity configs)

### ai-api
- Same pattern applied to WorkItem entity

## Testing Plan
- Unit tests: Verify SoftDelete() sets properties
- Integration tests: Verify filtered queries exclude deleted entities

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
