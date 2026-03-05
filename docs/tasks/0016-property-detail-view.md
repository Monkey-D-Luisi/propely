# Task: 0016-property-detail-view

## Metadata
- ID: 0016
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-03-05
- Related docs:
  - Walkthrough: `docs/walkthroughs/0016-property-detail-view.md`
  - Epic: `docs/backlog/epic-P2-properties.md` (Task 2.9)

## Goal
Implement the Property Detail View page displaying all property information including photo gallery with lightbox, structured info sections, description tabs, and status management actions.

## Context
Tasks 2.3 and 2.5 built the Property CRUD API and media management. This task creates the frontend detail page where agents can view all property data and manage property status transitions.

## Scope
### In scope
- PropertyPhotoGallery with grid thumbnails and lightbox (keyboard navigation)
- PropertyInfoSections (Basic Info, Location, Features with amenity badges, Financial, Descriptions with language tabs)
- PropertyStatusActions (current status badge + valid transition buttons)
- Detail page (/[locale]/properties/[id]) with breadcrumb navigation
- Status change confirmation dialog
- TDD test coverage

### Out of scope
- Media upload/reorder (handled by media management API)
- Property editing (Task 2.8)

## Requirements
- R1: Photo gallery with grid layout and lightbox overlay
- R2: Keyboard navigation in lightbox (Escape, ArrowLeft, ArrowRight)
- R3: Info sections displaying all property data organized by category
- R4: Description tabs for multiple languages (es, en, fr, pt, de, nl)
- R5: Status badge with valid transition action buttons
- R6: Confirmation dialog for status changes

## Acceptance Criteria
- AC1: Gallery renders thumbnails in grid; clicking opens lightbox
- AC2: Lightbox supports keyboard navigation
- AC3: All info sections render with correct data
- AC4: Description tabs switch between languages
- AC5: Status actions show only valid transitions for current status
- AC6: Status change triggers API call with confirmation
- AC7: All tests pass

## Files Created
- `apps/web/src/components/properties/detail/PropertyPhotoGallery.tsx`
- `apps/web/src/components/properties/detail/PropertyInfoSections.tsx`
- `apps/web/src/components/properties/detail/PropertyStatusActions.tsx`
- `apps/web/src/app/[locale]/properties/[id]/page.tsx`
- Tests: PropertyPhotoGallery.test.tsx, PropertyInfoSections.test.tsx, PropertyStatusActions.test.tsx, PropertyDetailPage.test.tsx

## Testing Plan
- 7 PropertyPhotoGallery tests (render, lightbox open/close, keyboard nav)
- 13 PropertyInfoSections tests (all sections render, amenity badges, description tabs)
- 9 PropertyStatusActions tests (status badge, transition buttons, confirmation)
- 8 PropertyDetailPage tests (page render, loading, error, breadcrumb)

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added and pass (37 tests)
- [x] No secrets committed
- [x] Walkthrough created
