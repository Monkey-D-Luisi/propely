# Code Review: cr-0040 — leave-org-pr237-review

## PR Metadata
- PR: #237
- Title: feat(orgs): add leave organization endpoint with soft-delete (#0036)
- Branch: `feat/0036-leave-org` -> `main`
- CI Status: All checks passed (Detect Changes: SUCCESS, Orgs API Build & Test: SUCCESS, Web Build & Test: SUCCESS, AI API: SKIPPED)

## Changed Files
1. `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Organizations/Commands/LeaveOrganization/LeaveOrganizationCommand.cs`
2. `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Organizations/Commands/LeaveOrganization/LeaveOrganizationCommandHandler.cs`
3. `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/OrgsController.cs`
4. `services/orgs-api/src/SaasTemplate.OrgsApi.Domain/Notifications/NotificationType.cs`
5. `services/orgs-api/tests/SaasTemplate.OrgsApi.UnitTests/Application/Organizations/Commands/LeaveOrganizationCommandHandlerTests.cs`
6. `apps/web/src/hooks/orgs.ts`
7. `apps/web/src/hooks/__tests__/orgs.test.ts`
8. `apps/web/messages/en.json`
9. `apps/web/messages/es.json`
10. `docs/walkthroughs/0036-leave-org.md`
11. `docs/tasks/0036-leave-org.md`
12. `docs/backlog/epic-004-org-management.md`

## Review Threads (5 inline comments, 2 general reviews, 2 issue comments)

### Comment 1 — Gemini (id: 2786148367)
- **File**: `LeaveOrganizationCommandHandler.cs:33-36`
- **Claim**: Two sequential DB calls can be merged into one by using `GetByOrgIdAsync` first, then finding the specific user's membership in memory.
- **Classification**: SHOULD_FIX
- **Action**: Implement — valid optimization that reduces DB roundtrips from 2 to 1. Low risk.

### Comment 2 — Copilot (id: 2786164943)
- **File**: `LeaveOrganizationCommandHandler.cs:41-45`
- **Claim**: Last-owner check is not concurrency-safe; two owners could leave simultaneously.
- **Classification**: OUT_OF_SCOPE
- **Rationale**: `UpdateMemberRoleCommandHandler` uses the identical check-then-act pattern without transactional isolation. This is a pre-existing cross-cutting concern. Fixing it here without fixing all handlers would create inconsistency. Should be addressed as a separate infrastructure task if needed.

### Comment 3 — Copilot (id: 2786164974)
- **File**: `LeaveOrganizationCommandHandler.cs:36-40`
- **Claim**: Loading all memberships is expensive for large orgs; add targeted queries.
- **Classification**: OUT_OF_SCOPE
- **Rationale**: Same pattern used in `UpdateMemberRoleCommandHandler`. For a SaaS template with typical org sizes, this is premature optimization. Would require new repository methods that expand scope.

### Comment 4 — Copilot (id: 2786165001)
- **File**: `docs/walkthroughs/0036-leave-org.md:56-58`
- **Claim**: Walkthrough lists `npm run build` as a command run, but PR says it wasn't run due to pre-existing error.
- **Classification**: SHOULD_FIX
- **Action**: Clarify in walkthrough that `npm run build` was attempted but failed due to pre-existing error.

### Comment 5 — Copilot (id: 2786165025)
- **File**: `docs/tasks/0036-leave-org.md:132-134`
- **Claim**: Task DoD marks "Build passes" as complete but PR says `dotnet build` wasn't run.
- **Classification**: QUESTION
- **Rationale**: This is a misread. `dotnet build` WAS run and succeeded. The unchecked `[ ] dotnet build` in the PR Testing section are reviewer checkboxes (verification checklist), not a record of what was executed. The DoD checkbox is correct.

## Comment Resolution Plan

### SHOULD_FIX
- [x] Comment 1: Merge two DB calls into one in `LeaveOrganizationCommandHandler`
- [x] Comment 4: Clarify `npm run build` in walkthrough "Commands Run" section

### OUT_OF_SCOPE
- [x] Comment 2: Concurrency safety — pre-existing pattern, cross-cutting concern
- [x] Comment 3: Targeted queries — premature optimization, consistent with existing handlers

### QUESTION
- [x] Comment 5: Clarify that `dotnet build` was indeed run and passed

## Parity Verification Checklist
- [x] Redirect parity checked — N/A, no auth redirects in this PR
- [x] Locale source correctness checked — N/A, no localized backend emails/redirects
- [x] API/UI contract parity checked — DELETE returns `{ ok: true }`, frontend doesn't parse response body, error handling via existing exception middleware
- [x] Test parity checked — 9 backend unit tests (happy + error paths), frontend hook tests updated, LeaveOrgButton tests unchanged and passing
