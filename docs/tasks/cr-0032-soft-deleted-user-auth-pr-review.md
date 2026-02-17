# Task: CR-0032 - Soft Deleted User Auth PR Review

## Metadata
- ID: CR-0032
- Type: Code Review
- Status: DONE
- Owner: Agent
- Created: 2026-02-09
- Completed: 2026-02-09
- Related PR: #222
- Scope: `feat/soft-deleted-user-auth-0031`

## PR Metadata
- PR Link: https://github.com/Monkey-D-Luisi/saas-template/pull/222
- Target Branch: `main`
- Source Branch: `feat/soft-deleted-user-auth-0031`
- CI Status Summary:
  - `Detect Changes`: SUCCESS
  - `Orgs API - Build & Test`: SUCCESS
  - `Web - Build & Test`: SUCCESS
  - `AI API - Build & Test`: SKIPPED (no AI API changes)

## Review Source Counts (Step 1.2)
- Inline review comments (`pulls/{pr}/comments`): 8
- General reviews (`pulls/{pr}/reviews`): 2
- Issue comments (`gh pr view --json comments`): 2

## Changed Files (PR)
- `apps/web/messages/en.json`
- `apps/web/messages/es.json`
- `apps/web/src/components/auth/VerifyEmailContent.tsx`
- `apps/web/src/components/auth/__tests__/VerifyEmailContent.test.tsx`
- `apps/web/src/hooks/__tests__/orgs.test.ts`
- `apps/web/src/hooks/orgs.ts`
- `apps/web/src/lib/auth-events.ts`
- `docs/backlog/epic-002-auth-security.md`
- `docs/tasks/0031-soft-deleted-user-auth.md`
- `docs/walkthroughs/0029-email-verification.md`
- `docs/walkthroughs/0031-soft-deleted-user-auth.md`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Middleware/ActiveUserMiddleware.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Program.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Domain/Users/User.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Migrations/20260209090508_AddUserSoftDelete.Designer.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Migrations/20260209090508_AddUserSoftDelete.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Migrations/AppDbContextModelSnapshot.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Configurations/UserConfiguration.cs`
- `services/orgs-api/tests/SaasTemplate.OrgsApi.IntegrationTests/Api/SoftDeletedUserAuthTests.cs`

## Review Threads
### Unresolved First (at review start)
- `r2781585938` (`services/orgs-api/src/SaasTemplate.OrgsApi.Api/Middleware/ActiveUserMiddleware.cs:37`) - active-user check bypass risk on anonymous/non-attributed endpoints.
- `r2781585947` (`services/orgs-api/src/SaasTemplate.OrgsApi.Api/Middleware/ActiveUserMiddleware.cs:54`) - performance concern on full entity load for existence check.
- `r2781646868` (`services/orgs-api/tests/SaasTemplate.OrgsApi.IntegrationTests/Api/SoftDeletedUserAuthTests.cs:31`) - use RFC 5737 test IP range.
- `r2781646904` (`services/orgs-api/src/SaasTemplate.OrgsApi.Api/Middleware/ActiveUserMiddleware.cs:31`) - metadata-only auth detection may miss fallback policy.
- `r2781646922` (`services/orgs-api/src/SaasTemplate.OrgsApi.Api/Middleware/ActiveUserMiddleware.cs:47`) - missing integration test for malformed `sub`.
- `r2781646942` (`docs/walkthroughs/0031-soft-deleted-user-auth.md:80`) - walkthrough status mismatch (`IN_PROGRESS` vs `DONE`).
- `r2781646959` (`services/orgs-api/tests/SaasTemplate.OrgsApi.IntegrationTests/Api/SoftDeletedUserAuthTests.cs:55`) - duplicate password literal.
- `r2781646983` (`services/orgs-api/tests/SaasTemplate.OrgsApi.IntegrationTests/Api/SoftDeletedUserAuthTests.cs:117`) - duplicate password literal (same issue).

### Resolved in This Pass
- Middleware now validates every authenticated principal, including fallback-policy routes.
- For anonymous endpoints with stale/invalid authenticated principals, middleware now clears auth cookie and downgrades current principal to anonymous before continuing.
- `IUserRepository.ExistsByIdAsync` added and used in middleware to avoid full entity hydration.
- Integration test added for malformed/non-GUID `sub` claim path.
- Integration tests updated to RFC 5737 IP range and shared password constant.
- Walkthrough line corrected to reflect task `0031` moved to `DONE`.

## Comment Resolution Plan
### MUST_FIX
- [x] `r2781585938` + `r2781646904`: remove `IAuthorizeData` dependency and enforce active-user validation for authenticated principals consistently.
- [x] `r2781646922`: add integration coverage for malformed `sub` claim path returning `401`.

### SHOULD_FIX
- [x] `r2781585947`: add lightweight repository existence check for middleware path.
- [x] `r2781646942`: fix walkthrough status text mismatch.

### SUGGESTION
- [x] `r2781646868`: use documentation-only IP range (`203.0.113.x`) in tests.
- [x] `r2781646959` + `r2781646983`: replace duplicated password literals with one constant.

### QUESTION
- [x] None.

### OUT_OF_SCOPE
- [x] None.

## Mandatory Parity Checks
- [x] Redirect parity checked (`next` propagation and sanitization)
  - Auth entry points (`login`, `register`, OAuth start/callback) still sanitize and propagate `next` consistently.
- [x] Locale source correctness checked (explicit locale + fallback)
  - `ResendVerificationRequest.Locale` is used when present, with deterministic normalization fallback.
- [x] API/UI contract parity checked (fields and payloads)
  - Web resend payload still sends `locale`; API contract remains aligned.
- [x] Test parity checked (happy + error/validation paths)
  - API side has happy/error coverage for middleware behavior, including malformed identity claim path.

## Notes
- Source 3 issue comments were informational only (no actionable code feedback).
- `dotnet build/test` pass with pre-existing NU/MSB warnings unrelated to this review pass.
- All 8 PR review threads were replied to and marked resolved.
