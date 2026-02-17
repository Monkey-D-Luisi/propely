# Walkthrough: 0037-delete-org

## Task Reference
- Task: `docs/tasks/0037-delete-org.md`
- Walkthrough: `docs/walkthroughs/0037-delete-org.md`
- Branch/PR: `feat/0037-delete-org`
- Date: 2026-02-10

## Summary
Implemented the DELETE /orgs/{orgId} endpoint allowing org owners to soft-delete their organization. Cascade soft-deletes all memberships and cancels pending invitations. Added a "Danger Zone" section to the org settings page with a confirmation dialog requiring the user to type the org name. All members are notified of the deletion.

## Context
- Background: Epic 004 requires complete org management CRUD. Delete org is the second of three tasks.
- Problem statement: No way for owners to delete an organization. The `Organization` entity already supports `ISoftDeletable` with `SoftDelete()` method and global query filter.
- Constraints: Soft delete only, owner authorization required, must cascade to memberships and invitations.

## Decisions & Trade-offs
- **Cascade approach:** Loaded all memberships in-memory and called `SoftDelete()` on each, rather than using a bulk SQL update. This preserves EF Core ChangeTracker audit logging for each entity.
- **Invitation cancellation:** Added `CancelByOrgAsync` repository method that loads pending invitations and soft-deletes them individually for audit trail consistency.
- **Frontend confirmation:** Used the same custom dialog pattern as `LeaveOrgButton` with an added text input requiring exact org name match. No external dialog library needed.

## Implementation Notes
- Key changes: New command/handler, controller endpoint, repository method, frontend hook + component, i18n strings
- Edge cases handled: Non-owner attempting deletion (403), org not found (404), no current member (404)
- Known limitations: No undo/restore, no grace period (out of scope per task spec)

## Data / Schema / Migrations
- DB changes: None. Uses existing soft-delete infrastructure.
- Migration strategy: N/A
- Backward compatibility: N/A

## Commands Run
```bash
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln
dotnet test services/orgs-api/tests/SaasTemplate.OrgsApi.UnitTests/SaasTemplate.OrgsApi.UnitTests.csproj
cd apps/web && npm test
cd apps/web && npm run build   # passes after the TS fix commit (fix(web): add type cast for router.replace)
```

## Files Changed
- `services/orgs-api/src/SaasTemplate.OrgsApi.Domain/Notifications/NotificationType.cs` — Added `OrgDeleted` enum value
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Organizations/Interfaces/IInvitationRepository.cs` — Added `CancelByOrgAsync` method
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Repositories/InvitationRepository.cs` — Implemented `CancelByOrgAsync`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Organizations/Commands/DeleteOrganization/DeleteOrganizationCommand.cs` — New command record
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Organizations/Commands/DeleteOrganization/DeleteOrganizationCommandHandler.cs` — New handler with cascade logic
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/OrgsController.cs` — Added DELETE endpoint
- `services/orgs-api/tests/SaasTemplate.OrgsApi.UnitTests/Application/Organizations/Commands/DeleteOrganizationCommandHandlerTests.cs` — Unit tests
- `apps/web/src/hooks/orgs.ts` — Added `useDeleteOrg` hook
- `apps/web/src/hooks/__tests__/orgs.test.ts` — Tests for `useDeleteOrg`
- `apps/web/src/components/orgs/DeleteOrgSection.tsx` — Danger zone component with confirmation dialog
- `apps/web/src/components/orgs/OrgSettingsForm.tsx` — Integrated `DeleteOrgSection` for owners
- `apps/web/messages/en.json` — Added delete org i18n strings
- `apps/web/messages/es.json` — Added delete org i18n strings (Spanish)

## Tests
### Unit
- Backend: `DeleteOrganizationCommandHandlerTests` covering owner success, non-owner forbidden, not-found, cascade memberships, cancel invitations, notifications, save changes, cancellation token
- Frontend: `useDeleteOrg` hook tests covering success, error propagation
- How to run: `dotnet test services/orgs-api/tests/SaasTemplate.OrgsApi.UnitTests` / `cd apps/web && npm test`

### Integration
- N/A (requires Docker/Testcontainers)

### Manual
- Create org with members -> delete as owner -> verify org and memberships are gone from lists

## Observability
- Audit logging: Automatic via AppDbContext SaveChangesAsync interceptor for org, membership, and invitation changes

## Security
- Authorization: Only org owners can delete. ForbiddenException thrown for non-owners.
- Validation: Org ID validated as GUID in route. User ID extracted from JWT claims.

## Follow-ups / Backlog
- [ ] Add undo/restore deleted organization feature (if needed)
- [ ] Add grace period before permanent deletion (if needed)

## Checklist
- [x] Task scope matches `docs/tasks/0037-delete-org.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
