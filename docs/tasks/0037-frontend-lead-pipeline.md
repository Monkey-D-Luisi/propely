# Task: 0037-frontend-lead-pipeline

## Metadata
- ID: 0037
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-03-06
- Related docs:
  - Walkthrough: `docs/walkthroughs/0037-frontend-lead-pipeline.md`
  - Epic: `docs/backlog/epic-P4-contacts-leads.md` (Task 4.7)

## Goal
Implement the frontend Lead Pipeline page in the Next.js web app with dual views (kanban board and list), lead detail panel, conversion modal for Qualified leads, filters, and all supporting components. Adds Contacts and Leads navigation links to AppHeader.

## Context
The contacts-api REST endpoints (task 0033) and lead conversion endpoint (task 0034) are complete. The Contacts UI (task 0036) is also done. This task creates the lead management interface where agents track leads through the pipeline (New -> Contacted -> Qualified -> Converted/Lost), with both kanban and list views. The kanban view enables visual pipeline management, while the conversion modal lets agents convert Qualified leads into Contacts.

## Scope
### In scope
- Lead pipeline page at `/[locale]/(dashboard)/leads` with view toggle (kanban/list)
- LeadKanbanBoard component with 5 status columns: New, Contacted, Qualified, Converted, Lost
- LeadKanbanCard component showing lead name, property reference, source, assigned agent, age
- LeadListView component with sortable table: Name, Email, Property, Status, Source, Agent, Created
- LeadDetailPanel (slide-over) showing all lead fields with property and contact links
- LeadConversionModal triggered from Qualified leads with contact preview and role selector
- LeadFilters component with property, agent, source, date range filters
- LeadStatusBadge component with semantic color coding per status
- SWR hooks: useLeads, useLead, useChangeLeadStatus, useConvertLead
- Filters persisted in URL query params for shareable links
- Drag-and-drop between kanban columns to change lead status (with transition validation)
- Invalid transitions show error toast and revert card position
- Empty state per column
- Loading skeletons
- i18n support (en, es)
- Navigation: Contacts + Leads links added to AppHeader
- Tests: LeadStatusBadge.test.tsx, LeadKanbanBoard.test.tsx, LeadListView.test.tsx, LeadConversionModal.test.tsx

### Out of scope
- Lead scoring
- Automated lead assignment
- Email integration
- Drag-and-drop library installation (assumes @dnd-kit/core already available or shimmed)

## Requirements
- R1: Kanban view shows 5 columns matching LeadStatus enum
- R2: Lead cards display: name, property short reference, source badge, assigned agent initials, days since creation
- R3: Drag-and-drop triggers status change API call; invalid transitions show error and revert
- R4: List view shows sortable table with all key lead fields
- R5: Detail panel shows all lead fields with links to property and contact (if converted)
- R6: Convert button appears only on Qualified leads
- R7: Conversion modal shows contact preview, role selector, notes field
- R8: Filters persist in URL query params
- R9: i18n for all user-visible text (en, es)

## Acceptance Criteria
- AC1: Lead pipeline page has toggle between Kanban and List views
- AC2: Kanban view shows 5 columns: New, Contacted, Qualified, Converted, Lost
- AC3: Lead cards display: name, property short reference, source badge, assigned agent initials, days since creation
- AC4: Drag-and-drop between columns triggers status change API call
- AC5: Invalid transitions (e.g., Converted -> New) show error toast and revert card position
- AC6: List view shows sortable table with columns: Name, Email, Property, Status, Source, Agent, Created
- AC7: Clicking a lead opens detail panel (slide-over)
- AC8: Detail panel shows all lead fields with links to property and contact (if converted)
- AC9: "Convert" button appears only on Qualified leads
- AC10: Conversion modal shows contact preview, role selector, notes field
- AC11: After successful conversion, lead card moves to Converted column and shows contact link
- AC12: Filters: property dropdown, agent dropdown, source text input, date range
- AC13: Filters persist in URL query params
- AC14: Empty state per column ("No leads in this status")
- AC15: Loading skeletons for initial load
- AC16: Contacts + Leads navigation links added to AppHeader
- AC17: All tests pass: LeadStatusBadge.test.tsx, LeadKanbanBoard.test.tsx, LeadListView.test.tsx, LeadConversionModal.test.tsx
- AC18: i18n translations for en and es

