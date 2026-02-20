# Walkthrough: 0004-nuget-sdk-client-infrastructure

## Task Reference
- Task: `docs/tasks/0004-nuget-sdk-client-infrastructure.md`
- Walkthrough: `docs/walkthroughs/0004-nuget-sdk-client-infrastructure.md`
- Branch/PR: `feat/0004-nuget-sdk-client-infrastructure` / TBD
- Date: `2026-02-20`

## Summary
Created the `Propely.OrgsApi.Client` NuGet SDK client project, establishing the reusable pattern for inter-service communication across Propely's microservices. The client uses Refit for declarative HTTP calls, a `TenantDelegatingHandler` for `X-Tenant-Id` header propagation, and Polly-based resilience policies (retry with exponential backoff + circuit breaker). The pattern is documented in `docs/architecture/sdk-client-pattern.md` for reuse by other service clients.

## Context
- Background: Propely has 6 backend microservices that need to communicate. The orgs-api is consumed by all other services for user validation, membership checks, and permission evaluation.
- Problem statement: No inter-service communication infrastructure existed. Services need a type-safe, resilient way to call each other with automatic tenant context propagation.
- Constraints: Must use Refit (declarative HTTP), Polly (resilience), and follow Clean Architecture patterns.

## Decisions & Trade-offs
- **Decision: DTOs duplicated in Client project (not shared from API)**
  - Options considered: (1) Share DTOs from API project, (2) Duplicate DTOs in Client project
  - Why this choice: Client DTOs are contract types consumed by external services. Sharing from the API project would create a dependency on the full API layer, violating Clean Architecture. Duplicated DTOs maintain clean separation.
  - Consequences / risks: DTOs must be kept in sync manually when API contracts change.

