# Epic P0: Foundation

## Overview

Prepare the repository for Propely development by renaming from SaasTemplate, creating service scaffolding, NuGet SDK client infrastructure, and updating CI/infrastructure. This epic covers all foundational work required before any domain-specific feature development can begin.

**Target:** Establish the Propely monorepo with 6 backend services (orgs-api, ai-api, properties-api, publishing-api, contacts-api, appointments-api), a renamed codebase, and a fully green CI pipeline.

---

## Tasks

---

### Task 0.1 --- Documentation & Roadmap Overhaul

- **Status:** DONE
- **Dependencies:** None
- **File:** `docs/tasks/0001-documentation-roadmap-overhaul.md`

#### Goal

Rewrite all documentation, agent instructions, roadmap, and backlog to reflect the Propely product. Remove all SaaS Starter Kit template-specific artifacts and replace them with Propely-specific content.

#### Scope

**In scope:**
- Rewrite `.agent.md` governance file for Propely workflows
- Rewrite `CLAUDE.md` with Propely-specific commands, architecture, and conventions
- Rewrite `AGENTS.md` and `GEMINI.md` for Propely context
- Rewrite `.github/copilot-instructions.md` for Propely
- Create new roadmap document (`docs/roadmap.md`) covering Propely phases P0-P4
- Create backlog epic files for each phase (P0 through P4)
- Clean up template-specific docs: delete `docs/screenshots/`, demo-recording guide, old `roadmap-v1.md`, and any template-era task/walkthrough/audit files that are no longer relevant
- Update architecture docs (`docs/architecture/`) to describe Propely's 6-service topology
- Update configuration docs for new service prefixes and environment variables
- Update getting-started guide for Propely onboarding
- Create Claude Code Skills (`.claude/commands/`) for workflow commands (`next task`, `code review`, `fast track`, `pr`, etc.)
- Rewrite `README.md` as the Propely project README

**Out of scope:**
- Actual code changes (renaming namespaces is Task 0.2)
- New service scaffolding (Task 0.3)
- Infrastructure changes (Task 0.5)

#### Acceptance Criteria

- [ ] **AC1:** `.agent.md` references Propely, not SaaS Starter Kit or SaasTemplate
- [ ] **AC2:** `CLAUDE.md` accurately describes Propely's architecture (6 services, port allocations, database names) and all commands work
- [ ] **AC3:** `AGENTS.md`, `GEMINI.md`, and `.github/copilot-instructions.md` are updated for Propely
- [ ] **AC4:** `docs/roadmap.md` exists with phases P0 through P4 defined
- [ ] **AC5:** Backlog epic files exist for each phase under `docs/backlog/`
- [ ] **AC6:** All template-specific documentation artifacts are removed (screenshots dir, demo-recording guide, roadmap-v1, irrelevant template tasks)
- [ ] **AC7:** `docs/architecture/` documents describe the 6-service Propely topology
- [ ] **AC8:** Getting-started guide references Propely and correct commands
- [ ] **AC9:** Claude Code Skills exist in `.claude/commands/` for at least: `next-task.md`, `code-review.md`, `fast-track.md`, `pr.md`
- [ ] **AC10:** `README.md` is rewritten for Propely with project description, setup instructions, and architecture overview

#### Implementation Steps

1. Audit all existing documentation files and categorize as keep/rewrite/delete
2. Delete template-specific artifacts: `docs/screenshots/`, demo-recording guide, `roadmap-v1.md`
3. Archive or delete template-era task files (`docs/tasks/0001-*.md` through `docs/tasks/0074-*.md`) and audit task files
4. Archive or delete template-era walkthrough files
5. Write new `docs/roadmap.md` with P0-P4 phases
6. Create backlog epics: `epic-P0-foundation.md`, `epic-P1-core-domain.md`, `epic-P2-integration.md`, `epic-P3-frontend.md`, `epic-P4-launch.md`
7. Rewrite `.agent.md` for Propely governance
8. Rewrite `CLAUDE.md` with updated architecture, commands, conventions, and port allocation
9. Rewrite `AGENTS.md` and `GEMINI.md`
10. Rewrite `.github/copilot-instructions.md`
11. Update `docs/architecture/` documents
12. Update getting-started guide
13. Create `.claude/commands/` skill files
14. Rewrite `README.md`
15. Verify all internal doc links resolve correctly

#### Files to Create/Modify

