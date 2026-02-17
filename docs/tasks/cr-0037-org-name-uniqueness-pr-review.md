# Code Review: cr-0037 — Org Name Uniqueness PR #234

## PR Metadata
- **PR**: #234 (`feat/0035-org-name-uniqueness`)
- **Target branch**: `main`
- **CI status**: Passing (303 orgs-api tests, 162 ai-api tests, web build OK)

## Changed Files
1. `OrganizationConfiguration.cs` — Unique index on name
2. `IOrganizationRepository.cs` — Added `ExistsByNameAsync`
3. `OrganizationRepository.cs` — Implemented `ExistsByNameAsync`
4. `CreateOrganizationCommandHandler.cs` — Uniqueness check on create
5. `UpdateOrganizationCommandHandler.cs` — Uniqueness check on update
6. `20260209171550_AddOrgNameUniqueness.cs` — Migration
7. `20260209171550_AddOrgNameUniqueness.Designer.cs` — Migration designer
8. `AppDbContextModelSnapshot.cs` — Snapshot update
9. `mine/page.tsx` — Frontend 409 handling (create)
10. `OrgSettingsForm.tsx` — Frontend 409 handling (update)
11. `en.json` / `es.json` — i18n strings
12. `CreateOrganizationCommandHandlerTests.cs` — 2 unit tests
13. `UpdateOrganizationCommandHandlerTests.cs` — 2 unit tests
14. `OrgsEndpointTests.cs` — 4 integration tests
15. `docs/tasks/0035-org-name-uniqueness.md` — Task status
16. `docs/walkthroughs/0035-org-name-uniqueness.md` — Walkthrough
17. `docs/backlog/epic-003-multi-tenancy.md` — Epic status

## Comment Sources
- **Inline review comments**: 7 (2 Gemini, 5 Copilot)
- **General reviews**: 2 (Gemini summary, Copilot summary) — non-actionable
- **Issue comments**: 2 (Codex usage limit, Gemini summary) — non-actionable

## Review Threads

### Inline Comments

| # | ID | Reviewer | File | Classification | Summary |
|---|-----|----------|------|---------------|---------|
| 1 | 2783722242 | Gemini (HIGH) | OrganizationConfiguration.cs:49 | MUST_FIX | Index is case-sensitive; need `LOWER(name)` functional index |
| 2 | 2783722251 | Gemini (MEDIUM) | OrganizationRepository.cs:29 | SUGGESTION | Use `EF.Functions.ILike()` instead of `ToLower()` |
| 3 | 2783732252 | Copilot | UpdateOrganizationCommandHandler.cs:43 | SHOULD_FIX | Normalize (trim) name before uniqueness check |
| 4 | 2783732285 | Copilot | OrganizationConfiguration.cs:49 | MUST_FIX | Same as #1: case-sensitive index |
| 5 | 2783732294 | Copilot | Migration 20260209171550:26 | MUST_FIX | Same as #1: migration needs `lower(name)` |
| 6 | 2783732314 | Copilot | OrganizationRepository.cs:29 | SHOULD_FIX | `ToLower()` won't use the index; needs trim consistency |
| 7 | 2783732329 | Copilot | CreateOrganizationCommandHandler.cs:30 | SHOULD_FIX | Normalize name before `ExistsByNameAsync` (trim before check) |

## Comment Resolution Plan

### MUST_FIX
- [x] **#1, #4, #5**: Change DB unique index to `LOWER(name)` functional index via raw SQL in migration. Keep `HasIndex` in EF configuration for snapshot metadata, with comment explaining the migration overrides to LOWER().

### SHOULD_FIX
- [x] **#3, #7**: Normalize (trim) `request.Name` in both Create and Update handlers before calling `ExistsByNameAsync`, and pass the normalized name to `Organization.Create/Update`.
- [x] **#6**: Add defensive trim in `ExistsByNameAsync` repository method. Keep `ToLower()` since it aligns with the `LOWER(name)` index.

### SUGGESTION
- [x] **#2**: Decline. With a `LOWER(name)` functional index, `ToLower() == ToLower()` translates to `WHERE LOWER(name) = LOWER(@p)` which uses the index directly. `ILike` is for pattern matching (LIKE), not exact equality — semantically incorrect for this use case.

## Behavioral Parity Checks
- [x] Redirect parity: N/A (no auth/redirect changes)
- [x] Locale source correctness: N/A (only added i18n keys)
- [x] API/UI contract parity: 409 status from ConflictException correctly handled in both frontend forms
- [x] Test parity: Happy + error paths covered (create + update, unique + duplicate, case-insensitive)

## Status: DONE
