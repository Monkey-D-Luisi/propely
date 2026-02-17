# Epic 002: Auth Completion & Security

## Overview

Complete the authentication system with OAuth social login (Google + GitHub), forgot password / password reset via email token, email verification flow, and auth-specific rate limiting. These are security-critical gaps that block a production-ready SaaS template.

## Success Criteria

- Users can sign in with Google or GitHub OAuth
- Users can reset their password via an email token link
- New registrations require email verification before full access
- Login endpoint has brute-force protection (per-IP/per-user rate limiting)
- Soft-deleted users are properly handled in /auth/me
- All new flows have i18n strings (EN + ES)
- Frontend forms use react-hook-form + zod

## Technology Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| OAuth | ASP.NET Identity external providers | Built-in Google/GitHub support, cookie-based flow |
| Password reset | Time-limited JWT token via email | Stateless, no DB token table needed |
| Email verification | Verification token on registration | Standard SaaS pattern, prevents fake accounts |

## Task List

> **Convention:** Each task's PR must include `Closes #<issue>` in the PR body to auto-close the GitHub Issue.

### Task 0027 - OAuth social login (Google + GitHub)
- **Status:** DONE
- **GitHub Issue:** #179
- **Dependencies:** None
- **File:** `docs/tasks/0027-oauth-social-login.md`
- **Scope:** Add Google and GitHub OAuth providers to ASP.NET auth. Create frontend buttons on login/register pages. Handle account linking for existing email users. Add i18n strings.
- **Old Issue:** #8

### Task 0028 - Forgot password / password reset via email token
- **Status:** DONE
- **GitHub Issue:** #180
- **Dependencies:** 0039 (email templating)
- **File:** `docs/tasks/0028-password-reset.md`
- **Scope:** Add forgot-password endpoint (generates token, sends email). Add reset-password endpoint (validates token, sets new password). Create frontend forgot/reset pages. Add i18n strings.
- **Old Issue:** #9

### Task 0029 - Email verification flow
- **Status:** DONE
- **GitHub Issue:** #181
- **Dependencies:** 0039 (email templating)
- **File:** `docs/tasks/0029-email-verification.md`
- **Scope:** Send verification email on registration. Add verify endpoint. Restrict access for unverified users. Add resend verification option. Add i18n strings.

### Task 0030 - Auth-specific rate limiting (login brute-force)
- **Status:** DONE
- **GitHub Issue:** #182
- **Dependencies:** None
- **File:** `docs/tasks/0030-auth-rate-limiting.md`
- **Scope:** Configure endpoint-specific rate limits for /auth/login (stricter than global 100/min). Add per-IP and optionally per-email throttling. Return appropriate 429 responses with Retry-After header.
- **Old Issue:** Codex #87

### Task 0031 - Soft-deleted user auth handling verification
- **Status:** DONE
- **GitHub Issue:** #183
- **Dependencies:** None
- **File:** `docs/tasks/0031-soft-deleted-user-auth.md`
- **Scope:** Verify that /auth/me correctly returns 401 for soft-deleted users. Add integration tests. Fix any gaps found.
- **Old Issue:** Codex #89

## Progress Tracker

| Phase | Tasks | Done | Remaining |
|-------|-------|------|-----------|
| Auth Completion | 0027-0031 | 5 | 0 |
| **Total** | **5** | **5** | **0** |

## Dependency Graph

```
0027 (OAuth) ── standalone
0039 (Email, Epic 005) ──► 0028 (Password reset)
0039 (Email, Epic 005) ──► 0029 (Email verification)
0030 (Rate limiting) ── standalone
0031 (Soft-deleted auth) ── standalone
```
