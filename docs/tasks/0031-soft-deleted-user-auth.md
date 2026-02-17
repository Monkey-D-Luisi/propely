# Task: 0031 - Soft-Deleted User Auth Handling Verification

## Metadata
- ID: 0031
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-07
- GitHub Issue: #183
- Epic: `docs/backlog/epic-002-auth-security.md`
- Old Issue: Codex #89

## Goal
Verify and ensure that soft-deleted users are properly handled across all authentication endpoints, returning appropriate errors instead of allowing access.

## Context
The codebase implements soft delete via `ISoftDeletable` interface and EF Core global query filters (`HasQueryFilter(u => !u.IsDeleted)`). The User entity implements `ISoftDeletable`. In theory, soft-deleted users should be invisible to all queries. However, we need to verify edge cases: JWT tokens issued before soft-delete, direct ID lookups, and the `/auth/me` endpoint behavior.

### Current Soft Delete Setup
- `ISoftDeletable` interface: `IsDeleted`, `DeletedAtUtc` properties
- Global query filter on User: `builder.HasQueryFilter(u => !u.IsDeleted)`
- EF global filters apply to all LINQ queries but can be bypassed with `IgnoreQueryFilters()`
- JWT tokens contain userId - token may still be valid after user is soft-deleted

## Scope
### In scope
- Verify `/auth/me` returns 401 when JWT belongs to a soft-deleted user
- Verify `/auth/login` fails for soft-deleted users
- Verify organization membership queries exclude soft-deleted users
- Add integration tests proving all edge cases
- Fix any gaps found during verification

### Out of scope
- JWT token revocation list (would be a separate feature)
- Hard delete of users
- Admin restore of soft-deleted users

## Requirements
- R1: `GET /auth/me` with JWT of soft-deleted user returns 401 Unauthorized
- R2: `POST /auth/login` with credentials of soft-deleted user returns 401
- R3: Soft-deleted users do not appear in organization member lists
- R4: Soft-deleted users cannot perform any authenticated operations
- R5: Clear error message (no internal details leaked)

## Acceptance Criteria
- AC1: Integration test: create user -> soft delete -> GET /auth/me with old JWT -> 401
- AC2: Integration test: create user -> soft delete -> POST /auth/login -> 401
- AC3: Integration test: soft-deleted user does not appear in GetMembers query
- AC4: Integration test: soft-deleted user cannot create organizations
- AC5: All existing tests still pass
- AC6: `dotnet build` and `dotnet test` pass

## Constraints (non-negotiable)
- Clean Architecture layers respected
- Do not bypass global query filters (that would defeat the purpose)
- English-only repo content
- Update walkthrough

## Implementation Steps

1. **Audit current auth flow for soft-delete handling**
   - Read `LoginUserCommandHandler` (`services/orgs-api/src/SaasTemplate.OrgsApi.Application/Auth/Commands/LoginUser/LoginUserCommandHandler.cs`)
   - Check if `IUserRepository.GetByEmailAsync()` uses global filter (it should via EF)
   - Read `GetCurrentUserQueryHandler` (`services/orgs-api/src/SaasTemplate.OrgsApi.Application/Auth/Queries/GetCurrentUser/GetCurrentUserQueryHandler.cs`)
   - Check if `IUserRepository.GetByIdAsync()` uses global filter

2. **Verify UserRepository** (`services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Repositories/UserRepository.cs`)
   - Confirm all queries go through DbContext which has global filter
   - Confirm no `IgnoreQueryFilters()` calls bypass soft-delete

3. **Check JWT validation middleware**
   - The JWT contains userId. After token validation, the app calls `/auth/me` which queries the user
   - If user is soft-deleted, the query returns null (global filter)
   - Verify that null user result is handled with 401, not 500

4. **Write integration tests** (`services/orgs-api/tests/`)
   - Test class: `SoftDeletedUserAuthTests`
   - Setup: Register user -> get JWT -> soft delete user via direct DB update
   - Test 1: GET /auth/me with old JWT -> expect 401
   - Test 2: POST /auth/login with deleted user creds -> expect 401
   - Test 3: Create org with deleted user JWT -> expect 401
   - Test 4: Deleted user not in member list (add to org before deletion, verify gone after)

5. **Fix any gaps found**
   - If GetCurrentUserQueryHandler returns 404 instead of 401 for null user, fix to return 401
   - If LoginUserCommandHandler leaks info about deletion, fix error message
   - Ensure consistent behavior: soft-deleted = user doesn't exist from API perspective

6. **Verify membership queries**
   - Check `GetMembersQueryHandler` -> does it join with User table? Does global filter apply?
   - If membership itself is not soft-deleted but user is, verify member list excludes them

## Files to Create / Modify

### Create
- `services/orgs-api/tests/SaasTemplate.OrgsApi.IntegrationTests/Auth/SoftDeletedUserAuthTests.cs` (or appropriate test location)
- `docs/walkthroughs/0031-soft-deleted-user-auth.md`

### Modify (only if gaps found)
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Auth/Queries/GetCurrentUser/GetCurrentUserQueryHandler.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Auth/Commands/LoginUser/LoginUserCommandHandler.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/AuthController.cs`

## Testing Plan
- Integration tests: All 4 scenarios described above
- Regression: All existing auth tests still pass
- Manual: Soft delete via DB, try accessing app with existing session

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] All edge cases verified with integration tests
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
