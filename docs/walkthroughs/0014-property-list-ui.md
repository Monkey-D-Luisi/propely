# Walkthrough: 0014-property-list-ui

## Summary
Implements the Property List UI for the Next.js frontend, including shared infrastructure (API client, Zod schemas, hooks, i18n), table/card views, filter bar, status badges, and pagination.

## Architecture Decisions

### Shared Infrastructure (API Client, Schemas, Hooks)
Added `propertiesApiFetch` to `api.ts` following the existing `aiApiFetch` pattern. Refactored `baseFetch` to use a `CROSS_ORIGIN_BASES` Set instead of checking only `AI_API_BASE`, so all cross-origin API bases share the same CORS/credential handling.

Property Zod schemas in `schemas.ts` mirror the backend DTOs exactly: `PropertyListItemSchema`, `PropertySchema`, `PropertiesListResponseSchema`, etc. These provide runtime type validation for API responses.

### Data-Fetching Hooks
`hooks/properties.ts` follows the existing SWR-based pattern from `hooks/work-items.ts`. Hooks use `useSWR` for reads (with key-based caching) and `useSWRMutation` for mutations (create, update, delete, status change). Cache invalidation via `mutate` after successful mutations.

### i18n Strategy
All user-facing strings are in `messages/en.json` and `messages/es.json` under a `properties` namespace. Components use `useTranslations('properties')` to access translated strings.

### Table vs Card View
Table view provides detailed columnar data via `PropertyTable` component. Card view via `PropertyCard` provides a visual grid layout. User preference persisted via `ViewToggle` state. Both views share the same data source from `useProperties` hook.

## Files Changed

### Infrastructure (shared with all property tasks)
- `apps/web/src/lib/api.ts` — Added `PROPERTIES_API_BASE`, `CROSS_ORIGIN_BASES`, `propertiesApiFetch`
- `apps/web/src/lib/schemas.ts` — Added all property Zod schemas (types, statuses, DTOs)
- `apps/web/src/hooks/properties.ts` — All property data-fetching hooks
- `apps/web/messages/en.json` — Properties i18n translations
- `apps/web/messages/es.json` — Properties i18n translations (Spanish)

### Components
- `PropertyStatusBadge.tsx` — Color-coded status pills (Draft=slate, Active=green, Reserved=amber, Sold=blue, Rented=purple, Archived=red)
- `PropertyTable.tsx` — Full table with actions menu supporting status transitions
- `PropertyCard.tsx` — Grid card with thumbnail placeholder, status badge, features
- `PropertyFilterBar.tsx` — Search input + type/operation/status/sort dropdowns
- `ViewToggle.tsx` — Table/card toggle with radiogroup semantics

### Pages
- `apps/web/src/app/[locale]/properties/page.tsx` — List page assembling all components with pagination

## Test Coverage
- PropertyStatusBadge: 6 tests (all statuses, correct CSS classes)
- PropertyTable: 5 tests (render, empty state, actions, status transitions)
- PropertyCard: 7 tests (render fields, badges, thumbnail)
- PropertyFilterBar: 6 tests (search, dropdowns, callbacks)
- ViewToggle: 6 tests (toggle, active state, accessibility)
- **Total: 30 tests, all passing**
