# CR-0081: Audit Remediation PR #307 Self-Review

## PR Metadata
- **PR**: #307 (fix/audit-remediation-2026-02-17)
- **Target**: main
- **CI Status**: AI API ✅, Web ✅, Orgs API (in progress)
- **Files Changed**: 100+

## Review Sources
- **Inline review comments**: 5 (Copilot x4, Gemini x1)
- **General reviews**: 2 (Copilot summary, Gemini summary)
- **Issue comments**: 2 (ChatGPT rate limit notice, Gemini intro summary)

## Comment Resolution Plan

### MUST_FIX

- [x] **OrgContextMiddleware backward compatibility** (Copilot #4, OrgContextMiddleware.cs:41)
  - **Issue**: Current code requires `org_ids` claim to be present for `org_id` injection. Old tokens (issued before this PR) lack `org_ids`, so ALL ai-api requests from those users will silently fail to set org context until re-login.
  - **Fix**: When `org_ids` claim is absent, fall through to allow `org_id` injection (old behavior). Only enforce membership verification when `org_ids` IS present.

- [x] **LoggingBehaviour sensitive field matching** (Copilot #1, LoggingBehaviour.cs:96)
  - **Issue**: `SensitivePropertyNames` uses exact match. Properties like `CurrentPassword`, `NewPassword`, `UserEmail` won't be redacted.
  - **Fix**: Switch to substring-based matching: if property name contains any sensitive keyword, redact it.

- [x] **Payment pagination validation** (Copilot #3, BillingController.cs:193)
  - **Issue**: `page` and `pageSize` accepted from query string without clamping. Negative/zero values cause runtime errors (negative Skip, divide-by-zero in PagedResult).
  - **Fix**: Clamp `page` to min 1, `pageSize` to range [1, 100] before constructing the query.

### SHOULD_FIX

- [x] **Bare catch in SanitizeRequest** (Copilot #2, LoggingBehaviour.cs:104)
  - **Issue**: Bare `catch` hides serialization failures. Should log at Debug level for diagnosability.
  - **Fix**: Change to `catch (Exception ex)` and log at Debug level.

- [x] **PII in logged payloads — Email** (Gemini #5, LoggingBehaviour.cs:52)
  - **Issue**: Email addresses appear in plaintext in logged command payloads.
  - **Fix**: Add `"email"` to sensitive keywords. This catches `Email` and `UserEmail` via substring matching.
  - **Note**: `Name` is NOT added — it maps to org names, feature flag names, and profile display names, which are domain data, not PII requiring log redaction.

### OUT_OF_SCOPE (with rationale)

- **Gemini nested PII concern**: Gemini noted that nested object properties won't be redacted. All MediatR commands in this codebase are flat records — no nested objects carry sensitive data. If this changes, the logging behaviour can be extended. Not a current risk.

## Parity Verification Checklist
- [x] Redirect parity checked — no redirect changes in this PR
- [x] Locale source correctness — no locale changes in this PR
- [x] API/UI contract parity — InviteResponse schema updated on both sides; payment pagination is server-only (frontend doesn't use page params yet)
- [x] Test parity — all command handlers with changed behavior have updated unit tests
