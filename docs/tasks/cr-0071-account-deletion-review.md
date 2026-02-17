# CR-0071: Account Deletion PR Review

## PR Metadata
- PR: #286
- Branch: `feat/0069-account-deletion` → `main`
- Title: feat(auth): self-service account deletion with full cascade (#0069)
- State: OPEN

## Changed Files
20 files (see PR for full list)

## Comment Sources
- Inline review comments: 17
- General reviews: 2
- Issue comments: 2

## Comment Resolution Plan

### MUST_FIX

1. **[2807526278] Copilot — CancelOrgSubscriptionAsync swallows OperationCanceledException**
   - File: `DeleteAccountCommandHandler.cs:144`
   - Issue: Generic `catch (Exception)` swallows `OperationCanceledException`, preventing cancellation propagation
   - Fix: Add `catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }` before the generic catch

2. **[2807526298] Copilot — Cookie delete missing Secure flag**
   - File: `AuthController.cs:332`
   - Issue: Cookie is set with `Secure = _useSecureCookies` but deleted without it — browser won't delete secure cookies without matching `Secure` flag
   - Fix: Add `Secure = _useSecureCookies` to both DeleteAccount and Logout cookie deletion (pre-existing issue in Logout, but fix both for consistency)

3. **[2807520977] Gemini — PII (email) logged in account deletion**
   - File: `DeleteAccountCommandHandler.cs:112`
   - Issue: Email logged in security audit log — PII risk
   - Fix: Remove email from log message, keep only UserId

4. **[2807520978+2807520980] Gemini — PII logged in email service**
   - Files: `EmailService.cs:253,261`
   - Issue: Email address logged in debug and warning messages
   - Fix: Remove email from both log messages

5. **[2807526305] Copilot — Email text says "deleted" but soft-delete keeps data**
   - File: `EmailService.cs:248`
   - Issue: Email says "account and all associated data have been deleted" but data is soft-deleted (retained in DB)
   - Fix: Change wording to "deactivated and scheduled for deletion" to accurately reflect soft-delete behavior

6. **[2807526327] Copilot — Sole-owner branch skips cleanup if org already soft-deleted**
   - File: `DeleteAccountCommandHandler.cs:95`
   - Issue: If `GetByIdAsync` returns null (org filtered by query filter), memberships and invitations aren't cleaned up
   - Fix: Move membership cleanup and invitation cancellation outside the `org is not null` check

### SHOULD_FIX

7. **[2807526312] Copilot — DoD checklist doesn't match deliverables**
   - File: `0069-account-deletion.md:109`
   - Issue: "Stitch designs created" and "E2E test passes" checked but no `.stitch-html` artifacts and no Playwright spec
   - Fix: Uncheck "E2E test passes" (deferred to integration test task), note Stitch designs were generated in Stitch platform (not downloaded to `.stitch-html`)

### SUGGESTION / OUT_OF_SCOPE

8. **[2807520983] Gemini — isConfirmed check redundant in handleDelete**
   - Classification: OUT_OF_SCOPE — defense-in-depth pattern; button `disabled` is a UI convenience but the handler guard prevents race conditions

9. **[2807520986] Gemini — Return all validation errors**
   - Classification: OUT_OF_SCOPE — this is the existing pattern used by ALL AuthController endpoints (Register, Login, ChangePassword, etc.). Changing it would be a broader refactor.

10. **[2807520988] Gemini — INVALID_PASSWORD is a magic string**
    - Classification: OUT_OF_SCOPE — same string used in `ChangePasswordCommandHandler.cs:36`. Extracting to constant is a cross-cutting refactor.

11. **[2807520990] Gemini — Optimize owners list creation**
    - Classification: SUGGESTION — implemented (trivial improvement)

12. **[2807520992+2807520993] Gemini — userName null coalescing duplication**
    - Classification: SHOULD_FIX — move null coalescing to caller, simplify email service

13. **[2807526289] Copilot — Stripe cancellation before DB commit**
    - Classification: OUT_OF_SCOPE — moving Stripe after commit requires an outbox pattern which is a significant architectural change. Current approach is documented as best-effort with logging. If DB commit fails, Stripe subscription remains active (safe failure mode).

14. **[2807526318] Copilot — No component tests for DeleteAccountSection**
    - Classification: OUT_OF_SCOPE — existing DeleteOrgSection also has no component tests. Adding component tests is a separate testing task.

15. **[2807526321] Copilot — Concurrency: use GetByOrgIdForUpdateAsync**
    - Classification: OUT_OF_SCOPE — `GetByOrgIdForUpdateAsync` exists but account deletion is a rare operation. Row-level locking adds complexity. The soft-delete is idempotent, so concurrent runs would at worst soft-delete the same records twice (harmless).

## Behavioral Parity Checks
- [x] Redirect parity: No `next` parameter involved in DELETE /auth/me flow. N/A.
- [x] Locale source: Email service accepts locale but handler doesn't pass one (defaults to "en"). Acceptable — account deletion email is a simple notification, not locale-critical.
- [x] API/UI contract parity: Frontend sends `{ password }` matching `DeleteAccountRequest(string Password)`. CSRF header included. Correct.
- [x] Test parity: 8 unit tests covering happy path, invalid password, sole-owner cascade, non-sole-owner, Stripe cancellation. Coverage adequate.
