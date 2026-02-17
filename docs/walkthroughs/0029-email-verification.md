# Walkthrough: 0029-email-verification

## Task Reference
- Task: `docs/tasks/0029-email-verification.md`
- Walkthrough: `docs/walkthroughs/0029-email-verification.md`
- Branch/PR: `feat/email-verification-0029` / `TBD`
- Date: `2026-02-08`

## Summary
Implemented a complete email verification flow for new registrations in `orgs-api` and `web`.
This includes verification token generation and validation, verification/resend endpoints, verification email templates (EN/ES), UI verification states, unverified-user banner, and full automated test coverage updates.

## Context
- Background: users could register and immediately use the application without proving ownership of the email address.
- Problem statement: missing first-party email ownership verification introduces security and communication reliability gaps.
- Constraints: keep scope limited to task 0029 and preserve Clean Architecture boundaries.

## Decisions & Trade-offs
- **Decision:** Use stateless JWT verification tokens with `purpose=email-verification`.
  - Option considered: DB-backed one-time tokens.
  - Why this choice: consistent with existing password-reset token pattern and no extra persistence surface.
  - Trade-off: explicit token revocation is not supported; strict claim and lifetime validation is required.

- **Decision:** Keep user sign-in behavior after registration and enforce verification awareness in UI.
  - Why this choice: satisfies task scope ("allow login but show verification banner") while preserving current auth flow.
  - Trade-off: unverified users are authenticated, so enforcement relies on feature-level restrictions and clear UI guidance.

- **Decision:** Localize verification emails and links using request locale with `Accept-Language` fallback.
  - Why this choice: aligns with EN/ES i18n scope and existing localization behavior.

## Implementation Notes
- Added verification timestamp tracking on `User` and persisted it in EF configuration/migration.
- Added email verification token API in `IJwtTokenService` and implementation in `JwtTokenService`.
- Added email verification link builder and localized email model/template support.
- Updated registration flow to send verification email after user creation.
- Added `VerifyEmail` and `ResendVerification` command handlers and controller endpoints.
- Added endpoint-specific rate limit (`1 request / 5 minutes`) for `POST /auth/resend-verification`.
- Added frontend verification page (`/[locale]/verify-email`) with auto-submit and resend UX.
- Added persistent verification banner for authenticated but unverified users in app header.
- Added registration password confirmation field with matching validation.
- Moved `<html>/<body>` to the real root layout (`app/layout.tsx`) to fix Next.js root-layout compliance for non-locale paths.
- Added an auth-state refresh event (`auth:user-updated`) after successful email verification so header user data is re-fetched and the verification banner disappears without manual page refresh.
- Updated verify-email post-success redirect to be session-aware: authenticated users go to `/`, unauthenticated users go to `/login`.
- Hardened `dev-up` scripts to rebuild and recreate containers on startup (`--build --force-recreate --remove-orphans`) to avoid stale runtime state.
- Added/updated unit and integration tests for backend and frontend verification scenarios.

## Data / Schema / Migrations
- DB change: added `email_verified_at_utc` to `users` and ensured `email_verified` default is `false`.
- Migration: `AddEmailVerification` created under `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Migrations/`.
- Backward compatibility: existing users remain valid; verification fields default safely.

## Commands Run
```bash
git checkout main
git pull origin main
git checkout -b feat/email-verification-0029

dotnet ef migrations add AddEmailVerification --project services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure --startup-project services/orgs-api/src/SaasTemplate.OrgsApi.Api

dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln --filter "FullyQualifiedName~JwtTokenServiceTests.GenerateAndValidateEmailVerificationToken_ShouldReturnPayload|FullyQualifiedName~AuthEndpointTests.VerifyEmail_WithValidToken_ShouldMarkUserAsVerified"

dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln

cd apps/web && npm test -- --run
cd apps/web && npm run build

dotnet build services/ai-api/SaasTemplate.AiApi.sln
dotnet test services/ai-api/SaasTemplate.AiApi.sln
```

## Files Changed
- Backend domain/persistence:
  - `services/orgs-api/src/SaasTemplate.OrgsApi.Domain/Users/User.cs`
  - `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Configurations/UserConfiguration.cs`
  - `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Migrations/20260208200904_AddEmailVerification.cs`
  - `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Migrations/20260208200904_AddEmailVerification.Designer.cs`
  - `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Migrations/AppDbContextModelSnapshot.cs`
