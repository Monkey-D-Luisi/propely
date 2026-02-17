# Walkthrough: 0016-soft-delete

## Task Reference
- Task: `docs/tasks/0016-soft-delete.md`
- Walkthrough: `docs/walkthroughs/0016-soft-delete.md`
- Branch/PR: `feat/0016-soft-delete`
- Date: `2026-02-06`

## Summary
Implemented a soft delete pattern across both .NET services (orgs-api and ai-api). Created an `ISoftDeletable` interface with `IsDeleted` and `DeletedAtUtc` properties, applied it to Organization, Membership, Invitation (orgs-api) and WorkItem (ai-api), and added EF Core global query filters to transparently exclude soft-deleted records from all queries.

## Context
- Background: All deletes were permanent. A professional SaaS needs data preservation for compliance, audit trails, and accidental deletion recovery.
- Problem statement: No soft delete capability existed in either service.
- Constraints: Each service is independent with its own domain — ISoftDeletable is defined separately in each.

## Decisions & Trade-offs
- **Interface per service vs shared library:**
  - Options considered: Shared NuGet package for ISoftDeletable vs duplicated interface per service.
  - Why this choice: Services are independent microservices with no shared domain. Duplicating a simple 3-method interface avoids coupling.
  - Consequences: Minor duplication (8 lines each), but full service independence preserved.

- **Query filter in entity configs vs convention in OnModelCreating:**
  - Options considered: Convention-based (reflection loop in OnModelCreating) vs explicit per-config.
  - Why this choice: Explicit per-config is clearer, easier to debug, and the entity count is small (4 total). Convention-based approach would require expression building.
  - Consequences: New ISoftDeletable entities must manually add the query filter line.

- **WorkItem: Integrate ISoftDeletable with existing Status-based soft delete:**
  - The ai-api WorkItem already had `Status = Deleted` for soft delete. Added ISoftDeletable alongside: `Delete()` now sets both `Status = Deleted` and `IsDeleted = true / DeletedAtUtc`. `SoftDelete()` delegates to `Delete()` to preserve domain events.
  - Consequences: Global query filter provides transparent filtering; existing Status checks in queries become redundant but harmless.

## Implementation Notes

### Domain Layer

**orgs-api — Created:**
- `Domain/Common/ISoftDeletable.cs` — Interface with `IsDeleted`, `DeletedAtUtc`, and `SoftDelete()` method.

**orgs-api — Modified:**
- `Domain/Organizations/Organization.cs` — Implements `ISoftDeletable`. Added `IsDeleted`, `DeletedAtUtc` properties and `SoftDelete()` method.
- `Domain/Organizations/Membership.cs` — Same pattern as Organization.
- `Domain/Organizations/Invitation.cs` — Same pattern as Organization.

**ai-api — Created:**
- `Domain/Common/ISoftDeletable.cs` — Same interface as orgs-api.

**ai-api — Modified:**
- `Domain/WorkItems/WorkItem.cs` — Implements `ISoftDeletable`. Added `IsDeleted`, `DeletedAtUtc` properties. Updated `Delete()` to set both `IsDeleted/DeletedAtUtc` alongside existing `Status = Deleted`. Added `SoftDelete()` that delegates to `Delete()`.

### Infrastructure Layer

**orgs-api — Modified:**
- `Persistence/Configurations/OrganizationConfiguration.cs` — Added `is_deleted` (bool, default false) and `deleted_at_utc` columns, plus `HasQueryFilter(o => !o.IsDeleted)`.
- `Persistence/Configurations/MembershipConfiguration.cs` — Same pattern.
- `Persistence/Configurations/InvitationConfiguration.cs` — Same pattern.

**ai-api — Modified:**
- `Persistence/Configurations/WorkItemConfiguration.cs` — Added `is_deleted` (bool, default false) and `deleted_at_utc` columns, plus `HasQueryFilter(w => !w.IsDeleted)`.

### Migrations

- `orgs-api: 20260206135247_AddSoftDelete` — Adds `is_deleted` (bool, default false) and `deleted_at_utc` (nullable timestamp) to `organizations`, `memberships`, and `invitations` tables.
- `ai-api: 20260206140027_AddSoftDelete` — Adds same columns to `work_items` table.

## Data / Schema / Migrations
- DB changes: 4 tables gain 2 new columns each (`is_deleted BOOLEAN DEFAULT false`, `deleted_at_utc TIMESTAMP WITH TIME ZONE NULL`).
- Migration strategy: Additive only — no data changes, no column removals. Existing rows get `is_deleted = false` by default.
- Backward compatibility: Fully backward compatible. No existing behavior changes.

