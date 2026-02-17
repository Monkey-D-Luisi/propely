# Walkthrough: audit-0006-session-invalidation

## Task Reference
- Task: `docs/tasks/audit-0006-session-invalidation.md`
- Walkthrough: `docs/walkthroughs/audit-0006-session-invalidation.md`
- Branch/PR: `fix/audit-0006-session-invalidation`
- Date: `2026-02-09`

## Summary
Added `PasswordVersion` counter to User entity and JWT tokens to ensure that password changes immediately invalidate all existing sessions. The `ActiveUserMiddleware` now validates the `pwd_ver` claim against the database on every authenticated request, returning 401 if the version doesn't match.

## Context
- Background: Audit epic-002 finding F4 identified that changing a password did not invalidate existing JWT tokens, allowing compromised tokens to remain valid for up to 24 hours.
- Problem statement: An attacker with a stolen JWT could continue using it even after the victim changed their password.
- Constraints: Must not break existing OAuth users (PasswordVersion defaults to 0), must use efficient single query in middleware.

## Decisions & Trade-offs
- **Decision: PasswordVersion counter vs token blacklist**
  - Options considered: (1) PasswordVersion counter in JWT, (2) Redis-backed token blacklist, (3) Refresh token rotation
  - Why this choice: PasswordVersion is stateless (no Redis dependency for core auth), simple to implement, and automatically invalidates ALL tokens on password change without tracking individual token IDs.
  - Consequences / risks: Requires a DB query per request in middleware (already existed via `ExistsByIdAsync`). Missing `pwd_ver` claim defaults to 0 for backward compatibility with pre-existing tokens.

## Implementation Notes
- Key changes: Added `PasswordVersion` int to User entity, included as `pwd_ver` JWT claim, validated in `ActiveUserMiddleware` via `IsActiveWithPasswordVersionAsync`.
- Edge cases handled: Missing `pwd_ver` claim defaults to version 0 (backward compat with existing tokens and OAuth users).
- Known limitations: This invalidates ALL sessions on password change, including the session that initiated the change. The user must re-login after changing their password.

## Data / Schema / Migrations
- DB changes: Added `password_version` integer column (NOT NULL, default 0) to `users` table.
- Migration: `20260209195818_AddPasswordVersionToUsers`
- Backward compatibility: Default value of 0 matches existing rows. No data migration needed.

## Commands Run
```bash
dotnet ef migrations add AddPasswordVersionToUsers --project services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure --startup-project services/orgs-api/src/SaasTemplate.OrgsApi.Api
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln
```

## Files Changed
- `services/orgs-api/src/SaasTemplate.OrgsApi.Domain/Users/User.cs` — Added `PasswordVersion` property, increment in `ChangePassword()`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Configurations/UserConfiguration.cs` — Added `password_version` column config
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Services/JwtTokenService.cs` — Added `pwd_ver` claim to `GenerateToken()`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Users/Interfaces/IUserRepository.cs` — Added `IsActiveWithPasswordVersionAsync` method
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Repositories/UserRepository.cs` — Implemented `IsActiveWithPasswordVersionAsync`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Middleware/ActiveUserMiddleware.cs` — Validates `pwd_ver` claim against DB
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Migrations/20260209195818_AddPasswordVersionToUsers.cs` — New migration

## Tests
### Unit
- `ActiveUserMiddlewareTests.cs` — 4 tests: matching version passes through, mismatched version returns 401, missing claim defaults to 0, unauthenticated passes through
- How to run: `dotnet test services/orgs-api/tests/SaasTemplate.OrgsApi.UnitTests`

### Integration
- `AuthEndpointTests.cs` — 1 test: `ChangePassword_ShouldInvalidateOldToken` — registers user, changes password, verifies old token is rejected with 401
- How to run: `dotnet test services/orgs-api/tests/SaasTemplate.OrgsApi.IntegrationTests`

### Manual
- N/A

## Observability
- Logs added/updated: Existing middleware logging unchanged
- Traces/metrics added/updated: N/A

## Security
- Validation: `pwd_ver` claim validated on every authenticated request
- AuthN/AuthZ impact: Password changes now invalidate all sessions immediately
- Sensitive data handling: No secrets or PII affected

## Follow-ups / Backlog
- None

## Checklist
- [x] Task scope matches `docs/tasks/audit-0006-session-invalidation.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
