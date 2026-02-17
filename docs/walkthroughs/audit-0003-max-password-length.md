# Walkthrough: audit-0003-max-password-length

## Task Reference
- Task: `docs/tasks/audit-0003-max-password-length.md`
- Walkthrough: `docs/walkthroughs/audit-0003-max-password-length.md`
- Branch/PR: `fix/epic-002-audit-remediation` / PR #231
- Date: `2026-02-09`

## Summary
Added `MaximumLength(128)` to all four password FluentValidation validators across the Api and Application layers. This prevents denial-of-service through extremely long password inputs that would cause excessive bcrypt computation time.

## Context
- Background: Epic 002 implemented password handling across registration, login, password change, and password reset flows. All validators enforced a minimum length of 8 characters but had no maximum.
- Problem statement: Without a maximum length, an attacker could send passwords of 100KB+ to any endpoint accepting a password. Bcrypt's computation time scales with input length, potentially consuming significant server CPU.
- Constraints: The fix must be applied consistently to all four validators. The maximum must be generous enough for passphrase-style passwords.

## Decisions & Trade-offs
- **Decision:** Set maximum at 128 characters
  - Options considered: (a) 72 (bcrypt's internal truncation point), (b) 128, (c) 256
  - Why this choice: 72 would be technically correct for bcrypt but confusing for users who don't know bcrypt's internals. 128 provides ample room for passphrases while still preventing abuse. 256 was unnecessarily high for the threat model.
  - Consequences / risks: None significant — 128 characters exceeds any reasonable password.

- **Decision:** Apply to both Api-layer and Application-layer validators
  - Options considered: (a) Api-layer only (3 validators), (b) All layers (4 validators)
  - Why this choice: The `ResetPasswordCommandValidator` in the Application layer validates the MediatR command before the handler runs. Even though the Api-layer validator catches it first for HTTP requests, the Application-layer validator provides defense-in-depth for any future internal callers.
  - Consequences / risks: Slight duplication, but this is the existing pattern in the codebase — validators exist at both layers for all commands.

## Implementation Notes
- Key changes: Added `.MaximumLength(128).WithMessage("...must not exceed 128 characters.")` as a chained rule to the password `RuleFor` in each validator
- Edge cases handled: N/A — straightforward validation addition
- Known limitations: Bcrypt internally truncates at 72 bytes in many implementations. The 128-char limit is above this, meaning characters beyond 72 are effectively ignored by bcrypt but still accepted by the validator. This is acceptable — the threat is DoS, not password strength.

## Data / Schema / Migrations
- DB changes: None
- Migration strategy: N/A
- Backward compatibility: Any existing users with passwords longer than 128 characters (highly unlikely) would not be able to re-register or change their password with the same length. Login would still work because login validation does not check max length.

## Commands Run
```bash
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln
```

## Files Changed
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Validators/RegisterRequestValidator.cs` — Added `MaximumLength(128)` to Password rule (line 17)
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Validators/ChangePasswordRequestValidator.cs` — Added `MaximumLength(128)` to NewPassword rule (line 16)
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Validators/ResetPasswordRequestValidator.cs` — Added `MaximumLength(128)` to NewPassword rule (line 16)
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Auth/Commands/ResetPassword/ResetPasswordCommandValidator.cs` — Added `MaximumLength(128)` to NewPassword rule (line 15)

## Tests
### Unit
- What was added/updated: None
- How to run: N/A

### Integration
- What was added/updated: None — existing tests use passwords under 128 characters and pass unchanged
- How to run: `dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln --filter "FullyQualifiedName~IntegrationTests"`

### Manual
- What you verified: N/A
- Steps: Send POST `/auth/register` with a 200-character password, verify 400 response with "must not exceed 128 characters" message

## Observability
- Logs added/updated: None
- Traces/metrics added/updated: None

## Security
- Validation: All password inputs now capped at 128 characters at the validation boundary
- AuthN/AuthZ impact: None — only affects registration and password change/reset inputs
- Sensitive data handling: N/A

## Follow-ups / Backlog
- None

## Checklist
- [x] Task scope matches `docs/tasks/audit-0003-max-password-length.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
