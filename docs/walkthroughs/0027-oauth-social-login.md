# Walkthrough: 0027-oauth-social-login

## Task Reference
- Task: `docs/tasks/0027-oauth-social-login.md`
- Walkthrough: `docs/walkthroughs/0027-oauth-social-login.md`
- Branch/PR: `main` / n/a
- Date: `2026-02-07`

## Summary
Implemented Google and GitHub OAuth login in `orgs-api` and web auth forms with strict verified-email enforcement, secure `next` redirect handling, and multi-provider account linking. The backend now stores external provider links in a dedicated table and reuses the existing JWT cookie flow. The frontend now exposes OAuth buttons on login/register and surfaces OAuth-specific error messages.

## Context
- Background: auth previously supported only email/password.
- Problem statement: users needed social login while preserving account security and existing auth contracts.
- Constraints: no secrets committed; Clean Architecture boundaries preserved.

## Decisions & Trade-offs
- **Decision: dedicated `user_external_logins` table for provider linkage**
  - Options considered: adding provider columns on `users` vs relational link table.
  - Why this choice: supports multi-provider linking cleanly and enforces DB uniqueness constraints.
  - Consequences / risks: one extra repository/entity and migration.

- **Decision: strict verified-email policy**
  - Options considered: allow provider accounts without verified email vs reject.
  - Why this choice: safer default to prevent linking/auth with untrusted email claims.
  - Consequences / risks: some provider users must verify email before first login.

- **Decision: redirect `next` must be relative**
  - Options considered: accept arbitrary redirect URLs vs sanitize to app-relative routes.
  - Why this choice: avoids open-redirect abuse.
  - Consequences / risks: invalid `next` values are dropped to `/`.

- **Decision: reject same-provider collision**
  - Options considered: silently overwrite provider external id vs conflict.
  - Why this choice: security-first behavior to avoid accidental account takeover/link drift.
  - Consequences / risks: manual remediation required if provider identity changes unexpectedly.

## Implementation Notes
- Key changes:
  - Added OAuth provider registration with external cookie scheme.
  - Added OAuth start/callback endpoints and error redirect flow.
  - Added `OAuthLoginCommand` + handler for link/create/verify logic.
  - Added frontend OAuth buttons and login/register query-param error rendering.
- Edge cases handled:
  - Unknown/unconfigured provider.
  - Missing external principal/email/external id.
  - Unverified provider email.
  - Provider collision (`PROVIDER_ALREADY_LINKED`).
- Known limitations:
  - OAuth error codes currently map to one generic UI message.

## Data / Schema / Migrations
- DB changes:
  - New table: `user_external_logins`
  - Unique indexes: `(provider, external_id)` and `(user_id, provider)`
  - FK: `user_id -> users.id`
- Migration strategy:
  - Added migration `AddUserExternalLogins` in `orgs-api` infrastructure migrations.
- Backward compatibility:
  - Additive schema change; existing users and login flows continue to work.

## Commands Run
```bash
dotnet ef migrations add AddUserExternalLogins --project services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure --startup-project services/orgs-api/src/SaasTemplate.OrgsApi.Api
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln
cd apps/web && npm test -- --run
cd apps/web && npm run build
cd apps/web && npm test -- --run src/components/auth/__tests__/OAuthButtons.test.tsx src/components/auth/__tests__/LoginForm.test.tsx src/components/auth/__tests__/RegisterForm.test.tsx
Remove-Item -LiteralPath "apps/web/src/hooks/feature-flags.ts"
Remove-Item -LiteralPath ".github/workflows/codeql.yml"
cmd /c del /f /q "\\?\C:\Users\luiss\source\repos\saas-template\nul"
```