- **Decision: `TenantDelegatingHandler` reads from `IHttpContextAccessor`**
  - Options considered: (1) Read from HttpContext, (2) Use a custom `ITenantAccessor` interface
  - Why this choice: `IHttpContextAccessor` is the standard ASP.NET Core pattern. It works for web request contexts. Background jobs that need explicit tenant IDs can set the header directly on the request (the handler won't overwrite existing headers).
  - Consequences / risks: Won't auto-propagate in background job scenarios, but explicit override is supported.

- **Decision: `TenantIdHeaderName` made `public const`**
  - Options considered: (1) Keep internal, (2) Make public
  - Why this choice: SDK consumers and test code benefit from referencing the constant. It's a stable, well-known header name.

- **Decision: Used `Microsoft.Extensions.Http.Resilience` instead of raw Polly**
  - Options considered: (1) Raw Polly v8 configuration, (2) `Microsoft.Extensions.Http.Resilience`
  - Why this choice: The resilience package is the official Microsoft integration layer for Polly v8 with `IHttpClientFactory`, providing better integration with the DI pipeline and standardized configuration.

## Implementation Notes
- Key changes:
  - Created `Propely.OrgsApi.Client` project with 8 source files
  - Added project to `Propely.OrgsApi.sln` under the `src` solution folder
  - Added project references from both test projects
  - Created 8 unit tests for `TenantDelegatingHandler` (all behaviors covered)
  - Created 4 integration tests for DI registration and end-to-end handler pipeline
  - Created SDK client pattern documentation
- Edge cases handled:
  - No HttpContext available (background jobs): header not added, no error
  - Empty/whitespace tenant ID: treated as absent
  - Header already present on outgoing request: not overwritten (explicit override)
  - Null `IHttpContextAccessor` or `ILogger`: `ArgumentNullException` thrown at construction
- Known limitations:
  - Authentication token propagation not yet implemented (planned for Task 1.5)
  - No contract tests yet (will be added when consumers start using the client)

## Data / Schema / Migrations
- DB changes: None
- Migration strategy: N/A
- Backward compatibility: N/A (new project, no existing consumers)

## Commands Run
```bash
# Create project directory
mkdir -p services/orgs-api/src/Propely.OrgsApi.Client/Dtos

# Add project to solution
dotnet sln services/orgs-api/Propely.OrgsApi.sln add src/Propely.OrgsApi.Client/Propely.OrgsApi.Client.csproj --solution-folder src

# Build
dotnet build services/orgs-api/Propely.OrgsApi.sln

# Test (unit + integration + architecture)
dotnet test services/orgs-api/Propely.OrgsApi.sln
```

## Files Changed
- `services/orgs-api/src/Propely.OrgsApi.Client/Propely.OrgsApi.Client.csproj` -- New project file with Refit and Polly dependencies
- `services/orgs-api/src/Propely.OrgsApi.Client/IOrgsApiClient.cs` -- Refit interface for orgs-api endpoints (GetCurrentUser, GetOrganization, GetMyOrganizations, GetMembers)
- `services/orgs-api/src/Propely.OrgsApi.Client/OrgsApiClientOptions.cs` -- Configuration options (BaseUrl, Timeout, RetryCount, CircuitBreaker settings)
- `services/orgs-api/src/Propely.OrgsApi.Client/TenantDelegatingHandler.cs` -- HTTP handler that propagates X-Tenant-Id from incoming to outgoing requests
- `services/orgs-api/src/Propely.OrgsApi.Client/ServiceCollectionExtensions.cs` -- DI extension method registering Refit client with Polly resilience
- `services/orgs-api/src/Propely.OrgsApi.Client/Dtos/UserResponse.cs` -- User profile DTO
- `services/orgs-api/src/Propely.OrgsApi.Client/Dtos/OrgResponse.cs` -- Organization DTO
- `services/orgs-api/src/Propely.OrgsApi.Client/Dtos/MemberResponse.cs` -- Member DTO
- `services/orgs-api/src/Propely.OrgsApi.Client/Dtos/PagedResult.cs` -- Paginated result DTO
- `services/orgs-api/Propely.OrgsApi.sln` -- Added Client project reference
- `services/orgs-api/tests/Propely.OrgsApi.UnitTests/Propely.OrgsApi.UnitTests.csproj` -- Added Client project reference
- `services/orgs-api/tests/Propely.OrgsApi.IntegrationTests/Propely.OrgsApi.IntegrationTests.csproj` -- Added Client project reference
- `services/orgs-api/tests/Propely.OrgsApi.UnitTests/Client/TenantDelegatingHandlerTests.cs` -- 8 unit tests for handler behavior
- `services/orgs-api/tests/Propely.OrgsApi.IntegrationTests/Client/OrgsApiClientIntegrationTests.cs` -- 4 integration tests for DI and pipeline
- `docs/architecture/sdk-client-pattern.md` -- Pattern documentation for reuse
- `docs/tasks/0004-nuget-sdk-client-infrastructure.md` -- Task specification
- `docs/walkthroughs/0004-nuget-sdk-client-infrastructure.md` -- This walkthrough
- `docs/backlog/epic-P0-foundation.md` -- Updated task status

## Tests
### Unit
- 8 tests in `TenantDelegatingHandlerTests`:
  - Propagates tenant ID from incoming to outgoing request
  - Does not add header when no tenant ID in incoming request
  - Does not add header when no HttpContext available
  - Does not overwrite explicitly set header
  - Treats empty/whitespace tenant IDs as absent
  - Throws ArgumentNullException for null httpContextAccessor
  - Throws ArgumentNullException for null logger
- How to run: `dotnet test services/orgs-api/tests/Propely.OrgsApi.UnitTests --filter "FullyQualifiedName~TenantDelegatingHandler"`

### Integration
- 4 tests in `OrgsApiClientIntegrationTests`:
  - DI registration resolves IOrgsApiClient and TenantDelegatingHandler
  - Handler propagates tenant ID through full DI pipeline
  - Handler does not add header when no tenant ID present
  - OrgsApiClientOptions has correct default values
- How to run: `dotnet test services/orgs-api/tests/Propely.OrgsApi.IntegrationTests --filter "FullyQualifiedName~OrgsApiClientIntegration"`

### Manual
- N/A

## Observability
- Logs added/updated: TenantDelegatingHandler logs at Debug level when propagating or skipping tenant header
- Traces/metrics added/updated: HTTP client tracing via IHttpClientFactory + OpenTelemetry (built-in)

## Security
- Validation: TenantDelegatingHandler reads from IHttpContextAccessor (async-local context), preventing cross-request tenant leakage
- AuthN/AuthZ impact: None (authentication token propagation planned for Task 1.5)
- Sensitive data handling: No sensitive data logged; HTTP payload logging only at Debug level

## Follow-ups / Backlog
- [ ] Add authentication token propagation (Task 1.5)
- [ ] Create SDK clients for other services (ai-api, properties-api, contacts-api) following this pattern
- [ ] Add contract tests when consumers start using the client
- [ ] Consider shared `TenantDelegatingHandler` base class or NuGet package if pattern proves stable across multiple clients

## Checklist
- [x] Task scope matches `docs/tasks/0004-nuget-sdk-client-infrastructure.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
