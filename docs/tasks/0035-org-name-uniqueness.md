# Task: 0035 - Org Name Uniqueness Constraint

## Metadata
- ID: 0035
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-07
- GitHub Issue: #187
- Epic: `docs/backlog/epic-003-multi-tenancy.md`
- Old Issue: Codex #103

## Goal
Add a unique constraint on organization names (case-insensitive) to prevent duplicate organization names, with proper application-level validation and user-friendly error messages.

## Context
Currently, multiple organizations can have the same name. The `CreateOrganizationCommand` handler creates organizations without checking for name uniqueness. The `Organization` entity has a `Name` property with max length but no uniqueness. This needs both a DB-level unique index (defense in depth) and application-level validation (better error messages).

### Current State
- `Organization.cs`: `Name` property, max length defined
- `OrganizationConfiguration.cs`: maps `name` column with max length, no unique index
- `CreateOrganizationCommandHandler.cs`: creates org without uniqueness check
- `IOrganizationRepository.cs`: has no `ExistsByNameAsync` method

## Scope
### In scope
- Add case-insensitive unique index on `organizations.name` column
- Add `ExistsByNameAsync(string name)` to `IOrganizationRepository`
- Validate uniqueness in `CreateOrganizationCommandHandler` before creating
- Validate uniqueness in `UpdateOrganizationCommandHandler` when renaming
- Return proper error message (not a raw DB constraint violation)
- Add EF Core migration
- Add i18n error strings (EN + ES)
- Frontend: show validation error on create/update org forms

### Out of scope
- Organization slugs/URLs (just name uniqueness)
- Reserved org names list

## Requirements
- R1: Two organizations cannot have the same name (case-insensitive)
- R2: Creating an org with an existing name returns a clear validation error
- R3: Renaming an org to an existing name returns a clear validation error
- R4: The error message is internationalized
- R5: DB unique index provides defense-in-depth

## Acceptance Criteria
- AC1: DB migration adds unique index on `organizations.name` (case-insensitive)
- AC2: `POST /orgs` with duplicate name returns 409 Conflict with error message
- AC3: `PATCH /orgs/{id}` with duplicate name returns 409 Conflict with error message
- AC4: Case variations (e.g., "Acme" vs "ACME") are caught as duplicates
- AC5: Frontend shows validation error on org creation form
- AC6: Frontend shows validation error on org settings form
- AC7: `dotnet build` and `dotnet test` pass
- AC8: `npm run build` and `npm test` pass

## Constraints (non-negotiable)
- Clean Architecture layers respected
- Both DB and application-level validation (defense in depth)
- English-only repo content
- Update walkthrough

## Implementation Steps

### Backend (orgs-api)

1. **Add unique index in OrganizationConfiguration** (`services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Configurations/OrganizationConfiguration.cs`)
   - `builder.HasIndex(o => o.Name).IsUnique()`
   - For case-insensitive in PostgreSQL, use: `builder.HasIndex(o => o.Name).IsUnique().HasDatabaseName("ix_organizations_name_unique")`
   - Consider using `NpgsqlIndexBuilderExtensions` for `LOWER(name)` index or a computed column

2. **Create EF migration**
   - `dotnet ef migrations add AddOrgNameUniqueness --project services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure --startup-project services/orgs-api/src/SaasTemplate.OrgsApi.Api`

3. **Add repository method** (`services/orgs-api/src/SaasTemplate.OrgsApi.Application/Organizations/Interfaces/IOrganizationRepository.cs`)
   - `Task<bool> ExistsByNameAsync(string name, Guid? excludeOrgId = null, CancellationToken cancellationToken = default)`
   - `excludeOrgId` is for update scenarios (don't match against self)

4. **Implement in OrganizationRepository** (`services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Repositories/OrganizationRepository.cs`)
   - Case-insensitive comparison: `.Where(o => o.Name.ToLower() == name.ToLower())`
   - Exclude current org on update: `.Where(o => excludeOrgId == null || o.Id != excludeOrgId)`

5. **Update CreateOrganizationCommandHandler** (`services/orgs-api/src/SaasTemplate.OrgsApi.Application/Organizations/Commands/CreateOrganization/CreateOrganizationCommandHandler.cs`)
   - Before creating, check `await _organizationRepository.ExistsByNameAsync(request.Name)`
   - If exists, throw `ConflictException` with message key `org.nameAlreadyExists`

6. **Update UpdateOrganizationCommandHandler** (if exists, or the command that handles org rename)
   - Before updating name, check `await _organizationRepository.ExistsByNameAsync(request.Name, orgId)`
   - If exists, throw `ConflictException`

7. **Ensure ExceptionHandlerMiddleware maps ConflictException to 409** (`services/orgs-api/src/SaasTemplate.OrgsApi.Api/Middleware/ExceptionHandlerMiddleware.cs`)
   - Verify ConflictException -> 409 Conflict mapping exists

### Frontend (apps/web)

8. **Handle 409 error in org creation** (`apps/web/src/hooks/orgs.ts` or `components/orgs/`)
   - Catch 409 response from `POST /orgs`
   - Display error message from i18n: "An organization with this name already exists"

9. **Handle 409 error in org settings** (`apps/web/src/components/orgs/OrgSettingsForm.tsx`)
   - Same pattern for the update/rename flow

10. **Add i18n strings** (`apps/web/messages/en.json`, `apps/web/messages/es.json`)
    - `orgs.nameAlreadyExists`: "An organization with this name already exists"

### Testing

11. **Backend tests**
    - Create org "Acme" -> create org "Acme" again -> expect ConflictException
    - Create org "Acme" -> create org "ACME" -> expect ConflictException (case-insensitive)
    - Create org "Acme" -> rename different org to "Acme" -> expect ConflictException
    - Create org "Acme" -> rename "Acme" to "Acme Inc" -> expect success

12. **Frontend tests**
    - Org creation form shows error on 409 response

## Files to Create / Modify

### Create
- `docs/walkthroughs/0035-org-name-uniqueness.md`

### Modify
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Configurations/OrganizationConfiguration.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Organizations/Interfaces/IOrganizationRepository.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Repositories/OrganizationRepository.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Organizations/Commands/CreateOrganization/CreateOrganizationCommandHandler.cs`
- `apps/web/src/hooks/orgs.ts` or relevant component
- `apps/web/src/components/orgs/OrgSettingsForm.tsx`
- `apps/web/messages/en.json`
- `apps/web/messages/es.json`

## Testing Plan
- Unit tests: CreateOrganizationCommandHandler with duplicate name
- Integration tests: DB constraint verification, case-insensitive matching
- Frontend tests: 409 error display
- Manual test: Try to create two orgs with same name

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Migration applies cleanly
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] i18n strings added (EN + ES)
- [x] Walkthrough updated