- **Create:** `docs/roadmap.md`, `docs/backlog/epic-P0-foundation.md`, `docs/backlog/epic-P1-core-domain.md`, `docs/backlog/epic-P2-integration.md`, `docs/backlog/epic-P3-frontend.md`, `docs/backlog/epic-P4-launch.md`
- **Create:** `.claude/commands/next-task.md`, `.claude/commands/code-review.md`, `.claude/commands/fast-track.md`, `.claude/commands/pr.md`
- **Modify:** `.agent.md`, `CLAUDE.md`, `AGENTS.md`, `GEMINI.md`, `.github/copilot-instructions.md`, `README.md`
- **Modify:** `docs/architecture/*.md`, `docs/getting-started.md` (or equivalent)
- **Delete:** `docs/screenshots/`, demo-recording guide, `roadmap-v1.md`, template-era tasks/walkthroughs/audits

#### Testing Plan

- Verify all markdown links resolve (no broken internal references)
- Verify Claude Code Skills can be invoked via `/` commands
- Verify `CLAUDE.md` commands table matches actual script names
- Read through each doc to confirm zero references to "SaaS Starter Kit" or "SaasTemplate"

---

### Task 0.2 --- Rename SaasTemplate to Propely

- **Status:** DONE
- **Dependencies:** Task 0.1 (documentation should reference new names)
- **File:** `docs/tasks/0002-rename-saastemplate-to-propely.md`

#### Goal

Perform a comprehensive rename of all SaasTemplate references throughout the entire codebase to Propely. This includes .NET namespaces, solution files, project files, Docker artifacts, database names, scripts, environment variables, and frontend brand configuration.

#### Scope

**In scope:**
- All .NET namespace renames: `SaasTemplate.*` to `Propely.*`
- Solution file renames: `SaasTemplate.AiApi.sln` to `Propely.AiApi.sln`, `SaasTemplate.OrgsApi.sln` to `Propely.OrgsApi.sln`
- All `.csproj` file renames and internal references
- All `using` statements and `namespace` declarations in `.cs` files
- Docker Compose service names and image references (`saastemplate-postgres` to `propely-postgres`, etc.)
- Database names in `infra/postgres/init/00-init-databases.sql`: `saastemplate_aiapi` to `propely_aiapi`, `saastemplate_orgsapi` to `propely_orgsapi`
- `.env.example` prefix updates (`AIAPI_*` remains but default values change)
- All scripts: `run-ai-api.ps1/.sh`, `run-orgs-api.ps1/.sh`, `dev-up.*`, `dev-down.*`, `dev-reset.*`
- CI workflow references in `.github/workflows/`
- `apps/web/src/` brand configuration (`brand.ts` or equivalent)
- EF Core migration snapshots and designer files (namespace references)
- Test project namespaces and references
- `scaffold-module.ps1` scripts in each service

**Out of scope:**
- Adding new services (Task 0.3)
- Changing architecture or functionality
- Changing port allocations

#### Acceptance Criteria

- [ ] **AC1:** `grep -ri "SaasTemplate" --include="*.cs" --include="*.csproj" --include="*.sln" --include="*.json" --include="*.ts" --include="*.tsx" --include="*.yml" --include="*.yaml" --include="*.ps1" --include="*.sh" --include="*.sql" --include="*.md" .` returns zero results (excluding git history and node_modules)
- [ ] **AC2:** `dotnet build services/ai-api/Propely.AiApi.sln` succeeds with zero errors
- [ ] **AC3:** `dotnet build services/orgs-api/Propely.OrgsApi.sln` succeeds with zero errors
- [ ] **AC4:** `dotnet test services/ai-api/Propely.AiApi.sln` passes all tests
- [ ] **AC5:** `dotnet test services/orgs-api/Propely.OrgsApi.sln` passes all tests
- [ ] **AC6:** `cd apps/web && npm run build` succeeds
- [ ] **AC7:** `docker compose up -d` starts all infrastructure with new container names (`propely-postgres`, `propely-rabbitmq`, etc.)
- [ ] **AC8:** Database init script creates `propely_aiapi` and `propely_orgsapi` databases
- [ ] **AC9:** All run scripts (`run-ai-api.ps1`, `run-orgs-api.ps1`, etc.) reference correct solution/project paths
- [ ] **AC10:** `grep -ri "saastemplate" --include="*.cs" --include="*.csproj" --include="*.sln" --include="*.json" --include="*.ts" --include="*.tsx" --include="*.yml" --include="*.yaml" --include="*.ps1" --include="*.sh" --include="*.sql" .` returns zero results (case-insensitive, excluding git history and node_modules)

#### Implementation Steps

1. **Inventory phase:** Use `grep -ri "saastemplate"` across the repo to catalog every occurrence, organized by file type
2. **Rename solution files:**
   - `services/ai-api/SaasTemplate.AiApi.sln` to `services/ai-api/Propely.AiApi.sln`
   - `services/orgs-api/SaasTemplate.OrgsApi.sln` to `services/orgs-api/Propely.OrgsApi.sln`
