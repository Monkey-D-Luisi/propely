# Task: 0029 - Email Verification Flow

## Metadata
- ID: 0029
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-07
- GitHub Issue: #181
- Epic: `docs/backlog/epic-002-auth-security.md`
- Dependencies: 0039 (email templating system)

## Goal
Require new users to verify their email address after registration before granting full access to the application.

## Context
Currently, users register with an email and password and immediately have full access. There is no verification that the email actually belongs to the user. This is a security gap: fake emails can be used to create accounts, and there's no way to reliably contact users. The User entity (`services/orgs-api/src/SaasTemplate.OrgsApi.Domain/Users/User.cs`) needs an `EmailVerified` flag.

## Scope
### In scope
- Add `EmailVerified` (bool) and `EmailVerifiedAtUtc` (DateTime?) to User entity
- Send verification email on registration with a time-limited token
- `POST /auth/verify-email` endpoint to verify the token
- `POST /auth/resend-verification` endpoint to resend the email
- Restrict unverified users: allow login but show a "verify your email" banner
- Frontend: verification pending page, resend button, email verified confirmation
- EF Core migration for new User columns
- i18n strings (EN + ES)

### Out of scope
- Phone number verification
- Blocking unverified users entirely (they can still access with limited functionality)
- Admin ability to manually verify users

## Requirements
- R1: New registrations trigger a verification email with a time-limited link (24h)
- R2: Clicking the verification link marks the user's email as verified
- R3: Users can request a new verification email (rate limited to 1 per 5 min)
- R4: Unverified users see a persistent banner prompting email verification
- R5: The `/auth/me` response includes `emailVerified` status
- R6: Verification token is a short-lived JWT with `purpose=email-verification`

## Acceptance Criteria
- AC1: After registration, a verification email is sent
- AC2: Clicking the verification link sets `EmailVerified = true`
- AC3: `GET /auth/me` returns `emailVerified: true` after verification
- AC4: `POST /auth/resend-verification` sends a new email (rate limited)
- AC5: Expired verification tokens return a clear error
- AC6: Unverified user sees a verification banner in the UI
- AC7: `dotnet build` and `dotnet test` pass
- AC8: `npm run build` and `npm test` pass

## Constraints (non-negotiable)
- Clean Architecture layers respected
- English-only repo content
- No secrets in repo
- Verification token must be JWT (consistent with password reset pattern from 0028)
- Update walkthrough

## Implementation Steps

### Backend (orgs-api)

1. **Extend User entity** (`services/orgs-api/src/SaasTemplate.OrgsApi.Domain/Users/User.cs`)
   - Add `EmailVerified` (bool, default false)
   - Add `EmailVerifiedAtUtc` (DateTime?, default null)
   - Add method `VerifyEmail()` that sets both

2. **Update UserConfiguration** (`services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Configurations/UserConfiguration.cs`)
   - Map `EmailVerified` column (bool, default false)
   - Map `EmailVerifiedAtUtc` column (nullable datetime)

3. **Create EF migration**
   - `dotnet ef migrations add AddEmailVerification --project services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure --startup-project services/orgs-api/src/SaasTemplate.OrgsApi.Api`

4. **Extend IJwtTokenService** (`services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Interfaces/IJwtTokenService.cs`)
   - Add: `string GenerateEmailVerificationToken(Guid userId, string email)`
   - Add: `(Guid userId, string email)? ValidateEmailVerificationToken(string token)`

5. **Update RegisterUserCommandHandler** (`services/orgs-api/src/SaasTemplate.OrgsApi.Application/Auth/Commands/RegisterUser/RegisterUserCommandHandler.cs`)
   - After creating user, generate verification token
   - Send verification email via IEmailService
   - Verification link: `{FRONTEND_URL}/verify-email?token={token}`

6. **Create VerifyEmailCommand** (`services/orgs-api/src/SaasTemplate.OrgsApi.Application/Auth/Commands/VerifyEmail/VerifyEmailCommand.cs`)
   - Property: `Token` (string)
   - Handler: validate token -> find user -> call `VerifyEmail()` -> save

