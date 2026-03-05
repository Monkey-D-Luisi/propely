# Task: 0011-agency-management-ui

## Metadata
- ID: 0011
- Type: Standard
- Status: TODO
- Owner: Agent
- Created: 2026-03-05
- Related docs:
  - Walkthrough: `docs/walkthroughs/0011-agency-management-ui.md`
  - Epic: `docs/backlog/epic-P1-agency-permissions.md` (Task 1.5)

## Goal
Build the frontend screens for agency management: creating an agency, viewing agency details with branches, switching between branches in the header, and managing agency settings.

## Context
The backend agency API endpoints exist (create, list, get, add branch, remove branch). The frontend needs to consume these endpoints and provide a user-facing interface for agency management. This establishes the multi-branch navigation pattern used throughout the application.

## Scope
### In scope
- Agency creation flow: form with name and slug input
- Agency dashboard: list branches with member counts
- Branch switcher in the application header
- Agency settings page: update name, danger zone (delete agency)
- API integration: call agency endpoints via apiFetch
- i18n translations (en/es)
- Component tests for all new components

### Out of scope
- Permission management UI (task 1.6)
- Branch creation (branches are created via existing org creation flow)
- Billing/subscription UI
- Invitation management

## Requirements
- R1: Users can create agencies with name and slug
- R2: Users can view agency dashboard with branches
- R3: Users can switch between branches via header dropdown
- R4: Agency owners can update agency settings
- R5: Agency owners can delete agencies with confirmation
- R6: All UI text is translatable (en/es)

## Acceptance Criteria
- AC1: "Create Agency" button appears on the dashboard for users without an agency
- AC2: Agency creation form has name and slug inputs with real-time validation
- AC3: Successful agency creation redirects to the agency dashboard
- AC4: Agency dashboard displays branch cards with name, member count, creation date
- AC5: Branch switcher in the header shows agency name and dropdown of branches
- AC6: Current branch is visually highlighted in the branch switcher
- AC7: Agency settings page allows updating the agency name
- AC8: Agency settings page has a danger zone with "Delete Agency" action requiring confirmation
- AC9: All components have unit tests
- AC10: All screens follow existing design system conventions

## Constraints (non-negotiable)
- Clean Architecture layers respected.
- English-only repo content.
- No secrets in repo.
- Update walkthrough.

## Proposed Approach (high-level)
1. Add Zod schemas for agency types
2. Create React hooks for agency API calls
3. Build components following existing patterns (OrgSettingsForm, MyOrgsPage)
4. Add page routes under /agencies/
5. Update AppHeader with BranchSwitcher
6. Add i18n translations
7. Write component tests

## Implementation Steps
1. Add agency Zod schemas to lib/schemas.ts
2. Create hooks: useAgencies, useAgency, useCreateAgency, useDeleteAgency
3. Create AgencyContext for shared agency state
4. Create AgencyCreateForm component
5. Create AgencyDashboard with BranchCard component
6. Create BranchSwitcher component
7. Create AgencySettingsForm component
8. Add page routes: /agencies/new, /agencies/[id], /agencies/[id]/settings
9. Update AppHeader to include agencies link
10. Add i18n translation keys (en/es)
11. Write component tests for all new components

## Files to Create / Modify
- `apps/web/src/lib/schemas.ts` — Add agency schemas
- `apps/web/src/hooks/agencies.ts` — Agency hooks
- `apps/web/src/contexts/AgencyContext.tsx` — Agency context
- `apps/web/src/components/agencies/AgencyCreateForm.tsx`
- `apps/web/src/components/agencies/AgencyDashboard.tsx`
- `apps/web/src/components/agencies/BranchCard.tsx`
- `apps/web/src/components/agencies/BranchSwitcher.tsx`
- `apps/web/src/components/agencies/AgencySettingsForm.tsx`
- `apps/web/src/app/[locale]/agencies/new/page.tsx`
- `apps/web/src/app/[locale]/agencies/[id]/page.tsx`
- `apps/web/src/app/[locale]/agencies/[id]/settings/page.tsx`
- `apps/web/src/components/layout/AppHeader.tsx` — Add agencies link
- `apps/web/messages/en.json` — Add agency translation keys
- `apps/web/messages/es.json` — Add agency translation keys
- `apps/web/src/components/agencies/__tests__/AgencyCreateForm.test.tsx`
- `apps/web/src/components/agencies/__tests__/AgencyDashboard.test.tsx`
- `apps/web/src/components/agencies/__tests__/BranchSwitcher.test.tsx`
- `apps/web/src/components/agencies/__tests__/AgencySettingsForm.test.tsx`

## Testing Plan
- Unit tests: Component tests using Vitest + React Testing Library
- Integration tests: N/A (frontend-only task)
- Manual verification: Visual check of agency screens in dev environment

## Security & Privacy
- Agency deletion requires explicit confirmation
- Only agency owners see Settings and Delete options
- Branch switcher only shows branches the user has membership in

## Observability
- Logs: N/A (frontend-only)
- Metrics: N/A
- Traces: N/A

## Rollback Plan
Revert the frontend changes. No backend or database changes required.

## Definition of Done Checklist
- [ ] Acceptance criteria met
- [ ] Build passes
- [ ] Tests added/updated and pass
- [ ] Formatting/analyzers pass
- [ ] No secrets committed
- [ ] Walkthrough updated
