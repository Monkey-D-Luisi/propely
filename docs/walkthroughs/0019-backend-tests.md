# Walkthrough: 0019 - Backend Test Coverage Improvement

**Date:** 2026-02-06
**Branch:** `feat/0019-backend-tests`

## Summary

Added comprehensive unit and integration tests for the orgs-api service. Created 63 new unit tests covering all 9 command/query handlers and 24 integration tests covering all API endpoints with real cookie-based JWT authentication flow. Also fixed a LINQ-to-SQL translation bug in `GetOrgsPagedAsync` discovered during testing.

## What Changed

### Unit Tests (63 tests)

#### Command Handlers (47 tests)

| Handler | File | Tests | Coverage |
|---------|------|-------|----------|
| RegisterUserCommandHandler | `tests/UnitTests/Application/Users/Commands/RegisterUserCommandHandlerTests.cs` | 7 | Success, persistence, save changes, duplicate email (ConflictException), no persist on conflict, token generation, cancellation token |
| LoginUserCommandHandler | `tests/UnitTests/Application/Users/Commands/LoginUserCommandHandlerTests.cs` | 5 | Valid credentials, user not found, wrong password, no token call on failure, cancellation token |
| CreateOrganizationCommandHandler | `tests/UnitTests/Application/Organizations/Commands/CreateOrganizationCommandHandlerTests.cs` | 7 | Returns org id/name, persists org, creates owner membership, saves changes, cancellation token, trimmed name |
| CreateInvitationCommandHandler | `tests/UnitTests/Application/Organizations/Commands/CreateInvitationCommandHandlerTests.cs` | 8 | Returns invite URL, persists invitation, sends email, org not found, no create/email on not found, saves changes, cancellation token |
| AcceptInvitationCommandHandler | `tests/UnitTests/Application/Organizations/Commands/AcceptInvitationCommandHandlerTests.cs` | 10 | Valid accept, invalid token, already accepted, expired, email mismatch, already member (no duplicate), saves changes, cancellation token, no persist on invalid token |
| UpdateMemberRoleCommandHandler | `tests/UnitTests/Application/Organizations/Commands/UpdateMemberRoleCommandHandlerTests.cs` | 10 | Valid update, member not found, last owner protection, multiple owners allow downgrade, owner-to-owner no check, non-owner no check, saves changes, not found no save, cancellation token |

#### Query Handlers (16 tests)

| Handler | File | Tests | Coverage |
|---------|------|-------|----------|
| GetCurrentUserQueryHandler | `tests/UnitTests/Application/Users/Queries/GetCurrentUserQueryHandlerTests.cs` | 3 | User exists, not found returns null, cancellation token |
| GetMyOrgsQueryHandler | `tests/UnitTests/Application/Organizations/Queries/GetMyOrgsQueryHandlerTests.cs` | 5 | Valid query, page clamp to 1, pageSize clamp to 1, pageSize clamp to 100, cancellation token |
| GetMembersQueryHandler | `tests/UnitTests/Application/Organizations/Queries/GetMembersQueryHandlerTests.cs` | 8 | Member returns paged result, non-member throws ForbiddenException, non-member no query, search parameter, page clamp, pageSize clamp, cancellation token |

### Integration Tests (24 tests)

| File | Tests | Coverage |
|------|-------|----------|
| `tests/IntegrationTests/Api/AuthEndpointTests.cs` | 12 | CSRF token, register (valid/duplicate/no-csrf/invalid-email), login (valid/invalid), me (authenticated/not), logout, correlation ID header, security headers |
| `tests/IntegrationTests/Api/OrgsEndpointTests.cs` | 12 | Create org (auth/unauth), get my orgs (list/pagination), get members (list/search), create invitation, accept invitation (valid/invalid), update role (valid/not-found) |

### Bug Fixes

#### LINQ-to-SQL Translation Bug in `GetOrgsPagedAsync`

**File:** `src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Repositories/MembershipRepository.cs`

**Problem:** The `Join` result selector constructed an `OrgWithRole` directly, then `OrderBy` referenced a property of the constructed object. EF Core cannot translate `new OrgWithRole(...).Name` inside `OrderBy`.

