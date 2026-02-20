# Task: 0003-scaffold-new-services

## Metadata
- ID: 0003
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-20
- Related docs:
  - Walkthrough: `docs/walkthroughs/0003-scaffold-new-services.md`

## Goal
Create four new .NET 10 microservice solutions (properties-api, publishing-api, contacts-api, appointments-api) following the same Clean Architecture + CQRS pattern used by orgs-api and ai-api. Each service will have a minimal but functional skeleton with health endpoints, OpenTelemetry, and architecture tests.

## Context
Tasks 0.1 (Documentation) and 0.2 (Rename) are complete. The codebase now uses `Propely.*` naming throughout. The existing services (orgs-api, ai-api) provide a proven Clean Architecture template that new services should follow. The new services are tenant-scoped (following ai-api's pattern with `ITenantAccessor` and `OrgContextMiddleware`).

## Scope
### In scope
- 4 new service solutions: properties-api (5030), publishing-api (5040), contacts-api (5050), appointments-api (5060)
- Full Clean Architecture 4-layer structure per service (Domain, Application, Infrastructure, Api)
- 3 test projects per service (UnitTests, IntegrationTests, ArchitectureTests)
- Minimal `Program.cs` with health checks, OpenTelemetry, Swagger, EF Core, MediatR, FluentValidation
- Tenant-scoped infrastructure (ITenantAccessor, OrgContextMiddleware, query filters)
- Outbox pattern infrastructure (RabbitMQ, OutboxDispatcher)
- Redis caching infrastructure
- Architecture tests enforcing layer dependency rules
- Health endpoint integration test
- Dockerfiles (production + development)
- scaffold-module.ps1 script per service

### Out of scope
- Domain entities or business logic (Phase 1+)
- API endpoints beyond health check
- Database migrations beyond initial empty schema
- Integration with other services
- Docker Compose updates (Task 0.5)
- CI pipeline updates (Task 0.6)

## Requirements
- R1: Each service follows the ai-api tenant-scoped pattern (not orgs-api auth-provider pattern)
- R2: Each service has unique port, database name, env var prefix, migration lock ID, RabbitMQ exchange
- R3: Architecture tests must enforce Clean Architecture layer dependencies
- R4: All services must build and pass tests successfully

## Acceptance Criteria
- AC1: `services/properties-api/Propely.PropertiesApi.sln` exists and `dotnet build` succeeds
- AC2: `services/publishing-api/Propely.PublishingApi.sln` exists and `dotnet build` succeeds
- AC3: `services/contacts-api/Propely.ContactsApi.sln` exists and `dotnet build` succeeds
- AC4: `services/appointments-api/Propely.AppointmentsApi.sln` exists and `dotnet build` succeeds
- AC5: Each service has all 4 Clean Architecture layers: Domain, Application, Infrastructure, Api
- AC6: Each service has UnitTests, IntegrationTests, and ArchitectureTests projects
- AC7: `dotnet test` passes for all 4 new solutions (architecture tests green)
- AC8: Each service has a Dockerfile that builds successfully
- AC9: Each service has a `scaffold-module.ps1` script
- AC10: No references to `SaasTemplate` in any new file

## Constraints (non-negotiable)
- Clean Architecture layers respected.
- English-only repo content.
- No secrets in repo.
- Update walkthrough.

## Proposed Approach (high-level)
1. Create properties-api as the reference implementation with all layers, middleware, configuration, and tests
2. Clone to the other 3 services with namespace/config replacements
3. Generate .sln files using dotnet CLI
4. Build and test all 4 services

## Implementation Steps
1. Create properties-api with full Clean Architecture skeleton (Domain, Application, Infrastructure, Api layers)
2. Clone to publishing-api, contacts-api, appointments-api with appropriate replacements
3. Generate .sln files and add project references
4. Build all 4 solutions
5. Run all tests

## Files to Create / Modify
- **Create:** ~62 files per service (4 x .csproj, Program.cs, middleware, configuration, domain common, app common, infra persistence/messaging/caching, tests, Dockerfiles, scripts)
- **Modify:** `docs/backlog/epic-P0-foundation.md` (status update)

## Testing Plan
- Unit tests: Architecture tests verifying layer dependency rules (5 tests per service)
- Integration tests: Health endpoint integration test (1 test per service)
- Manual verification: `dotnet build` and `dotnet test` for each solution

## Security & Privacy
- No secrets in configuration files (empty connection strings, dev-only defaults)
- DevAuthenticationHandler only active in Development/Testing environments
- Security:AllowAnonymous fails if set in production

## Observability
- Logs: Standard ASP.NET Core logging with correlation ID middleware
- Metrics: OpenTelemetry metrics (ASP.NET Core + HTTP client instrumentation)
- Traces: OpenTelemetry tracing with OTLP exporter support

## Rollback Plan
Delete the 4 new service directories. No existing code is modified.

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
