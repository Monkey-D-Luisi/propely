# Walkthrough: audit-0013-cross-tenant-injection-test

## Task Reference
- Task: `docs/audits/epic-003-executive-summary.md` (action #1)
- Walkthrough: `docs/walkthroughs/audit-0013-cross-tenant-injection-test.md`
- Branch/PR: `fix/audit-0013-epic-003-remediations`
- Date: `2026-02-09`

## Summary
Added an integration test that verifies `SetTenantIdOnNewEntities()` in `AppDbContext` throws `TenantMismatchException` when a handler attempts to save a WorkItem with an OrgId that doesn't match the current tenant context.

## Context
- Background: `AppDbContext.SetTenantIdOnNewEntities()` is a critical defense-in-depth mechanism that prevents cross-tenant data injection at the persistence layer.
- Problem statement: While the mechanism existed and worked correctly, there was no explicit test exercising the mismatched-OrgId code path, meaning it could silently break during refactoring.
- Constraints: Must use the existing `PostgresFixture` and `[Collection("Postgres")]` test infrastructure.

## Decisions & Trade-offs
- **Decision: Add to TenantQueryFilterTests vs new test class**
  - Options considered: (1) Add to existing `TenantQueryFilterTests.cs` which already tests DbContext-level behavior, (2) Create a new test class
  - Why this choice: The test is a DbContext-level persistence concern, fitting naturally alongside the existing query filter tests. No new infrastructure needed.

## Files Changed
- `services/ai-api/tests/SaasTemplate.AiApi.IntegrationTests/Persistence/TenantQueryFilterTests.cs` — Added `SaveChanges_WithMismatchedOrgId_ShouldThrowTenantMismatchException` test

## Tests
### Integration
- `SaveChanges_WithMismatchedOrgId_ShouldThrowTenantMismatchException` — Creates a WorkItem with tenant B's OrgId in a context scoped to tenant A, verifies `TenantMismatchException` is thrown with both OrgIds in the message

## Checklist
- [x] Task scope matches audit action #1 in `docs/audits/epic-003-executive-summary.md`
- [x] Tests updated and passing (163 total: 5 arch + 82 unit + 76 integration)
- [x] No secrets committed
