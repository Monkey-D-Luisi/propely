# Task: 0020-property-list-ui

## Metadata
- ID: 0020
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-03-05
- Related docs:
  - Walkthrough: `docs/walkthroughs/0020-property-list-ui.md`
  - Epic: `docs/backlog/epic-P2-properties.md` (Task 2.7)

## Goal
Implement the Property List page in the Next.js frontend with table/card views, filter bar, pagination, sorting, status badges, quick actions, empty state, and navigation link in the AppHeader.

## Context
The backend Property CRUD API (Task 2.3), JSON Schema/UI Schema (Task 2.4), media management (Task 2.5), and NuGet SDK client (Task 2.6) are complete. This task creates the primary frontend UI for browsing and managing property listings, providing the main entry point for agents interacting with properties. All property data-fetching hooks (CRUD) are also created here as shared infrastructure for subsequent UI tasks.

## Scope
### In scope
- Properties API fetch client (`propertiesApiFetch`) and cross-origin support in api.ts
- Zod schemas for property types (PropertyTypeEnum, OperationTypeEnum, PropertyStatusEnum, etc.)
- All CRUD data-fetching hooks in `hooks/properties.ts` (useProperties, useProperty, useStatusCounts, mutation hooks)
- i18n translations for properties (en.json, es.json)
- PropertyStatusBadge component with color-coded status display
- PropertyTable component with sortable columns and action menus
- PropertyCard component for grid view with thumbnail and badges
- PropertyFilterBar with search, type, operation, status, and sort dropdowns
- ViewToggle for table/card switching with radiogroup semantics
- PropertyQuickActions component with edit, view, delete, and status transitions
- Properties list page (`/[locale]/properties`) with pagination and empty state
- Properties link added to AppHeader navigation
- Full TDD test coverage for all components

### Out of scope
- Property create/edit form (Task 2.8 / Task 0021)
- Property detail view (Task 2.9 / Task 0022)
- Advanced search filters (Task 2.10 / Task 0023)

## Requirements
- R1: Table view with columns: title, type, operation, status, price, city, agent, date
- R2: Card view with thumbnail, status badge, price, key features (bedrooms, bathrooms, area)
- R3: Filter bar with search input, type/operation/status/sort dropdowns
- R4: Status badges with semantic color coding per PropertyStatusType (Draft=slate, Active=green, Reserved=amber, Sold=blue, Rented=purple, Archived=red)
- R5: Quick actions menu with edit, view, delete, and valid status transitions per current status
- R6: Pagination with page navigation controls
- R7: Data-fetching via propertiesApiFetch with SWR hooks (useSWR for reads, useSWRMutation for writes)
- R8: Empty state displayed when no properties match filters
- R9: Properties link in AppHeader navigation

## Acceptance Criteria
- AC1: Property list page renders with table view by default
- AC2: User can switch between table and card views via ViewToggle
- AC3: Filter bar filters by type, operation, status; search by keyword
- AC4: Quick actions menu shows valid status transitions per current status
- AC5: Delete and status change use confirmation dialogs
- AC6: Pagination controls navigate between pages
- AC7: Empty state renders when no properties found
- AC8: AppHeader shows Properties navigation link
- AC9: All component tests pass
- AC10: i18n translations for en and es

## Constraints (non-negotiable)
- Clean Architecture layers respected.
- English-only repo content.
- No secrets in repo.
- Update walkthrough.

## Proposed Approach (high-level)
1. Add Properties API fetch client and Zod schemas as shared infrastructure
2. Create all CRUD hooks in a single `hooks/properties.ts` file
3. Build individual components (StatusBadge, Table, Card, FilterBar, ViewToggle, QuickActions) with TDD
4. Assemble the list page with pagination, empty state, and filters
5. Add Properties link to AppHeader

## Implementation Steps
1. Add `propertiesApiFetch` to `api.ts` and update `CROSS_ORIGIN_BASES`
2. Add property Zod schemas to `schemas.ts`
3. Create `hooks/properties.ts` with all CRUD hooks
4. Add i18n translations to en.json and es.json
5. Create PropertyStatusBadge component with tests
6. Create PropertyTable component with tests
7. Create PropertyCard component with tests
8. Create PropertyFilterBar component with tests
9. Create ViewToggle component with tests
10. Create PropertyQuickActions component with tests
11. Create properties list page with pagination and empty state
12. Add Properties link to AppHeader

## Files to Create / Modify
### New files
- `apps/web/src/hooks/properties.ts`
- `apps/web/src/components/properties/PropertyStatusBadge.tsx`
- `apps/web/src/components/properties/PropertyTable.tsx`
- `apps/web/src/components/properties/PropertyCard.tsx`
- `apps/web/src/components/properties/PropertyFilterBar.tsx`
- `apps/web/src/components/properties/ViewToggle.tsx`
- `apps/web/src/components/properties/PropertyQuickActions.tsx`
- `apps/web/src/app/[locale]/properties/page.tsx`
- Tests: PropertyStatusBadge.test.tsx, PropertyTable.test.tsx, PropertyCard.test.tsx, PropertyFilterBar.test.tsx, ViewToggle.test.tsx, PropertyQuickActions.test.tsx

### Modified files
- `apps/web/src/lib/api.ts` -- Added propertiesApiFetch and CROSS_ORIGIN_BASES
- `apps/web/src/lib/schemas.ts` -- Added property Zod schemas
- `apps/web/messages/en.json` -- Added properties i18n section
- `apps/web/messages/es.json` -- Added properties i18n section
- `apps/web/src/components/layout/AppHeader.tsx` -- Added Properties navigation link

## Testing Plan
- Unit tests:
  - 6 PropertyStatusBadge tests (renders all 6 statuses with correct styling)
  - 5 PropertyTable tests (render, empty state, action menu, status transitions)
  - 7 PropertyCard tests (render all fields, thumbnail, badges)
  - 6 PropertyFilterBar tests (search, dropdowns, callbacks)
  - 6 ViewToggle tests (toggle between views, active state, accessibility)
  - PropertyQuickActions tests (edit, view, delete actions, status transitions)
- Integration tests: N/A (API integration via SWR hooks tested via mocking)
- Manual verification: Visual check of table/card views, filter bar, pagination

## Security & Privacy
- All API calls use authenticated fetch with tenant context headers
- No PII displayed beyond property data owned by the tenant
- Delete actions require confirmation to prevent accidental data loss

## Observability
- Logs: Client-side console errors for failed API calls (SWR error handler)
- Metrics: N/A (frontend-only)
- Traces: N/A (frontend-only)

## Rollback Plan
Revert the commit on the `feat/p2-properties-core` branch. No database migrations involved.

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass (627 frontend, 257 backend)
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
