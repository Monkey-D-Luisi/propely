# Walkthrough: 0028-password-reset

## Task Reference
- Task: `docs/tasks/0028-password-reset.md`
- Walkthrough: `docs/walkthroughs/0028-password-reset.md`
- Branch/PR: `feat/password-reset-0028` / n/a
- Date: `2026-02-08`

## Summary
Implemented forgot-password and reset-password flows end to end:
- New backend commands, API endpoints, JWT reset token generation/validation, and SMTP template-based password reset email.
- New frontend forgot-password and reset-password pages/forms with EN/ES i18n.
- Added endpoint rate limiting for `POST /auth/forgot-password`.
- Added backend and frontend automated test coverage for the new flows.

## Context
- Background:
  - Existing auth supported register/login/change password, but not "forgot password".
- Problem statement:
  - Users with forgotten passwords had no recovery path.
- Constraints (time, scope, dependencies):
  - Dependency `0039` email templating was already done and reused.
  - Must avoid email enumeration (always return 200 on forgot-password).
  - Token must be short-lived JWT, no DB token table.
  - Task includes manual Mailhog verification before final DONE.

## Decisions & Trade-offs
- **Decision: Stateless reset token with password-hash fingerprint**
  - Options considered:
    - JWT only with user/email/purpose claims.
    - JWT + password-hash fingerprint claim.
  - Why this choice:
    - Fingerprint invalidates old tokens automatically after password change, enabling "already used" detection without token storage.
  - Consequences / risks:
    - Depends on password hash change semantics.

- **Decision: Keep forgot-password response generic**
  - Options considered:
    - Return 404 for unknown email.
    - Always return 200.
  - Why this choice:
    - Prevents user/email enumeration.
  - Consequences / risks:
    - Client never knows whether email exists, by design.

- **Decision: Reuse SMTP template renderer from task 0039**
  - Options considered:
    - Inline email HTML.
    - Reuse templating system.
  - Why this choice:
    - Keeps consistent email rendering/localization and avoids duplicated HTML.
  - Consequences / risks:
    - New template must exist for each supported locale.

## Implementation Notes
- Key changes:
  - Added `ForgotPasswordCommand` and `ResetPasswordCommand` with validators/handlers.
  - Extended `IJwtTokenService` and `JwtTokenService` for reset tokens:
    - `purpose=password-reset` claim.
    - `pwd_fgp` claim for replay/used-token detection.
    - 15-minute expiry.
  - Added API endpoints:
    - `POST /auth/forgot-password`
    - `POST /auth/reset-password`
  - Added DTOs + API validators for both endpoints.
  - Added `SendPasswordResetEmailAsync` to `IEmailService` and SMTP implementation.
  - Added Razor templates:
    - `Email/Templates/en/PasswordReset.cshtml`
    - `Email/Templates/es/PasswordReset.cshtml`
  - Added frontend forms/pages:
    - `/[locale]/forgot-password`
    - `/[locale]/reset-password`
  - Added login link to forgot-password flow.
  - Added schema and i18n keys (EN + ES).
  - Added endpoint-specific rate limit:
    - `post:/auth/forgot-password` -> `5` requests per minute.

- Edge cases handled:
  - Unknown email on forgot-password returns success (no leak).
  - Missing/invalid/expired token returns reset error.
  - Reused token returns `RESET_TOKEN_ALREADY_USED`.
  - CSRF validation required for both new endpoints.
  - Locale for reset email inferred from `Accept-Language` and normalized.

- Known limitations:
  - Token revocation remains stateless by design; immediate invalidation is tied to password hash change.
  - Manual Mailhog UX verification is still pending in this walkthrough at this stage.

## Data / Schema / Migrations
- DB changes (if any):
  - No database migration required.
- Migration strategy:
  - Not applicable.
- Backward compatibility:
  - Existing login/register/change-password flows remain unchanged.

## Commands Run
```bash
dotnet build services/ai-api/SaasTemplate.AiApi.sln
dotnet test services/ai-api/SaasTemplate.AiApi.sln
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln
dotnet test services/orgs-api/tests/SaasTemplate.OrgsApi.IntegrationTests/SaasTemplate.OrgsApi.IntegrationTests.csproj --filter "FullyQualifiedName~AuthEndpointTests"
cd apps/web && npm test -- --run
cd apps/web && npm run build
```

