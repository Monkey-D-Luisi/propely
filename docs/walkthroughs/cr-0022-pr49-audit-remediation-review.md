# CR-0022 Walkthrough: PR #49 Full Audit Remediation Review

## Task Reference
- **Task**: `docs/tasks/cr-0022-pr49-audit-remediation-review.md`
- **PR**: #49 (`feat/full-audit-remediation` -> `main`)

## What Changed

### 1. Authorization Policy Registration (all services)
All services now register three role-based authorization policies: `RequireOwnerOrAdmin`, `RequireAgent`, `RequireViewer`. These use hierarchical claim checks (higher roles include lower roles). The ai-api additionally maps work-item policies to role equivalents.

### 2. JWT Role Claim Emission (orgs-api)
- `IJwtTokenService.GenerateToken` accepts optional `IReadOnlyList<string>? roles` parameter
- `JwtTokenService` emits distinct lowercase role claims from membership data
- `LoginUserCommandHandler`, `OAuthLoginCommandHandler`, `RefreshAccessTokenCommandHandler` extract roles from memberships and pass to token generation
- Unit test NSubstitute mocks updated for the new 3-parameter signature (LoginUser: 4, OAuthLogin: 1, RegisterUser: 7)

### 3. OrgsController Policy Cleanup (orgs-api)
Removed redundant `[Authorize(Policy = ...)]` attributes from OrgsController, AgenciesController, BillingController, and PermissionsController. These handlers enforce membership-based authorization internally; claim-based role policies at the controller level blocked new users who haven't joined any org yet.

### 4. TestWebApplicationFactory Fixes (all services)
- **Dual-provider fix**: Replace broad `DbContextOptions<AppDbContext>` removal with comprehensive removal of all Npgsql and EF Core provider registrations, then re-add with InMemory provider using static DB name
- **Infrastructure config**: Clear `Redis:ConnectionString` and `RabbitMQ:Host` in both `appsettings.Testing.json` and `AddInMemoryCollection` across all 6 services
- **DefaultUserId**: Changed from `Guid.Empty` to `00000000-0000-0000-0000-000000000002` in contacts, properties, appointments, publishing DevAuthenticationHandlers

### 5. Publishing-API Pipeline Fix
Moved `app.UseSecurityHeaders()` before `app.UseHealthCheckEndpoints()` in Program.cs so security headers are applied to health check responses. Removed duplicate registration.

### 6. Integration Test Fixes
- `AppointmentsControllerTests`: Added required `PropertyId` for `PropertyViewing` type appointments
- `PropertiesControllerTests`: Removed `?search=` parameter from ListProperties test (ILike is Postgres-specific, not available in InMemory provider)

### 7. CI Infrastructure
- Updated `.third-party-notices-hash` to match current dependency state
- Aligned `generate-third-party-notices.ps1` scope to include all 6 services (was only ai-api and orgs-api)
- Regenerated `package-lock.json` for `@types/node` ~22 sync

## Commands Run

```bash
# Full backend test suite (all 6 services)
dotnet test services/ai-api/Propely.AiApi.sln          # 643 passed (558 unit + 80 integration + 5 arch)
dotnet test services/orgs-api/Propely.OrgsApi.sln       # 935 passed (745 unit + 185 integration + 5 arch)
dotnet test services/contacts-api/Propely.ContactsApi.sln   # 276 passed (260 unit + 11 integration + 5 arch)
dotnet test services/properties-api/Propely.PropertiesApi.sln # 264 passed (251 unit + 8 integration + 5 arch)
dotnet test services/appointments-api/Propely.AppointmentsApi.sln # 294 passed (282 unit + 7 integration + 5 arch)
dotnet test services/publishing-api/Propely.PublishingApi.sln # 108 passed (97 unit + 6 integration + 5 arch)

# Frontend tests
cd apps/web && npx vitest run  # 824 passed (110 test files)

# Total: 3,344 tests, 0 failures
```

## Commits

1. `ad5a091` - fix(auth): emit role claims in JWT tokens and register authorization policies
2. `cd8b0cc` - fix(ci): update third-party notices hash and align script scope with CI
3. `4a97cc5` - fix(test): fix integration test infrastructure for CI environments
