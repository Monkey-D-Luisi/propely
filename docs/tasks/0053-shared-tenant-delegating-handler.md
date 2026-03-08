# Task: 0053-shared-tenant-delegating-handler

## Metadata
- ID: 0053
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-03-08
- Related docs:
  - Walkthrough: `docs/walkthroughs/0053-shared-tenant-delegating-handler.md`
  - Epic: `docs/backlog/epic-P8-ai-provider-abstraction.md` (Task 8.5)

## Goal
Deduplicate the `TenantDelegatingHandler` class by extracting it into a shared `Propely.Shared.Http` project, eliminating 5 identical copies across SDK client projects.

## Context
All 5 NuGet SDK clients (ai-api, orgs-api, properties-api, contacts-api, appointments-api) contain functionally identical copies of `TenantDelegatingHandler`. This handler propagates the `X-Org-Id` tenant header from incoming HTTP requests to outgoing inter-service calls. The duplication violates DRY and creates maintenance risk — a fix in one copy must be replicated to all 5.

## Scope
### In scope
- Create `services/shared/Propely.Shared.Http/` project with the canonical `TenantDelegatingHandler`
- Update all 5 SDK client `.csproj` files to reference the shared project
- Update all 5 `ServiceCollectionExtensions.cs` files to use the shared namespace
- Delete the 5 duplicate `TenantDelegatingHandler.cs` files
- Verify all solutions build and all tests pass

### Out of scope
- Changing `TenantDelegatingHandler` behavior
- Moving other shared code (resilience config, PagedResult)
- Publishing as a real NuGet package (use ProjectReference in monorepo)
- Docker compose or Dockerfile changes (handler is consumed at build time only)

## Requirements
- R1: Single source of truth for `TenantDelegatingHandler` in `Propely.Shared.Http`
- R2: All 5 SDK clients reference the shared project via `ProjectReference`
- R3: No duplicate `TenantDelegatingHandler` files remain
- R4: All existing tests pass unchanged (behavioral equivalence)

## Acceptance Criteria
- AC1: Single `TenantDelegatingHandler` exists in `services/shared/Propely.Shared.Http/`
- AC2: All 5 SDK clients reference the shared package via ProjectReference
- AC3: No duplicate `TenantDelegatingHandler` files remain in any Client project
- AC4: All 6 service solutions build successfully
- AC5: All 6 service test suites pass
- AC6: Web app builds successfully

## Constraints (non-negotiable)
- Clean Architecture layers respected.
- English-only repo content.
- No secrets in repo.
- Update walkthrough.

## Proposed Approach (high-level)
1. Create the shared project with the canonical handler implementation
2. Add ProjectReference from each SDK client to the shared project
3. Update using statements in ServiceCollectionExtensions.cs files
4. Delete the 5 duplicate files
5. Build and test all solutions

## Implementation Steps
1. Create `services/shared/Propely.Shared.Http/Propely.Shared.Http.csproj` targeting net10.0
2. Create `services/shared/Propely.Shared.Http/TenantDelegatingHandler.cs` with `Propely.Shared.Http` namespace
3. Add `ProjectReference` to the shared project in all 5 SDK client `.csproj` files
4. Update `using` namespace in all 5 `ServiceCollectionExtensions.cs` files
5. Delete 5 duplicate `TenantDelegatingHandler.cs` files
6. Build all 6 service solutions
7. Run all 6 test suites

## Files to Create / Modify
### Create
- `services/shared/Propely.Shared.Http/Propely.Shared.Http.csproj`
- `services/shared/Propely.Shared.Http/TenantDelegatingHandler.cs`

### Modify
- `services/ai-api/src/Propely.AiApi.Client/Propely.AiApi.Client.csproj`
- `services/orgs-api/src/Propely.OrgsApi.Client/Propely.OrgsApi.Client.csproj`
- `services/properties-api/src/Propely.PropertiesApi.Client/Propely.PropertiesApi.Client.csproj`
- `services/contacts-api/src/Propely.ContactsApi.Client/Propely.ContactsApi.Client.csproj`
- `services/appointments-api/src/Propely.AppointmentsApi.Client/Propely.AppointmentsApi.Client.csproj`
- `services/ai-api/src/Propely.AiApi.Client/Configuration/ServiceCollectionExtensions.cs` (add using)
- `services/orgs-api/src/Propely.OrgsApi.Client/ServiceCollectionExtensions.cs` (add using)
- `services/properties-api/src/Propely.PropertiesApi.Client/ServiceCollectionExtensions.cs` (add using)
- `services/contacts-api/src/Propely.ContactsApi.Client/ServiceCollectionExtensions.cs` (add using)
- `services/appointments-api/src/Propely.AppointmentsApi.Client/ServiceCollectionExtensions.cs` (add using)

### Delete
- `services/ai-api/src/Propely.AiApi.Client/TenantDelegatingHandler.cs`
- `services/orgs-api/src/Propely.OrgsApi.Client/TenantDelegatingHandler.cs`
- `services/properties-api/src/Propely.PropertiesApi.Client/TenantDelegatingHandler.cs`
- `services/contacts-api/src/Propely.ContactsApi.Client/TenantDelegatingHandler.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Client/TenantDelegatingHandler.cs`

## Testing Plan
- Unit tests: Existing handler and SDK tests verify behavioral equivalence
- Integration tests: All 6 service integration test suites pass
- Manual verification: None required

## Security & Privacy
- No behavior change — same handler logic, just deduplicated
- No new attack surface

## Observability
- Logs: No change (same handler, same log statements)
- Metrics: N/A
- Traces: N/A

## Rollback Plan
Revert the commit — restore individual handler files and remove shared project reference.

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
