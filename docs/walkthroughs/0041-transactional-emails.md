# Walkthrough: 0041-transactional-emails

## Task Reference
- Task: `docs/tasks/0041-transactional-emails.md`
- Walkthrough: `docs/walkthroughs/0041-transactional-emails.md`
- Branch/PR: `feat/0041-transactional-emails`
- Date: `2026-02-10`

## Summary
Added three new transactional email types (Welcome, Role Change, Member Removed) with models, localization, Razor templates (EN + ES), and service implementations. Integrated the welcome email into the user registration flow.

## Context
- Background: Task 0039 established the email templating system with RazorLight and a base layout. Task 0040 added a pluggable email provider (IEmailSender). This task creates the remaining transactional email types needed by the application.
- Problem statement: The application needs welcome, role change, and member removed email templates beyond the existing invitation, verification, and password reset templates.
- Constraints: Must follow existing patterns (model + localization + template per type), EN + ES locales, inline CSS for email client compatibility.

## Decisions & Trade-offs
- Followed existing pattern of one model + one localization class + two templates (EN/ES) per email type rather than introducing a generic abstraction. Keeps consistency with the existing invitation/verification/reset implementations.
- WelcomeEmailLocalization uses a two-overload pattern (`BuildSubject(locale)` returning format string, `BuildSubject(locale, appName)` returning formatted) to keep the app name dynamic. Other localization classes interpolate the org name directly since it's always available at call site.
- Welcome email is sent after the verification email in RegisterUserCommandHandler. Both use fire-and-forget pattern (exceptions logged as warnings, never thrown to caller).

## Implementation Notes
- Three new email model records extending `BaseEmailModel`:
  - `WelcomeEmailModel` (UserName, LoginUrl)
  - `RoleChangeEmailModel` (UserName, OrganizationName, OldRole, NewRole)
  - `MemberRemovedEmailModel` (UserName, OrganizationName)
- Three new localization static classes using `InvitationEmailLocalization.NormalizeLocale` for locale resolution.
- Six new Razor templates following the existing pattern: `@model` directive, `<h2>` heading, content paragraphs, `.button` class for CTAs where applicable. HTML entities used for Spanish special characters.
- `IEmailService` extended with `SendWelcomeEmailAsync`, `SendRoleChangeEmailAsync`, `SendMemberRemovedEmailAsync`.
- `EmailService` implements all three using the established pattern: resolve locale, build subject, create model, render template, send via IEmailSender, log result.
- `RegisterUserCommandHandler` sends welcome email after verification email. Login URL constructed as `{frontendBaseUrl}/login`.

## Data / Schema / Migrations
- DB changes: None
- Migration strategy: N/A
- Backward compatibility: N/A

## Commands Run
```bash
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln
```

## Files Changed

### Created
- `Application/Common/Email/WelcomeEmailModel.cs`
- `Application/Common/Email/WelcomeEmailLocalization.cs`
- `Application/Common/Email/RoleChangeEmailModel.cs`
- `Application/Common/Email/RoleChangeEmailLocalization.cs`
- `Application/Common/Email/MemberRemovedEmailModel.cs`
- `Application/Common/Email/MemberRemovedEmailLocalization.cs`
- `Infrastructure/Email/Templates/en/Welcome.cshtml`
- `Infrastructure/Email/Templates/es/Welcome.cshtml`
- `Infrastructure/Email/Templates/en/RoleChange.cshtml`
- `Infrastructure/Email/Templates/es/RoleChange.cshtml`
- `Infrastructure/Email/Templates/en/MemberRemoved.cshtml`
- `Infrastructure/Email/Templates/es/MemberRemoved.cshtml`

### Modified
- `Application/Common/Interfaces/IEmailService.cs` - Added 3 new method signatures
- `Infrastructure/Services/EmailService.cs` - Implemented 3 new methods
- `Application/Users/Commands/RegisterUser/RegisterUserCommandHandler.cs` - Added welcome email after registration
- `tests/.../EmailServiceTests.cs` - Added 3 delegation tests for new methods
- `tests/.../RegisterUserCommandHandlerTests.cs` - Added welcome email stub and test

## Tests
### Unit
- What was added/updated: 3 new delegation tests in EmailServiceTests (SendWelcomeEmailAsync, SendRoleChangeEmailAsync, SendMemberRemovedEmailAsync). 1 new test in RegisterUserCommandHandlerTests (Handle_ShouldSendWelcomeEmail). Updated existing tests to stub SendWelcomeEmailAsync and assert it's not called on conflict.
- How to run: `dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln`
- Result: 375 tests pass (235 unit + 135 integration + 5 architecture)

### Integration
- N/A (existing integration tests still pass)

### Manual
- N/A

## Observability
- Logs added/updated: Info-level log messages for welcome, role change, and member removed emails in EmailService. Warning-level logs on send failure.

## Security
- Validation: N/A (no user input changes)
- AuthN/AuthZ impact: None
- Sensitive data handling: Email addresses logged at Info level (existing pattern)

## Follow-ups / Backlog
- Epic 005 is now complete (3/3 tasks DONE)

## Checklist
- [x] Task scope matches `docs/tasks/0041-transactional-emails.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