3. **Rename .csproj files:** Rename every `SaasTemplate.*.csproj` to `Propely.*.csproj` and update all `<ProjectReference>` paths inside `.sln` and `.csproj` files
4. **Rename directories:** Rename `src/SaasTemplate.AiApi.Domain/` to `src/Propely.AiApi.Domain/`, etc. for all 4 layers in both services, plus test projects
5. **Update namespaces:** Find and replace all `namespace SaasTemplate.` with `namespace Propely.` and all `using SaasTemplate.` with `using Propely.` in `.cs` files
6. **Update EF Core artifacts:** Update migration snapshot namespaces, designer file namespaces, DbContext references
7. **Update Docker Compose:** Replace all `saastemplate-` container names with `propely-`, update image names, environment variable defaults
8. **Update database init SQL:** `infra/postgres/init/00-init-databases.sql` --- rename databases and roles
9. **Update `.env.example`:** Change default values from `saastemplate` to `propely` (e.g., `POSTGRES_USER=propely`)
10. **Update scripts:** All PowerShell and Bash scripts in `scripts/` --- update solution paths, project paths, container references
11. **Update `scaffold-module.ps1`:** Update namespace templates in both services
12. **Update CI workflows:** `.github/workflows/ci.yml`, `deploy.yml`, `publish.yml` --- update solution paths and service references
13. **Update frontend brand:** `apps/web/src/` brand.ts or config --- update product name
14. **Update CLAUDE.md:** Ensure all references use `Propely.*` names (coordinate with Task 0.1)
15. **Full verification:** Run all builds, all tests, and docker compose up

#### Files to Create/Modify

- **Rename:** All `SaasTemplate.*.sln`, `SaasTemplate.*.csproj` files and their parent directories
- **Modify:** Every `.cs` file with SaasTemplate namespace/using statements (estimated 200+ files)
- **Modify:** `docker-compose.yml`, `infra/postgres/init/00-init-databases.sql`
- **Modify:** `.env.example`, all scripts in `scripts/`
- **Modify:** `.github/workflows/ci.yml`, `deploy.yml`, `publish.yml`, and other workflow files
- **Modify:** `apps/web/` brand configuration files

#### Testing Plan

1. `dotnet build` both solutions --- zero errors
2. `dotnet test` both solutions --- all tests pass
3. `npm run build` in `apps/web` --- succeeds
4. `docker compose up -d` --- all containers start with new names
5. `docker compose --profile apps up -d` --- all app containers start
6. Full-text search for "saastemplate" (case-insensitive) returns zero hits in source files
7. Run each `run-*.ps1` script and verify it finds the correct project

#### Security & Privacy Considerations

- Ensure `.env.example` does not contain real secrets (only placeholder/default dev values)
- Verify no credentials are accidentally committed during the rename process
- Database rename requires destroying and recreating dev volumes (`dev-reset` script)

---

### Task 0.3 --- Scaffold New Service Solutions

- **Status:** DONE
- **Dependencies:** Task 0.2 (namespace must be Propely.* before scaffolding)
- **File:** `docs/tasks/0003-scaffold-new-services.md`

#### Goal

Create four new .NET 10 microservice solutions following the same Clean Architecture + CQRS pattern used by orgs-api and ai-api. Each service will have a minimal but functional skeleton with health endpoints, OpenTelemetry, and architecture tests.

#### Services to Create

| Service | Directory | Port | Database | Purpose |
|---------|-----------|------|----------|---------|
| Properties API | `services/properties-api/` | 5030 | `propely_propertiesapi` | Property listings CRUD, search, media |
| Publishing API | `services/publishing-api/` | 5040 | `propely_publishingapi` | Portal syndication, listing publication |
| Contacts API | `services/contacts-api/` | 5050 | `propely_contactsapi` | Contact/lead management, inquiries |
| Appointments API | `services/appointments-api/` | 5060 | `propely_appointmentsapi` | Viewing scheduling, calendar integration |

#### Scope

**In scope:**
- For each service, create the full Clean Architecture directory structure
- Minimal `Program.cs` with: Kestrel config, health checks, OpenTelemetry, Swagger, EF Core with PostgreSQL, MediatR, FluentValidation
- Health endpoint (`/health`) returning service status
- Architecture tests (enforcing layer dependency rules)
- Solution file (`.sln`) referencing all projects
- `Directory.Build.props` and `Directory.Packages.props` for centralized package management
- Dockerfile for each service
- `scaffold-module.ps1` script copied and adapted from existing services

**Out of scope:**
- Domain entities or business logic (that is Phase P1)
- API endpoints beyond health check
- Database migrations beyond initial empty migration
- Integration with other services

#### Acceptance Criteria