## Files Changed
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Auth/Commands/ForgotPassword/*`
  - Added forgot-password command, handler, validator.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Auth/Commands/ResetPassword/*`
  - Added reset-password command, handler, validator.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Auth/Security/PasswordResetTokenFingerprint.cs`
  - Added reset token fingerprint helper.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Users/Interfaces/IJwtTokenService.cs`
  - Added reset token generation/validation contract.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Services/JwtTokenService.cs`
  - Implemented reset token generation/validation.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Interfaces/IEmailService.cs`
  - Added password reset email send contract.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Email/PasswordResetEmailModel.cs`
  - Added reset email template model.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Email/PasswordResetEmailLocalization.cs`
  - Added reset email localized subject builder.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Services/SmtpEmailService.cs`
  - Added password reset email send implementation.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Email/Templates/en/PasswordReset.cshtml`
  - Added EN password reset template.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Email/Templates/es/PasswordReset.cshtml`
  - Added ES password reset template.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Dtos/ForgotPasswordRequest.cs`
  - Added forgot-password request DTO.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Dtos/ResetPasswordRequest.cs`
  - Added reset-password request DTO.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Validators/ForgotPasswordRequestValidator.cs`
  - Added forgot-password DTO validation.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Validators/ResetPasswordRequestValidator.cs`
  - Added reset-password DTO validation.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/AuthController.cs`
  - Added forgot/reset endpoints and command wiring.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/DependencyInjection.cs`
  - Added endpoint-specific rate limit rule.
- `services/orgs-api/tests/SaasTemplate.OrgsApi.UnitTests/Application/Auth/Commands/*`
  - Added command handler tests.
- `services/orgs-api/tests/SaasTemplate.OrgsApi.UnitTests/Infrastructure/Services/JwtTokenServiceTests.cs`
  - Added JWT reset token tests.
- `services/orgs-api/tests/SaasTemplate.OrgsApi.UnitTests/Application/Common/Email/PasswordResetEmailLocalizationTests.cs`
  - Added localization tests.
- `services/orgs-api/tests/SaasTemplate.OrgsApi.UnitTests/Infrastructure/Email/RazorEmailTemplateRendererTests.cs`
  - Added reset template render test.
- `services/orgs-api/tests/SaasTemplate.OrgsApi.IntegrationTests/Api/AuthEndpointTests.cs`
  - Added forgot/reset endpoint integration tests.
- `apps/web/src/components/auth/ForgotPasswordForm.tsx`
  - Added forgot-password form.
- `apps/web/src/components/auth/ResetPasswordForm.tsx`
  - Added reset-password form.
- `apps/web/src/app/[locale]/forgot-password/page.tsx`
  - Added forgot-password page.
- `apps/web/src/app/[locale]/reset-password/page.tsx`
  - Added reset-password page.
- `apps/web/src/components/auth/LoginForm.tsx`
  - Added forgot-password link.
- `apps/web/src/lib/schemas.ts`
  - Added forgot/reset form schemas.
- `apps/web/messages/en.json`
  - Added EN forgot/reset strings.
- `apps/web/messages/es.json`
  - Added ES forgot/reset strings.
- `apps/web/src/components/auth/__tests__/ForgotPasswordForm.test.tsx`
  - Added forgot-password form tests.
- `apps/web/src/components/auth/__tests__/ResetPasswordForm.test.tsx`
  - Added reset-password form tests.
- `apps/web/src/components/auth/__tests__/LoginForm.test.tsx`
  - Updated login tests for forgot-password link.
- `docs/backlog/epic-002-auth-security.md`
  - Marked task 0028 as `IN_PROGRESS`.

## Tests
### Unit
- `dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln` includes:
  - Forgot/reset command handler tests.
  - JWT password reset token tests.
  - Password reset email localization tests.
  - Template renderer tests.
- `dotnet test services/ai-api/SaasTemplate.AiApi.sln` passed (no task-specific changes, run per workflow quality gate).

### Integration
- `dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln` passed (includes full integration suite).
- `dotnet test services/orgs-api/tests/SaasTemplate.OrgsApi.IntegrationTests/SaasTemplate.OrgsApi.IntegrationTests.csproj --filter "FullyQualifiedName~AuthEndpointTests"` passed.
- `npm test -- --run` passed (new forgot/reset frontend tests included).
- `npm run build` passed (new routes generated for forgot/reset pages).

### Manual
- Completed (validated by user on 2026-02-08):
  - Full forgot-password flow using Mailhog delivery.
  - Opened reset link from Mailhog and completed reset on UI.
  - Confirmed old password fails and new password succeeds.

## Observability
- Logs added/updated:
  - SMTP service logs successful/failed password reset sends.
- Traces/metrics added/updated:
  - No new custom telemetry added in this task.

## Security
- Validation:
  - API validators for forgot/reset DTOs.
  - Zod client-side validation for forgot/reset forms.
  - CSRF required for both new endpoints.
- AuthN/AuthZ impact:
  - Endpoints are anonymous by design (`forgot-password`, `reset-password`).
  - Reset token validates signature, issuer, audience, purpose, and expiry.
  - Used-token detection via password-hash fingerprint.
- Sensitive data handling (secrets, PII):
  - No secrets committed.
  - Forgot-password response does not disclose account existence.
  - Reset email includes single-use semantic link via short-lived token.

## Follow-ups / Backlog
- [ ] Review/resolve existing package warnings in build output (`OpenTelemetry` advisory and EF package version alignment), out of this task scope.

## Checklist
- [x] Task scope matches `docs/tasks/0028-password-reset.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
