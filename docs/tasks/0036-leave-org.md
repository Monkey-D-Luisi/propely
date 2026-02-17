# Task: 0036 - Leave Organization Endpoint + UI

## Metadata
- ID: 0036
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-07
- GitHub Issue: #188
- Epic: `docs/backlog/epic-004-org-management.md`

## Goal
Allow users to leave an organization they belong to, with a safeguard preventing the last owner from leaving.

## Context
The current codebase has a `LeaveOrgButton` component and `useLeaveOrg` hook (`apps/web/src/hooks/orgs.ts`), but we need to verify the backend endpoint exists and is complete. The frontend component exists at `apps/web/src/components/orgs/LeaveOrgButton.tsx`. This task ensures both backend and frontend are fully functional with proper business rules.

### Current State
- `LeaveOrgButton.tsx` exists in `apps/web/src/components/orgs/`
- `useLeaveOrg()` hook exists in `apps/web/src/hooks/orgs.ts`
- Backend: Need to verify if leave org endpoint exists in `OrgsController`

## Scope
### In scope
- Verify/create `DELETE /orgs/{orgId}/members/me` endpoint
- Business rule: last owner cannot leave (must transfer ownership first)
- Create `LeaveOrganizationCommand` in Application layer
- Confirmation dialog before leaving
- Audit log entry on leave
- Notification to org admins when a member leaves
- i18n strings (EN + ES)

### Out of scope
- Transferring ownership (separate concern)
- Re-joining after leaving (handled by invitation flow)

## Requirements
- R1: Authenticated user can leave any org they are a member of
- R2: If user is the last Owner, the request is rejected with a clear error
- R3: Leaving removes the Membership record (soft delete)
- R4: An audit log entry is created
- R5: Org admins/owners are notified of the departure
- R6: User is redirected to the orgs list after leaving

## Acceptance Criteria
- AC1: `DELETE /orgs/{orgId}/members/me` removes the user's membership
- AC2: Last owner receives 400 error with "Cannot leave: you are the last owner"
- AC3: Non-last-owner leaves successfully
- AC4: Audit log entry created with action "MemberLeft"
- AC5: Frontend confirmation dialog shown before leaving
- AC6: After leaving, user is redirected to `/orgs/mine`
- AC7: `dotnet build` and `dotnet test` pass
- AC8: `npm run build` and `npm test` pass

## Constraints (non-negotiable)
- Clean Architecture layers respected
- Soft delete the membership (consistent with existing pattern)
- English-only repo content
- Update walkthrough

## Implementation Steps

### Backend (orgs-api)

1. **Create LeaveOrganizationCommand** (`services/orgs-api/src/SaasTemplate.OrgsApi.Application/Organizations/Commands/LeaveOrganization/LeaveOrganizationCommand.cs`)
   - Properties: `OrgId` (Guid), `UserId` (Guid)

2. **Create LeaveOrganizationCommandHandler**
   - Get membership for user in org
   - If not found, throw NotFoundException
   - Count remaining owners: if user is Owner and is the only one, throw ValidationException
   - Soft-delete the membership
   - Create audit log entry
   - Create notification for org admins

3. **Add endpoint to OrgsController** (`services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/OrgsController.cs`)
   - `DELETE /orgs/{orgId}/members/me` -> `LeaveOrganizationCommand(orgId, userId)`
   - userId comes from JWT claims

4. **Add repository method if needed** to `IMembershipRepository`
   - `CountByRoleAsync(Guid orgId, MembershipRole role)` -> count owners

### Frontend (apps/web)

5. **Verify LeaveOrgButton** (`apps/web/src/components/orgs/LeaveOrgButton.tsx`)
   - Ensure it has a confirmation dialog
   - Ensure it calls `useLeaveOrg()` hook
   - Ensure it redirects to `/orgs/mine` after success
   - Ensure error handling for "last owner" scenario

6. **Verify useLeaveOrg hook** (`apps/web/src/hooks/orgs.ts`)
   - Ensure it calls `DELETE /orgs/{orgId}/members/me`
   - Ensure error handling returns meaningful messages

7. **Add/verify i18n strings** (`apps/web/messages/en.json`, `apps/web/messages/es.json`)
   - `orgs.leaveOrg`, `orgs.leaveOrgConfirm`, `orgs.leaveOrgSuccess`, `orgs.cannotLeaveLastOwner`

### Testing

8. **Backend tests**
   - Leave org as regular member -> success
   - Leave org as one of multiple owners -> success
   - Leave org as last owner -> 400 error
   - Leave org you're not a member of -> 404

9. **Frontend tests**
   - LeaveOrgButton shows confirmation dialog
   - Successful leave redirects

## Files to Create / Modify

### Create
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Organizations/Commands/LeaveOrganization/LeaveOrganizationCommand.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Organizations/Commands/LeaveOrganization/LeaveOrganizationCommandHandler.cs`
- `docs/walkthroughs/0036-leave-org.md`

### Modify
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/OrgsController.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Organizations/Interfaces/IMembershipRepository.cs` (if adding count method)
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Repositories/MembershipRepository.cs`
- `apps/web/src/components/orgs/LeaveOrgButton.tsx` (verify/fix)
- `apps/web/messages/en.json`
- `apps/web/messages/es.json`

## Testing Plan
- Unit tests: LeaveOrganizationCommandHandler (all scenarios)
- Integration tests: API endpoint for leave org
- Frontend tests: LeaveOrgButton behavior
- Manual: Join org -> leave -> verify removed from member list

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] i18n strings added (EN + ES)
- [x] Walkthrough updated
