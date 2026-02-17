# Audit Action: audit-0002-timing-attack-mitigation

## Metadata
- ID: audit-0002
- Type: AuditAction
- Status: DONE
- Owner: Agent
- Created: 2026-02-09
- Priority: P0
- Source:
  - Executive summary: `docs/audits/epic-002-executive-summary.md`
  - Action plan item: "#2 - Timing attack mitigation in login"
- Dependencies:
  - None
- Related docs:
  - Walkthrough: `docs/walkthroughs/audit-0002-timing-attack-mitigation.md`

## Goal
Prevent timing-based email enumeration by ensuring bcrypt verification always runs during login, regardless of whether the user exists in the database.

## Context
The original `LoginUserCommandHandler` used short-circuit evaluation: `user is null || !_passwordHasher.VerifyPassword(...)`. When the user did not exist, the handler returned immediately (~5-10ms). When the user existed but the password was wrong, bcrypt ran (~150-300ms). This measurable timing difference allowed attackers to enumerate valid email addresses by analyzing response times.

## Scope
### In scope
- Add pre-computed dummy bcrypt hash constant (`TimingSafetyHash`)
- Restructure login logic to always call `VerifyPassword`
- Fix existing unit test that validated the old (insecure) behavior

### Out of scope
- Account lockout after failed attempts (action plan item #3)
- Rate limiting changes
- Logging of failed login attempts

## Requirements
- R1: Bcrypt `VerifyPassword` must execute on every login attempt, regardless of user existence
- R2: Response time difference between existing and non-existing users must be negligible
- R3: Existing unit test must be updated to verify the new timing-safe behavior
- R4: No functional change — valid credentials still succeed, invalid still fail

## Acceptance Criteria
- [x] AC1: `LoginUserCommandHandler` always calls `_passwordHasher.VerifyPassword`
- [x] AC2: When user is not found, bcrypt runs against `TimingSafetyHash` dummy hash
- [x] AC3: Unit test `Handle_WhenUserNotFound_ShouldStillCallPasswordHasherForTimingSafety` asserts `Received(1)` on VerifyPassword
- [x] AC4: Build passes
- [x] AC5: All tests pass (176 unit + 5 architecture + 114 integration)

## Constraints
- The dummy hash must be a valid bcrypt format string to avoid exceptions in the bcrypt library
- The `&&` operator ordering must ensure bcrypt completes before `user is not null` is evaluated

## Implementation Steps
1. Add `private const string TimingSafetyHash = "$2a$11$xxxx..."` constant with pre-computed bcrypt hash
2. Replace short-circuit logic with: `var hashToVerify = user?.PasswordHash ?? TimingSafetyHash`
3. Evaluate: `var isValid = _passwordHasher.VerifyPassword(request.Password, hashToVerify) && user is not null`
4. Keep the same `UnauthorizedAccessException("INVALID_CREDENTIALS")` throw for invalid attempts
5. Rename unit test from `Handle_WhenUserNotFound_ShouldNotCallPasswordHasherOrTokenService` to `Handle_WhenUserNotFound_ShouldStillCallPasswordHasherForTimingSafety`
6. Change test assertion from `_passwordHasher.DidNotReceive().VerifyPassword(...)` to `_passwordHasher.Received(1).VerifyPassword(...)`

## Testing Plan
- Unit tests: Updated existing test to verify timing-safe behavior (VerifyPassword IS called even when user not found)
- Integration tests: Existing login endpoint tests pass unchanged
- Manual checks: Measure response times for existing vs non-existing email — difference should be <10ms

## Definition of Done
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
