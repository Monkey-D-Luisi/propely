# Walkthrough: 0016-property-detail-view

## Summary
Implements the Property Detail View page with photo gallery (lightbox), structured info sections, description language tabs, and status management actions.

## Architecture Decisions

### Component Decomposition
The detail page is split into focused components:
- `PropertyPhotoGallery` — Pure presentation, handles image grid and lightbox with keyboard navigation
- `PropertyInfoSections` — Renders all property data in organized sections (Basic Info, Location, Features, Financial, Descriptions)
- `PropertyStatusActions` — Shows current status badge and available transition buttons

This decomposition allows each piece to be tested independently and reused if needed.

### Lightbox with Keyboard Navigation
The lightbox overlay supports Escape (close), ArrowLeft (previous), ArrowRight (next) via a useEffect keydown listener. The gallery component manages open/closed state and the current image index internally.

### Description Language Tabs
`PropertyInfoSections` contains a `DescriptionTabs` sub-component that renders tabs for each language that has content in the `LocalizedText` object (es, en, fr, pt, de, nl). Only languages with non-empty descriptions are shown as tabs.

### Status Transitions
`PropertyStatusActions` uses a static transition map mirroring the backend domain rules:
- Draft → Active, Archived
- Active → Reserved, Sold, Rented, Archived
- Reserved → Active, Sold, Rented
- Sold, Rented, Archived → terminal (no buttons)

## Files Changed

### Components
- `PropertyPhotoGallery.tsx` — Grid thumbnails + lightbox overlay with keyboard navigation
- `PropertyInfoSections.tsx` — All info sections with amenity badges and description tabs
- `PropertyStatusActions.tsx` — Status badge + transition action buttons

### Pages
- `apps/web/src/app/[locale]/properties/[id]/page.tsx` — Detail page with breadcrumb, gallery, info sections, status actions

## Test Coverage
- PropertyPhotoGallery: 7 tests (render, lightbox open/close, keyboard nav)
- PropertyInfoSections: 13 tests (all sections, amenity badges, description tabs)
- PropertyStatusActions: 9 tests (status badge, transition buttons, callbacks)
- PropertyDetailPage: 8 tests (page render, loading, error, breadcrumb)
- **Total: 37 tests, all passing**
