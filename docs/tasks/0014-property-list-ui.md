# Task: 0014-property-list-ui

## Metadata
- ID: 0014
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-03-05
- Related docs:
  - Walkthrough: `docs/walkthroughs/0014-property-list-ui.md`
  - Epic: `docs/backlog/epic-P2-properties.md` (Task 2.7)

## Goal
Implement the Property List page in the Next.js frontend, including table/card views, filter bar, status badges, pagination, and data-fetching hooks connected to the Properties API.

## Context
Tasks 2.1-2.6 built the backend property domain, persistence, CRUD API, JSON Schema, media management, and NuGet SDK client. This task creates the frontend UI for browsing and managing property listings, serving as the primary entry point for agents working with properties.

## Scope
### In scope
- Properties API fetch client (`propertiesApiFetch`) and cross-origin support in api.ts
- Zod schemas for property types (PropertyTypeEnum, OperationTypeEnum, PropertyStatusEnum, etc.)
- Data-fetching hooks (useProperties, useProperty, useStatusCounts, mutation hooks)
- i18n translations for properties (en.json, es.json)
- PropertyStatusBadge component with color-coded status display
- PropertyTable component with sortable columns and action menus
- PropertyCard component for grid view
- PropertyFilterBar with search, type, operation, status, and sort dropdowns
- ViewToggle for table/card switching
- Properties list page (/[locale]/properties) with pagination
- Full TDD test coverage (30 tests across 6 test files)

### Out of scope
- Property create/edit form (Task 2.8)
- Property detail view (Task 2.9)
- Advanced search filters (Task 2.10)

## Requirements
- R1: Table view with columns: title, type, operation, status, price, city, agent, date
- R2: Card view with thumbnail, status badge, price, key features
- R3: Filter bar with search, type, operation, status, sort dropdowns
- R4: Status badges with semantic color coding per PropertyStatusType
- R5: Actions menu with edit, view, delete, and status transitions
- R6: Pagination with page navigation
- R7: Data-fetching via propertiesApiFetch with SWR hooks

## Acceptance Criteria
- AC1: Property list page renders with table view by default
- AC2: User can switch between table and card views
- AC3: Filter bar filters by type, operation, status; search by keyword
- AC4: Action menu shows valid status transitions per current status
- AC5: Delete and status change use confirmation dialogs
- AC6: All 30+ tests pass
- AC7: i18n translations for en and es

## Files Created / Modified
### New files
- `apps/web/src/hooks/properties.ts`
- `apps/web/src/components/properties/PropertyStatusBadge.tsx`
- `apps/web/src/components/properties/PropertyTable.tsx`
- `apps/web/src/components/properties/PropertyCard.tsx`
- `apps/web/src/components/properties/PropertyFilterBar.tsx`
- `apps/web/src/components/properties/ViewToggle.tsx`
- `apps/web/src/app/[locale]/properties/page.tsx`
- Tests: PropertyStatusBadge.test.tsx, PropertyTable.test.tsx, PropertyCard.test.tsx, PropertyFilterBar.test.tsx, ViewToggle.test.tsx

### Modified files
- `apps/web/src/lib/api.ts` — Added propertiesApiFetch and CROSS_ORIGIN_BASES
- `apps/web/src/lib/schemas.ts` — Added property Zod schemas
- `apps/web/messages/en.json` — Added properties i18n section
- `apps/web/messages/es.json` — Added properties i18n section

## Testing Plan
- 6 PropertyStatusBadge tests (renders all 6 statuses with correct styling)
- 5 PropertyTable tests (render, empty state, action menu, status transitions)
- 7 PropertyCard tests (render all fields, thumbnail, badges)
- 6 PropertyFilterBar tests (search, dropdowns, callbacks)
- 6 ViewToggle tests (toggle between views, active state)

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added and pass (30+ tests)
- [x] No secrets committed
- [x] Walkthrough created