- [ ] **AC1:** `services/properties-api/Propely.PropertiesApi.sln` exists and `dotnet build` succeeds
- [ ] **AC2:** `services/publishing-api/Propely.PublishingApi.sln` exists and `dotnet build` succeeds
- [ ] **AC3:** `services/contacts-api/Propely.ContactsApi.sln` exists and `dotnet build` succeeds
- [ ] **AC4:** `services/appointments-api/Propely.AppointmentsApi.sln` exists and `dotnet build` succeeds
- [ ] **AC5:** Each service has all 4 Clean Architecture layers: Domain, Application, Infrastructure, Api
- [ ] **AC6:** Each service has UnitTests and IntegrationTests projects
- [ ] **AC7:** `dotnet test` passes for all 4 new solutions (architecture tests green)
- [ ] **AC8:** Each service starts on its designated port and responds to `GET /health` with 200
- [ ] **AC9:** Each service has a Dockerfile that builds successfully
- [ ] **AC10:** Each service has a `scaffold-module.ps1` script that generates module boilerplate

#### Implementation Steps

1. Study the existing `services/orgs-api/` structure as the canonical reference for Clean Architecture layout
2. For each new service (properties-api, publishing-api, contacts-api, appointments-api):
   a. Create the solution directory: `services/<service-name>/`
   b. Create `src/Propely.<ServiceName>.Domain/` with:
      - `Propely.<ServiceName>.Domain.csproj` (no external dependencies)
      - `Common/` folder with base entity classes
   c. Create `src/Propely.<ServiceName>.Application/` with:
      - `Propely.<ServiceName>.Application.csproj` referencing Domain
      - MediatR, FluentValidation package references
      - `DependencyInjection.cs` for service registration
   d. Create `src/Propely.<ServiceName>.Infrastructure/` with:
      - `Propely.<ServiceName>.Infrastructure.csproj` referencing Application
      - EF Core DbContext (empty, no entities yet)
      - `DependencyInjection.cs` for infrastructure registration
      - Persistence configuration
   e. Create `src/Propely.<ServiceName>.Api/` with:
      - `Propely.<ServiceName>.Api.csproj` referencing Infrastructure
      - `Program.cs` with full startup configuration
      - `appsettings.json` and `appsettings.Development.json`
      - Health check endpoint
      - Swagger/OpenAPI configuration
      - OpenTelemetry setup (traces, metrics, logs)
      - CORS configuration
   f. Create `tests/Propely.<ServiceName>.UnitTests/` with:
      - Architecture tests enforcing layer dependency rules
   g. Create `tests/Propely.<ServiceName>.IntegrationTests/` with:
      - Health endpoint integration test
   h. Create `Propely.<ServiceName>.sln` referencing all projects
   i. Create `Dockerfile`
   j. Copy and adapt `scripts/scaffold-module.ps1`
3. Create `Directory.Build.props` and `Directory.Packages.props` in each service root (or share from repo root)
4. Verify all 4 solutions build and test successfully

#### Files to Create

For each of the 4 services, approximately 15-20 files:
- `services/<name>/Propely.<Name>.sln`
- `services/<name>/Directory.Build.props`
- `services/<name>/Directory.Packages.props`
- `services/<name>/src/Propely.<Name>.Domain/Propely.<Name>.Domain.csproj`
- `services/<name>/src/Propely.<Name>.Domain/Common/BaseEntity.cs`
- `services/<name>/src/Propely.<Name>.Application/Propely.<Name>.Application.csproj`
- `services/<name>/src/Propely.<Name>.Application/DependencyInjection.cs`
- `services/<name>/src/Propely.<Name>.Infrastructure/Propely.<Name>.Infrastructure.csproj`
- `services/<name>/src/Propely.<Name>.Infrastructure/DependencyInjection.cs`
- `services/<name>/src/Propely.<Name>.Infrastructure/Persistence/<Name>DbContext.cs`
- `services/<name>/src/Propely.<Name>.Api/Propely.<Name>.Api.csproj`
- `services/<name>/src/Propely.<Name>.Api/Program.cs`
- `services/<name>/src/Propely.<Name>.Api/appsettings.json`
- `services/<name>/src/Propely.<Name>.Api/appsettings.Development.json`
- `services/<name>/tests/Propely.<Name>.UnitTests/Propely.<Name>.UnitTests.csproj`
- `services/<name>/tests/Propely.<Name>.UnitTests/ArchitectureTests.cs`
- `services/<name>/tests/Propely.<Name>.IntegrationTests/Propely.<Name>.IntegrationTests.csproj`
- `services/<name>/tests/Propely.<Name>.IntegrationTests/HealthEndpointTests.cs`
- `services/<name>/Dockerfile`
- `services/<name>/scripts/scaffold-module.ps1`

#### Testing Plan

1. `dotnet build` each solution individually --- zero errors
2. `dotnet test` each solution individually --- all tests pass
3. Start each service individually and verify `GET /health` returns 200
4. Verify architecture tests correctly reject cross-layer dependency violations
5. Run `scaffold-module.ps1` in one service to verify it generates correct boilerplate

