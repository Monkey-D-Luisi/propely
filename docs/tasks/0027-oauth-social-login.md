# Task: 0027 - OAuth Social Login (Google + GitHub)

## Metadata
- ID: 0027
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-07
- GitHub Issue: #179
- Epic: `docs/backlog/epic-002-auth-security.md`
- Old Issue: #8

## Goal
Add Google and GitHub OAuth social login providers so users can sign in without creating a password-based account.

## Context
The current auth system only supports email/password registration and login. Modern SaaS apps offer social login for convenience and security. The orgs-api uses JWT-based auth with `AuthController` for login/register. OAuth will add alternative authentication paths that still produce the same JWT tokens.

### Current Auth Flow
- `POST /auth/register` -> creates user with BCrypt-hashed password -> returns JWT
- `POST /auth/login` -> validates credentials -> returns JWT
- `GET /auth/me` -> returns current user from JWT claims
- JWT is stored in HTTP-only cookie (`access_token`)
- Frontend: `LoginForm.tsx`, `RegisterForm.tsx` use react-hook-form + zod

## Scope
### In scope
- Add Google OAuth provider configuration
- Add GitHub OAuth provider configuration
- Create OAuth callback endpoint in AuthController
- Handle account linking (existing email user signs in with OAuth)
- Create "Continue with Google" and "Continue with GitHub" buttons on login/register pages
- Store OAuth provider info on User entity (optional: ExternalProvider, ExternalId)
- Issue same JWT token after OAuth authentication
- Add i18n strings for OAuth buttons and error messages (EN + ES)
- Add environment variables for OAuth client IDs/secrets

