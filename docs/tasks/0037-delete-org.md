# Task: 0037 - Delete Organization (Soft Delete) Endpoint + UI

## Metadata
- ID: 0037
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-07
- GitHub Issue: #189
- Epic: `docs/backlog/epic-004-org-management.md`

## Goal
Allow organization owners to delete their organization via soft delete, cascading the deletion to all related memberships and invitations.

## Context
The `Organization` entity already implements `ISoftDeletable` with `IsDeleted` and `DeletedAtUtc` properties. The soft delete infrastructure (global query filter) is in place. What's missing is the DELETE endpoint, the command handler with cascade logic, and the UI button on the org settings page.

### Current State
- `Organization.cs` has `SoftDelete()` method (from task 0016)
- Global query filter: `HasQueryFilter(o => !o.IsDeleted)` exists
- Org settings page: `apps/web/src/app/[locale]/orgs/[orgId]/settings/page.tsx`
- No `DELETE /orgs/{orgId}` endpoint exists

## Scope
### In scope
- `DELETE /orgs/{orgId}` endpoint (owner only)
- Soft-delete the organization
- Cascade soft-delete to all memberships in the org
- Cancel/soft-delete all pending invitations for the org
- Confirmation dialog with org name typed as confirmation (destructive action)
- Audit log entry
- Notify all members of the deletion
- i18n strings (EN + ES)

### Out of scope
- Hard delete / data purge
- Undo/restore deleted organization
- Grace period before deletion

## Requirements
- R1: Only org owners can delete the organization
- R2: Deletion is soft (sets `IsDeleted = true`)
- R3: All memberships in the org are cascade soft-deleted
- R4: All pending invitations are cancelled
- R5: All members receive a notification about the deletion
- R6: Confirmation requires typing the org name
- R7: Audit log entry created

## Acceptance Criteria
- AC1: `DELETE /orgs/{orgId}` as owner -> org is soft-deleted
- AC2: `DELETE /orgs/{orgId}` as non-owner -> 403 Forbidden
- AC3: After deletion, org no longer appears in any user's org list
- AC4: After deletion, memberships are soft-deleted
- AC5: Frontend shows delete button on org settings page (owners only)
- AC6: Frontend requires typing org name to confirm
- AC7: `dotnet build` and `dotnet test` pass
- AC8: `npm run build` and `npm test` pass

## Constraints (non-negotiable)
- Clean Architecture layers respected
- Soft delete only (consistent with existing pattern)
- Owner authorization required
- English-only repo content
- Update walkthrough

## Implementation Steps

### Backend (orgs-api)

1. **Create DeleteOrganizationCommand** (`services/orgs-api/src/SaasTemplate.OrgsApi.Application/Organizations/Commands/DeleteOrganization/DeleteOrganizationCommand.cs`)
   - Properties: `OrgId` (Guid), `UserId` (Guid)

2. **Create DeleteOrganizationCommandHandler**
   - Verify user is an Owner of the org
   - Call `organization.SoftDelete()`
   - Soft-delete all memberships: `await _membershipRepository.SoftDeleteByOrgAsync(orgId)`
   - Cancel pending invitations: `await _invitationRepository.CancelByOrgAsync(orgId)`
   - Create audit log entry
   - Create notifications for all members
   - Save all changes in a transaction

3. **Add repository methods**
   - `IMembershipRepository.SoftDeleteByOrgAsync(Guid orgId)` - soft delete all memberships for org
   - `IInvitationRepository.CancelByOrgAsync(Guid orgId)` - cancel pending invitations

4. **Add endpoint to OrgsController** (`services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/OrgsController.cs`)
   - `DELETE /orgs/{orgId}` -> authorization check (owner) -> `DeleteOrganizationCommand`

### Frontend (apps/web)

5. **Add delete section to org settings** (`apps/web/src/app/[locale]/orgs/[orgId]/settings/page.tsx` or `OrgSettingsForm.tsx`)
   - "Danger Zone" section at bottom (only visible to owners)
   - "Delete Organization" button (red/destructive styling)
   - Confirmation dialog: "Type the organization name to confirm deletion"
   - Input field must match org name exactly

6. **Create useDeleteOrg hook** (`apps/web/src/hooks/orgs.ts`)
   - Calls `DELETE /orgs/{orgId}`
   - On success, redirect to `/orgs/mine`
   - Handle errors (403, 404)

7. **Add i18n strings** (`apps/web/messages/en.json`, `apps/web/messages/es.json`)
   - `orgs.deleteOrg`, `orgs.deleteOrgConfirm`, `orgs.deleteOrgTypeName`, `orgs.deleteOrgWarning`, `orgs.dangerZone`, `orgs.deleteOrgSuccess`

### Testing

8. **Backend tests**
   - Delete org as owner -> success, org and memberships soft-deleted
   - Delete org as admin (non-owner) -> 403
   - Delete org as member -> 403
   - Verify cascade: memberships and invitations cleaned up

9. **Frontend tests**
   - Delete button visible only for owners
   - Confirmation dialog requires exact org name
   - Successful deletion redirects

## Files to Create / Modify

### Create
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Organizations/Commands/DeleteOrganization/DeleteOrganizationCommand.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Organizations/Commands/DeleteOrganization/DeleteOrganizationCommandHandler.cs`
- `docs/walkthroughs/0037-delete-org.md`

### Modify
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/OrgsController.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Organizations/Interfaces/IMembershipRepository.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Organizations/Interfaces/IInvitationRepository.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Repositories/MembershipRepository.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Repositories/InvitationRepository.cs`
- `apps/web/src/app/[locale]/orgs/[orgId]/settings/page.tsx` (or OrgSettingsForm.tsx)
- `apps/web/src/hooks/orgs.ts`
- `apps/web/messages/en.json`
- `apps/web/messages/es.json`

## Testing Plan
- Unit tests: DeleteOrganizationCommandHandler
- Integration tests: Full deletion flow with cascade verification
- Frontend tests: Confirmation dialog, authorization visibility
- Manual: Create org with members -> delete -> verify all cleaned up

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] i18n strings added (EN + ES)
- [x] Walkthrough updated