---

### Task 0.4 --- NuGet SDK Client Infrastructure

- **Status:** PENDING
- **Dependencies:** Task 0.2 (namespaces must be Propely.*)
- **File:** `docs/tasks/0004-nuget-sdk-client-infrastructure.md`

#### Goal

Create a reusable SDK client pattern for inter-service communication, starting with `Propely.OrgsApi.Client`. This client allows other services to call orgs-api endpoints in a type-safe manner with automatic tenant header propagation, retry policies, and circuit-breaker resilience.

#### Scope

**In scope:**
- Create `Propely.OrgsApi.Client` project inside `services/orgs-api/src/`
- Refit-based typed HTTP client interface matching existing orgs-api endpoints
- `TenantDelegatingHandler` that automatically adds `X-Tenant-Id` header to outgoing requests
- Polly v8 retry and circuit-breaker policies for transient fault handling
- `IServiceCollection.AddOrgsApiClient(Action<OrgsApiClientOptions> configure)` extension method
- `OrgsApiClientOptions` configuration class (base URL, timeout, retry count, circuit breaker threshold)
- Integration test verifying tenant header propagation
- Documentation of the SDK client pattern so other services can follow it for their own clients

**Out of scope:**
- Creating SDK clients for other services (will follow this pattern later)
- Modifying the orgs-api itself
- Authentication token propagation (will be added in a later task)

#### Acceptance Criteria

- [ ] **AC1:** `services/orgs-api/src/Propely.OrgsApi.Client/Propely.OrgsApi.Client.csproj` exists and builds successfully
- [ ] **AC2:** Refit interface `IOrgsApiClient` exposes typed methods for existing orgs-api endpoints (at minimum: get organization, list organizations, get user profile)
- [ ] **AC3:** `TenantDelegatingHandler` adds `X-Tenant-Id` header from `IHttpContextAccessor` or explicit configuration
- [ ] **AC4:** Polly retry policy retries on transient HTTP errors (5xx, 408, 429) with exponential backoff (max 3 retries)
- [ ] **AC5:** Polly circuit-breaker opens after 5 consecutive failures, half-open after 30 seconds
- [ ] **AC6:** `services.AddOrgsApiClient(options => { options.BaseUrl = "..."; })` registers all necessary services in DI
- [ ] **AC7:** Integration test proves `X-Tenant-Id` header arrives at the server when using the client
- [ ] **AC8:** `dotnet build services/orgs-api/Propely.OrgsApi.sln` still succeeds with the new project added
- [ ] **AC9:** `dotnet test services/orgs-api/Propely.OrgsApi.sln` passes all tests including new client tests
- [ ] **AC10:** Pattern is documented in `docs/architecture/sdk-client-pattern.md`

#### Implementation Steps

1. Create `services/orgs-api/src/Propely.OrgsApi.Client/` project directory
2. Create `Propely.OrgsApi.Client.csproj` with dependencies: `Refit.HttpClientFactory`, `Microsoft.Extensions.Http.Polly`, `Microsoft.Extensions.Http.Resilience`
3. Define `IOrgsApiClient` Refit interface:
   ```csharp
   public interface IOrgsApiClient
   {
       [Get("/api/organizations/{id}")]
       Task<OrganizationDto> GetOrganizationAsync(Guid id, CancellationToken ct = default);

       [Get("/api/organizations")]
       Task<List<OrganizationDto>> ListOrganizationsAsync(CancellationToken ct = default);
       // ... additional endpoints
   }
   ```
4. Create DTO classes matching orgs-api response contracts
5. Create `TenantDelegatingHandler : DelegatingHandler`:
   - Reads tenant ID from `IHttpContextAccessor` (for web-request contexts)
   - Falls back to explicitly configured tenant ID (for background jobs)
   - Adds `X-Tenant-Id` header to outgoing requests
6. Create `OrgsApiClientOptions` class:
   ```csharp
   public class OrgsApiClientOptions
   {
       public string BaseUrl { get; set; } = "http://localhost:5020";
       public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
       public int RetryCount { get; set; } = 3;
       public int CircuitBreakerThreshold { get; set; } = 5;
       public TimeSpan CircuitBreakerDuration { get; set; } = TimeSpan.FromSeconds(30);
   }
   ```
7. Create `ServiceCollectionExtensions.cs` with `AddOrgsApiClient()`:
   - Registers `IOrgsApiClient` via Refit
   - Adds `TenantDelegatingHandler` to the HTTP client pipeline
   - Configures Polly retry with exponential backoff
   - Configures Polly circuit-breaker
   - Configures timeout policy
8. Add the new project to `Propely.OrgsApi.sln`
9. Write unit tests for `TenantDelegatingHandler`
10. Write integration test (using `WebApplicationFactory`) that verifies tenant header propagation end-to-end
11. Document the pattern in `docs/architecture/sdk-client-pattern.md`

