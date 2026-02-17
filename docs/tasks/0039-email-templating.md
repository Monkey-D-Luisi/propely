# Task: 0039 - Email Templating System (Razor/Liquid)

## Metadata
- ID: 0039
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-07
- GitHub Issue: #191
- Epic: `docs/backlog/epic-005-email-system.md`
- Old Issue: #71

## Goal
Add a proper email templating engine to replace hardcoded HTML strings, with a base layout and support for i18n templates.

## Context
The orgs-api has `SmtpEmailService` (`services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Services/SmtpEmailService.cs`) which sends invitation emails with inline HTML. This is fragile, untestable, and doesn't support multiple templates or languages. We need a templating engine that renders email HTML from template files with dynamic data.

### Current State
- `IEmailService` interface exists in Application layer
- `SmtpEmailService` implementation in Infrastructure uses raw HTML strings
- Mailhog configured for dev on port 10025 (SMTP) / 18025 (web UI)
- SMTP settings configured via `ORGSAPI_Smtp__*` env vars

## Scope
### In scope
- Add RazorLight NuGet package for Razor-based email templates
- Create `IEmailTemplateRenderer` interface in Application layer
- Implement `RazorEmailTemplateRenderer` in Infrastructure
- Create base email layout template (header, content area, footer, branding)
- Refactor existing invitation email to use the template
- Support i18n in templates (EN + ES versions)
- Create template models (strongly typed)
- Register in DI

### Out of scope
- Production SMTP provider (task 0040)
- Specific transactional email templates (task 0041)
- Email queue/retry logic

## Requirements
- R1: Email templates are Razor files (.cshtml) stored in the project
- R2: Base layout provides consistent header/footer/branding
- R3: Templates accept strongly-typed models for dynamic data
- R4: Templates support EN and ES locales
- R5: `IEmailTemplateRenderer.RenderAsync(templateName, model, locale)` returns HTML string
- R6: Existing invitation email works with new template system

## Acceptance Criteria
- AC1: `IEmailTemplateRenderer` renders a template with dynamic data
- AC2: Base layout wraps all email content consistently
- AC3: Invitation email uses the template system (not inline HTML)
- AC4: English and Spanish versions of templates render correctly
- AC5: `dotnet build` passes
- AC6: `dotnet test` passes
- AC7: Template rendering is tested with unit tests

## Constraints (non-negotiable)
- Clean Architecture: interface in Application, implementation in Infrastructure
- Templates embedded as resources or in a known file path
- English-only code/comments, but template content in EN + ES
- Update walkthrough

## Implementation Steps

1. **Add NuGet package** to `SaasTemplate.OrgsApi.Infrastructure`
   - `RazorLight` (or `RazorLight.Precompile` for AOT support)

2. **Create IEmailTemplateRenderer** (`services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Interfaces/IEmailTemplateRenderer.cs`)
   ```csharp
   public interface IEmailTemplateRenderer
   {
       Task<string> RenderAsync<TModel>(string templateName, TModel model, string locale = "en");
   }
   ```

3. **Create template models** (`services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Email/`)
   - `BaseEmailModel.cs`: AppName, SupportUrl, Year (for footer)
   - `InvitationEmailModel.cs` : InviterName, OrgName, InviteUrl (extends BaseEmailModel)

4. **Create base layout template** (`services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Email/Templates/`)
   - `_Layout.cshtml`: HTML boilerplate, responsive CSS, header with logo, content placeholder, footer
   - Use inline CSS for email client compatibility

5. **Create invitation template** (`services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Email/Templates/`)
   - `en/Invitation.cshtml`: English invitation email body
   - `es/Invitation.cshtml`: Spanish invitation email body
   - Both use `@{ Layout = "_Layout"; }`

6. **Implement RazorEmailTemplateRenderer** (`services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Email/RazorEmailTemplateRenderer.cs`)
   - Initialize RazorLight engine with embedded resource or file system provider
   - Template path resolution: `{locale}/{templateName}.cshtml`
   - Fallback to "en" locale if requested locale not found
   - Compile and cache templates

7. **Update SmtpEmailService** (`services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Services/SmtpEmailService.cs`)
   - Inject `IEmailTemplateRenderer`
   - Replace inline HTML with `await _renderer.RenderAsync("Invitation", model, locale)`

8. **Register in DI** (`services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/DependencyInjection.cs`)
   - `services.AddSingleton<IEmailTemplateRenderer, RazorEmailTemplateRenderer>()`

9. **Mark template files as embedded resources** in `.csproj`
   - Or use `CopyToOutputDirectory` for file-system based templates

### Testing

10. **Unit tests for RazorEmailTemplateRenderer**
    - Render invitation template with model data -> verify HTML contains expected values
    - Render with EN locale -> English content
    - Render with ES locale -> Spanish content
    - Render with unknown locale -> falls back to EN

## Files to Create / Modify

### Create
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Interfaces/IEmailTemplateRenderer.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Email/BaseEmailModel.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Email/InvitationEmailModel.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Email/RazorEmailTemplateRenderer.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Email/Templates/_Layout.cshtml`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Email/Templates/en/Invitation.cshtml`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Email/Templates/es/Invitation.cshtml`
- `docs/walkthroughs/0039-email-templating.md`

### Modify
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/SaasTemplate.OrgsApi.Infrastructure.csproj` (add RazorLight, embed templates)
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Services/SmtpEmailService.cs` (use renderer)
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/DependencyInjection.cs` (register renderer)

## Testing Plan
- Unit tests: Template rendering with different models and locales
- Integration tests: Send email via SmtpEmailService -> verify in Mailhog
- Manual: Check rendered email in Mailhog web UI for formatting

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Templates render correctly in EN and ES
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
