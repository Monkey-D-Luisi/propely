# Walkthrough: 0037-frontend-lead-pipeline

## Task Reference
- Task: `docs/tasks/0037-frontend-lead-pipeline.md`
- Walkthrough: `docs/walkthroughs/0037-frontend-lead-pipeline.md`
- Branch/PR: `feat/p4-contacts-leads`
- Date: `2026-03-06`

## Summary
Implemented the frontend Lead Pipeline page with dual views (kanban board and list), lead detail panel, conversion modal for Qualified leads, filters with URL param persistence, drag-and-drop status changes, and all supporting components. Added Contacts and Leads navigation links to AppHeader. Backed by 4 SWR hooks and fully i18n-supported (en/es). This completes Epic P4 (Contacts & Leads).

## Context
- Background: The contacts-api endpoints (task 0033) and lead conversion (task 0034) were complete. The Contacts UI (task 0036) was done. This is the final task in Epic P4.
- Problem statement: Agents needed a visual pipeline interface to track and manage leads through the sales funnel, with drag-and-drop for quick status changes and a conversion workflow for Qualified leads.
- Constraints: SWR data-fetching, TDD, i18n (en/es), Stitch design compliance, drag-and-drop with transition validation, responsive layout.

## Decisions & Trade-offs
- **Decision: Kanban + List dual views with toggle**
  - Options considered: (1) Kanban only, (2) List only, (3) Both with toggle
  - Why this choice: Kanban provides visual pipeline management (ideal for small datasets and status tracking), while list view handles large datasets with sorting/filtering. Agents can choose their preferred workflow.
  - Consequences: Two view components to maintain, but reuse the same data hooks.

- **Decision: Drag-and-drop with transition validation and optimistic revert**
  - Options considered: (1) Drop always succeeds then validates server-side, (2) Validate on drop before API call
  - Why this choice: Client-side validation on drop prevents unnecessary API calls for known-invalid transitions (e.g., Converted -> New). If the API call fails, the card position reverts with an error toast.
  - Consequences: Client must know the valid transition map (duplicated from domain), but prevents wasted API calls.

- **Decision: LeadStatusBadge color mapping**
  - Color mapping: New=blue, Contacted=amber, Qualified=purple, Converted=green, Lost=slate
  - Why: Intuitive progression -- blue (new/fresh), amber (in progress), purple (qualified/special), green (success), slate (closed/lost).

- **Decision: Filters persisted in URL query params**
  - Why: Enables shareable links to filtered views. Agents can bookmark or share a specific pipeline view with colleagues.
  - Consequences: URL can get long with multiple filters, but no PII in params (only IDs and enum values).

- **Decision: Slide-over panel for lead detail instead of separate page**
  - Options considered: (1) Separate /leads/[id] page, (2) Slide-over panel on the pipeline page
  - Why this choice: Slide-over keeps the pipeline context visible while viewing lead details. Agents do not lose their place in the kanban/list view.
  - Consequences: More complex component state management, but better UX.

## Implementation Notes
- Key changes:
  - 4 SWR hooks: useLeads (list + filters + pagination), useLead (by ID), useChangeLeadStatus (mutation), useConvertLead (mutation)
  - LeadKanbanBoard with 5 columns, drag-and-drop via @dnd-kit, transition validation
  - LeadKanbanCard with lead summary (name, property ref, source, agent initials, age)
  - LeadListView with sortable table (Name, Email, Property, Status, Source, Agent, Created)
  - LeadDetailPanel (slide-over) with all fields, property link, contact link (if converted), convert button (if Qualified)
  - LeadConversionModal with contact preview (what will be created/merged), role selector, notes field
  - LeadFilters with property dropdown, agent dropdown, source input, date range
  - LeadStatusBadge with semantic colors per status
  - Leads pipeline page with view toggle, filters, URL param persistence, empty states
  - AppHeader updated with Contacts + Leads navigation links
  - i18n keys for en and es
- Edge cases handled:
  - Invalid drag-and-drop transitions show error toast and revert card position
  - Empty state per kanban column ("No leads in this status")
  - Loading skeletons for initial load
  - Conversion modal shows "will create new contact" or "will merge with existing" based on email match
  - Kanban scrolls horizontally on mobile
