# Walkthrough: 0023-org-settings

## Task Reference
- Task: `docs/tasks/0023-org-settings.md`
- Walkthrough: `docs/walkthroughs/0023-org-settings.md`
- Branch/PR: `feat/0023-org-settings`
- Date: `2026-02-07`

## Summary
Added an organization settings page where owners and admins can edit the organization name and description. Implemented a backend PATCH endpoint, a GET single-org endpoint, and a frontend settings form with react-hook-form, zod validation, and full i18n support.

## Context
- Background: Organizations only had a name set at creation time. A SaaS template needs organization-level settings for customization.
- Problem statement: No way to edit organization details after creation. No single-org GET endpoint existed.
- Constraints: Only owners and admins can modify settings. Clean Architecture layers respected. All strings internationalized.

## Decisions & Trade-offs
- **PATCH over PUT for updates:**
  - Options considered: PUT (full replace), PATCH (partial update)
  - Why this choice: PATCH is semantically correct for partial updates and aligns with REST conventions. The endpoint accepts name (required) and description (optional).
  - Consequences / risks: None significant. The frontend always sends both fields.

- **Authorization in handler instead of middleware:**
  - The handler checks membership and role (owner/admin) before allowing updates. This follows the existing pattern used in other commands (e.g., role changes).
  - Alternatives considered: Policy-based authorization middleware. Rejected because the existing codebase uses handler-level authorization consistently.

- **FormTextarea as new UI component:**
  - Created `FormTextarea` following the exact same pattern as `FormField` but rendering a `<textarea>` element.
  - Kept in the same `ui/form` directory with re-export from index.ts.

## Implementation Notes
- Key changes:
  - Organization entity gained `Description` property (max 500 chars) and `Update()` method
  - `OrgWithRole` record extended from 3 to 4 parameters (added Description)
  - Two new CQRS items: `UpdateOrganizationCommand` + `GetOrganizationQuery`
  - Two new API endpoints: `PATCH /orgs/{orgId}` and `GET /orgs/{orgId}`
  - Frontend settings page at `/[locale]/orgs/[orgId]/settings`
  - Navigation link added to MembersManager header (visible to owners/admins only)
- Edge cases handled:
  - Non-member access returns 403
  - Member/viewer access returns 403 on PATCH
  - Empty name returns 400 (FluentValidation)
  - Null description is stored as NULL (optional field)
  - Name and description are trimmed before saving
  - Form fields are disabled for non-admin roles (read-only view)
  - Loading and error states handled with skeletons and error messages
- Known limitations: Description is plain text only (no rich text/markdown).

## Commands Run
```bash
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln
cd apps/web && npm run build
cd apps/web && npm test
cd apps/web && npm run lint
```

## Files Changed

### Backend - Domain
- `services/orgs-api/src/SaasTemplate.OrgsApi.Domain/Organizations/Organization.cs` — Added `DescriptionMaxLength`, `Description` property, and `Update()` method

### Backend - Application
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Organizations/Commands/UpdateOrganization/UpdateOrganizationCommand.cs` — New: command record and result record
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Organizations/Commands/UpdateOrganization/UpdateOrganizationCommandHandler.cs` — New: handler with authorization checks
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Organizations/Queries/GetOrganization/GetOrganizationQuery.cs` — New: query record
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Organizations/Queries/GetOrganization/GetOrganizationQueryHandler.cs` — New: handler
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Organizations/Queries/GetMyOrgs/GetMyOrgsQuery.cs` — Modified: added Description to OrgWithRole record

### Backend - Infrastructure
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Configurations/OrganizationConfiguration.cs` — Added Description column config
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Repositories/MembershipRepository.cs` — Updated SELECT to include Description
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Migrations/20260207122246_AddOrganizationDescription.cs` — New: EF migration

### Backend - API
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/OrgsController.cs` — Added GET and PATCH endpoints for single org
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Dtos/UpdateOrgRequest.cs` — New: request DTO
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Validators/UpdateOrgRequestValidator.cs` — New: FluentValidation validator

### Frontend
- `apps/web/src/app/[locale]/orgs/[orgId]/settings/page.tsx` — New: settings page (server component)
- `apps/web/src/app/[locale]/orgs/[orgId]/settings/loading.tsx` — New: loading skeleton
- `apps/web/src/components/orgs/OrgSettingsForm.tsx` — New: settings form with react-hook-form + zod
- `apps/web/src/components/orgs/MembersManager.tsx` — Added settings navigation link for admins/owners
- `apps/web/src/components/ui/form/form-textarea.tsx` — New: FormTextarea component
- `apps/web/src/components/ui/form/index.ts` — Added FormTextarea export
- `apps/web/src/hooks/orgs.ts` — Added `useOrg()` and `useUpdateOrg()` hooks
- `apps/web/src/lib/schemas.ts` — Added OrgDetailSchema, UpdateOrgFormData, description field
- `apps/web/messages/en.json` — Added orgs.settings section
- `apps/web/messages/es.json` — Added orgs.settings section (Spanish)

### Tests
- `services/orgs-api/tests/SaasTemplate.OrgsApi.UnitTests/Application/Organizations/Commands/UpdateOrganizationCommandHandlerTests.cs` — New: 8 unit tests
- `services/orgs-api/tests/SaasTemplate.OrgsApi.IntegrationTests/Api/OrgsEndpointTests.cs` — Added 6 integration tests
- `services/orgs-api/tests/SaasTemplate.OrgsApi.UnitTests/Application/Organizations/Queries/GetMyOrgsQueryHandlerTests.cs` — Fixed OrgWithRole constructor
- `apps/web/src/components/orgs/__tests__/OrgSettingsForm.test.tsx` — New: 9 frontend tests

## Tests

### Unit Tests (8 new)
- `UpdateOrganizationCommandHandlerTests`:
  - Owner can update name and description
  - Admin can update name and description
  - Member gets ForbiddenException
  - Viewer gets ForbiddenException
  - Non-member gets ForbiddenException
  - Missing org throws NotFoundException
  - SaveChangesAsync is called
  - Name and description are trimmed

### Integration Tests (6 new)
- `OrgsEndpointTests`:
  - GET /orgs/{id} returns org details with role
  - GET /orgs/{id} returns 403 when not a member
  - PATCH /orgs/{id} as owner updates name and description
  - PATCH /orgs/{id} as member returns 403
  - PATCH /orgs/{id} unauthenticated returns 401
  - PATCH /orgs/{id} with empty name returns 400

### Frontend Tests (9 new)
- `OrgSettingsForm.test.tsx`:
  - Renders loading state
  - Renders form with pre-filled org data
  - Shows settings title
  - Shows validation error for empty name
  - Submits successfully and shows success toast
  - Shows error toast on failure
  - Disables form fields for non-admin roles
  - Shows error state when loading fails
  - Shows back to members link

### Results
- orgs-api: 163 passed (98 unit + 5 architecture + 60 integration)
- web: 193 passed (15 test files)

## Security
- No secrets committed.
- Authorization enforced at handler level: only owner/admin roles can update settings.
- Input validation via FluentValidation (name required, max lengths enforced).
- Zod validation on frontend provides client-side validation.

## Checklist
- [x] Task scope matches `docs/tasks/0023-org-settings.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed
