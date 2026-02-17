# Task: 0047 - Feature Flag Admin Management UI

## Metadata
- ID: 0047
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-07
- GitHub Issue: #199
- Epic: `docs/backlog/epic-007-admin-dashboards.md`
- Old Issue: #66
- Milestone: MVP

## Goal
Create an admin page for managing feature flags: list all flags with toggle switches, add/edit flag dialog, and show flag source (config vs DB).

## Context
The backend feature flag system is complete (task 0026): `FeatureFlag` entity, `FeatureFlagsController` with GET/POST/PUT/TOGGLE endpoints, `IFeatureFlagService` with config-driven defaults + DB overrides. The frontend has `useFeatureFlag` hook and `<FeatureGate>` component. What's missing is the admin UI to manage flags without API calls.

### Existing Backend Endpoints
- `GET /feature-flags` - list all flags
- `POST /feature-flags` - create flag
- `PUT /feature-flags/{id}` - update flag
- `POST /feature-flags/{id}/toggle` - toggle flag on/off

### Existing Frontend
- `hooks/feature-flags.ts`: `useFeatureFlags()`, `useFeatureFlag(name)`
- `components/common/FeatureGate.tsx`: conditional rendering based on flag

## Scope
### In scope
- `/admin/feature-flags` page (admin-only route)
- Table listing all feature flags with: name, description, enabled/disabled toggle, source
- Create new flag dialog (name, description, default state)
- Edit flag dialog (update description, toggle state)
- Toggle switch for quick enable/disable
- Restrict access to admin users only
- i18n strings (EN + ES)

### Out of scope
- Per-org feature flag overrides (future feature)
- Feature flag analytics/usage tracking
- Feature flag scheduling (enable at specific time)

## Requirements
- R1: Admin users can view all feature flags
- R2: Admin users can create new flags
- R3: Admin users can toggle flags on/off
- R4: Admin users can edit flag details
- R5: Non-admin users cannot access the page (redirect to home)
- R6: Flag source (config/database) is displayed

## Acceptance Criteria
- AC1: `/admin/feature-flags` shows a table of all flags
- AC2: Toggle switch changes flag state via API
- AC3: "Create Flag" button opens dialog and creates new flag
- AC4: Edit button opens dialog with current values
- AC5: Non-admin user gets redirected
- AC6: `npm run build` and `npm test` pass
- AC7: i18n strings in EN + ES

## Constraints (non-negotiable)
- Use existing component patterns (react-hook-form + zod for dialogs)
- Use existing hooks pattern for API calls
- i18n for all strings
- Update walkthrough

## Implementation Steps

1. **Create admin feature flags page** (`apps/web/src/app/[locale]/admin/feature-flags/page.tsx`)
   - Check if user is admin, redirect if not
   - Fetch flags with `useFeatureFlags()`
   - Render table with columns: Name, Description, Status (toggle), Source, Actions

2. **Create FeatureFlagTable component** (`apps/web/src/components/admin/FeatureFlagTable.tsx`)
   - Table rows with flag data
   - Toggle switch in Status column
   - Edit/Delete action buttons

3. **Create CreateFlagDialog component** (`apps/web/src/components/admin/CreateFlagDialog.tsx`)
   - Form with react-hook-form + zod: name (required, slug format), description, enabled (default false)
   - Submit calls `POST /feature-flags`

4. **Create EditFlagDialog component** (`apps/web/src/components/admin/EditFlagDialog.tsx`)
   - Pre-filled form with current values
   - Submit calls `PUT /feature-flags/{id}`

5. **Add admin hooks** (`apps/web/src/hooks/feature-flags.ts`)
   - `useCreateFeatureFlag()`: POST /feature-flags
   - `useToggleFeatureFlag()`: POST /feature-flags/{id}/toggle
   - `useUpdateFeatureFlag()`: PUT /feature-flags/{id}

6. **Add admin route protection**
   - Check user role from `useCurrentUser()`
   - Redirect to home if not admin

7. **Add admin navigation link** to AppHeader (visible only to admins)
   - "Admin" dropdown or link -> Feature Flags

8. **Add Zod schemas** for create/edit flag forms

9. **Add i18n strings** (`apps/web/messages/en.json`, `apps/web/messages/es.json`)
   - `admin.featureFlags`, `admin.createFlag`, `admin.editFlag`, `admin.flagName`, `admin.flagDescription`, `admin.flagEnabled`, `admin.flagSource`, `admin.flagToggled`, `admin.adminOnly`

### Testing

10. **Frontend tests**
    - Table renders flag data
    - Toggle calls API
    - Create dialog submits correctly
    - Non-admin redirect

## Files to Create / Modify

### Create
- `apps/web/src/app/[locale]/admin/feature-flags/page.tsx`
- `apps/web/src/components/admin/FeatureFlagTable.tsx`
- `apps/web/src/components/admin/CreateFlagDialog.tsx`
- `apps/web/src/components/admin/EditFlagDialog.tsx`
- `docs/walkthroughs/0047-feature-flag-admin-ui.md`

### Modify
- `apps/web/src/hooks/feature-flags.ts` (add admin hooks)
- `apps/web/src/components/layout/AppHeader.tsx` (add admin link)
- `apps/web/src/lib/schemas.ts` (add flag form schemas)
- `apps/web/messages/en.json`
- `apps/web/messages/es.json`

## Testing Plan
- Frontend tests: All components and hooks
- Manual: Full admin flow (list, create, toggle, edit)

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] i18n strings added (EN + ES)
- [x] Formatting/analyzers pass
- [x] Walkthrough updated
