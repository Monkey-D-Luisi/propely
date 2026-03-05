# Walkthrough: 0012-permission-management-ui

## Task Reference
- Task: `docs/tasks/0012-permission-management-ui.md`
- Walkthrough: `docs/walkthroughs/0012-permission-management-ui.md`
- Branch/PR: `feat/p1-permission-ui-e2e` / #37
- Date: `2026-03-05`

## Summary
Implemented full frontend permission management UI allowing owners and admins to view and manage fine-grained permission overrides for branch members. The UI includes a members list page, per-member detail panel with toggle switches, confirmation dialogs for deny overrides, and i18n support (en/es).

## Context
- Background: The permission management API exists (task 0009). Agency management UI exists (task 0011). The members page exists at `/orgs/[orgId]/members`. This task adds a dedicated permission management UI.
- Problem statement: Admins and owners have no frontend UI to view or manage fine-grained permission overrides for branch members.
- Constraints: TDD, i18n (en/es).

## Decisions & Trade-offs
- **Client-side role determination**: `PermissionDetailPageClient` fetches all members via `useMembers(orgId, 1, 100)` to find both the target user and the current viewer's role. A dedicated single-member endpoint would be more efficient but doesn't exist yet.
- **Permission categories hardcoded**: `PERMISSION_CATEGORIES` and `ROLE_DEFAULTS` are defined as constants in the component. This was preferred over fetching from the API to avoid extra network requests and to keep the UI responsive.
- **No optimistic updates**: Permission changes wait for API confirmation before reflecting in the UI. Optimistic updates were deferred as a follow-up to keep scope manageable.
- **SVG icons inline in component**: Category-specific icons (BuildingIcon, UsersIcon, etc.) are defined in `PermissionDetailPanel.tsx` rather than the shared `icons.tsx` to keep them co-located with their only consumer.

## Implementation Notes
- 6 permission components: `PermissionMembersList`, `PermissionDetailPanel`, `PermissionToggle`, `PermissionSourceBadge`, `PermissionCategoryGroup`, `OverrideConfirmDialog`
- 3 hooks: `useUserPermissions`, `useSetPermissionOverride`, `useRemovePermissionOverride`
- 2 route pages: `/orgs/[orgId]/permissions` (list) and `/orgs/[orgId]/permissions/[userId]` (detail)
- Zod schemas for API response validation (`EffectivePermission`, `EffectivePermissionsResponse`)
- Permissions link added to `MembersManager` navigation (only visible to managers)
- Viewer role is computed from the current user's membership to gate editing capabilities

## Data / Schema / Migrations
- DB changes: None (frontend only)
- Migration strategy: N/A
- Backward compatibility: Additive only

## Commands Run
```bash
cd apps/web && npm run build
cd apps/web && npm test
```

## Files Changed
- `apps/web/src/app/[locale]/orgs/[orgId]/permissions/page.tsx` — permissions list route
- `apps/web/src/app/[locale]/orgs/[orgId]/permissions/[userId]/page.tsx` — detail route
- `apps/web/src/app/[locale]/orgs/[orgId]/permissions/[userId]/PermissionDetailPageClient.tsx` — client component
- `apps/web/src/components/permissions/PermissionMembersList.tsx` — members list
- `apps/web/src/components/permissions/PermissionDetailPanel.tsx` — detail panel
- `apps/web/src/components/permissions/PermissionToggle.tsx` — toggle switch
- `apps/web/src/components/permissions/PermissionSourceBadge.tsx` — source badge
- `apps/web/src/components/permissions/PermissionCategoryGroup.tsx` — category group
- `apps/web/src/components/permissions/OverrideConfirmDialog.tsx` — confirm dialog
- `apps/web/src/components/orgs/MembersManager.tsx` — added Permissions nav link
- `apps/web/src/components/ui/icons.tsx` — added ShieldCheckIcon
- `apps/web/src/hooks/permissions.ts` — permission hooks
- `apps/web/src/lib/schemas.ts` — EffectivePermission Zod schema
- `apps/web/messages/en.json`, `apps/web/messages/es.json` — i18n translations

## Tests
### Unit
- `OverrideConfirmDialog.test.tsx` — 6 tests: render, confirm, cancel, disabled state, aria
- `PermissionDetailPanel.test.tsx` — 11 tests: loading, error, render, categories, toggles, badges, owner note, back nav
- `PermissionMembersList.test.tsx` — 10 tests: loading, error, render, links, roles, read-only, empty state
- `PermissionSourceBadge.test.tsx` — 4 tests: role default, override styling
- `PermissionToggle.test.tsx` — 9 tests: switch role, click, disabled, loading, styling

### Integration
N/A (frontend only)

### Manual
Navigate to Members → Permissions to verify the UI renders correctly.

## Observability
No new observability instrumentation. Frontend-only changes.

## Security
- Permission editing is gated by the viewer's role (`viewerCanManage`), not the target user's role
- Manage links hidden from non-manager users
- Cancel button disabled during async operations to prevent race conditions

## Follow-ups / Backlog
- [ ] Permission change history UI (requires dedicated audit API endpoint filtered by entity type)
- [ ] Optimistic UI updates for permission toggles
- [ ] Dedicated single-member API endpoint to replace bulk member fetch

## Checklist
- [x] Task scope matches `docs/tasks/0012-permission-management-ui.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
