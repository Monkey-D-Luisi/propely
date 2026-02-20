# Task: 0004-nuget-sdk-client-infrastructure

## Metadata
- ID: 0004
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-20
- Related docs:
  - Walkthrough: `docs/walkthroughs/0004-nuget-sdk-client-infrastructure.md`

## Goal
Create a reusable NuGet SDK client pattern for inter-service communication, starting with `Propely.OrgsApi.Client`. This client allows other services to call orgs-api endpoints in a type-safe manner with automatic tenant header propagation, retry policies, and circuit-breaker resilience.

## Context
The Propely platform consists of 6 backend microservices that need to communicate with each other. Currently there is no inter-service communication infrastructure. The orgs-api is the first service that other services need to call (for user validation, membership checks, permission evaluation). This task establishes the SDK client pattern that all future inter-service clients will follow.

## Scope
### In scope
- Create `Propely.OrgsApi.Client` project inside `services/orgs-api/src/`
- Refit-based typed HTTP client interface matching existing orgs-api endpoints
- `TenantDelegatingHandler` that automatically adds `X-Tenant-Id` header to outgoing requests
- Polly v8 retry and circuit-breaker policies for transient fault handling
- `IServiceCollection.AddOrgsApiClient(Action<OrgsApiClientOptions> configure)` extension method
- `OrgsApiClientOptions` configuration class (base URL, timeout, retry count, circuit breaker threshold)
- Unit tests for `TenantDelegatingHandler`
- Integration test verifying tenant header propagation
- Documentation of the SDK client pattern

### Out of scope
- Creating SDK clients for other services (will follow this pattern later)
- Modifying the orgs-api itself
- Authentication token propagation (will be added in a later task)

## Requirements
- R1: The SDK client must use Refit for declarative HTTP client generation
- R2: The `TenantDelegatingHandler` must read tenant ID from `IHttpContextAccessor` and add `X-Tenant-Id` header
- R3: Polly retry policy must retry on transient HTTP errors (5xx, 408, 429) with exponential backoff (max 3 retries)
- R4: Polly circuit-breaker must open after 5 consecutive failures, half-open after 30 seconds
- R5: The DI extension must register all necessary services in a single call
- R6: The pattern must be documented for reuse by other service clients

## Acceptance Criteria
- AC1: `services/orgs-api/src/Propely.OrgsApi.Client/Propely.OrgsApi.Client.csproj` exists and builds successfully
- AC2: Refit interface `IOrgsApiClient` exposes typed methods for existing orgs-api endpoints (at minimum: get organization, list organizations, get user profile)
- AC3: `TenantDelegatingHandler` adds `X-Tenant-Id` header from `IHttpContextAccessor` or explicit configuration
- AC4: Polly retry policy retries on transient HTTP errors (5xx, 408, 429) with exponential backoff (max 3 retries)
- AC5: Polly circuit-breaker opens after 5 consecutive failures, half-open after 30 seconds
- AC6: `services.AddOrgsApiClient(options => { options.BaseUrl = "..."; })` registers all necessary services in DI
- AC7: Integration test proves `X-Tenant-Id` header arrives at the server when using the client
- AC8: `dotnet build services/orgs-api/Propely.OrgsApi.sln` still succeeds with the new project added
- AC9: `dotnet test services/orgs-api/Propely.OrgsApi.sln` passes all tests including new client tests
- AC10: Pattern is documented in `docs/architecture/sdk-client-pattern.md`

## Constraints (non-negotiable)
- Clean Architecture layers respected.
- English-only repo content.
- No secrets in repo.
- Update walkthrough.

## Proposed Approach (high-level)
1. Create Client project with Refit interfaces mirroring key orgs-api controller endpoints
2. Implement TenantDelegatingHandler as a DelegatingHandler that reads from HttpContext
3. Configure Polly resilience policies (retry + circuit breaker) via Microsoft.Extensions.Http.Resilience
4. Provide a single DI extension method for consumer registration
5. Write TDD unit tests for the handler, then integration tests with WebApplicationFactory

## Implementation Steps
1. Create `services/orgs-api/src/Propely.OrgsApi.Client/` project directory
2. Create `Propely.OrgsApi.Client.csproj` with Refit.HttpClientFactory, Microsoft.Extensions.Http.Resilience
3. Define `IOrgsApiClient` Refit interface matching key orgs-api endpoints
4. Create DTO classes matching orgs-api response contracts
5. Create `TenantDelegatingHandler` that reads from `IHttpContextAccessor`
6. Create `OrgsApiClientOptions` configuration class
7. Create `ServiceCollectionExtensions.cs` with `AddOrgsApiClient()`
8. Add the new project to `Propely.OrgsApi.sln`
9. Write unit tests for `TenantDelegatingHandler` (TDD: red first)
10. Write integration test verifying tenant header propagation
11. Document the pattern in `docs/architecture/sdk-client-pattern.md`

## Files to Create / Modify
- `services/orgs-api/src/Propely.OrgsApi.Client/Propely.OrgsApi.Client.csproj`
- `services/orgs-api/src/Propely.OrgsApi.Client/IOrgsApiClient.cs`
- `services/orgs-api/src/Propely.OrgsApi.Client/OrgsApiClientOptions.cs`
- `services/orgs-api/src/Propely.OrgsApi.Client/TenantDelegatingHandler.cs`
- `services/orgs-api/src/Propely.OrgsApi.Client/ServiceCollectionExtensions.cs`
- `services/orgs-api/src/Propely.OrgsApi.Client/Dtos/*.cs`
- `services/orgs-api/Propely.OrgsApi.sln` (add new project)
- `services/orgs-api/tests/Propely.OrgsApi.UnitTests/` (add handler tests)
- `services/orgs-api/tests/Propely.OrgsApi.IntegrationTests/` (add client tests)
- `docs/architecture/sdk-client-pattern.md`

## Testing Plan
- Unit tests: TenantDelegatingHandler in isolation using mock inner handler
- Integration tests: Start orgs-api via WebApplicationFactory, call through IOrgsApiClient, verify X-Tenant-Id header arrives
- Manual verification: None required

## Security & Privacy
- TenantDelegatingHandler must never leak tenant IDs across request boundaries; ensure it reads from the correct async-local context
- SDK client must not log sensitive request/response bodies at Information level; use Debug level for payload logging
- Circuit-breaker state must be per-service-instance (not shared across tenants) to prevent one tenant's failures from affecting others

## Observability
- Logs: HTTP client logging at Debug level via IHttpClientFactory defaults
- Metrics: Polly circuit-breaker state changes logged at Warning level
- Traces: OpenTelemetry HTTP client instrumentation (built-in via IHttpClientFactory)

## Rollback Plan
Remove the Client project from the solution, delete the project directory. No other services depend on it yet.

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