## Commands Run
```bash
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln
dotnet build services/ai-api/SaasTemplate.AiApi.sln
dotnet ef migrations add AddSoftDelete --project services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure --startup-project services/orgs-api/src/SaasTemplate.OrgsApi.Api
dotnet ef migrations add AddSoftDelete --project services/ai-api/src/SaasTemplate.AiApi.Infrastructure --startup-project services/ai-api/src/SaasTemplate.AiApi.Api
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln
dotnet test services/ai-api/SaasTemplate.AiApi.sln
```

## Files Changed

### orgs-api
- `Domain/Common/ISoftDeletable.cs` — New: ISoftDeletable interface
- `Domain/Organizations/Organization.cs` — Added ISoftDeletable implementation
- `Domain/Organizations/Membership.cs` — Added ISoftDeletable implementation
- `Domain/Organizations/Invitation.cs` — Added ISoftDeletable implementation
- `Infrastructure/Persistence/Configurations/OrganizationConfiguration.cs` — Added soft delete columns + query filter
- `Infrastructure/Persistence/Configurations/MembershipConfiguration.cs` — Added soft delete columns + query filter
- `Infrastructure/Persistence/Configurations/InvitationConfiguration.cs` — Added soft delete columns + query filter
- `Infrastructure/Migrations/20260206135247_AddSoftDelete.cs` — New: migration
- `Infrastructure/Migrations/20260206135247_AddSoftDelete.Designer.cs` — New: migration designer
- `Infrastructure/Migrations/AppDbContextModelSnapshot.cs` — Updated snapshot

### ai-api
- `Domain/Common/ISoftDeletable.cs` — New: ISoftDeletable interface
- `Domain/WorkItems/WorkItem.cs` — Added ISoftDeletable implementation, updated Delete()
- `Infrastructure/Persistence/Configurations/WorkItemConfiguration.cs` — Added soft delete columns + query filter
- `Infrastructure/Persistence/Migrations/20260206140027_AddSoftDelete.cs` — New: migration
- `Infrastructure/Persistence/Migrations/20260206140027_AddSoftDelete.Designer.cs` — New: migration designer
- `Infrastructure/Persistence/Migrations/AppDbContextModelSnapshot.cs` — Updated snapshot

### Tests
- `orgs-api/tests/UnitTests/Domain/Organizations/OrganizationSoftDeleteTests.cs` — New: 4 tests
- `orgs-api/tests/UnitTests/Domain/Organizations/MembershipSoftDeleteTests.cs` — New: 4 tests
- `orgs-api/tests/UnitTests/Domain/Organizations/InvitationSoftDeleteTests.cs` — New: 4 tests
- `ai-api/tests/UnitTests/Domain/WorkItems/WorkItemTests.cs` — Added 7 soft delete tests

### Documentation
- `docs/tasks/0016-soft-delete.md` — Updated status to DONE
- `docs/backlog/epic-001-professional-saas-refinement.md` — Task 0016 status → DONE, progress tracker updated

## Tests
### Unit
- **orgs-api (12 new tests):** SoftDelete sets IsDeleted, sets DeletedAtUtc, Create is not deleted, entity implements ISoftDeletable — for each of Organization, Membership, Invitation.
- **ai-api (7 new tests):** WorkItem implements ISoftDeletable, Create not deleted, Delete sets IsDeleted, Delete sets DeletedAtUtc, Delete sets Status, SoftDelete delegates to Delete, Delete idempotent on DeletedAtUtc.
- How to run: `dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln` / `dotnet test services/ai-api/SaasTemplate.AiApi.sln`

## Verification
- `dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln` — 0 errors
- `dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln` — 49 tests passed (20 unit + 5 architecture + 24 integration)
- `dotnet build services/ai-api/SaasTemplate.AiApi.sln` — 0 errors
- `dotnet test services/ai-api/SaasTemplate.AiApi.sln` — 128 tests passed (67 unit + 5 architecture + 56 integration)

## Security
- No AuthN/AuthZ impact — soft delete is a data persistence pattern.
- No sensitive data handling changes.

## Follow-ups / Backlog
- [ ] Undelete API endpoint (admin-only, uses `IgnoreQueryFilters()`)
- [ ] Cascade soft delete: when an org is soft-deleted, also soft-delete its memberships/invitations
- [ ] Scheduled hard-delete job for records soft-deleted > N days

## Checklist
- [x] Task scope matches `docs/tasks/0016-soft-delete.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
