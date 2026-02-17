# Walkthrough: cr-0037 — Org Name Uniqueness PR #234 Review

## Task Reference
- Task: `docs/tasks/cr-0037-org-name-uniqueness-pr-review.md`
- PR: #234 (`feat/0035-org-name-uniqueness`)
- Branch: `feat/0035-org-name-uniqueness`

## Summary
Addressed 7 inline review comments from Gemini and Copilot on PR #234. The primary issue was a case-sensitive unique index that should be case-insensitive, plus missing trim normalization before uniqueness checks.

## Changes Made

### MUST_FIX: Case-insensitive functional index

1. **20260209171550_AddOrgNameUniqueness.cs** — Replaced `migrationBuilder.CreateIndex` with raw SQL: `CREATE UNIQUE INDEX ix_organizations_name_unique ON organizations (LOWER(name)) WHERE is_deleted = false`. Down migration also uses raw SQL `DROP INDEX`.

2. **OrganizationConfiguration.cs** — Updated comment to document that the migration overrides this to a `LOWER(name)` functional index. The `HasIndex` stays for EF Core snapshot metadata.

### SHOULD_FIX: Trim normalization

3. **CreateOrganizationCommandHandler.cs** — Added `var normalizedName = request.Name.Trim();` before `ExistsByNameAsync` call and `Organization.Create`. This prevents whitespace-padded names from bypassing the app-level check.

4. **UpdateOrganizationCommandHandler.cs** — Same normalization pattern. Trim before equality check, uniqueness check, and `org.Update()`.

5. **OrganizationRepository.cs** — Added defensive `name.Trim()` in `ExistsByNameAsync` for defense-in-depth.

### SUGGESTION: Declined

6. **ILike vs ToLower** — Declined. `ToLower()` translates to `LOWER()` in PostgreSQL, which directly matches the `LOWER(name)` functional index. `ILike` is for pattern matching (LIKE operator), not exact equality comparison.

## Validation Results
- `dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln` — 0 errors
- `dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln` — all tests pass
- `cd apps/web && npm run build` — success

## Process Deviations
- None
