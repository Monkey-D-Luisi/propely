# Task: 0010-orgsapi-sdk-permissions

## Metadata
- ID: 0010
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-23
- Related docs:
  - Walkthrough: `docs/walkthroughs/0010-orgsapi-sdk-permissions.md`
  - Epic: `docs/backlog/epic-P1-agency-permissions.md` (Task 1.4)

## Goal
Extend the `Propely.OrgsApi.Client` NuGet SDK package with Refit interfaces for permission evaluation and agency hierarchy queries. Other microservices (properties-api, contacts-api, appointments-api, publishing-api) will use this client to enforce authorization without direct database access to orgs-api.

## Context
Task 1.3 (0009) established the permission management API with endpoints for querying effective permissions and managing overrides. Task 0.4 (0004) established the SDK client infrastructure with `IOrgsApiClient`, `TenantDelegatingHandler`, and Polly resilience. Currently the client only exposes user/org endpoints (`/auth/me`, `/orgs/{orgId}`, `/orgs/mine`, `/orgs/{orgId}/members`). No permission-related or agency-related endpoints are accessible through the SDK.

## Scope
### In scope
- `IPermissionsApi` Refit interface for permission evaluation endpoints
- `IAgenciesApi` Refit interface for agency hierarchy endpoints
- Permission-related DTOs in the client package
- Agency-related DTOs in the client package
- `IPermissionGuard` helper interface + `PermissionGuard` implementation for simplified permission checks
- Registration of new interfaces via extended `AddOrgsApiClient()` method
- Polly fallback policy: fail-closed (deny all) when orgs-api is unreachable
- Unit tests for `PermissionGuard`
- Contract tests verifying SDK behavior matches API behavior (deferred)

### Out of scope
- Modifying orgs-api endpoints (already done in task 1.3)
- Caching in consuming services (each service decides its own strategy)
- Authentication token management (handled by existing DelegatingHandler)

## Requirements
- R1: `IPermissionsApi` must expose typed methods for all 3 permission endpoints (`GET`, `PUT`, `DELETE`)
- R2: `IAgenciesApi` must expose typed methods for agency create, list, get and branch management
- R3: `IPermissionGuard.RequirePermissionAsync` must throw `ForbiddenException` on denial
- R4: Fail-closed: if orgs-api is unreachable, all permission checks must deny access
- R5: `AddOrgsApiClient()` must register all new interfaces in DI

## Acceptance Criteria
- AC1: `IPermissionsApi` Refit interface has methods for `GET /api/organizations/{orgId}/permissions/{userId}`, `PUT /api/organizations/{orgId}/permissions/{userId}/{permission}`, `DELETE /api/organizations/{orgId}/permissions/{userId}/{permission}`
- AC2: `IAgenciesApi` Refit interface has methods for `POST /api/agencies`, `GET /api/agencies`, `GET /api/agencies/{id}`, `POST /api/agencies/{id}/branches`, `DELETE /api/agencies/{id}/branches/{branchId}`
- AC3: `IPermissionGuard.RequirePermissionAsync(userId, orgId, permission)` throws `ForbiddenException` when permission is denied
- AC4: `IPermissionGuard.HasPermissionAsync(userId, orgId, permission)` returns `false` without throwing when denied
- AC5: `services.AddOrgsApiClient(options => ...)` registers `IPermissionsApi`, `IAgenciesApi`, and `IPermissionGuard`
- AC6: Polly resilience applies to permission and agency clients (retry on 5xx/408/429, circuit breaker)
- AC7: Fail-closed: permission checks deny when orgs-api is unreachable
- AC8: `Propely.OrgsApi.Client` package builds without warnings
- AC9: Contract tests deferred to a future task (no `WebApplicationFactory`-based contract tests in this PR)
- AC10: `dotnet test` passes all new and existing tests

## Constraints (non-negotiable)
- Clean Architecture layers respected.
- English-only repo content.
- No secrets in repo.
- Update walkthrough.
- TDD: write tests first, then implementation.

## Proposed Approach (high-level)
1. Create `IPermissionsApi` and `IAgenciesApi` Refit interfaces with DTOs
2. Create `IPermissionGuard` / `PermissionGuard` with fail-closed semantics
3. Extend `ServiceCollectionExtensions.AddOrgsApiClient()` to register new interfaces
4. Write unit tests for `PermissionGuard`
5. Write contract tests for SDK-to-API integration

