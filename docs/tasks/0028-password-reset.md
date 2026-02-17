# Task: 0028 - Forgot Password / Password Reset via Email Token

## Metadata
- ID: 0028
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-07
- GitHub Issue: #180
- Epic: `docs/backlog/epic-002-auth-security.md`
- Old Issue: #9
- Dependencies: 0039 (email templating system)

## Goal
Allow users to reset their password via an email token link when they forget their credentials.

## Context
Currently there is no password recovery mechanism. If a user forgets their password, they have no way to regain access. The orgs-api has `ChangePassword` (requires current password) but no "forgot password" flow. The email infrastructure (Mailhog in dev, SmtpEmailService) already exists but needs the templating system from task 0039.

### Current Auth Endpoints
- `POST /auth/register` - registration
- `POST /auth/login` - login
- `GET /auth/me` - current user
- `PUT /auth/password` - change password (requires old password)
- `PATCH /auth/me` - update profile

## Scope
### In scope
- `POST /auth/forgot-password` endpoint (accepts email, sends reset link)
- `POST /auth/reset-password` endpoint (accepts token + new password, resets password)
- Time-limited JWT token for password reset (15-30 min expiry)
- Frontend "Forgot password?" page with email input
- Frontend "Reset password" page (from email link) with new password input
- Rate limiting on forgot-password endpoint (prevent email bombing)
- i18n strings (EN + ES)

### Out of scope
- SMS-based password reset
- Security questions
- Admin-initiated password reset

## Requirements
- R1: User can request a password reset by entering their email
- R2: System sends an email with a time-limited reset link (15-30 min)
- R3: Reset link contains a JWT token scoped to password reset (not a session token)
- R4: Clicking the link opens a form to enter a new password
- R5: Submitting the new password with valid token updates the password
- R6: Expired or already-used tokens are rejected with a clear error message
- R7: Requesting reset for a non-existent email returns 200 (no email enumeration)
- R8: Rate limiting prevents abuse of the forgot-password endpoint

## Acceptance Criteria
- AC1: `POST /auth/forgot-password` with valid email sends a reset email
- AC2: `POST /auth/forgot-password` with unknown email returns 200 (no leak)
- AC3: `POST /auth/reset-password` with valid token and new password succeeds
- AC4: `POST /auth/reset-password` with expired token returns 400
- AC5: After reset, user can login with the new password
- AC6: After reset, old password no longer works
- AC7: Frontend forgot-password page submits email and shows success message
- AC8: Frontend reset-password page accepts new password and redirects to login
- AC9: `dotnet build` and `dotnet test` pass
- AC10: `npm run build` and `npm test` pass

## Constraints (non-negotiable)
- Clean Architecture layers respected
- English-only repo content
- No secrets in repo
- Reset token must be a JWT with short expiry (not a random DB-stored token)
- Never reveal whether an email exists in the system
- Update walkthrough

## Implementation Steps

### Backend (orgs-api)

1. **Create ForgotPasswordCommand** (`services/orgs-api/src/SaasTemplate.OrgsApi.Application/Auth/Commands/ForgotPassword/ForgotPasswordCommand.cs`)
   - Property: `Email` (string)
   - Handler: Look up user by email -> if exists, generate reset JWT token -> send email via IEmailService -> always return success
   - Validator: Email is required and valid format

2. **Create ResetPasswordCommand** (`services/orgs-api/src/SaasTemplate.OrgsApi.Application/Auth/Commands/ResetPassword/ResetPasswordCommand.cs`)
   - Properties: `Token` (string), `NewPassword` (string)
   - Handler: Validate JWT token -> extract userId -> hash new password -> update user -> return success
   - Validator: Token required, NewPassword required + min length

3. **Extend IJwtTokenService** (`services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Interfaces/IJwtTokenService.cs`)
   - Add method: `string GeneratePasswordResetToken(Guid userId, string email)`
   - Add method: `(Guid userId, string email)? ValidatePasswordResetToken(string token)`

4. **Implement in JwtTokenService** (`services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Services/JwtTokenService.cs`)
   - Reset token: short-lived JWT (15 min), with custom claim `purpose=password-reset`
   - Validation: check expiry, check purpose claim, extract userId

