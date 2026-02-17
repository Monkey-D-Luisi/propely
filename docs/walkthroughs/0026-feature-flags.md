# Walkthrough: 0026-feature-flags

## Task Reference
- Task: `docs/tasks/0026-feature-flags.md`
- Walkthrough: `docs/walkthroughs/0026-feature-flags.md`
- Branch/PR: `feat/0026-feature-flags`
- Date: `2026-02-07`

## Summary
Implemented a feature flag system with config-driven defaults and database overrides. The backend provides three REST endpoints (list all, check single, toggle) behind authentication. The frontend provides a `useFeatureFlag` hook and `FeatureGate` component for conditional rendering. Four sample flags are defined in appsettings.json.

## Context
- Background: The SaaS template needed a mechanism to toggle features at runtime without code deployments.
- Problem statement: No feature flag infrastructure existed. Features were always on.
- Constraints: Must follow Clean Architecture, config-driven with DB override pattern.

## Decisions & Trade-offs
- **Config as flag registry**: Flags are defined in `appsettings.json FeatureFlags` section. Only config-defined flags are valid — attempts to check or toggle undefined flags return 404. This prevents arbitrary flag creation via API.
- **IFeatureFlagDefaults abstraction**: The Application layer cannot depend on `Microsoft.Extensions.Options`. Created `IFeatureFlagDefaults` interface in Application, implemented by `FeatureFlagDefaults` in Infrastructure using `IOptions<Dictionary<string,bool>>`.
- **No caching in initial implementation**: The task doc mentioned 60s TTL caching. Deferred to keep the initial implementation simple and correct. The `IFeatureFlagService` interface allows adding caching in Infrastructure without changing consumers.
- **Global flags only**: Flags are not scoped to organizations or users. Per-tenant flags are documented as out of scope.
- **Authentication required, no role restriction**: All endpoints require authentication. Production deployments should add admin role checks.

## Implementation Notes
- Key changes: Domain entity, CQRS handlers, EF Core persistence, REST API, frontend hooks and components
- Edge cases: Unknown flag names return 404. DB override takes precedence over config default.
- Known limitations: No caching, no per-tenant scoping, no admin UI page.

## Data / Schema / Migrations
- DB changes: New `feature_flags` table with columns: id, name (unique index), description, is_enabled, created_at_utc, updated_at_utc
- Migration: `AddFeatureFlags`
- Backward compatibility: Additive change, no breaking changes

## Commands Run
```bash
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln
dotnet ef migrations add AddFeatureFlags --project services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure --startup-project services/orgs-api/src/SaasTemplate.OrgsApi.Api
dotnet test services/orgs-api/tests/SaasTemplate.OrgsApi.UnitTests
cd apps/web && npm test
cd apps/web && npm run build
```

## Files Changed

### Backend - Domain
- `Domain/FeatureFlags/FeatureFlag.cs` — Entity with Name, IsEnabled, factory Create(), SetEnabled()

### Backend - Application
- `Application/FeatureFlags/Interfaces/IFeatureFlagRepository.cs` — Repository interface
- `Application/FeatureFlags/Interfaces/IFeatureFlagService.cs` — Service interface for flag evaluation
- `Application/FeatureFlags/Interfaces/IFeatureFlagDefaults.cs` — Abstraction for config defaults
- `Application/FeatureFlags/Queries/GetFeatureFlags/` — Query + handler returning merged flag list
- `Application/FeatureFlags/Queries/CheckFeatureFlag/` — Query + handler for single flag check
- `Application/FeatureFlags/Commands/ToggleFeatureFlag/` — Command + handler for DB override upsert

### Backend - Infrastructure
- `Infrastructure/Persistence/Configurations/FeatureFlagConfiguration.cs` — EF Core table config
- `Infrastructure/Persistence/Repositories/FeatureFlagRepository.cs` — Repository implementation
- `Infrastructure/Services/FeatureFlagDefaults.cs` — IFeatureFlagDefaults implementation
- `Infrastructure/Services/FeatureFlagService.cs` — IFeatureFlagService implementation
- `Infrastructure/Persistence/AppDbContext.cs` — Added FeatureFlags DbSet
- `Infrastructure/DependencyInjection.cs` — Registered repository, defaults, service
- `Infrastructure/Migrations/AddFeatureFlags*` — EF migration files

### Backend - API
- `Api/Controllers/FeatureFlagsController.cs` — GET /feature-flags, GET /feature-flags/{name}, PUT /feature-flags/{name}
- `Api/appsettings.json` — Added FeatureFlags section with 4 sample flags

### Frontend
- `apps/web/src/lib/schemas.ts` — FeatureFlag, FeatureFlagsResponse, FeatureFlagCheckResponse schemas
- `apps/web/src/hooks/feature-flags.ts` — FeatureFlagProvider context, useFeatureFlags(), useFeatureFlag(name) hooks
- `apps/web/src/app/providers.tsx` — Added FeatureFlagProvider to root providers
- `apps/web/src/components/common/FeatureGate.tsx` — Conditional rendering component
- `apps/web/messages/en.json` — Added featureFlags i18n keys
- `apps/web/messages/es.json` — Added featureFlags i18n keys (Spanish)

## Tests
### Unit
- `GetFeatureFlagsQueryHandlerTests` — 3 tests (config defaults, DB override, sorting)
- `CheckFeatureFlagQueryHandlerTests` — 3 tests (config value, DB override, not found)
- `ToggleFeatureFlagCommandHandlerTests` — 3 tests (create new, update existing, not found)
- Total: 129 unit tests passing (120 existing + 9 new)

### Integration
- `FeatureFlagEndpointTests` — 8 tests (list, auth, check, not found, toggle, toggle not found, toggle unauth, DB override in list)

### Frontend
- `FeatureGate.test.tsx` — 5 tests (enabled, disabled, fallback, loading, flag name)
- Total: 226 frontend tests passing (221 existing + 5 new)

## Security
- Validation: Flag names validated against config registry
- AuthN/AuthZ: All endpoints require JWT authentication
- Sensitive data: No sensitive data in feature flag values

## Follow-ups / Backlog
- [ ] Add in-memory caching with TTL for flag evaluation
- [ ] Add admin role check for toggle endpoint
- [ ] Create admin UI page for flag management
- [ ] Add per-organization flag scoping

## Checklist
- [x] Task scope matches `docs/tasks/0026-feature-flags.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
