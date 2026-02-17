# CR-0029: Password Reset Follow-Up Review

## PR Metadata
- PR: [#219](https://github.com/Monkey-D-Luisi/saas-template/pull/219)
- Title: `feat(auth): password reset flow and register confirmation`
- Branch: `feat/password-reset-0028` -> `main`
- State: OPEN
- CI snapshot during this review pass:
  - Detect Changes: SUCCESS
  - AI API - Build & Test: SKIPPED
  - Orgs API - Build & Test: SUCCESS
  - Web - Build & Test: SUCCESS

## Review Comment Counts (Mandatory Verification)
- Inline review comments (source 1): `4`
- General reviews (source 2): `2`
- Issue comments (source 3): `2`

## Changed Files In This Follow-Up Pass
- `.agent/rules/code-review-workflow.md`
- `apps/web/src/components/auth/ForgotPasswordForm.tsx`
- `apps/web/src/components/auth/RegisterForm.tsx`
- `apps/web/src/components/auth/__tests__/ForgotPasswordForm.test.tsx`
- `apps/web/src/components/auth/__tests__/RegisterForm.test.tsx`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/AuthController.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Dtos/ForgotPasswordRequest.cs`
- `services/orgs-api/tests/SaasTemplate.OrgsApi.IntegrationTests/Api/AuthEndpointTests.cs`

## Review Threads

### Source 1: Inline Comments (4)
1. `2779697638` - gemini-code-assist - `ResetPasswordForm.tsx`
   - Claim: move `extractApiErrorCode` to shared utility.
   - Classification: SUGGESTION.
2. `2779697640` - gemini-code-assist - `AuthController.cs`
   - Claim: centralize locale normalization utility.
   - Classification: SHOULD_FIX (follow-up refactor).
3. `2779699301` - Copilot - `ResetPasswordCommandHandler.cs`
   - Claim: token single-use check is non-atomic under concurrency.
   - Classification: MUST_FIX (security correctness).
4. `2779699306` - Copilot - `docs/walkthroughs/0028-password-reset.md`
   - Claim: walkthrough status text must match final DONE state.
   - Classification: MUST_FIX.

### Source 2: General Reviews (2)
1. `3770196679` - gemini-code-assist - COMMENTED
   - Summary: positive review with maintainability suggestions.
   - Classification: SHOULD_FIX + SUGGESTION.
2. `3770198569` - copilot-pull-request-reviewer - COMMENTED
   - Summary: positive review plus actionable inline findings.
   - Classification: MUST_FIX + SHOULD_FIX.

### Source 3: Issue Comments (2)
1. `IC_kwDOP0hK4s7mjRQl` - chatgpt-codex-connector
   - Summary: usage-limit notification.
   - Classification: OUT_OF_SCOPE.
2. `IC_kwDOP0hK4s7mjRdS` - gemini-code-assist
   - Summary: automated PR summary/changelog.
   - Classification: OUT_OF_SCOPE.

## Mandatory Behavioral Parity Checks (Automatic)
- [x] Redirect parity checked (`next` propagation and sanitization)
- [x] Locale source correctness checked (explicit locale + fallback)
- [x] API/UI contract parity checked (fields and payloads)
- [x] Test parity checked (happy + error/validation paths)

## Comment Resolution Plan

### MUST_FIX
- [x] Fix locale propagation consistency for forgot-password (`locale` from frontend to backend).
- [x] Fix register success redirect parity so `next` is honored like OAuth/login.
- [ ] Address non-atomic reset-token single-use enforcement under concurrency.
- [ ] Correct walkthrough line that says `IN_PROGRESS` where final state is `DONE`.

### SHOULD_FIX
- [x] Add explicit parity checks to code-review workflow to prevent recurrence.
- [ ] Extract locale normalization into a generic localization utility.

### SUGGESTION
- [ ] Evaluate moving `extractApiErrorCode` to shared web API utility when another consumer appears.

### OUT_OF_SCOPE
- [x] Ignore non-actionable issue comments.

## Status
- IN_PROGRESS
