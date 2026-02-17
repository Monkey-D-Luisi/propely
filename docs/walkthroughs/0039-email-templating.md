# Walkthrough: 0039-email-templating

## Task Reference
- Task: `docs/tasks/0039-email-templating.md`
- Walkthrough: `docs/walkthroughs/0039-email-templating.md`
- Branch/PR: `feat/email-templating-0039` / `https://github.com/Monkey-D-Luisi/saas-template/pull/218`
- Date: `2026-02-08`

## Summary
Implemented a Razor-based email templating foundation in `orgs-api` and refactored invitation emails to render from typed templates (EN/ES) with a shared HTML layout.

## Context
- Background: Invitation emails were generated from inline HTML strings.
- Problem statement: Inline HTML is hard to maintain, untestable, and not locale-friendly.
- Constraints (time, scope, dependencies): Task 0039 scope only; no provider switch (0040) and no additional transactional templates (0041).

## Decisions & Trade-offs
- **Decision:** Use RazorLight with embedded `.cshtml` templates.
  - Options considered: raw string templates, Razor from filesystem, Razor embedded resources.
  - Why this choice: Embedded resources keep templates versioned and deterministic across environments.
  - Consequences / risks: Resource naming must match assembly naming conventions.

## Implementation Notes
- Key changes:
  - Added `IEmailTemplateRenderer` in Application.
  - Added `BaseEmailModel` and `InvitationEmailModel`.
  - Added `RazorEmailTemplateRenderer` in Infrastructure with locale fallback to `en`.
  - Added templates: shared `_Layout.cshtml` and `Invitation.cshtml` for `en`/`es`.
  - Refactored `SmtpEmailService` to render invitation HTML through the renderer.
  - Registered renderer in DI.
  - Added Docker SMTP overrides for orgs-api (`mailhog:1025`) in `.env.docker`.
- Edge cases handled:
  - Unknown locale fallback.
  - Missing template throws explicit exception.
  - Cancellation propagation preserved.
- Known limitations:
  - Locale selection for invitation emails currently defaults from method/config.

## Data / Schema / Migrations
- DB changes (if any): none.
- Migration strategy: not applicable.
- Backward compatibility: invitation send contract preserved (same method name and defaults).

## Commands Run
```bash
git status --short
git branch --show-current
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln
dotnet test services/orgs-api/tests/SaasTemplate.OrgsApi.UnitTests/SaasTemplate.OrgsApi.UnitTests.csproj --filter "FullyQualifiedName~RazorEmailTemplateRendererTests"
```

## Files Changed
- `docs/backlog/epic-005-email-system.md` - task 0039 marked `DONE` and progress tracker updated (`1/3`).
- `docs/tasks/0039-email-templating.md` - task status set to `DONE`; DoD checklist completed.
- `docs/walkthroughs/0039-email-templating.md` - implementation and validation notes.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Interfaces/IEmailTemplateRenderer.cs` - new renderer abstraction.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Interfaces/IEmailService.cs` - invitation email locale-aware signature.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Email/BaseEmailModel.cs` - base email model.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Email/InvitationEmailModel.cs` - invitation model.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Email/LayoutEmailModel.cs` - shared layout model.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Email/RazorEmailTemplateRenderer.cs` - Razor-based template renderer with locale fallback.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Email/Templates/_Layout.cshtml` - reusable email layout.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Email/Templates/en/Invitation.cshtml` - EN invitation template.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Email/Templates/es/Invitation.cshtml` - ES invitation template.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Services/SmtpEmailService.cs` - refactored to render invitation via template engine.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/DependencyInjection.cs` - renderer registration.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/SaasTemplate.OrgsApi.Infrastructure.csproj` - RazorLight package, embedded templates, preserve compilation context.
- `services/orgs-api/tests/SaasTemplate.OrgsApi.UnitTests/Infrastructure/Email/RazorEmailTemplateRendererTests.cs` - renderer unit tests (EN/ES/fallback/missing template).
- `.env.docker` - Docker-network SMTP host/port override for `orgs-api`.

## Tests
### Unit
- Added renderer tests for EN, ES, fallback, and missing template.
- How to run: `dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln`
- Result: PASS (`138` unit tests total in orgs-api unit test project).

### Integration
- No new integration test required for this task scope.
- Result: PASS (`91` integration tests in orgs-api).

### Manual
- Completed:
  1. Start stack: `.\scripts\dev-up.ps1` (or equivalent).
  2. Trigger an org invitation from UI/API.
  3. Open Mailhog (`http://localhost:18025`) and inspect invitation email.
  4. Verify layout/header/footer render correctly.
  5. Verify invitation content and accept URL are present and clickable.
  6. Confirm SMTP delivery in Dockerized orgs-api after setting `ORGSAPI_Smtp__Host=mailhog` and `ORGSAPI_Smtp__Port=1025` in `.env.docker`.
- Result: PASS (user-confirmed).

## Observability
- Logs added/updated:
  - `SmtpEmailService` now logs locale when invitation email is sent.

## Security
- Validation:
  - Templating uses strongly typed models.
- AuthN/AuthZ impact:
  - none.
- Sensitive data handling (secrets, PII):
  - no secrets added.

## Follow-ups / Backlog
- [ ] Use recipient/user locale in invitation command flow when locale becomes available in request context.
- [ ] Investigate invitation UX issues reported during manual testing (accepted invite works, but minor UI bugs remain outside task 0039 scope).

## Checklist
- [x] Task scope matches `docs/tasks/0039-email-templating.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
