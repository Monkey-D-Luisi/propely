# Walkthrough: 0017-property-search-advanced-filters

## Summary
Extends the property listing with full-text search (ILIKE) on the backend and an advanced filter panel on the frontend, supporting multi-select types/operations/statuses, price/area ranges, bedroom/bathroom minimums, and amenity toggles.

## Architecture Decisions

### Backend: ILIKE vs tsvector/tsquery
Chose pragmatic ILIKE approach for MVP. ILIKE with `%search%` pattern on title, city, province, and street fields provides adequate search without requiring a PostgreSQL migration for tsvector columns and GIN indexes. Can upgrade to full-text search in a future optimization pass if needed.

### Backend: Relevance Sorting
When a search term is active and no explicit sort is specified, results are ordered by relevance: title matches appear first (via `OrderByDescending(p => EF.Functions.ILike(p.Title, searchPattern))`), then by creation date. Users can also explicitly select `sortBy=relevance`.

### Backend: Amenity Boolean Filters
Amenity filters (hasPool, hasGarden, hasGarage, hasElevator, hasTerrace) only filter when `true`. A `null` or `false` value means "don't filter by this amenity." This is intentional — agents searching for "has pool" want to narrow results, but not searching for "no pool."

### Frontend: Expandable Panel
`AdvancedFilterPanel` is a collapsible panel with a toggle button showing an active filter count badge. This keeps the primary filter bar clean while providing power-user access to advanced criteria. The panel subdivides into sections: Property Type (multi-select chips), Operation (chips), Status (chips), Price Range (min/max inputs), Area Range (min/max inputs), Bedrooms/Bathrooms (min inputs), and Amenities (toggle switches).

### Frontend: Multi-Select Chip Pattern
Type, operation, and status filters use a multi-select chip pattern. Clicking a chip toggles it on/off. The filter state is an array of selected values, enabling compound filters (e.g. "Apartment OR Villa" + "Sale OR Rent").

## Files Changed

### Backend
- `IPropertyReadRepository.cs` — Added Search, MinBedrooms, MinBathrooms, MinArea, MaxArea, amenity boolean fields to `PropertyListFilter`
- `ListPropertiesQuery.cs` — Added all advanced filter properties
- `ListPropertiesQueryHandler.cs` — Maps all new fields from query to filter
- `PropertiesController.cs` — Added search, minBedrooms, minBathrooms, minArea, maxArea, amenity query params to List endpoint
- `PropertyReadRepository.cs` — Implemented ILIKE search on title/city/province/street, advanced feature/amenity filters, relevance-based sorting

### Frontend
- `AdvancedFilterPanel.tsx` — Expandable panel with multi-select, ranges, toggles, active count badge, clear all

## Test Coverage
- AdvancedFilterPanel: 5 tests (toggle, badge, multi-select, clear)
- Backend: 257 tests pass (251 unit + 5 architecture + 1 integration)
- Frontend: 610 tests pass across 71 files
