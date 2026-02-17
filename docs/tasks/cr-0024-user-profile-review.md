# Code Review: cr-0024-user-profile-review

## PR Metadata
- PR: #176 — feat(auth): add user profile page (#0024)
- Branch: `feat/0024-user-profile` → `main`
- CI: All checks passing (Detect Changes, Orgs API, Web)
- Status: **RESOLVED** — All actionable comments addressed in f424eb7

## Review Sources
- 8 inline review comments (Gemini: 4, Copilot: 4)
- 2 general reviews (Gemini summary, Copilot summary)
- 2 issue comments (Codex usage limit, Gemini summary — not actionable)

## Comment Resolution Plan

### MUST_FIX
- [x] **Thread 1+2+8 (Gemini + Copilot)**: Add CSRF protection to `PATCH /auth/me` endpoint and `useUpdateProfile` hook. Task docs say "CSRF on both endpoints" but implementation only has it on password change. Fix implementation + frontend to match docs.

### SHOULD_FIX
- [x] **Thread 3+4+6+7 (Gemini + Copilot)**: Use translated i18n string for form root error instead of `err.message` to avoid exposing raw API messages. Applies to both ProfileForm and ChangePasswordForm.

### OUT_OF_SCOPE
- [ ] **Thread 5 (Copilot)**: AppHeader stale name after profile update. Would require refactoring useCurrentUser into a shared state/context with refetch. Consistent with current app behavior — no component reactively updates after mutations elsewhere.

## Fix Summary (f424eb7)
- **Backend**: Added `ValidateCsrf()` check to `PATCH /auth/me` in `AuthController.cs`
- **Frontend hook**: `useUpdateProfile` now accepts `csrfToken` parameter, sends `x-csrf-token` header
- **ProfileForm**: Added `ensureCsrfToken()` on mount, passes token to update call, disabled submit without CSRF
- **Error handling**: Both ProfileForm and ChangePasswordForm use translated i18n strings for form errors
- **Tests**: Integration tests for UpdateProfile now include CSRF tokens; added `UpdateProfile_WithoutCsrfToken_ShouldReturn403` test; updated ProfileForm frontend tests for CSRF mock
