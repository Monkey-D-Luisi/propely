# CR-0028: Email Templating PR Review Follow-Up

## PR Metadata
- PR: [#218](https://github.com/Monkey-D-Luisi/saas-template/pull/218)
- Title: `feat(email): add Razor templating for invitation emails`
- Branch: `feat/email-templating-0039` -> `main`
- State: OPEN
- CI snapshot during review:
  - Detect Changes: SUCCESS
  - AI API - Build & Test: SKIPPED
  - Orgs API - Build & Test: SUCCESS
  - Web - Build & Test: SKIPPED

## Review Comment Counts (Mandatory Verification)
- Inline review comments (source 1): `11`
- General reviews (source 2): `2`
- Issue comments (source 3): `2`

## Changed Files (17)
- `.env.docker`
- `docs/backlog/epic-005-email-system.md`
- `docs/tasks/0039-email-templating.md`
- `docs/walkthroughs/0039-email-templating.md`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Email/BaseEmailModel.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Email/InvitationEmailModel.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Email/LayoutEmailModel.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Interfaces/IEmailService.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Interfaces/IEmailTemplateRenderer.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/DependencyInjection.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Email/RazorEmailTemplateRenderer.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Email/Templates/_Layout.cshtml`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Email/Templates/en/Invitation.cshtml`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Email/Templates/es/Invitation.cshtml`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/SaasTemplate.OrgsApi.Infrastructure.csproj`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Services/SmtpEmailService.cs`
- `services/orgs-api/tests/SaasTemplate.OrgsApi.UnitTests/Infrastructure/Email/RazorEmailTemplateRendererTests.cs`

## Review Threads

### Source 1: Inline Comments (11, unresolved first)
1. `2779215248` - Copilot - `SmtpEmailService.cs:42`
   - Claim: template rendering happens outside `try`, so failures bypass the intended handling.
   - Classification: MUST_FIX.
2. `2779215256` - Copilot - `SmtpEmailService.cs:86`
   - Claim: `catch (InvalidOperationException)` is misleading with current `try` scope.
   - Classification: MUST_FIX.
3. `2779215253` - Copilot - `SmtpEmailService.cs:36`
   - Claim: Spanish subject text missing accents.
   - Classification: MUST_FIX.
4. `2779215258` - Copilot - `Email/Templates/es/Invitation.cshtml:10`
   - Claim: Spanish email template text missing accents.
   - Classification: MUST_FIX.
5. `2779215267` - Copilot - `RazorEmailTemplateRendererTests.cs:63`
   - Claim: Spanish assertions must match corrected text.
   - Classification: MUST_FIX.
6. `2779215262` - Copilot - `Email/Templates/_Layout.cshtml:3`
   - Claim: `lang="en"` is hardcoded and wrong for Spanish rendering.
   - Classification: SHOULD_FIX.
7. `2779215265` - Copilot - `Email/Templates/_Layout.cshtml:72`
   - Claim: footer text remains English-only for Spanish emails.
   - Classification: SHOULD_FIX.
8. `2779215238` - Copilot - `SmtpEmailService.cs:104`
   - Claim: locale resolution has branching without dedicated tests.
   - Classification: SHOULD_FIX.
9. `2779212679` - gemini-code-assist - `RazorEmailTemplateRenderer.cs:34`
   - Claim: add generic type constraint to enforce `BaseEmailModel`.
   - Classification: SHOULD_FIX.
10. `2779212685` - gemini-code-assist - `SmtpEmailService.cs:37`
   - Claim: subject localization should move to `.resx`.
   - Classification: OUT_OF_SCOPE (not required by task 0039; can follow up in future i18n task).
11. `2779212687` - gemini-code-assist - `SmtpEmailService.cs:112`
   - Claim: centralize locale handling in renderer/config.
   - Classification: SUGGESTION (non-blocking refactor; current behavior is functionally valid).

### Source 2: General Reviews (2)
1. `3769688735` - gemini-code-assist - COMMENTED
   - Summary: positive review with robustness suggestions (constraints, locale, subjects).
   - Classification: SHOULD_FIX + SUGGESTION.
2. `3769690467` - copilot-pull-request-reviewer - COMMENTED
   - Summary: positive review with 8 actionable inline comments.
   - Classification: MUST_FIX + SHOULD_FIX.

### Source 3: Issue Comments (2)
1. `IC_kwDOP0hK4s7mfxjx` - chatgpt-codex-connector
   - Summary: usage-limit notification.
   - Classification: OUT_OF_SCOPE.
2. `IC_kwDOP0hK4s7mfxok` - gemini-code-assist
   - Summary: automated PR summary.
   - Classification: OUT_OF_SCOPE.

## Comment Resolution Plan

### MUST_FIX
- [x] Move invitation template rendering into the guarded `try` scope and align exception handling.
- [x] Correct Spanish-visible copy in subject/template/tests (accents).
- [x] Ensure tests remain green after localization copy fixes.

### SHOULD_FIX
- [x] Add locale-awareness to layout (`lang` and localized footer copy).
- [x] Add targeted unit tests for locale resolution/subject selection logic via shared localization utility.
- [x] Add `BaseEmailModel` generic constraint in renderer interface/implementation.

### SUGGESTION
- [x] Keep current locale resolution split for this PR; document follow-up refactor possibility.

### OUT_OF_SCOPE
- [x] Do not move subject localization to `.resx` within task 0039 follow-up.
- [x] Ignore non-actionable issue comments.

## Status
- COMPLETE