## Constraints (non-negotiable)
- Clean Architecture layers respected.
- English-only repo content.
- No secrets in repo.
- Update walkthrough.
- TDD: write component tests first.
- Pixel-perfect Stitch design compliance.

## Proposed Approach (high-level)
1. Create SWR hooks for leads data fetching and mutations
2. Add i18n translations for leads
3. Build individual components (LeadStatusBadge, LeadKanbanBoard, LeadKanbanCard, LeadListView, LeadDetailPanel, LeadConversionModal, LeadFilters) with TDD
4. Assemble the lead pipeline page with view toggle, filters, and URL param persistence
5. Add Contacts + Leads navigation links to AppHeader

## Implementation Steps
1. Create SWR hooks: useLeads (list + filters + pagination), useLead (by ID), useChangeLeadStatus (mutation), useConvertLead (mutation)
2. Add i18n translations to en.json and es.json
3. Create LeadStatusBadge component with color coding and tests
4. Create LeadKanbanCard component displaying lead summary
5. Create LeadKanbanBoard component with 5 status columns and drag-and-drop
6. Create LeadListView component with sortable table
7. Create LeadDetailPanel component (slide-over) with all fields and links
8. Create LeadConversionModal with contact preview and role selector
9. Create LeadFilters component with property, agent, source, date range filters
10. Create leads pipeline page with view toggle, filters, URL param persistence
11. Add Contacts + Leads navigation links to AppHeader
12. Write tests: LeadStatusBadge, LeadKanbanBoard, LeadListView, LeadConversionModal

## Files to Create / Modify
**Create:**
- `apps/web/src/app/[locale]/(dashboard)/leads/page.tsx`
- `apps/web/src/components/leads/LeadKanbanBoard.tsx`
- `apps/web/src/components/leads/LeadKanbanCard.tsx`
- `apps/web/src/components/leads/LeadListView.tsx`
- `apps/web/src/components/leads/LeadDetailPanel.tsx`
- `apps/web/src/components/leads/LeadConversionModal.tsx`
- `apps/web/src/components/leads/LeadFilters.tsx`
- `apps/web/src/components/leads/LeadStatusBadge.tsx`
- `apps/web/src/hooks/useLeads.ts`
- `apps/web/src/hooks/useLead.ts`
- `apps/web/src/hooks/useChangeLeadStatus.ts`
- `apps/web/src/hooks/useConvertLead.ts`
- `apps/web/src/components/leads/__tests__/LeadStatusBadge.test.tsx`
- `apps/web/src/components/leads/__tests__/LeadKanbanBoard.test.tsx`
- `apps/web/src/components/leads/__tests__/LeadListView.test.tsx`
- `apps/web/src/components/leads/__tests__/LeadConversionModal.test.tsx`

**Modify:**
- `apps/web/messages/en.json` -- Add leads i18n keys
- `apps/web/messages/es.json` -- Add leads i18n keys
- `apps/web/src/components/layout/AppHeader.tsx` -- Add Contacts + Leads navigation links

## Testing Plan
- Unit tests:
  - LeadStatusBadge: renders correct color and text for each status (New, Contacted, Qualified, Converted, Lost)
  - LeadKanbanBoard: renders 5 columns, renders lead cards in correct columns
  - LeadListView: renders sortable table with all columns, handles empty state
  - LeadConversionModal: renders contact preview, submits with role and notes, handles cancel
- Integration tests: N/A (API integration via SWR mocking)
- Manual verification: Full kanban drag-and-drop flow, lead conversion end-to-end, visual comparison against Stitch designs

## Security & Privacy
- Lead data displayed only within authenticated, tenant-scoped context
- Conversion modal handles PII creation; no extra client-side storage
- URL query params contain only IDs and enum values (no PII)
- All API calls use authenticated fetch with tenant context headers

## Observability
- Logs: SWR error handler logs failed API calls to console
- Metrics: N/A (frontend-only)
- Traces: N/A (frontend-only)

## Rollback Plan
Revert the commit. No database migrations or backend changes involved.

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