#### Files to Create/Modify

- **Create:** `services/orgs-api/src/Propely.OrgsApi.Client/Propely.OrgsApi.Client.csproj`
- **Create:** `services/orgs-api/src/Propely.OrgsApi.Client/IOrgsApiClient.cs`
- **Create:** `services/orgs-api/src/Propely.OrgsApi.Client/OrgsApiClientOptions.cs`
- **Create:** `services/orgs-api/src/Propely.OrgsApi.Client/TenantDelegatingHandler.cs`
- **Create:** `services/orgs-api/src/Propely.OrgsApi.Client/ServiceCollectionExtensions.cs`
- **Create:** `services/orgs-api/src/Propely.OrgsApi.Client/Dtos/` (DTO classes)
- **Create:** `docs/architecture/sdk-client-pattern.md`
- **Modify:** `services/orgs-api/Propely.OrgsApi.sln` (add new project)
- **Modify:** Test projects to add client tests

#### Testing Plan

1. Unit test `TenantDelegatingHandler` in isolation using a mock inner handler
2. Unit test retry policy behavior with simulated transient failures
3. Integration test: start orgs-api via `WebApplicationFactory`, call through `IOrgsApiClient`, verify `X-Tenant-Id` header arrives in the controller
4. `dotnet build` the full solution --- zero errors
5. `dotnet test` the full solution --- all tests pass

#### Security & Privacy Considerations

- `TenantDelegatingHandler` must never leak tenant IDs across request boundaries; ensure it reads from the correct async-local context
- SDK client must not log sensitive request/response bodies at Information level; use Debug level for payload logging
- Circuit-breaker state must be per-service-instance (not shared across tenants) to prevent one tenant's failures from affecting others

---

### Task 0.5 --- Docker Compose & Infrastructure Updates

- **Status:** PENDING
- **Dependencies:** Task 0.2 (rename), Task 0.3 (new services exist)
- **File:** `docs/tasks/0005-docker-compose-infrastructure-updates.md`

#### Goal

Update all Docker Compose configuration and infrastructure scripts to support the full 6-service Propely topology. Ensure `docker compose --profile apps up -d` starts all services, and each service has corresponding run scripts for host-based development.

#### Scope

**In scope:**
- Add properties-api, publishing-api, contacts-api, and appointments-api services to `docker-compose.yml` (under the `apps` profile)
- Update `infra/postgres/init/00-init-databases.sql` to create 6 databases: `propely_aiapi`, `propely_orgsapi`, `propely_propertiesapi`, `propely_publishingapi`, `propely_contactsapi`, `propely_appointmentsapi`
- Create run scripts for each new service:
  - `scripts/run-properties-api.ps1` / `scripts/run-properties-api.sh`
  - `scripts/run-publishing-api.ps1` / `scripts/run-publishing-api.sh`
  - `scripts/run-contacts-api.ps1` / `scripts/run-contacts-api.sh`
  - `scripts/run-appointments-api.ps1` / `scripts/run-appointments-api.sh`
- Update `.env.example` with configuration variables for all new services
- Update `dev-up.ps1/.sh` and `dev-down.ps1/.sh` if they reference specific services
- Update port allocation documentation in `CLAUDE.md`

**Out of scope:**
- Kubernetes manifests or production deployment config
- Service mesh or API gateway configuration
- Database migrations for new services (empty databases are sufficient)

#### Acceptance Criteria

- [ ] **AC1:** `docker compose config` validates successfully with all 6 app services defined
- [ ] **AC2:** `docker compose up -d` starts all infrastructure services (postgres, rabbitmq, redis, aspire, mailhog) with `propely-` container name prefix
- [ ] **AC3:** `docker compose --profile apps up -d` additionally starts all 6 app services
- [ ] **AC4:** PostgreSQL init script creates all 6 databases on fresh startup
- [ ] **AC5:** `scripts/run-properties-api.ps1` starts properties-api on port 5030
- [ ] **AC6:** `scripts/run-publishing-api.ps1` starts publishing-api on port 5040
- [ ] **AC7:** `scripts/run-contacts-api.ps1` starts contacts-api on port 5050
- [ ] **AC8:** `scripts/run-appointments-api.ps1` starts appointments-api on port 5060
- [ ] **AC9:** `.env.example` contains variables for all 6 services with correct prefixes
- [ ] **AC10:** `dev-up.ps1 -Apps` starts the full stack successfully

#### Implementation Steps

1. Update `docker-compose.yml`:
   a. Add `properties-api` service definition (port 5030, depends on postgres/rabbitmq/redis, apps profile)
   b. Add `publishing-api` service definition (port 5040, same dependencies, apps profile)
   c. Add `contacts-api` service definition (port 5050, same dependencies, apps profile)
   d. Add `appointments-api` service definition (port 5060, same dependencies, apps profile)
   e. Ensure all services use `propely-` container name prefix
   f. Add appropriate health checks for each new service
   g. Set memory limits consistent with existing services
