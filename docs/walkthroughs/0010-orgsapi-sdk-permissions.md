# Walkthrough: 0010-orgsapi-sdk-permissions

## Task Reference
- Task: `docs/tasks/0010-orgsapi-sdk-permissions.md`
- Walkthrough: `docs/walkthroughs/0010-orgsapi-sdk-permissions.md`
- Branch/PR: `feat/orgsapi-sdk-permissions` / TBD
- Date: `2026-02-23`

## Summary
Extended the `Propely.OrgsApi.Client` NuGet SDK package with `IPermissionsApi` and `IAgenciesApi` Refit interfaces plus an `IPermissionGuard` helper. This enables other microservices (properties-api, contacts-api, appointments-api, publishing-api) to evaluate permissions and query agency hierarchy via typed HTTP clients with fail-closed semantics -- no direct database access to orgs-api needed.

## Context
- Background: Task 0009 (1.3) established the permission management API endpoints. Task 0004 established the SDK client infrastructure with `IOrgsApiClient`, `TenantDelegatingHandler`, and Polly resilience. The client only exposed user/org endpoints.
- Problem statement: Other services cannot check permissions or query agency hierarchy without direct DB access to orgs-api.
- Constraints: TDD, Clean Architecture, fail-closed semantics, consistent with existing SDK client patterns.

## Decisions & Trade-offs
- **Decision:** Separate Refit interfaces per domain (`IPermissionsApi`, `IAgenciesApi`) rather than adding to `IOrgsApiClient`
  - Options considered: (1) Add all methods to `IOrgsApiClient`, (2) Create separate interfaces per domain area
  - Why this choice: Separate interfaces follow Interface Segregation Principle. Consuming services only need to depend on/inject the interfaces they actually use. Each interface gets its own named resilience pipeline.
  - Consequences / risks: More DI registrations, slightly more setup code. But each client has isolated resilience policies.

- **Decision:** `PermissionGuard` uses `GetEffectivePermissionsAsync` (full list) rather than a single-permission check endpoint
  - Options considered: (1) Add a dedicated `GET /permissions/{userId}/{permission}` endpoint to the API, (2) Use the existing get-all-permissions endpoint
  - Why this choice: The existing API returns all effective permissions in one call. Adding a new endpoint would require modifying the API (out of scope for this task). The overhead of fetching all permissions is negligible (8 permissions max).

- **Decision:** Fail-closed on all exceptions, not just connectivity errors
  - Options considered: (1) Only fail-closed on `HttpRequestException`/`TaskCanceledException`, (2) Fail-closed on any exception
  - Why this choice: Fail-closed is the secure default. Any error (deserialization, unexpected response format, etc.) should deny access rather than risk granting it. The warning log captures the details for debugging.

## Implementation Notes
- Key changes:
  - 8 new DTO records for client-side representation of permission and agency data
  - `IPermissionsApi` Refit interface matching `PermissionsController` routes (`/api/organizations/{orgId}/permissions/...`)
  - `IAgenciesApi` Refit interface matching `AgenciesController` routes (`/api/agencies/...`)
  - `ForbiddenException` thrown by `PermissionGuard` when access is denied
  - `IPermissionGuard` / `PermissionGuard` wrapping `IPermissionsApi` with fail-closed semantics
  - `ServiceCollectionExtensions.AddOrgsApiClient()` refactored to use a shared `RegisterRefitClient<T>()` helper, registering all 3 Refit clients + `IPermissionGuard`
  - Each Refit client gets its own named resilience handler (`orgs-api`, `orgs-api-permissions`, `orgs-api-agencies`)

- Edge cases handled:
  - API unreachable: `HasPermissionAsync` returns false (fail-closed)
  - API timeout: `HasPermissionAsync` returns false (fail-closed)
  - Permission not in returned list: treated as not granted
  - Case-insensitive permission matching
  - Empty permissions list: all permissions denied
  - CancellationToken propagation

- Known limitations:
  - No client-side caching of permission results (consuming services decide their own strategy)
  - `PermissionGuard.HasPermissionAsync` fetches all permissions to check one; acceptable since there are only 8 permissions

## Data / Schema / Migrations
- DB changes: None (SDK client only)
- Migration strategy: N/A
- Backward compatibility: Additive only, no breaking changes

## Commands Run
```bash
# Build
dotnet build services/orgs-api/Propely.OrgsApi.sln

# Run unit tests (663 passed)
dotnet test services/orgs-api/tests/Propely.OrgsApi.UnitTests/Propely.OrgsApi.UnitTests.csproj

# Run integration tests (173 passed)
dotnet test services/orgs-api/tests/Propely.OrgsApi.IntegrationTests/Propely.OrgsApi.IntegrationTests.csproj
```