### Out of scope
- Other OAuth providers (Microsoft, Apple, etc.)
- OAuth token refresh (we use our own JWT, not the provider's token)
- Profile picture sync from OAuth provider

## Requirements
- R1: Users can click "Continue with Google" on login/register page and authenticate via Google OAuth
- R2: Users can click "Continue with GitHub" on login/register page and authenticate via GitHub OAuth
- R3: If a user with the same email already exists, OAuth login links to the existing account
- R4: If no user exists with that email, a new user is created automatically
- R5: After OAuth authentication, user receives the same JWT token as password login
- R6: OAuth client IDs and secrets are configured via environment variables (never committed)
- R7: OAuth buttons are internationalized (EN + ES)

## Acceptance Criteria
- AC1: Clicking "Continue with Google" redirects to Google consent screen and returns authenticated
- AC2: Clicking "Continue with GitHub" redirects to GitHub authorization and returns authenticated
- AC3: A new user created via OAuth can access all authenticated endpoints
- AC4: An existing password-based user can also sign in via OAuth if emails match
- AC5: `dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln` succeeds
- AC6: `dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln` passes
- AC7: `cd apps/web && npm run build` succeeds
- AC8: `cd apps/web && npm test` passes
- AC9: i18n strings exist in both EN and ES message files

## Constraints (non-negotiable)
- Clean Architecture layers respected (OAuth logic in Infrastructure, interfaces in Application)
- English-only repo content
- No secrets in repo (OAuth credentials via env vars only)
- Update walkthrough at `docs/walkthroughs/0027-oauth-social-login.md`

## Implementation Steps

### Backend (orgs-api)

1. **Add NuGet packages**
   - Add `Microsoft.AspNetCore.Authentication.Google` to `SaasTemplate.OrgsApi.Api`
   - Add `Microsoft.AspNetCore.Authentication.GitHub` (or `AspNet.Security.OAuth.GitHub`) to `SaasTemplate.OrgsApi.Api`

2. **Extend User entity** (`services/orgs-api/src/SaasTemplate.OrgsApi.Domain/Users/User.cs`)
   - Add optional properties: `ExternalProvider` (string?), `ExternalId` (string?)
   - These allow tracking how a user signed up (password vs OAuth)

3. **Add EF migration for User columns**
   - Update `UserConfiguration.cs` (`services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Configurations/UserConfiguration.cs`)
   - Add `ExternalProvider` (nullable, max 50) and `ExternalId` (nullable, max 256) columns
   - Run: `dotnet ef migrations add AddOAuthFields --project services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure --startup-project services/orgs-api/src/SaasTemplate.OrgsApi.Api`

4. **Create OAuth configuration** (`services/orgs-api/src/SaasTemplate.OrgsApi.Api/Configuration/OAuthConfiguration.cs`)
   - Static extension method `AddOAuthProviders(this AuthenticationBuilder builder, IConfiguration config)`
   - Read `OAuth:Google:ClientId`, `OAuth:Google:ClientSecret` from config
   - Read `OAuth:GitHub:ClientId`, `OAuth:GitHub:ClientSecret` from config
   - Configure callback paths: `/auth/oauth/google-callback`, `/auth/oauth/github-callback`

5. **Create Application-layer command** (`services/orgs-api/src/SaasTemplate.OrgsApi.Application/Auth/Commands/OAuthLogin/OAuthLoginCommand.cs`)
   - Properties: `Email`, `Name`, `Provider`, `ExternalId`
   - Handler: find existing user by email -> if exists, update ExternalProvider/ExternalId -> if not, create new user -> return JWT

6. **Add OAuth endpoints to AuthController** (`services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/AuthController.cs`)
   - `GET /auth/oauth/google` -> Challenge with Google scheme, redirect to `/auth/oauth/callback`
   - `GET /auth/oauth/github` -> Challenge with GitHub scheme, redirect to `/auth/oauth/callback`
   - `GET /auth/oauth/callback` -> Extract claims from external cookie -> send OAuthLoginCommand -> set JWT cookie -> redirect to frontend

7. **Register OAuth in Program.cs** (`services/orgs-api/src/SaasTemplate.OrgsApi.Api/Program.cs`)
   - Call `AddOAuthProviders()` after existing JWT auth configuration
   - Only register if OAuth env vars are present (graceful degradation)

8. **Add env vars to `.env`**
   - `ORGSAPI_OAuth__Google__ClientId=` (empty, user fills in)
   - `ORGSAPI_OAuth__Google__ClientSecret=`
   - `ORGSAPI_OAuth__GitHub__ClientId=`
   - `ORGSAPI_OAuth__GitHub__ClientSecret=`

### Frontend (apps/web)

9. **Add OAuth buttons component** (`apps/web/src/components/auth/OAuthButtons.tsx`)
   - "Continue with Google" button with Google icon
   - "Continue with GitHub" button with GitHub icon
   - Each button links to `${API_URL}/auth/oauth/google` or `/auth/oauth/github`
   - Show loading state while redirecting

10. **Update LoginForm** (`apps/web/src/components/auth/LoginForm.tsx`)
    - Add `<OAuthButtons />` above or below the email/password form
    - Add divider "or" between OAuth and password sections

11. **Update RegisterForm** (`apps/web/src/components/auth/RegisterForm.tsx`)
    - Same OAuth buttons as login page

12. **Add i18n strings**
    - EN (`apps/web/messages/en.json`): `auth.continueWithGoogle`, `auth.continueWithGithub`, `auth.orDivider`, `auth.oauthError`
    - ES (`apps/web/messages/es.json`): Spanish translations

### Testing

13. **Backend integration tests**
    - Test OAuthLoginCommand handler: creates new user when no existing email
    - Test OAuthLoginCommand handler: links to existing user when email matches
    - Test OAuth callback endpoint returns JWT cookie

14. **Frontend tests**
    - Test OAuthButtons renders both buttons
    - Test LoginForm includes OAuth buttons
    - Test RegisterForm includes OAuth buttons

## Files to Create / Modify

### Create
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Configuration/OAuthConfiguration.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Auth/Commands/OAuthLogin/OAuthLoginCommand.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Auth/Commands/OAuthLogin/OAuthLoginCommandHandler.cs`
- `apps/web/src/components/auth/OAuthButtons.tsx`
- `docs/walkthroughs/0027-oauth-social-login.md`

### Modify
- `services/orgs-api/src/SaasTemplate.OrgsApi.Domain/Users/User.cs` (add ExternalProvider, ExternalId)
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Configurations/UserConfiguration.cs` (map new columns)
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/AuthController.cs` (add OAuth endpoints)
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Program.cs` (register OAuth)
- `apps/web/src/components/auth/LoginForm.tsx` (add OAuth buttons)
- `apps/web/src/components/auth/RegisterForm.tsx` (add OAuth buttons)
- `apps/web/messages/en.json` (add OAuth strings)
- `apps/web/messages/es.json` (add OAuth strings)
- `.env` (add OAuth env var placeholders)

## Testing Plan
- Unit tests: OAuthLoginCommandHandler (new user, existing user, linked account)
- Integration tests: OAuth callback flow (mocked external provider)
- Frontend tests: OAuthButtons component, updated LoginForm, updated RegisterForm
- Manual test: Full OAuth flow with real Google/GitHub credentials in dev environment

## Implemented Decisions
- Provider linkage is stored in `user_external_logins` (not `users` columns).
- OAuth sign-in requires verified email; unverified or missing email is rejected.
- Redirect `next` accepts only relative paths; invalid input falls back to `/`.
- Existing user with same provider but different external id is rejected with conflict (`PROVIDER_ALREADY_LINKED`).
- OAuth providers are registered only when credentials are configured.

## API Endpoints
- `GET /auth/oauth/google?next=<relative-path>`
- `GET /auth/oauth/github?next=<relative-path>`
- `GET /auth/oauth/callback?provider=<google|github>&next=<relative-path>`

## Data / Schema Changes
- Added table `user_external_logins` with:
  - unique `(provider, external_id)`
  - unique `(user_id, provider)`
  - FK `user_id -> users.id`
- Migration: `AddUserExternalLogins`

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes (`dotnet build` + `npm run build`)
- [x] Tests added/updated and pass (`dotnet test` + `npm test`)
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] i18n strings added (EN + ES)
- [x] Walkthrough updated
