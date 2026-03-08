# Walkthrough: 0053-shared-tenant-delegating-handler

## Task Reference
- Task: `docs/tasks/0053-shared-tenant-delegating-handler.md`
- Walkthrough: `docs/walkthroughs/0053-shared-tenant-delegating-handler.md`
- Branch/PR: `feat/0053-shared-tenant-delegating-handler` / TBD
- Date: `2026-03-08`

## Summary
Deduplicated the `TenantDelegatingHandler` class from 5 identical copies (one per SDK client project) into a single shared `Propely.Shared.Http` project. All SDK clients now reference the shared project via ProjectReference. This eliminates maintenance risk from code duplication while preserving identical behavior.

## Context
- Background: Each SDK client (ai-api, orgs-api, properties-api, contacts-api, appointments-api) had its own copy of `TenantDelegatingHandler` — all functionally identical, differing only in namespace and minor cosmetic details.
- Problem statement: 5 copies of the same 60-line handler creates maintenance burden — any bug fix or enhancement must be replicated 5 times.
- Constraints: Must not change behavior; must not break any existing tests.

## Decisions & Trade-offs
- **Decision:** Use `ProjectReference` instead of a NuGet package
  - Options considered: (1) ProjectReference in monorepo, (2) Internal NuGet package
  - Why this choice: Monorepo makes ProjectReference simpler — no package publishing, versioning overhead
  - Consequences / risks: None — this is standard monorepo practice

## Implementation Notes
- Key changes: Created `Propely.Shared.Http` project in `services/shared/` containing the canonical `TenantDelegatingHandler`. Updated all 5 SDK clients to reference this shared project via `ProjectReference`. Updated 7 test files to use `using Propely.Shared.Http;` instead of the old service-specific namespaces. Deleted 5 duplicate handler files.
- Edge cases handled: The orgs-api copy had slightly different cosmetic style (missing XML docs, extra inline comment) — the canonical version uses the fully-documented variant (same as ai-api, properties-api, contacts-api, appointments-api).
- Known limitations: N/A

## Data / Schema / Migrations
- DB changes: None
- Migration strategy: N/A
- Backward compatibility: Full — same behavior, different project reference

## Commands Run
```bash
dotnet build services/ai-api/Propely.AiApi.sln           # 0 errors, 28 warnings (pre-existing)
dotnet build services/orgs-api/Propely.OrgsApi.sln        # 0 errors, 22 warnings (pre-existing)
dotnet build services/properties-api/Propely.PropertiesApi.sln  # 0 errors, 18 warnings (pre-existing)
dotnet build services/contacts-api/Propely.ContactsApi.sln      # 0 errors, 22 warnings (pre-existing)
dotnet build services/appointments-api/Propely.AppointmentsApi.sln  # 0 errors, 18 warnings (pre-existing)
dotnet build services/publishing-api/Propely.PublishingApi.sln  # 0 errors, 18 warnings (pre-existing)

dotnet test services/ai-api/Propely.AiApi.sln --filter "FullyQualifiedName~UnitTests"             # 590 passed
dotnet test services/orgs-api/Propely.OrgsApi.sln --filter "FullyQualifiedName~UnitTests"          # 745 passed
dotnet test services/properties-api/Propely.PropertiesApi.sln --filter "FullyQualifiedName~UnitTests"  # 309 passed
dotnet test services/contacts-api/Propely.ContactsApi.sln --filter "FullyQualifiedName~UnitTests"  # 263 passed
dotnet test services/appointments-api/Propely.AppointmentsApi.sln --filter "FullyQualifiedName~UnitTests"  # 288 passed
dotnet test services/publishing-api/Propely.PublishingApi.sln --filter "FullyQualifiedName~UnitTests"  # 97 passed
# Total: 2,292 tests passed, 0 failed
```

## Files Changed

