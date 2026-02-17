# Walkthrough: audit-0014-integration-tests-delete-endpoints

## Task Reference
- Task: `docs/tasks/audit-0014-integration-tests-delete-endpoints.md`
- Walkthrough: `docs/walkthroughs/audit-0014-integration-tests-delete-endpoints.md`
- Branch/PR: `fix/audit-epic-004-batch`
- Date: `2026-02-10`

## Summary
Added 14 integration tests for the three DELETE endpoints on the OrgsController, using Testcontainers PostgreSQL and the existing `ApiWebApplicationFactory` fixture.

## Context
- Background: The three DELETE endpoints added in Epic 004 had unit tests but no integration tests verifying end-to-end behavior including auth middleware, EF Core, and database constraints.
- Problem statement: No integration-level coverage for destructive operations.

## Files Changed
- `services/orgs-api/tests/SaasTemplate.OrgsApi.IntegrationTests/Api/OrgsDeleteEndpointTests.cs` (CREATED) — 14 integration tests

## Tests
### Integration (14 tests)

**Leave Organization (4 tests):**
1. `LeaveOrg_AsMember_ShouldReturn200` — Member leaves org successfully
2. `LeaveOrg_AsLastOwner_ShouldReturn400` — Last owner cannot leave
3. `LeaveOrg_WhenNotAuthenticated_ShouldReturn401` — Unauthenticated returns 401
4. `LeaveOrg_WhenNotMember_ShouldReturn403` — Non-member returns 403

**Remove Member (6 tests):**
5. `RemoveMember_OwnerRemovesMember_ShouldReturn200` — Owner removes member successfully
6. `RemoveMember_MemberRemovesMember_ShouldReturn403` — Member cannot remove other members
7. `RemoveMember_RemoveSelf_ShouldReturn400` — Cannot remove self
8. `RemoveMember_TargetNotFound_ShouldReturn404` — Target user not in org
9. `RemoveMember_WhenNotAuthenticated_ShouldReturn401` — Unauthenticated returns 401
10. `RemoveMember_WhenNotMember_ShouldReturn403` — Non-member returns 403

**Delete Organization (4 tests):**
11. `DeleteOrg_AsOwner_ShouldReturn200` — Owner deletes org successfully, verified via GET
12. `DeleteOrg_AsMember_ShouldReturn403` — Member cannot delete org
13. `DeleteOrg_WhenNotAuthenticated_ShouldReturn401` — Unauthenticated returns 401
14. `DeleteOrg_WhenNotMember_ShouldReturn403` — Non-member returns 403

## Checklist
- [x] Task scope matches `docs/tasks/audit-0014-integration-tests-delete-endpoints.md`
- [x] Tests updated and passing
- [x] No secrets committed