## Files Changed
### Backend - Domain/Application/Infrastructure
- `services/orgs-api/src/SaasTemplate.OrgsApi.Domain/Users/User.cs` - added `emailVerified` creation option and `MarkEmailAsVerified`.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Domain/Users/UserExternalLogin.cs` - new external login entity.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Users/Interfaces/IUserExternalLoginRepository.cs` - new repository port.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Users/Commands/OAuthLogin/OAuthLoginCommand.cs` - OAuth command/result.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Users/Commands/OAuthLogin/OAuthLoginCommandHandler.cs` - link/create/verify/collision flow + JWT generation.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Configurations/UserExternalLoginConfiguration.cs` - EF mapping + constraints.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Repositories/UserExternalLoginRepository.cs` - repo implementation.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/AppDbContext.cs` - added `DbSet<UserExternalLogin>` and config.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/DependencyInjection.cs` - DI registration.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Migrations/20260207210317_AddUserExternalLogins.cs` - migration.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Migrations/20260207210317_AddUserExternalLogins.Designer.cs` - migration designer.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Migrations/AppDbContextModelSnapshot.cs` - updated snapshot.

### Backend - API
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/SaasTemplate.OrgsApi.Api.csproj` - added Google auth package.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Configuration/OAuthConstants.cs` - provider/scheme constants.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Configuration/OAuthConfiguration.cs` - provider registration and GitHub profile/email mapping.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/DependencyInjection.cs` - wired `AddOAuthProviders`.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/AuthController.cs` - OAuth start/callback endpoints and redirect/error helpers.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/appsettings.json` - OAuth/Auth fallback keys.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/appsettings.Development.json` - frontend base URL.

### Frontend
- `apps/web/src/components/auth/OAuthButtons.tsx` - Google/GitHub OAuth buttons with click-lock and `next` propagation.
- `apps/web/src/components/auth/LoginForm.tsx` - OAuth section, divider, `oauthError` handling.
- `apps/web/src/components/auth/RegisterForm.tsx` - OAuth section, divider, `oauthError` handling.
- `apps/web/src/app/[locale]/register/page.tsx` - wrapped register form in `Suspense` for `useSearchParams`.
- `apps/web/messages/en.json` - OAuth labels/divider/error message.
- `apps/web/messages/es.json` - OAuth labels/divider/error message.
- `.env.example` - added OAuth/Auth env placeholders.
- `apps/web/src/hooks/feature-flags.ts` - removed accidental duplicate file that shadowed `feature-flags.tsx` and broke build/tests.
- `.github/workflows/codeql.yml` - removed accidental untracked reintroduction (workflow intentionally removed historically).
- `nul` - removed invalid zero-byte root artifact causing tooling scan errors.

### Tests
- `services/orgs-api/tests/SaasTemplate.OrgsApi.UnitTests/Application/Users/Commands/OAuthLoginCommandHandlerTests.cs`
- `services/orgs-api/tests/SaasTemplate.OrgsApi.IntegrationTests/Api/AuthEndpointTests.cs`
- `services/orgs-api/tests/SaasTemplate.OrgsApi.IntegrationTests/Fixtures/ApiWebApplicationFactory.cs`
- `apps/web/src/components/auth/__tests__/OAuthButtons.test.tsx`
- `apps/web/src/components/auth/__tests__/LoginForm.test.tsx`
- `apps/web/src/components/auth/__tests__/RegisterForm.test.tsx`

## Tests
### Unit
- Added `OAuthLoginCommandHandlerTests` for:
  - existing linked login
  - new user creation + link
  - existing user link
  - provider collision reject
  - email verification update
- Result: included in `dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln` (pass).

### Integration
- Added OAuth endpoint coverage in `AuthEndpointTests`:
  - Google/GitHub challenge redirects
  - callback without principal -> oauth error redirect
  - unknown provider -> provider-not-configured redirect
- Result: included in full `orgs-api` integration run (pass).

### Frontend
- Added/updated OAuth auth tests:
  - `OAuthButtons.test.tsx`
  - `LoginForm.test.tsx` OAuth section + oauthError rendering
  - `RegisterForm.test.tsx` OAuth section + oauthError rendering
- Result:
  - Targeted run passed (`30/30` tests).
  - Full web suite passed (`235/235` tests) after cleanup.
  - `npm run build` passed after cleanup.

## Observability
- No telemetry/tracing changes required for this task.

## Security
- Validation: `next` sanitized to relative path only.
- AuthN/AuthZ impact: existing JWT scheme remains default; OAuth uses isolated external cookie scheme.
- Sensitive data handling:
  - OAuth credentials configured via env/appsettings placeholders only.
  - No secrets committed.
  - Provider-email verification enforced before login/link.

## Follow-ups / Backlog
- [ ] Add provider-specific client-side copy for distinct OAuth failure codes.
- [ ] Add callback success integration test with mocked external principal.

## Checklist
- [x] Task scope matches `docs/tasks/0027-oauth-social-login.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
