# Task: 0022-property-detail-view

## Metadata
- ID: 0022
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-03-05
- Related docs:
  - Walkthrough: `docs/walkthroughs/0022-property-detail-view.md`
  - Epic: `docs/backlog/epic-P2-properties.md` (Task 2.9)

## Goal
Implement the Property Detail View page with photo gallery (lightbox with keyboard navigation), structured info sections, status action buttons with state machine transitions, status change confirmation dialog, and floor plans viewer.

## Context
Tasks 2.3 and 2.5 built the Property CRUD API and media management. Task 0020 created the data-fetching hooks. This task creates the frontend detail page where agents can view all property data, browse photos in a lightbox, review floor plans, and manage property status transitions through a state-machine-aware UI.

## Scope
### In scope
- PropertyDetailPage container component with data fetching, loading/error states
- PropertyPhotoGallery with grid thumbnails and lightbox (keyboard navigation: Escape, ArrowLeft, ArrowRight)
- PropertyInfoSections: Basic Info, Location, Features (with amenity badges), Financial, Descriptions (with language tabs)
- PropertyStatusActions showing current status badge and valid transition buttons
- StatusChangeDialog for confirming status transitions
- PropertyFloorPlans viewer for floor plan documents/images
- Detail page route (`/[locale]/properties/[id]`) with breadcrumb navigation
- TDD test coverage (37 tests across all detail components)

### Out of scope
- Media upload/reorder (handled by media management API)
- Property editing from detail page (navigates to edit page)
- Print/export functionality

## Requirements
- R1: Photo gallery with grid layout showing property media
- R2: Lightbox overlay with full-size image, previous/next navigation, keyboard support (Escape, ArrowLeft, ArrowRight)
- R3: Info sections displaying all property data organized by category (Basic Info, Location, Features, Financial, Descriptions)
- R4: Features section shows amenity badges (pool, garden, garage, elevator, terrace)
- R5: Description tabs for multiple languages (es, en, fr, pt, de, nl) showing only languages with content
- R6: Status badge with valid transition action buttons based on state machine
- R7: Confirmation dialog before executing status changes
- R8: Floor plans viewer for associated floor plan media
- R9: Breadcrumb navigation (Properties > Property Title)

## Acceptance Criteria
- AC1: Gallery renders thumbnails in grid; clicking opens lightbox
- AC2: Lightbox supports keyboard navigation (Escape closes, arrows navigate)
- AC3: All info sections render with correct data
- AC4: Description tabs switch between languages with content
- AC5: Status actions show only valid transitions for current status
- AC6: Status change triggers API call after confirmation dialog
- AC7: Floor plans viewer displays floor plan media
- AC8: Breadcrumb navigation works correctly
- AC9: Loading and error states handled gracefully
- AC10: All 37 tests pass

## Constraints (non-negotiable)
- Clean Architecture layers respected.
- English-only repo content.
- No secrets in repo.
- Update walkthrough.

## Proposed Approach (high-level)
1. Build PropertyPhotoGallery with lightbox and keyboard navigation
2. Build PropertyInfoSections with all 5 categories and description tabs
3. Build PropertyStatusActions with state machine transition map
4. Build StatusChangeDialog for confirmation flow
5. Build PropertyFloorPlans viewer
6. Assemble PropertyDetailPage with breadcrumb, loading/error states
7. Create detail page route

## Implementation Steps
1. Create PropertyPhotoGallery component with grid and lightbox (tests)
2. Create PropertyInfoSections component with all 5 categories (tests)
3. Create PropertyStatusActions component with state machine map (tests)
4. Create StatusChangeDialog component (tests)
5. Create PropertyFloorPlans component (tests)
6. Create PropertyDetailPage container component (tests)
7. Create `/properties/[id]` page route

## Files to Create / Modify
### New files
- `apps/web/src/components/properties/detail/PropertyDetailPage.tsx`
- `apps/web/src/components/properties/detail/PropertyPhotoGallery.tsx`
- `apps/web/src/components/properties/detail/PropertyInfoSections.tsx`
- `apps/web/src/components/properties/detail/PropertyStatusActions.tsx`
- `apps/web/src/components/properties/detail/StatusChangeDialog.tsx`
- `apps/web/src/components/properties/detail/PropertyFloorPlans.tsx`
- `apps/web/src/app/[locale]/properties/[id]/page.tsx`
- Tests: PropertyPhotoGallery.test.tsx, PropertyInfoSections.test.tsx, PropertyStatusActions.test.tsx, StatusChangeDialog.test.tsx, PropertyFloorPlans.test.tsx, PropertyDetailPage.test.tsx

## Testing Plan
- Unit tests:
  - 7 PropertyPhotoGallery tests (render grid, lightbox open/close, keyboard navigation prev/next/escape)
  - 13 PropertyInfoSections tests (all 5 sections render, amenity badges, description language tabs)
  - 9 PropertyStatusActions tests (status badge display, transition buttons per state, click handlers)
  - StatusChangeDialog tests (open/close, confirm/cancel actions)
  - PropertyFloorPlans tests (render floor plans, empty state)
  - 8 PropertyDetailPage tests (page render, loading state, error state, breadcrumb)
  - Total: 37 tests
- Integration tests: N/A
- Manual verification: Browse property detail, open lightbox, navigate photos, switch description tabs, test status transitions with confirmation

## Security & Privacy
- Status transitions are validated on the backend; frontend buttons only show valid transitions
- Confirmation dialog prevents accidental status changes
- All data fetched through authenticated API client

## Observability
- Logs: Console errors for failed property fetch or status change API calls
- Metrics: N/A (frontend-only)
- Traces: N/A (frontend-only)

## Rollback Plan
Revert the commit on the `feat/p2-properties-core` branch. No database migrations involved.

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass (37 tests)
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