- Known limitations:
  - No real-time updates (polling on refocus via SWR, not WebSocket)
  - No drag-and-drop on mobile (touch events handled but limited by @dnd-kit mobile support)
  - No lead card reordering within a column (only cross-column moves)

## Data / Schema / Migrations
- DB changes: None (frontend-only task)
- Migration strategy: N/A
- Backward compatibility: N/A

## Commands Run
```bash
cd apps/web && npx tsc --noEmit
cd apps/web && npx vitest run
```

## Files Changed

**Created (Components):**
- `apps/web/src/components/leads/LeadKanbanBoard.tsx` -- 5-column kanban with drag-and-drop
- `apps/web/src/components/leads/LeadKanbanCard.tsx` -- Lead summary card for kanban
- `apps/web/src/components/leads/LeadListView.tsx` -- Sortable table view
- `apps/web/src/components/leads/LeadDetailPanel.tsx` -- Slide-over panel with full details
- `apps/web/src/components/leads/LeadConversionModal.tsx` -- Conversion workflow with role selector
- `apps/web/src/components/leads/LeadFilters.tsx` -- Filter controls
- `apps/web/src/components/leads/LeadStatusBadge.tsx` -- Color-coded status pills

**Created (Hooks):**
- `apps/web/src/hooks/useLeads.ts` -- List with filters and pagination
- `apps/web/src/hooks/useLead.ts` -- Single lead by ID
- `apps/web/src/hooks/useChangeLeadStatus.ts` -- Status change mutation
- `apps/web/src/hooks/useConvertLead.ts` -- Conversion mutation

**Created (Pages):**
- `apps/web/src/app/[locale]/(dashboard)/leads/page.tsx` -- Lead pipeline page with view toggle

**Created (Tests):**
- `apps/web/src/components/leads/__tests__/LeadStatusBadge.test.tsx` -- Status badge rendering tests
- `apps/web/src/components/leads/__tests__/LeadKanbanBoard.test.tsx` -- Kanban column and card rendering tests
- `apps/web/src/components/leads/__tests__/LeadListView.test.tsx` -- Table rendering and empty state tests
- `apps/web/src/components/leads/__tests__/LeadConversionModal.test.tsx` -- Modal rendering, submission, and cancel tests

**Modified:**
- `apps/web/messages/en.json` -- Added leads i18n keys
- `apps/web/messages/es.json` -- Added leads i18n keys
- `apps/web/src/components/layout/AppHeader.tsx` -- Added Contacts + Leads navigation links

## Tests
### Unit
- What was added/updated:
  - LeadStatusBadge: tests for each status (New, Contacted, Qualified, Converted, Lost) with correct CSS classes
  - LeadKanbanBoard: tests for 5 columns rendered, lead cards placed in correct columns
  - LeadListView: tests for table rendering with all columns, empty state handling
  - LeadConversionModal: tests for rendering contact preview, submitting with role and notes, cancel behavior
- How to run: `cd apps/web && npx vitest run --filter "Lead"`

### Integration
- What was added/updated: N/A (API integration via SWR mocking)
- How to run: N/A

### Manual
- What you verified: Full kanban drag-and-drop flow, lead conversion end-to-end, view toggle, filters, responsive layout
- Steps: Start dev server, navigate to /leads, test drag-and-drop, open detail panel, convert a qualified lead, verify kanban/list toggle and filters

## Observability
- Logs added/updated: SWR error handler logs failed API calls to console
- Traces/metrics added/updated: N/A (frontend-only)

## Security
- Validation: Drag-and-drop validates transitions client-side; server validates on API call
- AuthN/AuthZ impact: Authenticated fetch with tenant context headers
- Sensitive data handling: No PII in URL query params; lead data displayed only within authenticated context

## Follow-ups / Backlog
- [ ] Real-time lead updates via WebSocket (future enhancement)
- [ ] Drag-and-drop mobile improvements
- [ ] Lead card reordering within columns (priority ordering)

## Checklist
- [x] Task scope matches `docs/tasks/0037-frontend-lead-pipeline.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