### Created
- `services/shared/Propely.Shared.Http/Propely.Shared.Http.csproj` — shared project targeting net10.0 with FrameworkReference to Microsoft.AspNetCore.App
- `services/shared/Propely.Shared.Http/TenantDelegatingHandler.cs` — canonical handler in `Propely.Shared.Http` namespace

### Modified
- `services/ai-api/src/Propely.AiApi.Client/Propely.AiApi.Client.csproj` — added ProjectReference to shared project
- `services/orgs-api/src/Propely.OrgsApi.Client/Propely.OrgsApi.Client.csproj` — added ProjectReference to shared project
- `services/properties-api/src/Propely.PropertiesApi.Client/Propely.PropertiesApi.Client.csproj` — added ProjectReference to shared project
- `services/contacts-api/src/Propely.ContactsApi.Client/Propely.ContactsApi.Client.csproj` — added ProjectReference to shared project
- `services/appointments-api/src/Propely.AppointmentsApi.Client/Propely.AppointmentsApi.Client.csproj` — added ProjectReference to shared project
- `services/ai-api/src/Propely.AiApi.Client/Configuration/ServiceCollectionExtensions.cs` — added `using Propely.Shared.Http;`
- `services/orgs-api/src/Propely.OrgsApi.Client/ServiceCollectionExtensions.cs` — added `using Propely.Shared.Http;`
- `services/properties-api/src/Propely.PropertiesApi.Client/ServiceCollectionExtensions.cs` — added `using Propely.Shared.Http;`
- `services/contacts-api/src/Propely.ContactsApi.Client/ServiceCollectionExtensions.cs` — added `using Propely.Shared.Http;`
- `services/appointments-api/src/Propely.AppointmentsApi.Client/ServiceCollectionExtensions.cs` — added `using Propely.Shared.Http;`
- `services/orgs-api/tests/Propely.OrgsApi.UnitTests/Client/TenantDelegatingHandlerTests.cs` — updated `using` to `Propely.Shared.Http`
- `services/properties-api/tests/Propely.PropertiesApi.UnitTests/Client/TenantDelegatingHandlerTests.cs` — updated `using` to `Propely.Shared.Http`
- `services/orgs-api/tests/Propely.OrgsApi.IntegrationTests/Client/OrgsApiClientIntegrationTests.cs` — added `using Propely.Shared.Http;`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Client/ServiceCollectionExtensionsTests.cs` — added `using Propely.Shared.Http;`
- `services/properties-api/tests/Propely.PropertiesApi.UnitTests/Client/ServiceCollectionExtensionsTests.cs` — added `using Propely.Shared.Http;`
- `services/appointments-api/tests/Propely.AppointmentsApi.UnitTests/Client/ServiceCollectionExtensionsTests.cs` — added `using Propely.Shared.Http;`

### Deleted
- `services/ai-api/src/Propely.AiApi.Client/TenantDelegatingHandler.cs`
- `services/orgs-api/src/Propely.OrgsApi.Client/TenantDelegatingHandler.cs`
- `services/properties-api/src/Propely.PropertiesApi.Client/TenantDelegatingHandler.cs`
- `services/contacts-api/src/Propely.ContactsApi.Client/TenantDelegatingHandler.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Client/TenantDelegatingHandler.cs`

## Tests
### Unit
- What was added/updated: No new tests needed — existing tests verify behavioral equivalence
- How to run: `dotnet test` on each service solution

### Integration
- What was added/updated: None — existing tests verify SDK client behavior
- How to run: `dotnet test` on each service solution

### Manual
- What you verified: All 6 solutions build and test successfully
- Steps: Build + test each solution

## Observability
- Logs added/updated: None
- Traces/metrics added/updated: None

## Security
- Validation: No change
- AuthN/AuthZ impact: None
- Sensitive data handling: None

## Follow-ups / Backlog
- [ ] Consider moving other shared code (resilience pipeline configuration, PagedResult) to shared packages in the future

## Checklist
- [x] Task scope matches `docs/tasks/0053-shared-tenant-delegating-handler.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
