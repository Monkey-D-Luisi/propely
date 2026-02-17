# Task: 0040 - Production SMTP Driver (SendGrid/Postmark/SES)

## Metadata
- ID: 0040
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-07
- GitHub Issue: #192
- Epic: `docs/backlog/epic-005-email-system.md`
- Old Issue: #72

## Goal
Add configurable production email provider support so the template can send emails via SendGrid, Postmark, or other providers in production, while keeping Mailhog for development.

## Context
The current `SmtpEmailService` sends all emails via raw SMTP (configured for Mailhog in dev). For production, most SaaS apps use a transactional email service like SendGrid or Postmark for deliverability, analytics, and compliance. The email provider should be switchable via configuration.

### Current State
- `IEmailService` interface in Application layer
- `SmtpEmailService` in Infrastructure sends via SMTP
- Config: `ORGSAPI_Smtp__Host`, `ORGSAPI_Smtp__Port`, etc.

## Scope
### In scope
- Create `IEmailSender` abstraction (low-level send) separate from `IEmailService` (high-level)
- Implement `SmtpEmailSender` (existing SMTP logic, for Mailhog/generic SMTP)
- Implement `SendGridEmailSender` using SendGrid SDK
- Configuration switch: `Email__Provider` = `smtp` | `sendgrid`
- Provider selection at DI registration time based on config
- Health check for email connectivity
- Environment variable documentation

### Out of scope
- Postmark/SES implementations (just SendGrid as the first production provider)
- Email analytics/tracking
- Email queue/retry (handled by MediatR pipeline or outbox)

## Requirements
- R1: Email provider is configurable via `Email__Provider` environment variable
- R2: `smtp` provider uses existing SMTP settings (Mailhog in dev)
- R3: `sendgrid` provider uses SendGrid API with API key from config
- R4: Default provider is `smtp` (backward compatible)
- R5: Health check endpoint reports email provider status
- R6: Provider switch requires zero code changes (just config)

## Acceptance Criteria
- AC1: With `Email__Provider=smtp`, emails sent via SMTP (Mailhog)
- AC2: With `Email__Provider=sendgrid`, emails sent via SendGrid API
- AC3: Missing API key for sendgrid provider fails at startup with clear error
- AC4: Health check reports email provider status
- AC5: `dotnet build` passes
- AC6: `dotnet test` passes

## Constraints (non-negotiable)
- Clean Architecture: IEmailSender in Application, implementations in Infrastructure
- No API keys committed to repo
- Default to SMTP (dev-friendly)
- English-only repo content
- Update walkthrough

## Implementation Steps

1. **Create IEmailSender interface** (`services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Interfaces/IEmailSender.cs`)
   ```csharp
   public interface IEmailSender
   {
       Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
       Task<bool> CheckHealthAsync(CancellationToken cancellationToken = default);
   }
   ```

2. **Refactor SmtpEmailService** to use IEmailSender
   - Extract raw SMTP sending into `SmtpEmailSender` (`services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Email/SmtpEmailSender.cs`)
   - `SmtpEmailService` (high-level) uses `IEmailSender` (low-level) for actual sending

3. **Add SendGrid NuGet package** to Infrastructure project
   - `SendGrid` package

4. **Create SendGridEmailSender** (`services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Email/SendGridEmailSender.cs`)
   - Inject `SendGridClient` configured with API key
   - Implement `SendAsync` using SendGrid SDK
   - Implement `CheckHealthAsync` by calling SendGrid API

5. **Create EmailConfiguration** (`services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Email/EmailConfiguration.cs`)
   - `Provider` (string): "smtp" or "sendgrid"
   - `FromAddress` (string)
   - `FromName` (string)
   - `SendGrid__ApiKey` (string, for sendgrid provider)

6. **Register provider in DI** (`services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/DependencyInjection.cs`)
   ```csharp
   var emailProvider = configuration.GetValue<string>("Email:Provider") ?? "smtp";
   if (emailProvider == "sendgrid")
       services.AddSingleton<IEmailSender, SendGridEmailSender>();
   else
       services.AddSingleton<IEmailSender, SmtpEmailSender>();
   ```

7. **Add health check** (`services/orgs-api/src/SaasTemplate.OrgsApi.Api/Configuration/HealthChecksConfiguration.cs`)
   - Register email health check that calls `IEmailSender.CheckHealthAsync()`

8. **Update env vars** in `.env`
   - `ORGSAPI_Email__Provider=smtp` (default)
   - `ORGSAPI_Email__FromAddress=noreply@saastemplate.dev`
   - `ORGSAPI_Email__FromName=SaaS Template`
   - `ORGSAPI_Email__SendGrid__ApiKey=` (empty, user fills in for production)

### Testing

9. **Unit tests**
   - SmtpEmailSender: verify SMTP client called correctly (mocked)
   - SendGridEmailSender: verify SendGrid client called correctly (mocked)
   - DI registration: correct provider selected based on config

10. **Integration tests**
    - Send email via SMTP (Mailhog) -> verify delivery

## Files to Create / Modify

### Create
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Interfaces/IEmailSender.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Email/SmtpEmailSender.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Email/SendGridEmailSender.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Email/EmailConfiguration.cs`
- `docs/walkthroughs/0040-production-smtp.md`

### Modify
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/SaasTemplate.OrgsApi.Infrastructure.csproj` (add SendGrid package)
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Services/SmtpEmailService.cs` (refactor to use IEmailSender)
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/DependencyInjection.cs` (provider registration)
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Configuration/HealthChecksConfiguration.cs`
- `.env` (add email config vars)

## Testing Plan
- Unit tests: Both email sender implementations
- Integration tests: SMTP sending via Mailhog
- Manual: Configure SendGrid API key and verify delivery (optional, requires real key)

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
