# Task: 0012-permission-management-ui

## Metadata
- ID: 0012
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-03-05
- Related docs:
  - Walkthrough: `docs/walkthroughs/0012-permission-management-ui.md`
  - Epic: `docs/backlog/epic-P1-agency-permissions.md` (Task 1.6)

## Goal
Build the frontend screens for viewing and managing user permissions within a branch. Admins and owners can see each member's effective permissions (base role + overrides) and toggle individual permission overrides.

## Context
Task 1.3 (0009) established the permission management API endpoints in orgs-api:
- `GET /api/organizations/{orgId}/permissions/{userId}` -- returns effective permissions
- `PUT /api/organizations/{orgId}/permissions/{userId}/{permission}` -- set override
- `DELETE /api/organizations/{orgId}/permissions/{userId}/{permission}` -- remove override

Task 1.5 (0011) delivered the agency management UI with branch dashboard, branch switcher, and agency settings. The members page at `/orgs/[orgId]/members` already exists with role management. This task adds a "Permissions" tab/page accessible from the members area.

## Scope
### In scope
- Permission hooks: `useUserPermissions`, `useSetPermissionOverride`, `useRemovePermissionOverride`
- Permission members list page at `/orgs/[orgId]/permissions` showing branch members with override counts
- Permission detail page at `/orgs/[orgId]/permissions/[userId]` showing all permissions with toggles
- Toggle switches with visual distinction for role defaults vs overrides
- Confirmation dialog for deny overrides
- Source badge component ("Role Default" / "Override")
- i18n translations (en, es) for all permission strings
- Navigation link from members page to permissions page
- Component tests for all new components

### Out of scope
- Role changes (existing feature)
- Bulk permission changes across multiple users
- Permission templates or presets
- Permission change history (audit log is admin-only; not exposed in this UI)

## Requirements
- R1: Only admins and owners can view/modify permissions; agents/viewers see read-only
- R2: Toggling a role default "off" creates a deny override
- R3: Toggling a non-default "on" creates a grant override
- R4: Toggling back to role default state removes the override
- R5: Deny overrides require confirmation dialog
- R6: Permission changes reflect immediately (optimistic update with rollback on error)
- R7: Owner permissions are always shown as granted and non-toggleable

## Acceptance Criteria
- AC1: Members page shows a table of branch members with columns: name, email, role, override count; sortable by role
- AC2: Clicking a member row opens the permission detail view for that user
- AC3: Permission detail view shows all 8 permissions in categorized groups with toggle switches
- AC4: Each permission row shows: permission name, description, effective state (granted/denied), source badge, and toggle switch
- AC5: Toggling a role default to "off" creates a deny override
- AC6: Toggling back to role default state removes the override
- AC7: Creating a deny override shows confirmation dialog
- AC8: Permission changes are reflected immediately without page reload
- AC9: Only admins and owners can toggle; agents/viewers see read-only
- AC10: All components have unit tests

## Constraints (non-negotiable)
- Clean Architecture layers respected.
- English-only repo content.
- No secrets in repo.
- Update walkthrough.
- TDD: write tests first, then implementation.
- Pixel-perfect against Stitch MCP design.

## Proposed Approach (high-level)
1. Generate Stitch MCP designs for permission screens
2. Create permission data hooks following existing patterns (useState + apiFetch)
3. Create components with TDD: tests first, then implementation
4. Add i18n translations
5. Add navigation integration

## Implementation Steps
1. Generate Stitch MCP designs for: permission members list, permission detail, confirmation dialog
2. Create Zod schemas for permission API responses
3. Create `useUserPermissions` hook (GET effective permissions)
4. Create `useSetPermissionOverride` mutation hook (PUT)
5. Create `useRemovePermissionOverride` mutation hook (DELETE)
6. Create `PermissionSourceBadge` component
7. Create `PermissionToggle` component with role default vs override logic
8. Create `OverrideConfirmDialog` component
9. Create `PermissionCategoryGroup` component
10. Create `PermissionDetailPanel` component
11. Create `PermissionMembersList` component
12. Create route pages: `/orgs/[orgId]/permissions/page.tsx` and `/orgs/[orgId]/permissions/[userId]/page.tsx`
13. Add "Permissions" link to members page navigation
14. Add i18n translations (en.json, es.json)
15. Write component tests for all new components

## Files to Create / Modify
### Create
- `apps/web/src/hooks/permissions.ts`
- `apps/web/src/components/permissions/PermissionSourceBadge.tsx`
- `apps/web/src/components/permissions/PermissionToggle.tsx`
- `apps/web/src/components/permissions/OverrideConfirmDialog.tsx`
- `apps/web/src/components/permissions/PermissionCategoryGroup.tsx`
- `apps/web/src/components/permissions/PermissionDetailPanel.tsx`
- `apps/web/src/components/permissions/PermissionMembersList.tsx`
- `apps/web/src/app/[locale]/(dashboard)/orgs/[orgId]/permissions/page.tsx`
- `apps/web/src/app/[locale]/(dashboard)/orgs/[orgId]/permissions/[userId]/page.tsx`
- `apps/web/src/components/permissions/__tests__/PermissionSourceBadge.test.tsx`
- `apps/web/src/components/permissions/__tests__/PermissionToggle.test.tsx`
- `apps/web/src/components/permissions/__tests__/OverrideConfirmDialog.test.tsx`
- `apps/web/src/components/permissions/__tests__/PermissionDetailPanel.test.tsx`
- `apps/web/src/components/permissions/__tests__/PermissionMembersList.test.tsx`
- `.stitch-html/permission-members-list.html`
- `.stitch-html/permission-detail.html`

### Modify
- `apps/web/messages/en.json` (add permissions section)
- `apps/web/messages/es.json` (add permissions section)
- `apps/web/src/lib/schemas.ts` (add permission schemas)
- `apps/web/src/components/orgs/MembersManager.tsx` (add Permissions link)

## Testing Plan
- Unit tests:
  - PermissionMembersList renders member rows with correct role and override count
  - PermissionDetailPanel renders all permissions categorized with correct toggle states
  - PermissionToggle calls correct API when toggling on/off
  - OverrideConfirmDialog shows warning and calls callback on confirm
  - PermissionSourceBadge shows correct label and styling
- Manual verification:
  - Full flow: navigate to permissions, select member, toggle override, verify update
  - Visual comparison against Stitch designs
  - Responsive layout on mobile viewport

## Security & Privacy
- Permission management UI only accessible to admins and owners
- Toggle controls disabled for agents/viewers
- Confirmation dialog prevents accidental permission revocation
- Optimistic update with rollback on API error

## Observability
- Logs: Console errors for failed permission mutations
- Metrics: N/A (frontend)
- Traces: N/A (frontend)

## Rollback Plan
Remove new files and route directories. Revert changes to en.json, es.json, schemas.ts, and MembersManager.tsx.

## Definition of Done Checklist
- [ ] Acceptance criteria met
- [ ] Build passes
- [ ] Tests added/updated and pass
- [ ] Formatting/analyzers pass
- [ ] No secrets committed
- [ ] Walkthrough updated
