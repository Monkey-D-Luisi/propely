# Task: 0041 - Transactional Email Types

## Metadata
- ID: 0041
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-07
- GitHub Issue: #193
- Epic: `docs/backlog/epic-005-email-system.md`
- Dependencies: 0039 (email templating system)

## Goal
Create all transactional email templates needed by the application: welcome, password reset, email verification, invitation (improve existing), and role change notification.

## Context
Task 0039 establishes the templating system with RazorLight and a base layout. This task creates all the specific email templates that other features need. Tasks 0028 (password reset) and 0029 (email verification) depend on these templates existing.

## Scope
### In scope
- Welcome email template (sent after registration)
- Password reset email template (sent by forgot-password flow)
- Email verification template (sent after registration)
- Invitation email template (improve existing with proper template)
- Role change notification template (sent when role updated)
- Member removed notification template
- All templates in EN + ES
- Email model classes for each template type
- Update IEmailService to support all email types

### Out of scope
- Marketing/promotional emails
- Digest/summary emails
- Email preferences/unsubscribe

## Requirements
- R1: Each email type has a dedicated Razor template
- R2: All templates use the base layout from task 0039
- R3: All templates available in EN and ES
- R4: Templates are responsive HTML (works in major email clients)
- R5: Each template has a strongly-typed model
- R6: IEmailService has methods for each email type

## Acceptance Criteria
- AC1: Welcome email renders with user name and login link
- AC2: Password reset email renders with reset link and expiry info
- AC3: Email verification renders with verification link
- AC4: Invitation email renders with inviter name, org name, and accept link
- AC5: Role change email renders with old/new role and org name
- AC6: All templates render in EN and ES
- AC7: `dotnet build` and `dotnet test` pass

## Constraints (non-negotiable)
- Clean Architecture layers respected
- Templates use inline CSS (email client compatibility)
- English-only code/comments, template content in EN + ES
- Update walkthrough

## Implementation Steps

1. **Create email model classes** (`services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Email/`)
   - `WelcomeEmailModel.cs`: UserName, LoginUrl
   - `PasswordResetEmailModel.cs`: UserName, ResetUrl, ExpiryMinutes
   - `EmailVerificationModel.cs`: UserName, VerifyUrl, ExpiryHours
   - `InvitationEmailModel.cs`: (already from 0039, enhance if needed) InviterName, OrgName, AcceptUrl
   - `RoleChangeEmailModel.cs`: UserName, OrgName, OldRole, NewRole
   - `MemberRemovedEmailModel.cs`: UserName, OrgName

2. **Create template files** (`services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Email/Templates/`)
   - `en/Welcome.cshtml` - Welcome message, getting started tips, login button
   - `es/Welcome.cshtml` - Spanish version
   - `en/PasswordReset.cshtml` - Reset instructions, reset button, expiry warning
   - `es/PasswordReset.cshtml`
   - `en/EmailVerification.cshtml` - Verify instructions, verify button, expiry info
   - `es/EmailVerification.cshtml`
   - `en/Invitation.cshtml` - Invitation details, accept button (improve existing)
   - `es/Invitation.cshtml`
   - `en/RoleChange.cshtml` - Role change notification
   - `es/RoleChange.cshtml`
   - `en/MemberRemoved.cshtml` - Removal notification
   - `es/MemberRemoved.cshtml`

3. **Update IEmailService** (`services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Interfaces/IEmailService.cs`)
   - Add method signatures for each email type (or use a generic approach):
   ```csharp
   Task SendWelcomeEmailAsync(string to, WelcomeEmailModel model, string locale, CancellationToken ct);
   Task SendPasswordResetEmailAsync(string to, PasswordResetEmailModel model, string locale, CancellationToken ct);
   Task SendEmailVerificationAsync(string to, EmailVerificationModel model, string locale, CancellationToken ct);
   Task SendInvitationEmailAsync(string to, InvitationEmailModel model, string locale, CancellationToken ct);
   Task SendRoleChangeEmailAsync(string to, RoleChangeEmailModel model, string locale, CancellationToken ct);
   Task SendMemberRemovedEmailAsync(string to, MemberRemovedEmailModel model, string locale, CancellationToken ct);
   ```

4. **Update SmtpEmailService** (`services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Services/SmtpEmailService.cs`)
   - Implement each method using IEmailTemplateRenderer to render template then IEmailSender to send

5. **Update RegisterUserCommandHandler** to send welcome email after registration

6. **Update existing invitation flow** to use the new template

### Testing

7. **Unit tests**
   - Render each template with model data -> verify HTML contains expected content
   - Test EN and ES locales for each template
   - Test that each email type is sent correctly

## Files to Create / Modify

### Create
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Email/WelcomeEmailModel.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Email/PasswordResetEmailModel.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Email/EmailVerificationModel.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Email/RoleChangeEmailModel.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Email/MemberRemovedEmailModel.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Email/Templates/en/Welcome.cshtml`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Email/Templates/es/Welcome.cshtml`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Email/Templates/en/PasswordReset.cshtml`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Email/Templates/es/PasswordReset.cshtml`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Email/Templates/en/EmailVerification.cshtml`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Email/Templates/es/EmailVerification.cshtml`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Email/Templates/en/RoleChange.cshtml`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Email/Templates/es/RoleChange.cshtml`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Email/Templates/en/MemberRemoved.cshtml`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Email/Templates/es/MemberRemoved.cshtml`
- `docs/walkthroughs/0041-transactional-emails.md`

### Modify
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Interfaces/IEmailService.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Services/SmtpEmailService.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Auth/Commands/RegisterUser/RegisterUserCommandHandler.cs`

## Testing Plan
- Unit tests: Template rendering for all 6 email types in both locales
- Integration tests: Full send flow via Mailhog
- Manual: Check each email in Mailhog web UI for formatting

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] All 6 templates in EN + ES
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
