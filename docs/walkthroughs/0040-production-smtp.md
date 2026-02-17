# Walkthrough: 0040-production-smtp

## Task Reference
- Task: `docs/tasks/0040-production-smtp.md`
- Walkthrough: `docs/walkthroughs/0040-production-smtp.md`
- Branch/PR: `feat/0040-production-smtp-driver`
- Date: `2026-02-10`

## Summary
Extracted email transport from `SmtpEmailService` into a pluggable `IEmailSender` interface. Added `SmtpEmailSender` (for Mailhog/SMTP) and `SendGridEmailSender` (for production via SendGrid SDK). Provider is switchable via `Email:Provider` configuration. Added email health check to the readiness endpoint.

## Context
- Background: The orgs-api `SmtpEmailService` directly created `System.Net.Mail.SmtpClient` instances. This worked for Mailhog in development but had no path to production email providers.
- Problem statement: Need a pluggable email transport layer to switch between SMTP (dev) and SendGrid (production) via configuration.
- Constraints: Must be backward-compatible with existing `Smtp:*` configuration.

## Decisions & Trade-offs
- **Decision:** Separate `IEmailSender` (low-level transport) from `IEmailService` (high-level orchestration)
  - Options considered: (1) Add provider switch inside SmtpEmailService, (2) Extract transport into separate interface
  - Why this choice: Clean Architecture separation — the service orchestrates templates/localization, the sender handles transport. Follows Interface Segregation Principle.
  - Consequences: One extra level of indirection, but enables any future provider without touching the service.

- **Decision:** `ISendGridClient` injected via DI (not created internally)
  - Options considered: (1) Create `SendGridClient` inside `SendGridEmailSender`, (2) Inject `ISendGridClient`
  - Why this choice: Testability — the SendGrid SDK exposes `ISendGridClient` interface, enabling mock injection in unit tests.

- **Decision:** Health check for SMTP uses TCP connection, for SendGrid uses GET /scopes
  - Options considered: (1) No health check, (2) TCP-only, (3) Provider-specific checks
  - Why this choice: Provider-specific checks give accurate health signals — TCP for SMTP (port connectivity), API key validation for SendGrid.

## Implementation Notes
- `SmtpEmailSender` extracts the SMTP logic previously in `SmtpEmailService.SendHtmlEmailAsync()`
- `SendGridEmailSender` uses `ISendGridClient` from the SendGrid NuGet SDK (v9.29.3)
- `SmtpEmailService` catch blocks simplified from 4 specific exception types to a single `catch (Exception)` since the transport is now abstract
- DI registration selects sender at startup based on `Email:Provider` config; if `sendgrid` is selected but `SendGrid:ApiKey` is missing, startup fails fast with a clear error

## Commands Run
```bash
dotnet add services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure package SendGrid
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln
dotnet test services/orgs-api/tests/SaasTemplate.OrgsApi.UnitTests
```

## Files Changed
- `Application/Common/Interfaces/IEmailSender.cs` (CREATED) — Low-level email transport interface with `SendAsync` and `CheckHealthAsync`
- `Infrastructure/Email/SmtpEmailSender.cs` (CREATED) — SMTP transport via System.Net.Mail, TCP health check
- `Infrastructure/Email/SendGridEmailSender.cs` (CREATED) — SendGrid transport via SDK, API key health check via GET /scopes
- `Infrastructure/Email/EmailConfiguration.cs` (CREATED) — Options class with `Provider` property
- `Api/Configuration/EmailHealthCheck.cs` (CREATED) — IHealthCheck implementation delegating to IEmailSender
- `Infrastructure/SaasTemplate.OrgsApi.Infrastructure.csproj` (MODIFIED) — Added SendGrid 9.29.3 package
- `Infrastructure/Services/SmtpEmailService.cs` (MODIFIED) — Injected IEmailSender, replaced inline SMTP with delegation, simplified catch blocks
- `Infrastructure/DependencyInjection.cs` (MODIFIED) — Provider-based IEmailSender registration
- `Api/Configuration/HealthChecksConfiguration.cs` (MODIFIED) — Added email health check
- `Api/appsettings.json` (MODIFIED) — Added Email and SendGrid config sections
- `Api/appsettings.Development.json` (MODIFIED) — Added Email:Provider=smtp default
- `tests/UnitTests/Infrastructure/Email/SendGridEmailSenderTests.cs` (CREATED) — 5 tests
- `tests/UnitTests/Infrastructure/Services/SmtpEmailServiceTests.cs` (CREATED) — 5 tests
- `tests/UnitTests/Api/Configuration/EmailHealthCheckTests.cs` (CREATED) — 2 tests

## Tests
### Unit (12 new tests)
**SendGridEmailSenderTests (5):**
1. `SendAsync_WhenSuccessful_ShouldCallSendGridClient` — Happy path
2. `SendAsync_WhenApiFails_ShouldThrowInvalidOperationException` — Error response
3. `SendAsync_UsesSmtpFromFallback_WhenSendGridFromNotConfigured` — Config fallback
4. `CheckHealthAsync_WhenApiReturnsSuccess_ShouldReturnTrue` — Health OK
5. `CheckHealthAsync_WhenApiThrows_ShouldReturnFalse` — Health failure

**SmtpEmailServiceTests (5):**
1. `SendOrgInvitationEmailAsync_ShouldDelegateToEmailSender` — Delegation
2. `SendEmailVerificationEmailAsync_ShouldDelegateToEmailSender` — Delegation
3. `SendPasswordResetEmailAsync_ShouldDelegateToEmailSender` — Delegation
4. `SendOrgInvitationEmailAsync_WhenSenderThrows_ShouldNotRethrow` — Error swallowed
5. `SendOrgInvitationEmailAsync_WhenCancelled_ShouldRethrow` — Cancellation propagated

**EmailHealthCheckTests (2):**
1. `CheckHealthAsync_WhenSenderIsHealthy_ShouldReturnHealthy`
2. `CheckHealthAsync_WhenSenderIsUnhealthy_ShouldReturnDegraded`

### Integration
N/A — requires actual SMTP/SendGrid infrastructure.

### Manual
- `dotnet build` — 0 errors
- `dotnet test` — 231 pass (219 existing + 12 new)

## Observability
- Logs added: SendGrid sender logs HTTP status + body on failure at Warning level
- SMTP sender logs health check failure at Warning level
- Traces/metrics: N/A

## Security
- Validation: SendGrid API key must come from environment variable (`ORGSAPI_SendGrid__ApiKey`), startup fails fast if `sendgrid` provider selected without key
- Sensitive data: API keys never in appsettings files (empty placeholders only)

## Follow-ups / Backlog
- [ ] Add Postmark/SES sender implementations
- [ ] Add email retry/queue mechanism

## Checklist
- [x] Task scope matches `docs/tasks/0040-production-smtp.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed
