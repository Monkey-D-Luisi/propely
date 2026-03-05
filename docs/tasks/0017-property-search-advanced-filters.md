# Task: 0017-property-search-advanced-filters

## Metadata
- ID: 0017
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-03-05
- Related docs:
  - Walkthrough: `docs/walkthroughs/0017-property-search-advanced-filters.md`
  - Epic: `docs/backlog/epic-P2-properties.md` (Task 2.10)

## Goal
Implement advanced property search and filtering with full-text search (ILIKE) on the backend and an expandable advanced filter panel on the frontend, supporting multi-select types/operations/statuses, price/area ranges, bedroom/bathroom minimums, and amenity toggles.

## Context
Task 2.3 built the basic property list API with simple filters. This task extends both the backend and frontend to support full-text search across title/address fields and advanced filtering by property features and amenities.

## Scope
### In scope
- Backend: Full-text ILIKE search on title, city, province, street
- Backend: Advanced filter parameters (minBedrooms, minBathrooms, minArea, maxArea, amenity booleans)
- Backend: Relevance-based sorting when search is active
- Frontend: AdvancedFilterPanel with expandable/collapsible UI
- Frontend: Multi-select chips for types, operations, statuses
- Frontend: Price and area range inputs
- Frontend: Amenity toggle switches
- Frontend: Active filter count badge
- TDD test coverage

### Out of scope
- Full PostgreSQL tsvector/tsquery (pragmatic ILIKE approach for MVP)
- Saved searches / search history
- Map-based search

## Requirements
- R1: ILIKE search across title, city, province, street fields
- R2: Relevance sorting (title matches first) when search active without explicit sort
- R3: Advanced filters: bedrooms, bathrooms, area range, amenities (pool, garden, garage, elevator, terrace)
- R4: Controller accepts all filter query parameters
- R5: Expandable filter panel with active count badge
- R6: Clear all filters action

## Acceptance Criteria
- AC1: Backend search filters by ILIKE on title and address fields
- AC2: Backend advanced filters narrow results by features/amenities
- AC3: Controller exposes search + all advanced filter query params
- AC4: Frontend panel toggles open/closed
- AC5: Active filter count badge accurately reflects applied filters
- AC6: Multi-select chip toggles work for types, operations, statuses
- AC7: Clear all resets all filters
- AC8: All tests pass (backend: 257, frontend: 610)

## Files Created / Modified
### Backend modified
- `IPropertyReadRepository.cs` — Added Search, MinBedrooms, MinBathrooms, MinArea, MaxArea, amenity booleans to PropertyListFilter
- `ListPropertiesQuery.cs` — Added Search and all advanced filter properties
- `ListPropertiesQueryHandler.cs` — Maps all new filter fields to PropertyListFilter
- `PropertiesController.cs` — Added search, minBedrooms, minBathrooms, minArea, maxArea, amenity query params
- `PropertyReadRepository.cs` — Implemented ILIKE search, advanced filters, relevance sorting

### Frontend created
- `apps/web/src/components/properties/search/AdvancedFilterPanel.tsx`
- Tests: AdvancedFilterPanel.test.tsx

## Testing Plan
- 5 AdvancedFilterPanel frontend tests (toggle, badges, multi-select, clear)
- 257 backend tests pass (including existing CRUD/lifecycle tests)

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes (0 errors)
- [x] Tests added and pass (backend: 257, frontend: 610)
- [x] No secrets committed
- [x] Walkthrough created
