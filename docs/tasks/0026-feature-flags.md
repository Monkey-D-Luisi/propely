# Task: 0026 - Feature Flags Infrastructure

## Metadata
- ID: 0026
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-04
- Related docs:
  - Epic: `docs/backlog/epic-001-professional-saas-refinement.md`
  - Walkthrough: `docs/walkthroughs/0026-feature-flags.md`

## Goal
Create a feature flag system that allows toggling features via configuration and database overrides. Backend provides flag evaluation. Frontend provides a hook and gate component to conditionally render features.

## Context
A SaaS template needs feature flags for gradual rollouts, A/B testing, and enabling/disabling features per environment or tenant. This creates the infrastructure for all future feature toggling.

## Scope
### In scope
- Backend: `FeatureFlag` entity (Name, IsEnabled, Description, UpdatedAtUtc)
- Backend: `IFeatureFlagService` interface with `IsEnabled(key)` method
- Backend: Config-driven defaults (appsettings.json) with DB override
- Backend: `GET /feature-flags` - list all flags (authenticated)
- Backend: `GET /feature-flags/{name}` - check single flag (authenticated)
- Backend: `PUT /feature-flags/{name}` - toggle flag (authenticated)
- Frontend: `FeatureFlagProvider` context that fetches all flags once
- Frontend: `useFeatureFlag(key)` hook that reads from context
- Frontend: `<FeatureGate flag="key">` component for conditional rendering
- EF Core migration

### Out of scope
- Per-user or per-org flag targeting (flags are global for now)
- Percentage-based rollouts
- Feature flag admin UI page (API only, UI is future work)
- Third-party feature flag services (LaunchDarkly, etc.)

## Requirements
- R1: Feature flags are evaluated server-side for API behavior and client-side for UI
- R2: Default values come from appsettings.json; DB overrides take precedence
- R3: Server-side caching deferred to follow-up (interface supports adding cache without consumer changes)
- R4: All endpoints require authentication; admin role restriction deferred to follow-up
- R5: Frontend hook returns `{ isEnabled: boolean, isLoading: boolean }`

## Acceptance Criteria
- AC1: `GET /feature-flags` returns list of all flags (authenticated)
- AC2: `PUT /feature-flags/Notifications` toggles the notifications feature
- AC3: `useFeatureFlag('Notifications')` returns correct value from context
- AC4: `<FeatureGate flag="Notifications">` renders children only when enabled
- AC5: Disabling a flag in DB overrides the config default
- AC6: `dotnet build` + `dotnet test` pass
- AC7: `npm run build` + `npm run lint` pass

## Constraints (non-negotiable)
- Clean Architecture layers respected.
- English-only repo content.
- No secrets in repo.
- Update walkthrough.
- Feature flag evaluation must be fast (< 1ms from cache).

## Implementation Steps
### Backend
1. Create FeatureFlag entity in Domain
2. Create IFeatureFlagService in Application
3. Create FeatureFlagService in Infrastructure (reads config + DB, caches in memory)
4. Create FeatureFlagRepository
5. Create admin endpoints (list, toggle)
6. Register in DI
7. EF migration
8. Seed initial flags

### Frontend
9. Create `useFeatureFlags` hook (fetches all flags on mount, caches)
10. Create `useFeatureFlag(key)` hook (reads from cached flags)
11. Create `FeatureGate` component
12. Create FeatureFlagProvider context

## Files to Create / Modify
### Backend
- `Domain/FeatureFlags/FeatureFlag.cs` (create)
- `Application/FeatureFlags/IFeatureFlagService.cs` (create)
- `Infrastructure/Services/FeatureFlagService.cs` (create)
- `Infrastructure/Persistence/Configurations/FeatureFlagConfiguration.cs` (create)
- `Api/Controllers/AdminController.cs` (create)
- `appsettings.json` (modify - add FeatureFlags section)

### Frontend
- `apps/web/src/hooks/feature-flags.ts` (create)
- `apps/web/src/components/common/FeatureGate.tsx` (create)
- `apps/web/src/contexts/FeatureFlagProvider.tsx` (create)

## Testing Plan
- Unit tests: FeatureFlagService (config defaults, DB override, cache)
- Integration tests: Admin endpoints
- Frontend tests: useFeatureFlag hook, FeatureGate component

## Security & Privacy
- All endpoints require JWT authentication
- Admin role restriction deferred to follow-up
- Feature flags contain no PII

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
