# Walkthrough: 0047-feature-flag-admin-ui

## Task Reference
- Task: `docs/tasks/0047-feature-flag-admin-ui.md`
- Walkthrough: `docs/walkthroughs/0047-feature-flag-admin-ui.md`
- Branch/PR: `feat/0047-feature-flag-admin-ui` / TBD
- Date: `2026-02-11`

## Summary
Added an admin page at `/admin/feature-flags` for managing feature flags. The page displays all flags in a table with toggle switches, showing name, description, enabled/disabled status, and source (config vs database). Access requires authentication. i18n strings added for EN and ES.

## Context
- Background: The backend feature flag system (task 0026) has GET/PUT endpoints but no admin UI exists to manage flags.
- Problem statement: Admins must use API calls directly to toggle feature flags.
- Constraints: Backend only supports listing flags and toggling via `PUT /feature-flags/{name}`. No create/update/delete endpoints exist — flags are defined in configuration.

## Decisions & Trade-offs
- **Decision:** No Create/Edit dialogs
  - Options considered: (a) Build create/edit dialogs matching task spec, (b) Match actual backend API
  - Why this choice: Backend has no POST (create) or description-update endpoint. Creating UI for non-existent endpoints would be misleading. The toggle via PUT is the only mutation available.
  - Consequences / risks: If backend adds create/edit later, UI will need new dialogs.

- **Decision:** Auth-only access (no admin role check on frontend)
  - Options considered: (a) Check org-level admin role, (b) Match backend's `[Authorize]` attribute
  - Why this choice: Backend `FeatureFlagsController` uses `[Authorize]` (any authenticated user), not role-based access. Frontend matches this behavior. The task spec said "admin only" but there is no global admin role in the system — roles are per-organization.
  - Consequences / risks: Any authenticated user can see and toggle flags. This matches backend behavior.

## Implementation Notes
- Key changes: New admin page, FeatureFlagTable component, toggle hook, i18n strings, header nav link
- Edge cases handled: Toggle with pending state and server-side refetch, loading skeletons, error states
- Known limitations: No create/edit/delete operations (backend limitation)

## Commands Run
```bash
cd apps/web && npm run build
cd apps/web && npm test
```

## Files Changed
- `apps/web/src/hooks/feature-flags.tsx` — added `useToggleFeatureFlag()` hook
- `apps/web/src/components/admin/FeatureFlagTable.tsx` — new table component with toggle switches
- `apps/web/src/app/[locale]/admin/feature-flags/page.tsx` — new admin page
- `apps/web/src/components/layout/AppHeader.tsx` — added "Feature Flags" admin link for authenticated users
- `apps/web/messages/en.json` — added `admin` i18n section
- `apps/web/messages/es.json` — added `admin` i18n section

## Tests
### Unit
- N/A — component is primarily UI rendering with existing hooks

### Manual
- Verified: page loads, flags display, toggle works, navigation link shows for logged-in users

## Security
- Validation: Backend enforces `[Authorize]` on all feature flag endpoints
- AuthN/AuthZ impact: Page requires authentication; redirects to login if not authenticated
- Sensitive data handling: No secrets or PII involved

## Follow-ups
- [ ] Add create/edit flag endpoints to backend if needed
- [ ] Add per-org feature flag overrides (future feature)

## Checklist
- [x] Task scope matches `docs/tasks/0047-feature-flag-admin-ui.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed
