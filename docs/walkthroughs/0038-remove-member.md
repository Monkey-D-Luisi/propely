# Walkthrough: 0038-remove-member

## Task Reference
- Task: `docs/tasks/0038-remove-member.md`
- Walkthrough: `docs/walkthroughs/0038-remove-member.md`
- Branch/PR: `feat/0038-remove-member`
- Date: `2026-02-10`

## Summary
Implemented the remove member feature allowing organization owners and admins to remove members from an organization. Backend includes a new `RemoveMemberCommand` with role hierarchy authorization (owners can remove anyone, admins can remove members/viewers only), self-removal prevention, last-owner guard, soft-delete, and notification to the removed user. Frontend adds a Remove button to the MembersTable with a confirmation dialog in MembersManager.

## Context
- Background: Epic 004 — final task completing org & member management CRUD
- Problem statement: Admins/owners had no way to remove members from organizations
- Constraints: Must follow existing Clean Architecture + CQRS patterns, soft-delete convention, i18n (EN + ES)

## Decisions & Trade-offs
- **Authorization hierarchy:**
  - Options: Flat (any manager can remove anyone) vs. Hierarchical (admin can't remove admin/owner)
  - Why: Hierarchical — matches the task spec's constraint and prevents admin abuse
  - Consequences: Admins cannot remove other admins; only owners can

- **Self-removal prevention:**
  - Options: Allow self-removal vs. require "Leave" action
  - Why: Prevent confusion — "Leave" exists for self-removal with its own UX/messaging
  - Consequences: `CANNOT_REMOVE_SELF` domain exception returned as 400

- **Single notification vs. broadcast:**
  - Options: Notify only removed member vs. notify all managers
  - Why: Only the removed member needs immediate notification — managers initiated the action
  - Consequences: Uses `AddAsync` (singular) vs. `AddRangeAsync` from delete-org

## Implementation Notes
- Key changes: RemoveMemberCommand/Handler, OrgsController endpoint, MembersTable remove button, MembersManager confirmation dialog
- Edge cases: Self-removal check (before any DB queries), admin hierarchy check, last-owner guard (defensive — unreachable with current hierarchy but protects against future changes)
- Known limitations: Last-owner guard in RemoveMember handler is defensive-only — currently unreachable because only owners can remove owners (making ownerCount >= 2)

## Data / Schema / Migrations
- DB changes: Added `MemberRemoved` to `NotificationType` enum
- Migration strategy: None needed — EF Core stores enum as string
- Backward compatibility: Additive only

## Commands Run
```bash
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln
cd apps/web && npm test
cd apps/web && npm run build
```

## Files Changed
- `services/orgs-api/src/.../Commands/RemoveMember/RemoveMemberCommand.cs` — MediatR command record
- `services/orgs-api/src/.../Commands/RemoveMember/RemoveMemberCommandHandler.cs` — Handler with authorization, soft-delete, notification
- `services/orgs-api/src/.../Controllers/OrgsController.cs` — Added `DELETE /orgs/{orgId}/members/{userId}` endpoint
- `services/orgs-api/src/.../Notifications/NotificationType.cs` — Added `MemberRemoved` enum value
- `services/orgs-api/tests/.../RemoveMemberCommandHandlerTests.cs` — 13 unit tests
- `apps/web/src/hooks/orgs.ts` — Added `useRemoveMember` hook
- `apps/web/src/components/orgs/MembersTable.tsx` — Added `onRemove` prop and Remove button with role hierarchy
- `apps/web/src/components/orgs/MembersManager.tsx` — Added remove handler, confirmation dialog, error handling
- `apps/web/src/components/orgs/__tests__/MembersTable.test.tsx` — 4 new tests for remove button visibility and interaction
- `apps/web/src/components/orgs/__tests__/MembersManager.test.tsx` — Updated mock for `useRemoveMember`
- `apps/web/src/hooks/__tests__/orgs.test.ts` — 2 new tests for `useRemoveMember` hook
- `apps/web/messages/en.json` — Added `removeMember` i18n section
- `apps/web/messages/es.json` — Added `removeMember` i18n section (Spanish)

## Tests
### Unit
- Backend: 13 tests in `RemoveMemberCommandHandlerTests.cs` — owner removes member, admin removes member, admin can't remove admin/owner, member can't remove, self-removal, last-owner guard, notifications, save changes, cancellation token, target not found, admin removes viewer
- Frontend: 4 new MembersTable tests (remove button visibility for owner/admin, click handler), 2 new useRemoveMember hook tests
- How to run: `dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln` and `cd apps/web && npm test`

### Integration
- Existing integration tests pass (121 tests)

### Manual
- Test with different roles: owner removing member/admin/owner, admin removing member/viewer
- Verify confirmation dialog appears and can be cancelled
- Verify removed member receives notification

## Observability
- Audit log: Automatic via EF Core ChangeTracker (existing pattern for soft-deletes)

## Security
- Validation: Role hierarchy enforced in handler, self-removal check before DB queries
- AuthN/AuthZ: JWT required (controller-level [Authorize]), role-based authorization in handler
- Sensitive data: No PII beyond existing member data

## Follow-ups / Backlog
- [ ] Extract shared DialogOverlay component (currently duplicated in LeaveOrgButton, DeleteOrgSection, and now MembersManager)

## Checklist
- [x] Task scope matches `docs/tasks/0038-remove-member.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
