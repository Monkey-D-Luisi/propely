# Task: 0038 - Remove Member Endpoint + UI

## Metadata
- ID: 0038
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-07
- GitHub Issue: #190
- Epic: `docs/backlog/epic-004-org-management.md`

## Goal
Allow organization admins and owners to remove a member from the organization, with safeguards against removing the last owner.

## Context
The MembersTable component (`apps/web/src/components/orgs/MembersTable.tsx`) shows organization members with role badges. Currently, admins can change roles via `useUpdateMemberRole` but cannot remove members. This task adds the remove capability.

### Current State
- `MembersTable.tsx` displays members with roles
- `MembersManager.tsx` manages the member list view
- `useUpdateMemberRole()` exists for role changes
- Backend `OrgsController` has invite and update-role endpoints
- No `DELETE /orgs/{orgId}/members/{userId}` endpoint exists

## Scope
### In scope
- `DELETE /orgs/{orgId}/members/{userId}` endpoint (admin/owner only)
- Business rule: cannot remove the last owner
- Business rule: cannot remove yourself (use "leave" for that)
- Remove button in MembersTable for each member row
- Confirmation dialog before removal
- Audit log entry
- Notification to the removed member
- i18n strings (EN + ES)

### Out of scope
- Bulk member removal
- Temporary suspension (just removal)

## Requirements
- R1: Owners and admins can remove members from the org
- R2: Cannot remove the last owner
- R3: Cannot remove yourself (must use "leave org" instead)
- R4: Removed member loses access immediately
- R5: Removed member receives a notification
- R6: Audit log entry created
- R7: Only owners/admins see the remove button

## Acceptance Criteria
- AC1: `DELETE /orgs/{orgId}/members/{userId}` as owner -> membership soft-deleted
- AC2: `DELETE /orgs/{orgId}/members/{userId}` as admin -> membership soft-deleted
- AC3: `DELETE /orgs/{orgId}/members/{userId}` as regular member -> 403
- AC4: Removing last owner -> 400 error
- AC5: Removing self -> 400 error (use leave instead)
- AC6: Remove button appears in MembersTable for admins/owners
- AC7: Confirmation dialog shown before removal
- AC8: `dotnet build` and `dotnet test` pass
- AC9: `npm run build` and `npm test` pass

## Constraints (non-negotiable)
- Clean Architecture layers respected
- Soft delete the membership (consistent pattern)
- Authorization: owner can remove anyone, admin can remove members (not other admins/owners)
- English-only repo content
- Update walkthrough

## Implementation Steps

### Backend (orgs-api)

1. **Create RemoveMemberCommand** (`services/orgs-api/src/SaasTemplate.OrgsApi.Application/Organizations/Commands/RemoveMember/RemoveMemberCommand.cs`)
   - Properties: `OrgId` (Guid), `TargetUserId` (Guid), `RequestingUserId` (Guid)

2. **Create RemoveMemberCommandHandler**
   - Validate: requesting user has Owner or Admin role in org
   - Validate: target user is not the same as requesting user (use leave instead)
   - Validate: if target is Owner, check there's at least one other Owner
   - Authorization hierarchy: Admin can remove Members only, Owner can remove anyone
   - Soft-delete the target's membership
   - Create audit log entry
   - Create notification for the removed user

3. **Add endpoint to OrgsController** (`services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/OrgsController.cs`)
   - `DELETE /orgs/{orgId}/members/{userId}` -> `RemoveMemberCommand`

### Frontend (apps/web)

4. **Add remove button to MembersTable** (`apps/web/src/components/orgs/MembersTable.tsx`)
   - Add a "Remove" action button/icon for each member row
   - Only visible when current user is admin/owner
   - Don't show for the current user's own row (they should use "Leave")
   - Disable for the last owner

5. **Create useRemoveMember hook** (`apps/web/src/hooks/orgs.ts`)
   - `DELETE /orgs/{orgId}/members/{userId}`
   - Refetch member list on success
   - Handle errors

6. **Add confirmation dialog**
   - "Are you sure you want to remove {memberName} from {orgName}?"
   - Confirm/Cancel buttons

7. **Add i18n strings** (`apps/web/messages/en.json`, `apps/web/messages/es.json`)
   - `members.remove`, `members.removeConfirm`, `members.removeSuccess`, `members.cannotRemoveLastOwner`, `members.cannotRemoveSelf`

### Testing

8. **Backend tests**
   - Owner removes member -> success
   - Admin removes member -> success
   - Admin removes admin -> 403 (hierarchy violation)
   - Member removes member -> 403
   - Remove last owner -> 400
   - Remove self -> 400

9. **Frontend tests**
   - Remove button visibility based on role
   - Confirmation dialog shown
   - Successful removal updates member list

## Files to Create / Modify

### Create
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Organizations/Commands/RemoveMember/RemoveMemberCommand.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Organizations/Commands/RemoveMember/RemoveMemberCommandHandler.cs`
- `docs/walkthroughs/0038-remove-member.md`

### Modify
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/OrgsController.cs`
- `apps/web/src/components/orgs/MembersTable.tsx`
- `apps/web/src/hooks/orgs.ts`
- `apps/web/messages/en.json`
- `apps/web/messages/es.json`

## Testing Plan
- Unit tests: RemoveMemberCommandHandler (all authorization scenarios)
- Integration tests: Full removal flow via API
- Frontend tests: MembersTable remove button, confirmation dialog
- Manual: Add members with different roles -> test removal permissions

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] i18n strings added (EN + ES)
- [x] Walkthrough updated
