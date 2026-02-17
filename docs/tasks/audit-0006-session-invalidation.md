# Audit Action: audit-0006-session-invalidation

## Metadata
- ID: audit-0006
- Type: AuditAction
- Status: DONE
- Owner: Agent
- Created: 2026-02-09
- Priority: P1
- Source:
  - Executive summary: `docs/audits/epic-002-executive-summary.md`
  - Action plan item: "#4 — Invalidate sessions on password change"
- Dependencies:
  - None
- Related docs:
  - Walkthrough: `docs/walkthroughs/audit-0006-session-invalidation.md`

## Goal
Ensure that all existing JWT sessions are invalidated when a user changes or resets their password, preventing continued access with compromised tokens.

## Context
Currently, changing a password does not invalidate existing JWT tokens. If a user's token is compromised, changing the password does not prevent the attacker from using the old token until it expires (24h). Adding a `PasswordVersion` counter to the JWT and validating it on each request ensures that password changes immediately invalidate all existing sessions.

## Scope
### In scope
- `PasswordVersion` property on User entity
- `pwd_ver` claim in access tokens
- Middleware validation of `pwd_ver` against DB
- EF Core migration for new column
- Unit and integration tests

### Out of scope
- Token blacklisting / revocation lists
- Refresh token rotation
- Session management UI

## Requirements
- R1: `User.ChangePassword()` increments `PasswordVersion`
- R2: `JwtTokenService.GenerateToken()` includes `pwd_ver` claim
- R3: `ActiveUserMiddleware` validates `pwd_ver` on every authenticated request
- R4: Missing `pwd_ver` claim defaults to 0 (backward compatibility with existing tokens)
- R5: Password reset also triggers invalidation (via `ChangePassword()`)

## Acceptance Criteria
- [x] Password change invalidates existing JWT tokens
- [x] Password reset also invalidates existing JWT tokens
- [x] Old tokens without `pwd_ver` claim default to version 0 (no disruption)
- [x] New tokens include `pwd_ver` claim matching the user's PasswordVersion
- [x] Unit tests cover middleware validation and handler behavior
- [x] Integration test verifies session invalidation after password change

## Constraints
- C1: Must not break existing OAuth login flow (OAuth users have PasswordVersion 0, never changes)
- C2: Must use a single efficient query in middleware (no N+1)

## Implementation Steps
1. Add `PasswordVersion` property to User entity, increment in `ChangePassword()`
2. Add EF Core column configuration and migration
3. Update `JwtTokenService.GenerateToken()` to include `pwd_ver` claim
4. Add `IsActiveWithPasswordVersionAsync` to IUserRepository
5. Implement in UserRepository (single EXISTS query)
6. Update ActiveUserMiddleware to validate `pwd_ver`
7. Add unit and integration tests

## Testing Plan
- Unit tests: Middleware pwd_ver validation, User.ChangePassword increments version
- Integration tests: Login → change password → old token rejected (401)
- Manual checks: N/A

## Definition of Done
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
