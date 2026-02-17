# Code Review: cr-0041 — delete-org-pr238-review

## PR Metadata
- PR: #238
- Title: feat(orgs): add delete organization endpoint with soft-delete (#0037)
- Branch: `feat/0037-delete-org` -> `main`
- CI Status: All checks passed (Detect Changes: SUCCESS, Orgs API Build & Test: SUCCESS, Web Build & Test: SUCCESS, AI API: SKIPPED)

## Changed Files
1. `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Organizations/Commands/DeleteOrganization/DeleteOrganizationCommand.cs`
2. `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Organizations/Commands/DeleteOrganization/DeleteOrganizationCommandHandler.cs`
3. `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/OrgsController.cs`
4. `services/orgs-api/src/SaasTemplate.OrgsApi.Domain/Notifications/NotificationType.cs`
5. `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Organizations/Interfaces/IInvitationRepository.cs`
6. `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Repositories/InvitationRepository.cs`
7. `services/orgs-api/tests/SaasTemplate.OrgsApi.UnitTests/Application/Organizations/Commands/DeleteOrganizationCommandHandlerTests.cs`
8. `apps/web/src/components/orgs/DeleteOrgSection.tsx`
9. `apps/web/src/components/orgs/OrgSettingsForm.tsx`
10. `apps/web/src/components/orgs/__tests__/OrgSettingsForm.test.tsx`
11. `apps/web/src/hooks/orgs.ts`
12. `apps/web/src/hooks/__tests__/orgs.test.ts`
13. `apps/web/messages/en.json`
14. `apps/web/messages/es.json`
15. `apps/web/src/components/auth/RegisterForm.tsx`
16. `apps/web/src/components/auth/ResetPasswordForm.tsx`
17. `docs/walkthroughs/0037-delete-org.md`
18. `docs/tasks/0037-delete-org.md`
19. `docs/backlog/epic-004-org-management.md`

## Review Threads (7 inline comments, 2 general reviews, 2 issue comments)

### Comment 1 — Copilot (id: 2786351509)
- **File**: `DeleteOrgSection.tsx:52`
- **Claim**: Error handler swallows the exception and shows generic toast. Should branch on known statuses (403 -> forbidden, 404 -> not-found) and include error message for unexpected failures.
- **Classification**: SHOULD_FIX
- **Action**: Improve error handling using the existing `isApiError` helper (codebase pattern) to distinguish 403 from generic errors. Will not use Copilot's verbose `(error as any).status` pattern.

### Comment 2 — Copilot (id: 2786351534)
- **File**: `docs/walkthroughs/0037-delete-org.md:38`
- **Claim**: Walkthrough lists `npm run build` under "Commands Run" but PR says it fails due to pre-existing TS error.
- **Classification**: SHOULD_FIX
- **Action**: Update walkthrough — `npm run build` now passes after the second commit that fixed the pre-existing TypeScript errors in RegisterForm.tsx and ResetPasswordForm.tsx.

### Comment 3 — Copilot (id: 2786351552)
- **File**: `docs/tasks/0037-delete-org.md:145`
- **Claim**: DoD "Build passes" is checked but PR description says web build fails.
- **Classification**: QUESTION
- **Rationale**: The second commit (`fix(web): add type cast for router.replace`) fixed the pre-existing TypeScript errors. `npm run build` now passes. The DoD checkbox is correct.

### Comment 4 — Copilot (id: 2786351569)
- **File**: `DeleteOrganizationCommandHandlerTests.cs:255`
- **Claim**: Test "ShouldPassCancellationTokenToAllDependencies" doesn't assert `_notificationRepository.AddRangeAsync` receives the cancellation token.
- **Classification**: SHOULD_FIX
- **Action**: Add the missing assertion for `_notificationRepository.Received(1).AddRangeAsync(Arg.Any<IEnumerable<Notification>>(), token)`.

### Comment 5 — Gemini (id: 2786353963)
- **File**: `DeleteOrgSection.tsx:49`
- **Claim**: Catch block should capture the error object for logging/debugging.
- **Classification**: SHOULD_FIX
- **Action**: Address together with Comment 1 — capturing the error for `isApiError` status checking naturally resolves this.

### Comment 6 — Gemini (id: 2786353970)
- **File**: `DeleteOrganizationCommandHandler.cs:46`
- **Claim**: Two DB queries (members + org) could be combined into one via `GetByIdWithMembersAsync`.
- **Classification**: OUT_OF_SCOPE
- **Rationale**: Would require a new repository method, navigation property, and EF Core configuration changes. The handler is for a rare destructive operation — premature optimization. Same two-query pattern used in `LeaveOrganizationCommandHandler` and `UpdateMemberRoleCommandHandler`.

### Comment 7 — Gemini (id: 2786353977)
- **File**: `DeleteOrganizationCommandHandlerTests.cs:185`
- **Claim**: Notification test should also verify notifications are sent to the correct users (memberId and adminId), not just count.
- **Classification**: SHOULD_FIX
- **Action**: Add user ID verification to the notification assertion.

## Comment Resolution Plan

### SHOULD_FIX
- [x] Comments 1+5: Improve error handling in `DeleteOrgSection.tsx` using `isApiError`
- [x] Comment 2: Update walkthrough to reflect that `npm run build` passes
- [x] Comment 4: Add missing notification token assertion in cancellation token test
- [x] Comment 7: Add user ID verification to notification test

### OUT_OF_SCOPE
- [x] Comment 6: Combined DB query — premature optimization, consistent with existing handlers

### QUESTION
- [x] Comment 3: Clarify that build passes after second commit

## Parity Verification Checklist
- [x] Redirect parity checked — N/A, no auth redirects added (TS fix for pre-existing issue)
- [x] Locale source correctness checked — N/A, no localized backend emails/redirects
- [x] API/UI contract parity checked — DELETE returns `{ ok: true }`, frontend doesn't parse response body, error handling via existing exception middleware
- [x] Test parity checked — 10 backend unit tests (happy + error paths), frontend hook tests, OrgSettingsForm tests updated