2. Update `infra/postgres/init/00-init-databases.sql`:
   ```sql
   CREATE DATABASE propely_propertiesapi;
   CREATE DATABASE propely_publishingapi;
   CREATE DATABASE propely_contactsapi;
   CREATE DATABASE propely_appointmentsapi;
   ```
3. Create run scripts (follow pattern from existing `run-ai-api.ps1/.sh`):
   - Each script: loads `.env`, sets service-specific env vars, runs `dotnet run` on the Api project
   - PowerShell version for Windows, Bash version for Linux/macOS
4. Update `.env.example`:
   ```
   # Properties API
   PROPERTIESAPI_ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=propely_propertiesapi;Username=propely;Password=propely_dev_password
   PROPERTIESAPI_RabbitMQ__Host=localhost
   # ... (repeat pattern for publishing, contacts, appointments)
   ```
5. Update `dev-up.ps1/.sh` if it has hardcoded service lists
6. Update `dev-down.ps1/.sh` if it references specific containers
7. Update port allocation table in `CLAUDE.md`
8. Run `dev-reset` followed by `dev-up --apps` to verify full stack

#### Files to Create/Modify

- **Modify:** `docker-compose.yml`
- **Modify:** `infra/postgres/init/00-init-databases.sql`
- **Create:** `scripts/run-properties-api.ps1`, `scripts/run-properties-api.sh`
- **Create:** `scripts/run-publishing-api.ps1`, `scripts/run-publishing-api.sh`
- **Create:** `scripts/run-contacts-api.ps1`, `scripts/run-contacts-api.sh`
- **Create:** `scripts/run-appointments-api.ps1`, `scripts/run-appointments-api.sh`
- **Modify:** `.env.example`
- **Modify:** `scripts/dev-up.ps1`, `scripts/dev-up.sh` (if they reference specific services)
- **Modify:** `scripts/dev-down.ps1`, `scripts/dev-down.sh` (if applicable)
- **Modify:** `CLAUDE.md` (port allocation table)

#### Testing Plan

1. `docker compose config` --- validates without errors
2. `docker compose up -d` --- infrastructure containers start cleanly
3. Connect to PostgreSQL and verify all 6 databases exist: `psql -U propely -c "\l"`
4. `docker compose --profile apps up -d` --- all 6 app containers start and become healthy
5. `curl http://localhost:5030/health` --- 200 OK (properties-api)
6. `curl http://localhost:5040/health` --- 200 OK (publishing-api)
7. `curl http://localhost:5050/health` --- 200 OK (contacts-api)
8. `curl http://localhost:5060/health` --- 200 OK (appointments-api)
9. Run each `run-*.ps1` script individually and verify it starts on the correct port
10. `dev-reset` followed by `dev-up --apps` --- clean start works

#### Security & Privacy Considerations

- Ensure all new service ports bind to `127.0.0.1` only (not `0.0.0.0`) in Docker Compose for local development
- Verify `.env.example` contains only placeholder passwords, not real credentials
- Ensure database init script grants minimum required privileges per service database

---

### Task 0.6 --- CI Pipeline Updates

- **Status:** PENDING
- **Dependencies:** Task 0.2 (rename), Task 0.3 (new services), Task 0.5 (Docker/infra)
- **File:** `docs/tasks/0006-ci-pipeline-updates.md`

#### Goal

Update the CI/CD pipeline to build, test, and validate all 6 backend services plus the frontend. Use a matrix strategy to avoid duplicating workflow steps for each service.

#### Scope

**In scope:**
- Update `.github/workflows/ci.yml` to use a matrix strategy for building/testing all 6 .NET services
- Update `.github/workflows/deploy.yml` for new service deployment targets
- Update `.github/workflows/publish.yml` for Docker image publishing of all 6 services
- Create or update Dockerfiles for new services if needed
- Ensure CI validates: build, unit tests, integration tests, architecture tests, frontend build, linting
- Update any workflow references to old `SaasTemplate.*` solution names

**Out of scope:**
- Production deployment configuration (Terraform, Cloud Run --- separate epic)
- Setting up staging environments
- Performance testing in CI

#### Acceptance Criteria

