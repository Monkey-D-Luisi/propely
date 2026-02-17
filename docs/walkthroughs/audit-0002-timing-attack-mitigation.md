# Walkthrough: audit-0002-timing-attack-mitigation

## Task Reference
- Task: `docs/tasks/audit-0002-timing-attack-mitigation.md`
- Walkthrough: `docs/walkthroughs/audit-0002-timing-attack-mitigation.md`
- Branch/PR: `fix/epic-002-audit-remediation` / PR #231
- Date: `2026-02-09`

## Summary
Added timing attack mitigation to `LoginUserCommandHandler` by always running bcrypt verification, even when the user is not found. A pre-computed dummy hash (`TimingSafetyHash`) ensures consistent response times regardless of email existence. An existing unit test that validated the old insecure behavior (bcrypt NOT called for missing users) was updated to verify the new timing-safe behavior.

## Context
- Background: Epic 002 implemented the login flow with a standard pattern: check if user exists, then verify password. The audit identified that this pattern leaks information through response time differences.
- Problem statement: Short-circuit evaluation in `LoginUserCommandHandler.cs:25` caused ~5-10ms responses for non-existent emails vs ~150-300ms for existing emails (bcrypt computation), enabling timing-based email enumeration.
- Constraints: The fix must not change the functional behavior — valid credentials succeed, invalid credentials fail with the same error message.

## Decisions & Trade-offs
- **Decision:** Use a hardcoded dummy bcrypt hash constant rather than a runtime-generated hash
  - Options considered: (a) Generate a dummy hash on each request via `_passwordHasher.HashPassword("dummy")`, (b) Pre-compute and hardcode a constant
  - Why this choice: The hash does not need to be secret or valid — it only needs to trigger bcrypt's constant-time computation. A constant avoids an unnecessary hash generation on every failed login for non-existent users. The work factor (11) matches the real password hash work factor.
  - Consequences / risks: The constant is visible in source code, but this is not a security concern — it's a dummy value that never matches any real password.

- **Decision:** Use `&& user is not null` after `VerifyPassword` instead of separate `if` blocks
  - Options considered: (a) Two separate `if` blocks (user null check, then verify), (b) Combined expression with `&&`
  - Why this choice: The `&&` short-circuit in C# guarantees `VerifyPassword` executes first (left operand), then `user is not null` is evaluated. This is more concise and the intent is clear with the accompanying comment. The original code had two separate paths; the combined expression unifies them.
  - Consequences / risks: Developers unfamiliar with the pattern might be confused. A code comment explains the rationale.

## Implementation Notes
- Key changes:
  - Added `private const string TimingSafetyHash = "$2a$11$xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"` (line 10)
  - Replaced short-circuit evaluation with: `var hashToVerify = user?.PasswordHash ?? TimingSafetyHash` (line 32)
  - New validation: `var isValid = _passwordHasher.VerifyPassword(request.Password, hashToVerify) && user is not null` (line 33)
  - Added comments explaining the timing-safe approach (lines 30-31)
- Edge cases handled:
  - User is null → bcrypt runs against dummy hash → `isValid = false` (because `&& user is not null` is false)
  - User exists, wrong password → bcrypt runs against real hash → `isValid = false` (because VerifyPassword returns false)
  - User exists, correct password → bcrypt runs against real hash → `isValid = true`
- Known limitations: Adds ~150-300ms to failed login attempts for non-existent users. This is intentional and expected — it's the cost of timing safety.

- **Unit test fix:** The existing test `Handle_WhenUserNotFound_ShouldNotCallPasswordHasherOrTokenService` was validating the insecure behavior (asserting `DidNotReceive()` on VerifyPassword). It was:
  - Renamed to `Handle_WhenUserNotFound_ShouldStillCallPasswordHasherForTimingSafety`
  - Assertion changed from `_passwordHasher.DidNotReceive().VerifyPassword(...)` to `_passwordHasher.Received(1).VerifyPassword(...)`
  - The `_jwtTokenService.DidNotReceive().GenerateToken(...)` assertion remains (token should NOT be generated for non-existent users)

## Data / Schema / Migrations
- DB changes: None
- Migration strategy: N/A
- Backward compatibility: N/A

## Commands Run
```bash
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln
```

Note: After the initial fix, the build cache needed to be cleared with an explicit rebuild because the test project was still using the cached old handler. Running `dotnet build` before `dotnet test` resolved the issue.

## Files Changed
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Users/Commands/LoginUser/LoginUserCommandHandler.cs` — Added `TimingSafetyHash` constant, restructured verification logic to always run bcrypt
- `services/orgs-api/tests/SaasTemplate.OrgsApi.UnitTests/Application/Users/Commands/LoginUserCommandHandlerTests.cs` — Renamed timing-related test, changed assertion from `DidNotReceive()` to `Received(1)` for VerifyPassword

## Tests
### Unit
- What was added/updated: Renamed and updated `Handle_WhenUserNotFound_ShouldStillCallPasswordHasherForTimingSafety` (lines 81-95). Now asserts that `VerifyPassword` IS called once with any arguments, and `GenerateToken` is NOT called.
- How to run: `dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln --filter "FullyQualifiedName~LoginUserCommandHandlerTests"`

### Integration
- What was added/updated: None — existing login endpoint tests pass unchanged
- How to run: `dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln --filter "FullyQualifiedName~IntegrationTests"`

### Manual
- What you verified: N/A (timing difference would be measured in a controlled environment)
- Steps: Send identical POST `/auth/login` requests with existing and non-existing emails, compare response times. Difference should be <10ms (both should take ~150-300ms due to bcrypt).

## Observability
- Logs added/updated: None
- Traces/metrics added/updated: None

## Security
- Validation: Bcrypt always executes regardless of user existence
- AuthN/AuthZ impact: Prevents timing-based email enumeration — attackers cannot determine which emails have accounts
- Sensitive data handling: The dummy hash is not sensitive — it's a placeholder that never matches real passwords

## Follow-ups / Backlog
- [ ] Consider adding account lockout after N failed attempts (action plan item #3)

## Checklist
- [x] Task scope matches `docs/tasks/audit-0002-timing-attack-mitigation.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