## Implementation Steps
1. Create permission DTOs: `EffectivePermissionResponse`, `SetPermissionOverrideRequest`
2. Create agency DTOs: `AgencyResponse`, `AgencyDetailResponse`, `BranchResponse`, `CreateAgencyRequest`, `CreateAgencyResponse`, `AddBranchRequest`
3. Create `IPermissionsApi` Refit interface matching PermissionsController routes
4. Create `IAgenciesApi` Refit interface matching AgenciesController routes
5. Create `ForbiddenException` in the client package
6. Create `IPermissionGuard` interface
7. Create `PermissionGuard` implementation (wraps `IPermissionsApi`, fail-closed on errors)
8. Update `ServiceCollectionExtensions.AddOrgsApiClient()` to register `IPermissionsApi`, `IAgenciesApi`, `IPermissionGuard`
9. Write unit tests for `PermissionGuard` (throws on denial, passes on grant, fail-closed on error)
10. Write DI registration test
11. Write contract tests using `WebApplicationFactory` (deferred to future task)

## Files to Create / Modify
### Create
- `services/orgs-api/src/Propely.OrgsApi.Client/Permissions/IPermissionsApi.cs`
- `services/orgs-api/src/Propely.OrgsApi.Client/Permissions/IPermissionGuard.cs`
- `services/orgs-api/src/Propely.OrgsApi.Client/Permissions/PermissionGuard.cs`
- `services/orgs-api/src/Propely.OrgsApi.Client/Permissions/ForbiddenException.cs`
- `services/orgs-api/src/Propely.OrgsApi.Client/Dtos/EffectivePermissionResponse.cs`
- `services/orgs-api/src/Propely.OrgsApi.Client/Dtos/SetPermissionOverrideRequest.cs`
- `services/orgs-api/src/Propely.OrgsApi.Client/Agencies/IAgenciesApi.cs`
- `services/orgs-api/src/Propely.OrgsApi.Client/Dtos/AgencyResponse.cs`
- `services/orgs-api/src/Propely.OrgsApi.Client/Dtos/AgencyDetailResponse.cs`
- `services/orgs-api/src/Propely.OrgsApi.Client/Dtos/BranchResponse.cs`
- `services/orgs-api/src/Propely.OrgsApi.Client/Dtos/CreateAgencyRequest.cs`
- `services/orgs-api/src/Propely.OrgsApi.Client/Dtos/CreateAgencyResponse.cs`
- `services/orgs-api/src/Propely.OrgsApi.Client/Dtos/AddBranchRequest.cs`
- `services/orgs-api/tests/Propely.OrgsApi.UnitTests/Client/PermissionGuardTests.cs`

### Modify
- `services/orgs-api/src/Propely.OrgsApi.Client/ServiceCollectionExtensions.cs` (register new interfaces)

## Testing Plan
- Unit tests:
  - `PermissionGuard.RequirePermissionAsync` throws `ForbiddenException` when API returns permission denied
  - `PermissionGuard.HasPermissionAsync` returns `false` without throwing when denied
  - `PermissionGuard.HasPermissionAsync` returns `false` when `IPermissionsApi` throws (fail-closed)
  - `PermissionGuard.HasPermissionAsync` returns `true` when permission granted
  - DI registration resolves `IPermissionsApi`, `IAgenciesApi`, `IPermissionGuard`
- Integration/contract tests:
  - Owner has all permissions via SDK
  - Agent without override lacks `PropertiesViewAll` via SDK
  - Agent with grant override has `PropertiesViewAll` via SDK
  - Agency create, list, get via SDK matches API behavior
- Manual verification: None required

## Security & Privacy
- Fail-closed: unreachable orgs-api denies all permission checks rather than granting
- SDK propagates `X-Tenant-Id` header via `TenantDelegatingHandler`
- Permission check responses do not leak internal error details
- `IPermissionGuard` does not cache results; consuming services decide caching strategy
- Log permission check failures at Warning level without logging full request body

## Observability
- Logs: Permission guard denials logged at Warning level
- Metrics: None (defer)
- Traces: HTTP calls traced via OpenTelemetry HttpClient instrumentation

## Rollback Plan
Remove new files from the Client project. Revert `ServiceCollectionExtensions.cs` changes. No database or API changes involved.

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