- [ ] **AC1:** `ci.yml` uses a matrix strategy with entries for all 6 services: `[ai-api, orgs-api, properties-api, publishing-api, contacts-api, appointments-api]`
- [ ] **AC2:** CI matrix correctly resolves solution file paths: `services/${{ matrix.service }}/Propely.*.sln`
- [ ] **AC3:** CI runs `dotnet build` for each service in the matrix
- [ ] **AC4:** CI runs `dotnet test` for each service in the matrix
- [ ] **AC5:** CI builds the Next.js frontend (`npm run build` in `apps/web/`)
- [ ] **AC6:** CI runs frontend linting (`npm run lint` in `apps/web/`)
- [ ] **AC7:** `deploy.yml` references all 6 services for deployment
- [ ] **AC8:** `publish.yml` builds Docker images for all 6 services
- [ ] **AC9:** All workflows pass on a clean PR branch (green CI)
- [ ] **AC10:** Zero references to `SaasTemplate` remain in any workflow file

#### Implementation Steps

1. Read current `ci.yml` to understand existing structure
2. Refactor `ci.yml` to use matrix strategy:
   ```yaml
   strategy:
     matrix:
       service:
         - { name: ai-api, solution: Propely.AiApi }
         - { name: orgs-api, solution: Propely.OrgsApi }
         - { name: properties-api, solution: Propely.PropertiesApi }
         - { name: publishing-api, solution: Propely.PublishingApi }
         - { name: contacts-api, solution: Propely.ContactsApi }
         - { name: appointments-api, solution: Propely.AppointmentsApi }
   ```
3. Update build step: `dotnet build services/${{ matrix.service.name }}/${{ matrix.service.solution }}.sln`
4. Update test step: `dotnet test services/${{ matrix.service.name }}/${{ matrix.service.solution }}.sln`
5. Keep frontend build/lint as a separate job (not part of the matrix)
6. Update `deploy.yml`:
   - Add deployment steps/jobs for properties-api, publishing-api, contacts-api, appointments-api
   - Use matrix or parameterized approach for consistency
7. Update `publish.yml`:
   - Add Docker build/push for all 6 services
   - Ensure Dockerfile paths are correct for new services
8. Update any other workflow files (`infra-ci.yml`, `release.yml`, `rollback.yml`) that reference solution names
9. Search all workflow files for `SaasTemplate` and replace with `Propely`
10. Push to a test branch and verify CI passes

#### Files to Create/Modify

- **Modify:** `.github/workflows/ci.yml`
- **Modify:** `.github/workflows/deploy.yml`
- **Modify:** `.github/workflows/publish.yml`
- **Modify:** `.github/workflows/release.yml` (if it references services)
- **Modify:** `.github/workflows/rollback.yml` (if it references services)
- **Modify:** `.github/workflows/infra-ci.yml` (if it references services)
- **Verify:** Dockerfiles exist for all 6 services (create if missing from Task 0.3)

#### Testing Plan

1. Push changes to a feature branch and verify CI workflow triggers
2. All 6 service builds succeed in the matrix
3. All 6 service test runs succeed in the matrix
4. Frontend build and lint pass
5. Docker image builds succeed for all 6 services in `publish.yml`
6. No workflow step references `SaasTemplate` in logs or configuration
7. Matrix failures are isolated (one service failure does not block others from running)

#### Security & Privacy Considerations

- Ensure CI workflows do not expose secrets in logs (use `${{ secrets.* }}` properly)
- Verify Docker image publishing only occurs on protected branches (main/release)
- Ensure `GITHUB_TOKEN` permissions are scoped appropriately for the expanded matrix
- Verify no new services accidentally bypass security scanning steps (Trivy, etc.)

---

## Summary

| Task | Description | Status | Dependencies |
|------|-------------|--------|--------------|
| 0.1 | Documentation & Roadmap Overhaul | DONE | None |
| 0.2 | Rename SaasTemplate to Propely | DONE | 0.1 |
| 0.3 | Scaffold New Service Solutions | DONE | 0.2 |
| 0.4 | NuGet SDK Client Infrastructure | PENDING | 0.2 |
| 0.5 | Docker Compose & Infrastructure Updates | PENDING | 0.2, 0.3 |
| 0.6 | CI Pipeline Updates | PENDING | 0.2, 0.3, 0.5 |

## Port Allocation (Phase 0 Final State)

| Service | Port |
|---------|------|
| web | 3000 |
| ai-api | 5010 |
| orgs-api | 5020 |
| properties-api | 5030 |
| publishing-api | 5040 |
| contacts-api | 5050 |
| appointments-api | 5060 |
| PostgreSQL | 5432 |
| RabbitMQ | 5672 / 15672 |
| Redis | 6379 |
| Aspire Dashboard | 18888 / 4317 / 4318 |
| Mailhog | 10025 / 18025 |

## Completion Criteria

Phase 0 is complete when:
1. All documentation references Propely, not SaasTemplate
2. All .NET namespaces, solutions, and projects use `Propely.*` naming
3. All 6 backend services build, test, and start successfully
4. `docker compose --profile apps up -d` starts the full stack
5. CI pipeline is green for all services
6. SDK client pattern is documented and demonstrated with orgs-api client
