# Walkthrough: 0031-soft-deleted-user-auth

## Task Reference
- Task: `docs/tasks/0031-soft-deleted-user-auth.md`
- Walkthrough: `docs/walkthroughs/0031-soft-deleted-user-auth.md`
- Branch/PR: `feat/soft-deleted-user-auth-0031` / `TBD`
- Date: `2026-02-09`

## Summary
Implemented user soft-delete support (`users.is_deleted`, `users.deleted_at_utc`) with EF global query filtering and a middleware that invalidates authenticated requests for soft-deleted/missing users.
Added integration tests that prove:
- stale JWTs from soft-deleted users return `401` on `/auth/me`,
- login with deleted user credentials returns `401`,
- deleted users cannot call protected org operations,
- deleted users are excluded from organization members query results.

## Context
- Background: Soft delete exists for organizations/memberships/invitations, but user behavior required verification.
- Problem statement: task 0031 requires explicit coverage for `/auth/me`, login, protected operations, and members query with soft-deleted users.
- Constraints (time, scope, dependencies): keep scope focused on task 0031 and update task/backlog/walkthrough docs.

## Decisions & Trade-offs
- **Decision:** Enforce deleted-user rejection at API middleware level for authorized endpoints.
  - Options considered:
    - Check user existence in every controller/handler.
    - Add centralized middleware after authentication.
  - Why this choice:
    - Centralized behavior avoids missing endpoints and guarantees consistent `401` for any authenticated route.
  - Consequences / risks:
    - Middleware now performs one repository lookup per authenticated request.
- **Decision:** Reuse EF global query filters for deleted-user exclusion instead of ad-hoc predicates.
  - Options considered:
    - Manual `IsDeleted` checks in each query.
    - Add query filter on `User` entity.
  - Why this choice:
    - Keeps behavior consistent and reduces query drift, including joins such as members list.
  - Consequences / risks:
    - Admin/debug workflows that need deleted users must explicitly use `IgnoreQueryFilters()`.

## Implementation Notes
- Key changes:
  - `User` now implements `ISoftDeletable` with `IsDeleted`, `DeletedAtUtc`, and `SoftDelete()`.
  - `UserConfiguration` maps soft-delete columns and applies `HasQueryFilter(u => !u.IsDeleted)`.
  - New middleware `ActiveUserMiddleware` validates that authenticated principal maps to an active user; otherwise returns `401` and clears `access_token` cookie.
  - Pipeline updated with `app.UseActiveUserValidation()` after `UseAuthentication()`.
  - New integration tests in `SoftDeletedUserAuthTests` for AC1-AC4.
- Edge cases handled:
  - Authenticated request with malformed/missing `sub` claim now returns `401`.
  - Login test uses a fresh client to ensure failure is due to deleted user lookup, not stale cookie from prior session.
  - Member-list test asserts pre-condition (member visible) before soft-delete, then verifies exclusion.
- Known limitations:
  - No restore endpoint is introduced in this task (out of scope).

## Data / Schema / Migrations
- DB changes (if any):
  - Added columns to `users` table:
    - `is_deleted` (`boolean`, default `false`)
    - `deleted_at_utc` (`timestamp with time zone`, nullable)
- Migration strategy:
  - Added EF migration: `20260209090508_AddUserSoftDelete`.
- Backward compatibility:
  - Existing users default to `is_deleted = false`, preserving current behavior for active accounts.

## Commands Run
```bash
git checkout main
git pull origin main
git checkout -b feat/soft-deleted-user-auth-0031

dotnet ef migrations add AddUserSoftDelete --project services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure --startup-project services/orgs-api/src/SaasTemplate.OrgsApi.Api

dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln

dotnet build services/ai-api/SaasTemplate.AiApi.sln
dotnet test services/ai-api/SaasTemplate.AiApi.sln
```

## Files Changed
- `docs/backlog/epic-002-auth-security.md` - task 0031 moved to DONE.
- `docs/walkthroughs/0031-soft-deleted-user-auth.md` - this walkthrough.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Domain/Users/User.cs` - added soft-delete behavior/properties.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Configurations/UserConfiguration.cs` - mapped user soft-delete columns and global filter.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Middleware/ActiveUserMiddleware.cs` - centralized active-user enforcement for authorized endpoints.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Program.cs` - registered active-user middleware in request pipeline.
- `services/orgs-api/tests/SaasTemplate.OrgsApi.IntegrationTests/Api/SoftDeletedUserAuthTests.cs` - added AC1-AC4 integration coverage.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Migrations/20260209090508_AddUserSoftDelete.cs` - schema migration.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Migrations/20260209090508_AddUserSoftDelete.Designer.cs` - migration snapshot.
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Migrations/AppDbContextModelSnapshot.cs` - updated model snapshot.

## Tests
### Unit
- What was added/updated:
- No new unit tests; integration coverage added per task AC.
- How to run:
- `dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln`

### Integration
- What was added/updated:
- Added `SoftDeletedUserAuthTests` with 4 scenarios:
  - stale JWT + `/auth/me` => `401`
  - login with deleted credentials => `401`
  - protected org endpoint with deleted JWT => `401`
  - deleted users excluded from `/orgs/{orgId}/members`
- How to run:
- `dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln`

### Manual
- What you verified:
- User performed the full manual verification flow after test user registration.
- Test user `test@delete.test` was soft-deleted in DB and endpoints were validated.
- Steps:
  1. Register a user and keep session cookie.
  2. Soft-delete that user directly in DB (`users.is_deleted = true`).
  3. Call `/auth/me` and one protected endpoint (e.g., `/orgs/mine`) from browser/app.
  4. Confirm both are rejected (`401`) and session is effectively invalidated.
  5. Attempt login again with deleted user credentials and confirm `401`.

## Observability
- Logs added/updated:
- Added warning log for malformed authenticated identity claim in active-user middleware.
- Traces/metrics added/updated:
- None.

## Security
- Validation:
- Deleted users are treated as non-existent for authenticated access and credential login.
- AuthN/AuthZ impact:
- All authorized endpoints now enforce that JWT subject maps to an active (non-soft-deleted) user.
- Sensitive data handling (secrets, PII):
- No secrets added or committed.

## Follow-ups / Backlog
- [ ] Evaluate adding a lightweight cache for active-user existence checks if request volume grows.

## Checklist
- [x] Task scope matches `docs/tasks/0031-soft-deleted-user-auth.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
