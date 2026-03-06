# Walkthrough: 0022-property-detail-view

## Task Reference
- Task: `docs/tasks/0022-property-detail-view.md`
- Walkthrough: `docs/walkthroughs/0022-property-detail-view.md`
- Branch/PR: `feat/p2-properties-core`
- Date: `2026-03-05`

## Summary
Built the Property Detail View page with a photo gallery featuring lightbox with keyboard navigation, structured info sections (Basic, Location, Features, Financial, Descriptions), status action buttons driven by a state machine transition map, a status change confirmation dialog, and a floor plans viewer. The page includes breadcrumb navigation and graceful loading/error states.

## Context
- Background: Backend CRUD API and media management were complete. Data-fetching hooks were available from Task 0020. Agents needed a comprehensive view of all property data and the ability to manage status transitions.
- Problem statement: Agents need to view full property details, browse photos, and manage property lifecycle (status transitions) from a single detail page.
- Constraints: Status transitions must mirror backend domain rules exactly. TDD mandatory. Keyboard accessibility required for lightbox.

## Decisions & Trade-offs
- **Decision: Static transition map mirroring backend domain rules**
  - Options considered: Fetch valid transitions from API, hardcode transition map on frontend
  - Why this choice: Hardcoded map is simpler and faster. The backend validates transitions authoritatively anyway. The frontend map is only for UX (showing/hiding buttons).
  - Consequences / risks: If backend transition rules change, frontend map must be updated manually. Acceptable for MVP.

- **Decision: Confirmation dialog for all status changes**
  - Options considered: Inline confirmation, modal dialog, no confirmation
  - Why this choice: Status changes are significant business actions (e.g. marking as Sold). A modal dialog prevents accidental state changes and provides a clear confirmation step.
  - Consequences / risks: Extra click for agents, but safety is worth it for irreversible-like transitions.

- **Decision: Keyboard-navigable lightbox**
  - Options considered: Third-party lightbox library, custom implementation
  - Why this choice: Custom implementation is lightweight and gives full control over keyboard handling. No additional dependency needed.
  - Consequences / risks: Must handle edge cases (first/last image wrapping) and cleanup (event listeners) correctly.

- **Decision: Separate FloorPlans component**
  - Options considered: Include floor plans in photo gallery, standalone viewer
  - Why this choice: Floor plans are a different media type with different display needs (may be PDFs or technical drawings). Separating them from the photo gallery gives a cleaner UX.
  - Consequences / risks: Additional component to maintain, but cleaner separation of concerns.

## Implementation Notes
- Key changes:
  - PropertyPhotoGallery: Grid of thumbnails, click to open lightbox overlay. Lightbox shows full-size image with prev/next arrows. useEffect keydown listener handles Escape (close), ArrowLeft (prev), ArrowRight (next). Manages open state and current image index internally.
  - PropertyInfoSections: 5 collapsible sections. Features section renders amenity badges (colored pills for pool, garden, garage, elevator, terrace). Descriptions section has DescriptionTabs sub-component rendering tabs for each language with non-empty content.
  - PropertyStatusActions: Uses static transition map: Draft -> Active/Archived, Active -> Reserved/Sold/Rented/Archived, Reserved -> Active/Sold/Rented. Sold/Rented/Archived are terminal (no buttons). Renders current status badge + transition buttons.
  - StatusChangeDialog: Modal with title, message, confirm/cancel buttons. Calls status change mutation on confirm.
  - PropertyFloorPlans: Renders floor plan media items in a grid. Shows empty state when no floor plans available.
  - PropertyDetailPage: Container that fetches property data via useProperty hook, handles loading/error states, renders breadcrumb + all sub-components.
- Edge cases handled: Empty photo gallery, no floor plans, terminal status (no transition buttons), language tabs only for non-empty descriptions, lightbox at first/last image boundaries
- Known limitations: Lightbox does not support zoom or swipe gestures (desktop keyboard navigation only)

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

### Components
- `apps/web/src/components/properties/detail/PropertyDetailPage.tsx` -- Container with data fetching, loading/error, breadcrumb
- `apps/web/src/components/properties/detail/PropertyPhotoGallery.tsx` -- Grid thumbnails + lightbox overlay with keyboard navigation
- `apps/web/src/components/properties/detail/PropertyInfoSections.tsx` -- All 5 info sections with amenity badges and description language tabs
- `apps/web/src/components/properties/detail/PropertyStatusActions.tsx` -- Status badge + state-machine-driven transition action buttons
- `apps/web/src/components/properties/detail/StatusChangeDialog.tsx` -- Confirmation modal for status changes
- `apps/web/src/components/properties/detail/PropertyFloorPlans.tsx` -- Floor plan media viewer with empty state

### Pages
- `apps/web/src/app/[locale]/properties/[id]/page.tsx` -- Detail page route with breadcrumb navigation

## Tests
### Unit
- What was added/updated:
  - PropertyPhotoGallery: 7 tests (render grid, lightbox open/close, keyboard navigation Escape/ArrowLeft/ArrowRight, boundary handling)
  - PropertyInfoSections: 13 tests (all 5 sections render correct data, amenity badge rendering, description language tabs switching, empty descriptions handling)
  - PropertyStatusActions: 9 tests (status badge display, transition buttons per state: Draft/Active/Reserved/terminal, click handlers fire with correct status)
  - StatusChangeDialog: tests for open/close, confirm triggers mutation, cancel closes dialog
  - PropertyFloorPlans: tests for rendering floor plans, empty state display
  - PropertyDetailPage: 8 tests (page render with data, loading spinner, error state, breadcrumb navigation, edit link)
  - **Total: 37 tests, all passing**
- How to run: `cd apps/web && npx vitest run`

### Integration
- What was added/updated: N/A
- How to run: N/A

### Manual
- What you verified: Detail page renders all sections, lightbox opens and navigates with keyboard, description tabs switch languages, status buttons show valid transitions, confirmation dialog works, floor plans display
- Steps: Navigate to /properties/[id], verify all sections, open lightbox with keyboard nav, test status transition with confirmation

## Observability
- Logs added/updated: Console errors for failed property fetch or status change API calls
- Traces/metrics added/updated: N/A (frontend-only)

## Security
- Validation: Status transitions validated on backend; frontend only shows UX-appropriate buttons
- AuthN/AuthZ impact: Property data fetched through authenticated API client with tenant context
- Sensitive data handling: No secrets or PII beyond tenant-scoped property data

## Follow-ups / Backlog
- [x] Advanced search filters (Task 0023)
- [ ] Mobile swipe gestures for lightbox (future enhancement)
- [ ] Lightbox zoom functionality (future enhancement)

## Checklist
- [x] Task scope matches `docs/tasks/0022-property-detail-view.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
