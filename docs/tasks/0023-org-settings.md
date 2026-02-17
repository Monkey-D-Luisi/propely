# Task: 0023 - Organization Settings Page

## Metadata
- ID: 0023
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-04
- Related docs:
  - Epic: `docs/backlog/epic-001-professional-saas-refinement.md`
  - Walkthrough: `docs/walkthroughs/0023-org-settings.md`

## Goal
Add an organization settings page where owners and admins can edit the organization name and description. Implement the backend PATCH endpoint and the frontend form with react-hook-form and i18n.

## Context
Organizations currently only have a name set at creation time. A SaaS template needs organization-level settings for customization. This also serves as a pattern for future settings pages.

## Scope
### In scope
- Backend: Add `description` field to Organization entity
- Backend: `PATCH /orgs/{id}` endpoint (name, description)
- Backend: Authorization - only owners and admins can update
- Frontend: `/orgs/[orgId]/settings` page with react-hook-form
- Frontend: Navigation link from members page to settings
- Frontend: i18n for all strings
- EF Core migration for description column

### Out of scope
- Organization avatar/logo
- Organization deletion
- Billing settings

## Requirements
- R1: Only owners and admins can access org settings
- R2: Name is required, description is optional
- R3: Changes are saved with optimistic UI feedback (toast)
- R4: Settings page uses same layout as members page (org context)

## Acceptance Criteria
- AC1: `PATCH /orgs/{id}` updates name and description
- AC2: Non-owners/admins get 403 on PATCH
- AC3: Settings page renders with current org data pre-filled
- AC4: Saving shows success toast
- AC5: Settings available in both EN and ES
- AC6: `dotnet build` + `dotnet test` pass
- AC7: `npm run build` + `npm run lint` pass

## Constraints (non-negotiable)
- Clean Architecture layers respected.
- English-only repo content.
- No secrets in repo.
- Update walkthrough.

## Implementation Steps
1. Add `Description` property to Organization entity
2. Create `UpdateOrganizationCommand` + handler
3. Create PATCH endpoint in OrgsController
4. Create EF migration
5. Create frontend settings page with react-hook-form
6. Add navigation from members page
7. Add i18n strings
8. Test

## Files to Create / Modify
### Backend
- `Domain/Orgs/Organization.cs` (modify - add Description)
- `Application/Orgs/Commands/UpdateOrganization/` (create)
- `Api/Controllers/OrgsController.cs` (modify - add PATCH)
- `Infrastructure/Persistence/Configurations/OrganizationConfiguration.cs` (modify)
- EF migration (create)

### Frontend
- `apps/web/src/app/[locale]/orgs/[orgId]/settings/page.tsx` (create)
- `apps/web/src/components/orgs/OrgSettingsForm.tsx` (create)
- `apps/web/src/hooks/orgs.ts` (modify - add useUpdateOrg)
- `apps/web/messages/en.json` (modify)
- `apps/web/messages/es.json` (modify)

## Testing Plan
- Unit tests: UpdateOrganization handler tests
- Integration tests: PATCH endpoint tests
- Frontend tests: OrgSettingsForm test
- Manual verification: Edit org name/description

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