**Before:**
```csharp
var query = _context.Memberships
    .Where(m => m.UserId == userId)
    .Join(_context.Organizations, m => m.OrganizationId, o => o.Id,
        (m, o) => new OrgWithRole(o.Id, o.Name, m.Role.ToString().ToLowerInvariant()))
    .OrderBy(o => o.Name);
```

**After:**
```csharp
var query = _context.Memberships
    .Where(m => m.UserId == userId)
    .Join(_context.Organizations, m => m.OrganizationId, o => o.Id,
        (m, o) => new { Membership = m, Organization = o });

var items = await query
    .OrderBy(x => x.Organization.Name)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .Select(x => new OrgWithRole(
        x.Organization.Id, x.Organization.Name,
        x.Membership.Role.ToString().ToLowerInvariant()))
    .ToListAsync(cancellationToken);
```

This follows the same pattern already used by `GetMembersPagedAsync`: anonymous type in Join, then OrderBy and Select separately.

### Test Infrastructure Changes

**File:** `tests/IntegrationTests/Fixtures/ApiWebApplicationFactory.cs`

- Added `Jwt:Secret` to in-memory configuration (for `JwtTokenService` at runtime)
- Added `IEmailService` mock (prevents SMTP calls during tests)
- Overrode default auth scheme from DevScheme to JWT Bearer (enables real cookie-based auth flow)
- Added explicit `JwtBearerOptions` configuration with token validation parameters and cookie event handler

**Why explicit JWT Bearer config?** In the minimal hosting model, `WebApplicationFactory.ConfigureAppConfiguration` runs after `Program.cs` registers services. The app's `AddAuthenticationAndAuthorization` reads `Jwt:Secret` at registration time — before the factory's in-memory config is available. So `useJwt = false` and `AddJwtBearer()` is called without options. The factory must configure JWT Bearer options directly via `services.Configure<JwtBearerOptions>`.

## Design Decisions

1. **Real auth flow over DevScheme** — Integration tests use actual JWT Bearer authentication with cookie flow, not the DevScheme bypass. This tests the real authentication pipeline end-to-end.

2. **NSubstitute + FluentAssertions** — Matches the existing ai-api test patterns. Uses `Arg.Do<T>` for argument capture and `Received()` for call verification.

3. **Unique emails per test** — Each test generates a unique email via `Guid.NewGuid()` to avoid cross-test interference, since all tests in a class share the same database.

4. **Test covers both success and failure** — Every handler has at least one test verifying the happy path and at least one testing error conditions (not found, forbidden, conflict, etc.).

## Verification

```
orgs-api:  146 tests passed (90 unit + 5 architecture + 51 integration)
ai-api:    132 tests passed (76 unit + 5 architecture + 56 integration)
Total:     278 tests, 0 failures
```

## Files Created

- `tests/UnitTests/Application/Users/Commands/RegisterUserCommandHandlerTests.cs`
- `tests/UnitTests/Application/Users/Commands/LoginUserCommandHandlerTests.cs`
- `tests/UnitTests/Application/Users/Queries/GetCurrentUserQueryHandlerTests.cs`
- `tests/UnitTests/Application/Organizations/Commands/CreateOrganizationCommandHandlerTests.cs`
- `tests/UnitTests/Application/Organizations/Commands/CreateInvitationCommandHandlerTests.cs`
- `tests/UnitTests/Application/Organizations/Commands/AcceptInvitationCommandHandlerTests.cs`
- `tests/UnitTests/Application/Organizations/Commands/UpdateMemberRoleCommandHandlerTests.cs`
- `tests/UnitTests/Application/Organizations/Queries/GetMyOrgsQueryHandlerTests.cs`
- `tests/UnitTests/Application/Organizations/Queries/GetMembersQueryHandlerTests.cs`
- `tests/IntegrationTests/Api/AuthEndpointTests.cs`
- `tests/IntegrationTests/Api/OrgsEndpointTests.cs`

## Files Modified

- `src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Repositories/MembershipRepository.cs` (LINQ bug fix)
- `tests/IntegrationTests/Fixtures/ApiWebApplicationFactory.cs` (JWT Bearer auth config)