## Files Changed

### Created
- `services/orgs-api/src/Propely.OrgsApi.Client/Dtos/EffectivePermissionResponse.cs` -- Permission evaluation response DTO
- `services/orgs-api/src/Propely.OrgsApi.Client/Dtos/AgencyResponse.cs` -- Agency list response DTO
- `services/orgs-api/src/Propely.OrgsApi.Client/Dtos/AgencyDetailResponse.cs` -- Agency detail response DTO with branches
- `services/orgs-api/src/Propely.OrgsApi.Client/Dtos/BranchResponse.cs` -- Branch within agency DTO
- `services/orgs-api/src/Propely.OrgsApi.Client/Dtos/CreateAgencyRequest.cs` -- Agency creation request DTO
- `services/orgs-api/src/Propely.OrgsApi.Client/Dtos/CreateAgencyResponse.cs` -- Agency creation response DTO
- `services/orgs-api/src/Propely.OrgsApi.Client/Dtos/AddBranchRequest.cs` -- Add branch request DTO
- `services/orgs-api/src/Propely.OrgsApi.Client/Dtos/SetPermissionOverrideRequest.cs` -- Permission override request DTO
- `services/orgs-api/src/Propely.OrgsApi.Client/Permissions/IPermissionsApi.cs` -- Refit interface for permission endpoints
- `services/orgs-api/src/Propely.OrgsApi.Client/Permissions/IPermissionGuard.cs` -- Simplified permission checking interface
- `services/orgs-api/src/Propely.OrgsApi.Client/Permissions/PermissionGuard.cs` -- Permission guard with fail-closed semantics
- `services/orgs-api/src/Propely.OrgsApi.Client/Permissions/ForbiddenException.cs` -- Exception for denied permissions
- `services/orgs-api/src/Propely.OrgsApi.Client/Agencies/IAgenciesApi.cs` -- Refit interface for agency endpoints
- `services/orgs-api/tests/Propely.OrgsApi.UnitTests/Client/PermissionGuardTests.cs` -- 15 unit tests for PermissionGuard

### Modified
- `services/orgs-api/src/Propely.OrgsApi.Client/ServiceCollectionExtensions.cs` -- Refactored to register IPermissionsApi, IAgenciesApi, IPermissionGuard via shared helper
- `services/orgs-api/tests/Propely.OrgsApi.IntegrationTests/Client/OrgsApiClientIntegrationTests.cs` -- Extended DI registration test to verify new interfaces
- `docs/backlog/epic-P1-agency-permissions.md` -- Updated task 1.3 status to DONE, task 1.4 to IN_PROGRESS

## Tests
### Unit
- `PermissionGuardTests` (15 tests):
  - `HasPermissionAsync`: granted returns true, denied returns false, not in list returns false, API throws returns false (fail-closed), timeout returns false (fail-closed), case-insensitive matching, empty list returns false, cancellation token propagation
  - `RequirePermissionAsync`: granted does not throw, denied throws ForbiddenException, API unreachable throws ForbiddenException (fail-closed), not in list throws ForbiddenException
  - Constructor: null permissionsApi throws, null logger throws
- How to run: `dotnet test services/orgs-api/tests/Propely.OrgsApi.UnitTests --filter "FullyQualifiedName~PermissionGuard"`

### Integration
- `OrgsApiClientIntegrationTests` (4 tests, updated):
  - DI registration resolves `IOrgsApiClient`, `TenantDelegatingHandler`, `IPermissionsApi`, `IAgenciesApi`, `IPermissionGuard`
  - Tenant header propagation (existing)
  - No tenant header when absent (existing)
  - Options defaults (existing)
- How to run: `dotnet test services/orgs-api/tests/Propely.OrgsApi.IntegrationTests --filter "FullyQualifiedName~Client"`

### Manual
- None required

## Observability
- Logs added/updated: `PermissionGuard` logs at Warning level when permission checks fail due to API errors (fail-closed)
- Traces/metrics added/updated: HTTP calls traced via existing OpenTelemetry HttpClient instrumentation

## Security
- Validation: Permission names compared case-insensitively
- AuthN/AuthZ impact: Fail-closed -- unreachable orgs-api denies all permission checks
- Sensitive data handling: No PII logged; only userId, orgId, and permission name in warning logs

## Follow-ups / Backlog
- [ ] Task 1.5: Agency Management UI
- [ ] Task 1.6: Permission Management UI
- [ ] Consider adding client-side caching in consuming services
- [ ] Consider adding a dedicated single-permission-check API endpoint to reduce payload size

## Checklist
- [x] Task scope matches `docs/tasks/0010-orgsapi-sdk-permissions.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
