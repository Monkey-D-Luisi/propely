# Walkthrough: 0020-property-list-ui

## Task Reference
- Task: `docs/tasks/0020-property-list-ui.md`
- Walkthrough: `docs/walkthroughs/0020-property-list-ui.md`
- Branch/PR: `feat/p2-properties-core`
- Date: `2026-03-05`

## Summary
Implemented the Property List UI for the Next.js frontend, including shared infrastructure (Properties API fetch client, Zod schemas, all CRUD hooks, i18n translations), table/card views with toggle, filter bar with search and dropdowns, status badges, quick actions with status transitions, pagination with empty state, and a Properties navigation link in the AppHeader.

## Context
- Background: The backend property domain (CRUD API, JSON Schema, media management, SDK client) was complete and needed a frontend interface for agents to browse and manage property listings.
- Problem statement: Agents needed a primary entry point to view, filter, sort, and take actions on properties within the web application.
- Constraints: Must follow SWR-based data-fetching pattern established by existing hooks. Must support i18n (en/es). TDD mandatory.

## Decisions & Trade-offs
- **Decision: SWR-based hooks for all property CRUD operations**
  - Options considered: React Query, custom fetch hooks, SWR
  - Why this choice: SWR was already established as the data-fetching pattern (e.g. `hooks/work-items.ts`). Consistency is more valuable than marginal library differences.
  - Consequences / risks: All CRUD hooks centralized in `hooks/properties.ts` for reuse by all property UI tasks.

- **Decision: propertiesApiFetch with CROSS_ORIGIN_BASES refactor**
  - Options considered: Duplicate cross-origin logic per API, shared set
  - Why this choice: Refactored `baseFetch` to use a `CROSS_ORIGIN_BASES` Set so all cross-origin API bases share CORS/credential handling. Cleaner than checking individual bases.
  - Consequences / risks: Minor refactor to shared infra, but avoids accumulating per-API special cases.

- **Decision: Separate PropertyQuickActions component**
  - Options considered: Inline actions in PropertyTable, standalone component
  - Why this choice: Extracting quick actions into its own component allows reuse in both table and card views, and simplifies testing.
  - Consequences / risks: None significant.

## Implementation Notes
- Key changes:
  - `api.ts` gains `PROPERTIES_API_BASE`, `propertiesApiFetch`, and `CROSS_ORIGIN_BASES` set
  - `schemas.ts` gains all property Zod schemas (enums, DTOs, list responses)
  - `hooks/properties.ts` provides useProperties, useProperty, useStatusCounts, useCreateProperty, useUpdateProperty, useDeleteProperty, useChangePropertyStatus
  - PropertyStatusBadge maps each status to semantic colors (Draft=slate, Active=green, Reserved=amber, Sold=blue, Rented=purple, Archived=red)
  - PropertyTable renders sortable columns with an actions dropdown per row
  - PropertyCard renders a visual grid card with thumbnail, badge, price, features
  - PropertyFilterBar provides search + type/operation/status/sort dropdowns
  - ViewToggle uses radiogroup semantics for table/card switching
  - PropertyQuickActions supports edit, view, delete, and valid status transitions
  - Properties page assembles all components with pagination and empty state
  - AppHeader gains a Properties navigation link
- Edge cases handled: Empty state when no properties match, confirmation dialogs for destructive actions
- Known limitations: Pagination is client-driven (offset-based); no server-side cursor pagination yet

## Data / Schema / Migrations
- DB changes: None (frontend-only task)
- Migration strategy: N/A
- Backward compatibility: N/A

## Commands Run
```bash
dotnet build services/properties-api/Propely.PropertiesApi.sln
dotnet test services/properties-api/Propely.PropertiesApi.sln
cd apps/web && npx tsc --noEmit
cd apps/web && npx vitest run
```

## Files Changed
### Infrastructure (shared with all property tasks)
- `apps/web/src/lib/api.ts` -- Added `PROPERTIES_API_BASE`, `CROSS_ORIGIN_BASES`, `propertiesApiFetch`
- `apps/web/src/lib/schemas.ts` -- Added all property Zod schemas (types, statuses, DTOs)
- `apps/web/src/hooks/properties.ts` -- All property CRUD data-fetching hooks
- `apps/web/messages/en.json` -- Properties i18n translations
- `apps/web/messages/es.json` -- Properties i18n translations (Spanish)

### Components
- `apps/web/src/components/properties/PropertyStatusBadge.tsx` -- Color-coded status pills
- `apps/web/src/components/properties/PropertyTable.tsx` -- Full table with sortable columns and actions menu
- `apps/web/src/components/properties/PropertyCard.tsx` -- Grid card with thumbnail, status badge, features
- `apps/web/src/components/properties/PropertyFilterBar.tsx` -- Search input + type/operation/status/sort dropdowns
- `apps/web/src/components/properties/ViewToggle.tsx` -- Table/card toggle with radiogroup semantics
- `apps/web/src/components/properties/PropertyQuickActions.tsx` -- Actions menu with edit, view, delete, status transitions

### Pages
- `apps/web/src/app/[locale]/properties/page.tsx` -- List page assembling all components with pagination and empty state

### Layout
- `apps/web/src/components/layout/AppHeader.tsx` -- Added Properties navigation link

## Tests
### Unit
- What was added/updated:
  - PropertyStatusBadge: 6 tests (all 6 statuses with correct CSS classes)
  - PropertyTable: 5 tests (render, empty state, actions, status transitions)
  - PropertyCard: 7 tests (render fields, badges, thumbnail)
  - PropertyFilterBar: 6 tests (search, dropdowns, callbacks)
  - ViewToggle: 6 tests (toggle, active state, accessibility)
  - PropertyQuickActions tests (action menu rendering, status transition availability)
- How to run: `cd apps/web && npx vitest run`

### Integration
- What was added/updated: N/A (API integration tested via SWR mocking)
- How to run: N/A

### Manual
- What you verified: Table/card views render correctly, filter bar filters properties, pagination navigates between pages, quick actions menu appears with valid transitions, empty state displays when no results
- Steps: Start dev server, navigate to /properties, verify views/filters/pagination/actions

## Observability
- Logs added/updated: SWR error handler logs failed API calls to console
- Traces/metrics added/updated: N/A (frontend-only)

## Security
- Validation: All API responses validated through Zod schemas
- AuthN/AuthZ impact: Authenticated fetch with tenant context headers
- Sensitive data handling: No secrets or PII beyond tenant-scoped property data

## Follow-ups / Backlog
- [x] Property create/edit form (Task 0021)
- [x] Property detail view (Task 0022)
- [x] Advanced search filters (Task 0023)

## Checklist
- [x] Task scope matches `docs/tasks/0020-property-list-ui.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
