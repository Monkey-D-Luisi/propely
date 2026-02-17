# Code Review: cr-0047 — Audit Epic 005 Email PR #244

## PR Metadata
- **PR:** #244
- **Branch:** `fix/audit-epic-005-batch` -> `main`
- **Task:** Epic 005 email system audit batch
- **CI Status:** All checks passed

## Changed Files
11 files — RegisterUserCommandHandler, EmailService, SmtpEmailSender, DependencyInjection, appsettings, SmtpEmailSenderTests, RegisterUserCommandHandlerTests, PasswordResetEmailLocalizationTests, docs

## Review Comments

### Source 1: Inline Review Comments (6)
| # | ID | Reviewer | File | Classification | Summary |
|---|---|---|---|---|---|
| 1 | 2789967711 | Gemini | RegisterUserCommandHandler.cs:71 | **Partial OUT_OF_SCOPE / SHOULD_FIX** | FrontendBaseUrl allow-list (out of scope) + log warning on fallback (fixed) |
| 2 | 2789967717 | Gemini | EmailService.cs:249 | **SHOULD_FIX** | PII `{Email}` in failure log at Warning level |
| 3 | 2790027859 | Copilot | RegisterUserCommandHandler.cs:68 | **SHOULD_FIX** | Log warning when FrontendBaseUrl falls back to localhost |
| 4 | 2790027892 | Copilot | EmailService.cs:249 | **SHOULD_FIX** | Duplicate of #2 — PII in failure log |
| 5 | 2790027906 | Copilot | SmtpEmailSenderTests.cs:22 | **SHOULD_FIX** | `192.0.2.1` causes slow/flaky tests, use `127.0.0.1` |
| 6 | 2790027924 | Copilot | RegisterUserCommandHandler.cs:86 | **SHOULD_FIX** | try/catch too broad — covers token gen + URL building, not just email |

### Source 2: General Reviews (2)
- Gemini review: Summary praising structure. Inline comments captured above. No additional issues.
- Copilot review: Summary of changes. Inline comments captured above. No additional issues.

### Source 3: Issue Comments (0)
- No actionable issue comments (only Gemini summary and Codex quota notice).

## Behavioral Parity Checks
- [x] Redirect parity checked — no redirect changes in this PR
- [x] Locale source correctness checked — `InvitationEmailLocalization.NormalizeLocale()` reused consistently, `Email:DefaultLocale` used in `EmailService.ResolveLocale()`
- [x] API/UI contract parity checked — no API contract changes
- [x] Test parity checked — existing `Handle_WhenEmailServiceThrows_ShouldStillReturnSuccessResult` test covers email resilience; semantics preserved after narrowing try/catch

## Comment Resolution Plan

### SHOULD_FIX
- [x] #2, #4: Remove `{Email}` from failure log in `SendTemplateEmailAsync` — keep `{TemplateName}` only
- [x] #1 (partial), #3: Add `LogWarning` when `FrontendBaseUrl` falls back to localhost + narrow try/catch to email-only calls (also addresses #6)
- [x] #5: Replace `192.0.2.1` with `127.0.0.1` in both SmtpEmailSender tests for fast, deterministic failures
- [x] #6: Move token generation and URL building outside try/catch — only `_emailService` calls wrapped

### OUT_OF_SCOPE
- [ ] #1 (allow-list): FrontendBaseUrl domain allow-list validation requires design decisions about configuration, error behavior. Tracked as future backlog item.