7. **Create ResendVerificationCommand** (`services/orgs-api/src/SaasTemplate.OrgsApi.Application/Auth/Commands/ResendVerification/ResendVerificationCommand.cs`)
   - Property: `UserId` (Guid)
   - Handler: find user -> if already verified, return success -> generate new token -> send email

8. **Add endpoints to AuthController** (`services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/AuthController.cs`)
   - `POST /auth/verify-email` body: `{ token }` -> VerifyEmailCommand
   - `POST /auth/resend-verification` (requires auth) -> ResendVerificationCommand

9. **Update UserResponse DTO** (`services/orgs-api/src/SaasTemplate.OrgsApi.Api/DTOs/UserResponse.cs`)
   - Add `EmailVerified` (bool) to the response

10. **Update GetCurrentUserQueryHandler** to include `EmailVerified` in response

### Frontend (apps/web)

11. **Create verify-email page** (`apps/web/src/app/[locale]/verify-email/page.tsx`)
    - Read `token` from URL query params
    - Auto-submit `POST /auth/verify-email` on mount
    - Show success: "Email verified! Redirecting..." -> redirect to home
    - Show error: "Token expired" with resend button

12. **Create verification banner component** (`apps/web/src/components/auth/VerificationBanner.tsx`)
    - Show at top of authenticated pages when `user.emailVerified === false`
    - Message: "Please verify your email. Check your inbox or [resend verification email]"
    - Resend button calls `POST /auth/resend-verification`

13. **Update AppHeader or layout** (`apps/web/src/components/layout/AppHeader.tsx`)
    - Include `<VerificationBanner />` when user is not verified

14. **Update useCurrentUser hook** (`apps/web/src/hooks/orgs.ts`)
    - Ensure `emailVerified` is in the user type and exposed

15. **Update Zod schemas** (`apps/web/src/lib/schemas.ts`)
    - Add `emailVerified` to MeResponseSchema

16. **Add i18n strings** (`apps/web/messages/en.json`, `apps/web/messages/es.json`)
    - `auth.verifyEmail`, `auth.verifyEmailSuccess`, `auth.verifyEmailExpired`, `auth.resendVerification`, `auth.resendVerificationSent`, `auth.verificationBanner`, `auth.verificationBannerResend`

### Testing

17. **Backend tests**
    - VerifyEmailCommand: valid token verifies, expired token fails, already verified is idempotent
    - ResendVerificationCommand: sends email, rate limited
    - RegisterUserCommand: sends verification email on registration

18. **Frontend tests**
    - Verify email page: auto-submits, shows success/error
    - VerificationBanner: shows for unverified users, hides for verified

## Files to Create / Modify

### Create
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Auth/Commands/VerifyEmail/VerifyEmailCommand.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Auth/Commands/VerifyEmail/VerifyEmailCommandHandler.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Auth/Commands/ResendVerification/ResendVerificationCommand.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Auth/Commands/ResendVerification/ResendVerificationCommandHandler.cs`
- `apps/web/src/app/[locale]/verify-email/page.tsx`
- `apps/web/src/components/auth/VerificationBanner.tsx`
- `docs/walkthroughs/0029-email-verification.md`

### Modify
- `services/orgs-api/src/SaasTemplate.OrgsApi.Domain/Users/User.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Configurations/UserConfiguration.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Interfaces/IJwtTokenService.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Services/JwtTokenService.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Auth/Commands/RegisterUser/RegisterUserCommandHandler.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/AuthController.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/DTOs/UserResponse.cs`
- `apps/web/src/hooks/orgs.ts`
- `apps/web/src/lib/schemas.ts`
- `apps/web/src/components/layout/AppHeader.tsx`
- `apps/web/messages/en.json`
- `apps/web/messages/es.json`

## Testing Plan
- Unit tests: VerifyEmailCommandHandler, ResendVerificationCommandHandler, updated RegisterUserCommandHandler
- Integration tests: Full verify flow, resend flow
- Frontend tests: Verify email page, VerificationBanner
- Manual test: Register -> check Mailhog -> click link -> verify

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] i18n strings added (EN + ES)
- [x] Walkthrough updated
