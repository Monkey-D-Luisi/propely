# Task: cr-0031-auth-rate-limiting-pr-review

## Metadata
- ID: cr-0031
- Type: Code Review
- Status: DONE
- Owner: Agent
- Created: 2026-02-09
- PR: #221
- Branch: feat/auth-rate-limiting-0030
- Base Branch: main

## PR Metadata
- PR Number: 221
- PR URL: https://github.com/Monkey-D-Luisi/saas-template/pull/221
- PR Title: feat(auth): add auth-specific rate limiting with Retry-After UX
- Author: Monkey-D-Luisi
- Target Branch: main
- CI Status Summary:
  - Detect Changes: SUCCESS
  - AI API - Build & Test: SKIPPED
  - Orgs API - Build & Test: SUCCESS
  - Web - Build & Test: SUCCESS

## Changed Files
- `.env.example`
- `apps/web/messages/en.json`
- `apps/web/messages/es.json`
- `apps/web/src/components/auth/ForgotPasswordForm.tsx`
- `apps/web/src/components/auth/LoginForm.tsx`
- `apps/web/src/components/auth/RegisterForm.tsx`
- `apps/web/src/components/auth/__tests__/ForgotPasswordForm.test.tsx`
- `apps/web/src/components/auth/__tests__/LoginForm.test.tsx`
- `apps/web/src/components/auth/__tests__/RegisterForm.test.tsx`
- `apps/web/src/lib/__tests__/api.test.ts`
- `apps/web/src/lib/api.ts`
- `docs/backlog/epic-002-auth-security.md`
- `docs/walkthroughs/0030-auth-rate-limiting.md`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/DependencyInjection.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/appsettings.json`
- `services/orgs-api/tests/SaasTemplate.OrgsApi.IntegrationTests/Api/AuthEndpointTests.cs`
- `services/orgs-api/tests/SaasTemplate.OrgsApi.IntegrationTests/Api/FeatureFlagEndpointTests.cs`
- `services/orgs-api/tests/SaasTemplate.OrgsApi.IntegrationTests/Api/NotificationEndpointTests.cs`
- `services/orgs-api/tests/SaasTemplate.OrgsApi.IntegrationTests/Api/OrgsEndpointTests.cs`

## Review Threads

### Unresolved First
- None.

### Resolved
- Inline comment `2781298127`: replied and resolved.
- Inline comment `2781297999`: replied and resolved.
- Inline comment `2781298061`: replied and resolved.
- Inline comment `2781298086`: replied and resolved.
- Inline comment `2781298109`: replied and resolved.
- Inline comment `2781298037`: replied and resolved.
- Inline comment `2781280288`: replied and resolved.
- General review `3771887579`: addressed via code changes and PR summary comment.
- General review `3771869332`: addressed via code changes and PR summary comment.
- Issue comments: informational only; no code action needed.

## Comment Source Counts
- Inline review comments: 7
- General reviews: 2
- Issue comments: 2

## Comment Resolution Plan

### MUST_FIX
- [x] Make retry-after parsing strict in `api.ts` and cover edge cases in tests.
- [x] Remove randomized IP helpers from integration tests to prevent rate-limit flakiness due to collisions.
- [x] Add `X-Real-IP` to CSRF helper requests to avoid accidental global fallback throttling during parallel test execution.

### SHOULD_FIX
- [x] Rename fallback-rate-limit test to match its true scope (`/auth/csrf` + fallback rule).
- [x] Reduce duplicated request construction in new auth rate-limit tests.

### SUGGESTION
- [x] Apply low-risk maintainability improvements from bot review comments.

### QUESTION
- [x] No open questions remain after analysis of all three comment sources.

### OUT_OF_SCOPE
- [x] No out-of-scope items required deferral in this review pass.

## Parity Verification Checklist
- [x] Redirect parity checked (`next` propagation and sanitization)
- [x] Locale source correctness checked (explicit locale + fallback)
- [x] API/UI contract parity checked (fields and payloads)
- [x] Test parity checked (happy + error/validation paths)

## Notes
- This task is generated and executed using `.agent/rules/code-review-workflow.md`.
- All comments from Sources 1/2/3 were read and classified.
- All 7 inline review threads were replied to and resolved on 2026-02-09.
