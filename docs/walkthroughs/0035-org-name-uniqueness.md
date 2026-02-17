# Walkthrough: 0035 - Org Name Uniqueness Constraint

## Task Reference
- Task: `docs/tasks/0035-org-name-uniqueness.md`
- Epic: `docs/backlog/epic-003-multi-tenancy.md`
- Branch: `feat/0035-org-name-uniqueness`
- GitHub Issue: #187

## Summary
Added case-insensitive unique constraint on organization names with both database-level defense-in-depth (unique index) and application-level validation (ConflictException → HTTP 409). Updated both backend handlers (create + update) and frontend forms (create + settings) with i18n error messages.

## Changes Made

### Backend (orgs-api)

1. **OrganizationConfiguration.cs** — Added unique index `ix_organizations_name_unique` on `name` column, filtered to exclude soft-deleted rows (`is_deleted = false`).

2. **IOrganizationRepository.cs** — Added `ExistsByNameAsync(string name, Guid? excludeOrgId, CancellationToken)` interface method. The `excludeOrgId` parameter allows the update handler to exclude the current org from the check.

3. **OrganizationRepository.cs** — Implemented `ExistsByNameAsync` with case-insensitive comparison via `ToLower()` (translates to PostgreSQL `LOWER()`) and optional org exclusion.

4. **CreateOrganizationCommandHandler.cs** — Added uniqueness check before `Organization.Create()`. Throws `ConflictException` if name exists.

5. **UpdateOrganizationCommandHandler.cs** — Added uniqueness check only when the name is actually being changed (case-insensitive comparison against current name). Passes `request.OrgId` as `excludeOrgId` to avoid false positives. Throws `ConflictException` if name taken by another org.

6. **EF Core Migration** — `20260209171550_AddOrgNameUniqueness.cs` creates the unique index with soft-delete filter.

### Frontend (apps/web)

7. **mine/page.tsx** — Added `isApiError` import and 409-specific error handling in create org form. Shows `t('mine.nameAlreadyExists')` on conflict.

8. **OrgSettingsForm.tsx** — Added 409-specific error handling in update form. Shows `t('settings.nameAlreadyExists')` on conflict, with toast notification.

9. **en.json / es.json** — Added i18n strings:
   - `orgs.mine.nameAlreadyExists`: "An organization with this name already exists." / "Ya existe una organización con este nombre."
   - `orgs.settings.nameAlreadyExists`: same message in both sections.

### Tests

10. **CreateOrganizationCommandHandlerTests.cs** — Added 2 tests:
    - `Handle_WhenNameAlreadyExists_ShouldThrowConflictException`
    - `Handle_WhenNameIsUnique_ShouldNotThrow`

11. **UpdateOrganizationCommandHandlerTests.cs** — Added 2 tests:
    - `Handle_WhenRenamingToDuplicateName_ShouldThrowConflictException`
    - `Handle_WhenKeepingSameName_ShouldNotCheckUniqueness`

12. **OrgsEndpointTests.cs** — Added 4 integration tests:
    - `CreateOrg_WithDuplicateName_ShouldReturn409`
    - `CreateOrg_WithDuplicateNameDifferentCase_ShouldReturn409`
    - `UpdateOrg_ToDuplicateName_ShouldReturn409`
    - `UpdateOrg_KeepSameName_ShouldReturn200`

## Decisions

1. **Unique index with soft-delete filter**: The index uses `WHERE is_deleted = false` so deleted orgs don't block name reuse. This is correct for the business requirement (active orgs must have unique names).

2. **Application-level case comparison before DB check**: The update handler skips the `ExistsByNameAsync` call if the name hasn't changed (case-insensitive compare). This avoids a redundant DB round-trip when only the description is being updated.

3. **ConflictException (not ValidationException)**: Used `ConflictException` → 409 rather than 400 because name uniqueness is a state-dependent conflict, not a format/schema validation issue.

## Validation Results

- `dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln` — 0 errors
- `dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln` — 303 tests pass (5 arch + 180 unit + 118 integration)
- `dotnet test services/ai-api/SaasTemplate.AiApi.sln` — 162 tests pass (no regressions)
- `cd apps/web && npm run build` — success

## Checklist
- [x] Task scope matches docs/tasks/0035-org-name-uniqueness.md
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed

## Process Deviations
- None

## Manual Verification Pending
- Manual test: Create two orgs with the same name via UI and verify the error message appears.