5. **Add endpoints to AuthController** (`services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/AuthController.cs`)
   - `POST /auth/forgot-password` -> send ForgotPasswordCommand -> return 200 always
   - `POST /auth/reset-password` -> send ResetPasswordCommand -> return 200 on success

6. **Add DTOs** (`services/orgs-api/src/SaasTemplate.OrgsApi.Api/DTOs/`)
   - `ForgotPasswordRequest.cs`: `Email` property
   - `ResetPasswordRequest.cs`: `Token`, `NewPassword` properties

7. **Update IEmailService call** in ForgotPasswordCommandHandler
   - Use email template from task 0039 (or inline HTML if 0039 not done yet)
   - Reset link format: `{FRONTEND_URL}/reset-password?token={token}`

### Frontend (apps/web)

8. **Create forgot-password page** (`apps/web/src/app/[locale]/forgot-password/page.tsx`)
   - Email input form with react-hook-form + zod
   - Submit calls `POST /auth/forgot-password`
   - Show success message: "If an account exists, we sent a reset link"
   - Link back to login page

9. **Create reset-password page** (`apps/web/src/app/[locale]/reset-password/page.tsx`)
   - Read `token` from URL query params
   - New password + confirm password form with react-hook-form + zod
   - Submit calls `POST /auth/reset-password` with token and new password
   - On success: show message and redirect to login
   - On error (expired token): show error with link to request new reset

10. **Add "Forgot password?" link to LoginForm** (`apps/web/src/components/auth/LoginForm.tsx`)
    - Link below password field: "Forgot password?" -> `/forgot-password`

11. **Add Zod schemas** (`apps/web/src/lib/schemas.ts`)
    - `forgotPasswordFormSchema`: email required
    - `resetPasswordFormSchema`: password required + min length + confirmation match

12. **Add i18n strings** (`apps/web/messages/en.json`, `apps/web/messages/es.json`)
    - `auth.forgotPassword`, `auth.forgotPasswordDescription`, `auth.resetPassword`, `auth.resetPasswordSuccess`, `auth.resetTokenExpired`, `auth.newPassword`, `auth.confirmPassword`, `auth.sendResetLink`, `auth.backToLogin`, `auth.resetLinkSent`

### Testing

13. **Backend tests**
    - ForgotPasswordCommandHandler: existing email sends email, unknown email succeeds silently
    - ResetPasswordCommandHandler: valid token resets password, expired token fails
    - JwtTokenService: generate and validate reset tokens

14. **Frontend tests**
    - Forgot password page: submits email, shows success
    - Reset password page: submits new password, handles errors

## Files to Create / Modify

### Create
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Auth/Commands/ForgotPassword/ForgotPasswordCommand.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Auth/Commands/ForgotPassword/ForgotPasswordCommandHandler.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Auth/Commands/ForgotPassword/ForgotPasswordCommandValidator.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Auth/Commands/ResetPassword/ResetPasswordCommand.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Auth/Commands/ResetPassword/ResetPasswordCommandHandler.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Auth/Commands/ResetPassword/ResetPasswordCommandValidator.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/DTOs/ForgotPasswordRequest.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/DTOs/ResetPasswordRequest.cs`
- `apps/web/src/app/[locale]/forgot-password/page.tsx`
- `apps/web/src/app/[locale]/reset-password/page.tsx`
- `docs/walkthroughs/0028-password-reset.md`

### Modify
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Interfaces/IJwtTokenService.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Services/JwtTokenService.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/AuthController.cs`
- `apps/web/src/components/auth/LoginForm.tsx` (add forgot password link)
- `apps/web/src/lib/schemas.ts` (add form schemas)
- `apps/web/messages/en.json`
- `apps/web/messages/es.json`

## Testing Plan
- Unit tests: ForgotPasswordCommandHandler, ResetPasswordCommandHandler, JwtTokenService reset methods
- Integration tests: Full forgot/reset flow via API
- Frontend tests: Both new pages, LoginForm link
- Manual test: Full flow with Mailhog

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] i18n strings added (EN + ES)
- [x] Walkthrough updated