- Backend application/API:
  - `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Users/Interfaces/IJwtTokenService.cs`
  - `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Services/JwtTokenService.cs`
  - `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Interfaces/IEmailService.cs`
  - `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Services/SmtpEmailService.cs`
  - `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Users/Commands/RegisterUser/RegisterUserCommand.cs`
  - `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Users/Commands/RegisterUser/RegisterUserCommandHandler.cs`
  - `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Users/Commands/VerifyEmail/VerifyEmailCommand.cs`
  - `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Users/Commands/VerifyEmail/VerifyEmailCommandHandler.cs`
  - `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Users/Commands/ResendVerification/ResendVerificationCommand.cs`
  - `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Users/Commands/ResendVerification/ResendVerificationCommandHandler.cs`
  - `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Users/EmailVerificationLinkBuilder.cs`
  - `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/AuthController.cs`
  - `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Dtos/RegisterRequest.cs`
  - `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Dtos/VerifyEmailRequest.cs`
  - `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Validators/VerifyEmailRequestValidator.cs`
  - `services/orgs-api/src/SaasTemplate.OrgsApi.Api/DependencyInjection.cs`
- Email templates/localization:
  - `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Email/EmailVerificationEmailModel.cs`
  - `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Email/EmailVerificationEmailLocalization.cs`
  - `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Email/Templates/en/EmailVerification.cshtml`
  - `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Email/Templates/es/EmailVerification.cshtml`
- Frontend:
  - `apps/web/src/app/layout.tsx`
  - `apps/web/src/app/[locale]/layout.tsx`
  - `apps/web/src/app/[locale]/verify-email/page.tsx`
  - `apps/web/src/components/auth/VerifyEmailContent.tsx`
  - `apps/web/src/components/auth/VerificationBanner.tsx`
  - `apps/web/src/components/auth/RegisterForm.tsx`
  - `apps/web/src/components/layout/AppHeader.tsx`
  - `apps/web/src/lib/schemas.ts`
  - `apps/web/messages/en.json`
  - `apps/web/messages/es.json`
- Tests:
  - `services/orgs-api/tests/SaasTemplate.OrgsApi.UnitTests/Application/Users/Commands/RegisterUserCommandHandlerTests.cs`
  - `services/orgs-api/tests/SaasTemplate.OrgsApi.UnitTests/Application/Users/Commands/VerifyEmailCommandHandlerTests.cs`
  - `services/orgs-api/tests/SaasTemplate.OrgsApi.UnitTests/Application/Users/Commands/ResendVerificationCommandHandlerTests.cs`
  - `services/orgs-api/tests/SaasTemplate.OrgsApi.UnitTests/Infrastructure/Services/JwtTokenServiceTests.cs`
  - `services/orgs-api/tests/SaasTemplate.OrgsApi.UnitTests/Infrastructure/Email/RazorEmailTemplateRendererTests.cs`
  - `services/orgs-api/tests/SaasTemplate.OrgsApi.IntegrationTests/Api/AuthEndpointTests.cs`
  - `apps/web/src/components/auth/__tests__/RegisterForm.test.tsx`
  - `apps/web/src/components/auth/__tests__/VerificationBanner.test.tsx`
  - `apps/web/src/components/auth/__tests__/VerifyEmailContent.test.tsx`
  - `apps/web/src/components/layout/__tests__/AppHeader.test.tsx`
  - `apps/web/src/lib/__tests__/schemas.test.ts`
- Scripts:
  - `scripts/dev-up.ps1`
  - `scripts/dev-up.sh`

## Tests
### Unit
- Backend unit tests updated for register, verify-email, resend-verification, JWT verification token behavior, and email template rendering.
- Frontend component/unit tests added for verification page and banner behavior.

### Integration
- Added auth integration tests for:
  - verification email dispatch after register,
  - successful verification with valid token,
  - invalid/expired token handling,
  - resend verification flow and rate limiting.

### Quality Gates Results
- `dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln`: pass
- `dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln`: pass
- `dotnet build services/ai-api/SaasTemplate.AiApi.sln`: pass
- `dotnet test services/ai-api/SaasTemplate.AiApi.sln`: pass
- `cd apps/web && npm test -- --run`: pass
- `cd apps/web && npm run build`: pass

### Manual
- Pending user hand-off (required before marking task DONE):
  1. Register a new user from `/en/register` or `/es/register`.
  2. Open Mailhog and confirm verification email is received in the selected language.
  3. Open verification link and verify success UI and redirect to login.
  4. Call/refresh `/auth/me` and confirm `emailVerified` is `true`.
  5. While unverified (new account), click resend in banner and verify:
     - first call succeeds,
     - second call within 5 minutes returns rate-limited behavior.

## Observability
- No new telemetry primitives added in this task.
- Existing correlation ID, logging, and middleware behavior remains unchanged.

## Security
- Verification token validation enforces:
  - JWT signature/lifetime/issuer/audience validation,
  - `purpose=email-verification` claim,
  - user/email claim match against persisted user.
- `POST /auth/verify-email` and `POST /auth/resend-verification` remain CSRF-protected.
- `POST /auth/resend-verification` is authenticated and endpoint-rate-limited.
- No secrets were introduced in repository content.

## Follow-ups / Backlog
- [ ] Align OpenTelemetry package versions to remove persistent `NU1603`/`NU1902` restore warnings.
- [ ] Consider one-time verification token revocation strategy if stricter token invalidation is required.

## Checklist
- [x] Task scope matches `docs/tasks/0029-email-verification.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
