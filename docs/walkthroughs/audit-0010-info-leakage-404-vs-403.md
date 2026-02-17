# Walkthrough: audit-0010-info-leakage-404-vs-403

## Task Reference
- Task: `docs/tasks/audit-0010-info-leakage-404-vs-403.md`
- Walkthrough: `docs/walkthroughs/audit-0010-info-leakage-404-vs-403.md`
- Branch/PR: `fix/audit-epic-004-batch`
- Date: `2026-02-10`

## Summary
Changed "requesting member not found" from `NotFoundException` (HTTP 404) to `ForbiddenException` (HTTP 403) in all three command handlers. This prevents authenticated users from probing arbitrary org IDs to determine membership status by observing the status code difference.

## Context
- Background: All three handlers (Leave, Remove, Delete) threw `NotFoundException("Member not found.")` when the requesting user was not a member of the org, but threw `ForbiddenException` when they were a member but lacked permission. The middleware maps these to HTTP 404 and 403 respectively.
- Problem statement: An authenticated attacker could probe arbitrary org IDs to determine whether they are a member by observing whether they get 404 or 403.
- Constraints: Target member not found in RemoveMember must remain as NotFoundException (legitimate entity lookup by an authorized user).

## Decisions & Trade-offs
- **Decision: Use 403 for both "not a member" and "insufficient role"**
  - Options considered: (1) Always return 403, (2) Always return 404, (3) Return a generic error
  - Why this choice: Returning 403 for both cases is the standard approach. Non-members cannot distinguish between "you're not a member" and "you don't have permission." The message is generic ("Not authorized.") to avoid leaking any further information. Using 404 for everything would confuse legitimate users who ARE members but lack permissions.
  - Consequences: The frontend error handling already handles 403 responses correctly. No frontend changes needed.

- **Decision: Keep NotFoundException for target member in RemoveMember**
  - Why: When a requesting user IS authorized (passed the membership check), looking up a specific target member that doesn't exist is a legitimate entity-not-found scenario, not an authorization concern.

## Files Changed
- `LeaveOrganizationCommandHandler.cs:37` — `NotFoundException("Member not found.")` → `ForbiddenException("Not authorized.")`
- `RemoveMemberCommandHandler.cs:44` — `NotFoundException("Requesting member not found.")` → `ForbiddenException("Not authorized.")`
- `DeleteOrganizationCommandHandler.cs:38` — `NotFoundException("Member not found.")` → `ForbiddenException("Not authorized.")`
- `LeaveOrganizationCommandHandlerTests.cs` — Updated two tests to expect `ForbiddenException` instead of `NotFoundException`
- `DeleteOrganizationCommandHandlerTests.cs` — Updated one test to expect `ForbiddenException`
- `RemoveMemberCommandHandlerTests.cs` — Added `Handle_RequestingMemberNotFound_ShouldThrowForbiddenException` test

## Tests
### Unit
- All 219 unit tests pass
- Updated `Handle_WhenMemberNotFound_ShouldThrowForbiddenException` in LeaveOrganization and DeleteOrganization tests
- Added `Handle_RequestingMemberNotFound_ShouldThrowForbiddenException` in RemoveMember tests
- `Handle_TargetNotFound_ShouldThrowNotFoundException` in RemoveMember tests unchanged (correct behavior)

## Checklist
- [x] Task scope matches `docs/tasks/audit-0010-info-leakage-404-vs-403.md`
- [x] Tests updated and passing
- [x] No secrets committed
