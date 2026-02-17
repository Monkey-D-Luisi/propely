# Walkthrough: 0036-leave-org

## Task Reference
- Task: `docs/tasks/0036-leave-org.md`
- Walkthrough: `docs/walkthroughs/0036-leave-org.md`
- Branch/PR: `feat/0036-leave-org` / TBD
- Date: `2026-02-10`

## Summary
Implemented a proper "leave organization" feature that soft-deletes the user's membership via `DELETE /orgs/{orgId}/members/me`, replacing the previous workaround that only downgraded the user's role to viewer. The backend handler validates the last-owner constraint and notifies org admins/owners. The frontend hook now calls the DELETE endpoint directly.

## Context
- Background: The existing leave flow used `useUpdateRole` to downgrade the user to "viewer" — a temporary workaround that left the user visible in the members list.
- Problem statement: Users who "left" were still listed as members with viewer role. A proper leave should remove the membership entirely (soft delete).
- Constraints: Must follow existing Clean Architecture + CQRS patterns. Audit logging is automatic via DbContext.

## Decisions & Trade-offs
- **Decision:** Use soft-delete on Membership entity (already has `SoftDelete()` method)
  - Options considered: Hard delete vs soft delete
  - Why this choice: Consistent with existing soft-delete pattern used across the codebase
  - Consequences / risks: Soft-deleted memberships remain in DB but are filtered by global query filter

- **Decision:** Added `MemberLeft` to `NotificationType` enum
  - Options considered: Reuse `RoleChanged` vs new type
  - Why this choice: Semantically distinct event, allows different notification rendering in the frontend
  - Consequences / risks: None — additive enum change

- **Decision:** Notify only admins and owners (not all members)
  - Options considered: Notify all members vs admins/owners only
  - Why this choice: Regular members don't need to know about departures; admins/owners are the management layer

## Implementation Notes
- Key changes:
  - Created `LeaveOrganizationCommand` + `LeaveOrganizationCommandHandler` following existing CQRS patterns
  - Handler validates last-owner constraint, soft-deletes membership, notifies admins/owners
  - Added `DELETE /orgs/{orgId}/members/me` endpoint to `OrgsController`
  - Updated `useLeaveOrg` hook to call DELETE endpoint instead of PUT role downgrade
  - Updated i18n success messages to reflect actual membership removal
- Edge cases handled:
  - Last owner cannot leave (DomainException with CANNOT_REMOVE_LAST_OWNER)
  - Non-existent membership returns NotFoundException
  - Client-side last-owner check prevents unnecessary API calls
- Known limitations:
  - No notification to the leaving user themselves (they know they're leaving)

## Data / Schema / Migrations
- DB changes: None — Membership already supports soft delete
- Migration strategy: N/A
- Backward compatibility: Full — new endpoint, existing entities

## Commands Run
```bash
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln
dotnet test services/orgs-api/tests/SaasTemplate.OrgsApi.UnitTests/SaasTemplate.OrgsApi.UnitTests.csproj
dotnet test services/orgs-api/tests/SaasTemplate.OrgsApi.ArchitectureTests/SaasTemplate.OrgsApi.ArchitectureTests.csproj
cd apps/web && npm test
cd apps/web && npm run build  # Failed due to pre-existing TypeScript error in RegisterForm.tsx (unrelated to this task)
```

## Files Changed
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Organizations/Commands/LeaveOrganization/LeaveOrganizationCommand.cs` — new MediatR command record
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Organizations/Commands/LeaveOrganization/LeaveOrganizationCommandHandler.cs` — handler with last-owner validation, soft-delete, and admin notification
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/OrgsController.cs` — added `DELETE /orgs/{orgId}/members/me` endpoint
- `services/orgs-api/src/SaasTemplate.OrgsApi.Domain/Notifications/NotificationType.cs` — added `MemberLeft` enum value
- `services/orgs-api/tests/SaasTemplate.OrgsApi.UnitTests/Application/Organizations/Commands/LeaveOrganizationCommandHandlerTests.cs` — 9 unit tests for the handler
- `apps/web/src/hooks/orgs.ts` — updated `useLeaveOrg` to call DELETE endpoint
- `apps/web/src/hooks/__tests__/orgs.test.ts` — updated hook tests for new DELETE behavior
- `apps/web/messages/en.json` — updated leave success description
- `apps/web/messages/es.json` — updated leave success description
- `docs/backlog/epic-004-org-management.md` — status PENDING -> IN_PROGRESS -> DONE
- `docs/tasks/0036-leave-org.md` — status updated, DoD checkboxes checked

## Tests
### Unit
- What was added/updated: `LeaveOrganizationCommandHandlerTests` with 9 tests: valid member soft-delete, not-found exception, last-owner block, multiple-owners allowed, save changes called, not-found skips save, last-owner doesn't soft-delete, admin/owner notifications, cancellation token propagation
- How to run: `dotnet test services/orgs-api/tests/SaasTemplate.OrgsApi.UnitTests/SaasTemplate.OrgsApi.UnitTests.csproj`
- Results: 193 passed, 0 failed

### Frontend
- What was updated: `useLeaveOrg` hook tests updated to verify DELETE calls instead of PUT role downgrade
- How to run: `cd apps/web && npm test`
- Results: 271 passed, 0 failed

### Integration
- Skipped: Docker not available in current environment (pre-existing limitation)

### Manual
- N/A

## Observability
- Logs added/updated: Automatic via middleware
- Traces/metrics added/updated: Automatic via OpenTelemetry

## Security
- Validation: UserId extracted from JWT claims (no user input for user identification)
- AuthN/AuthZ impact: Endpoint requires authentication. User can only remove their own membership (`/members/me`).
- Sensitive data handling: None

## Follow-ups / Backlog
- [ ] Task 0037: Delete organization (soft delete)
- [ ] Task 0038: Remove member by admin/owner

## Checklist
- [x] Task scope matches `docs/tasks/0036-leave-org.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
