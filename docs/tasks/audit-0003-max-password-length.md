# Audit Action: audit-0003-max-password-length

## Metadata
- ID: audit-0003
- Type: AuditAction
- Status: DONE
- Owner: Agent
- Created: 2026-02-09
- Priority: P1
- Source:
  - Executive summary: `docs/audits/epic-002-executive-summary.md`
  - Action plan item: "#5 - Max password length validation"
- Dependencies:
  - None
- Related docs:
  - Walkthrough: `docs/walkthroughs/audit-0003-max-password-length.md`

## Goal
Add `MaximumLength(128)` to all password FluentValidation validators to prevent denial-of-service through extremely long bcrypt inputs.

## Context
All password validators only enforced a minimum length of 8 characters but had no maximum. Bcrypt computation time scales with input length; an attacker sending passwords of 100KB+ could consume significant server CPU, causing denial-of-service. Four validators across the Api and Application layers needed the same fix.

## Scope
### In scope
- Add `.MaximumLength(128)` to password rules in all 4 validators

### Out of scope
- Password complexity rules (uppercase, special characters)
- Password breach database checking (HaveIBeenPwned)
- Other input length validations

## Requirements
- R1: All four password validators enforce a maximum of 128 characters
- R2: Validation message is clear and user-friendly

## Acceptance Criteria
- [x] AC1: `RegisterRequestValidator` has `MaximumLength(128)` on Password
- [x] AC2: `ChangePasswordRequestValidator` has `MaximumLength(128)` on NewPassword
- [x] AC3: `ResetPasswordRequestValidator` has `MaximumLength(128)` on NewPassword
- [x] AC4: `ResetPasswordCommandValidator` has `MaximumLength(128)` on NewPassword
- [x] AC5: Build passes
- [x] AC6: All tests pass

## Constraints
- Must use FluentValidation's built-in `MaximumLength` method for consistency
- 128 characters chosen as upper bound — sufficient for any reasonable password/passphrase

## Implementation Steps
1. Add `.MaximumLength(128).WithMessage("Password must not exceed 128 characters.")` to `RegisterRequestValidator.cs`
2. Add `.MaximumLength(128).WithMessage("New password must not exceed 128 characters.")` to `ChangePasswordRequestValidator.cs`
3. Add `.MaximumLength(128).WithMessage("New password must not exceed 128 characters.")` to `ResetPasswordRequestValidator.cs`
4. Add `.MaximumLength(128).WithMessage("New password must not exceed 128 characters.")` to `ResetPasswordCommandValidator.cs`

## Testing Plan
- Unit tests: N/A — FluentValidation validators are tested implicitly through integration tests
- Integration tests: Existing endpoint tests pass unchanged (all use passwords under 128 chars)
- Manual checks: Send registration request with 200-character password, verify 400 response

## Definition of Done
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
