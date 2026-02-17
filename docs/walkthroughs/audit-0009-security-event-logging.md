# Walkthrough: audit-0009-security-event-logging

## Task Reference
- Task: `docs/tasks/audit-0009-security-event-logging.md`
- Walkthrough: `docs/walkthroughs/audit-0009-security-event-logging.md`
- Branch/PR: `fix/audit-0006-session-invalidation`
- Date: `2026-02-09`

## Summary
Added structured security event logging to four auth command handlers: LoginUser, ChangePassword, ResetPassword, and ForgotPassword. Security events are logged at Warning level for failures and Information level for successful operations.

## Context
- Background: Audit epic-002 finding F8 identified that security-sensitive events were not logged, making incident investigation difficult.
- Problem statement: No audit trail for failed logins, password changes, or token validation failures.

## Decisions & Trade-offs
- **Decision: ILogger per handler vs dedicated SecurityAuditService**
  - Options considered: (1) ILogger in each handler, (2) Dedicated SecurityAuditService
  - Why this choice: Using standard ILogger with "Security:" prefix keeps it simple and leverages existing logging infrastructure. A dedicated service can be added later if structured security event storage is needed.

## Files Changed
- `LoginUserCommandHandler.cs` — Added ILogger; log failed login (Warning), successful login (Information)
- `ChangePasswordCommandHandler.cs` — Added ILogger; log failed password verify (Warning), successful change (Information)
- `ResetPasswordCommandHandler.cs` — Added ILogger; log invalid token (Warning), token reuse (Warning), successful reset (Information)
- `ForgotPasswordCommandHandler.cs` — Added ILogger; log non-existent email request (Information), token generated (Information)
- All corresponding unit test files updated with NullLogger dependency

## Checklist
- [x] Task scope matches `docs/tasks/audit-0009-security-event-logging.md`
- [x] Tests updated and passing
- [x] No secrets committed
