# Task: 0036-frontend-contacts-ui

## Metadata
- ID: 0036
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-03-06
- Related docs:
  - Walkthrough: `docs/walkthroughs/0036-frontend-contacts-ui.md`
  - Epic: `docs/backlog/epic-P4-contacts-leads.md` (Task 4.6)

## Goal
Implement the frontend Contacts UI in the Next.js web app, including the contacts list page with search, filter, and pagination, the contact detail page with property interests, and create/edit/delete contact functionality. All components backed by SWR data-fetching hooks.

## Context
The contacts-api REST endpoints (task 0033) are complete and provide CRUD operations for contacts. This task creates the frontend interface for agents to browse, create, edit, view, and delete contacts. It follows the same patterns established by the Property List UI (task 0020) -- SWR hooks, Vitest + RTL tests, i18n support, and Stitch design compliance.

## Scope
### In scope
- Contacts list page at `/[locale]/(dashboard)/contacts` with paginated table
- Contact detail page at `/[locale]/(dashboard)/contacts/[id]` with property interests section
- ContactsTable component with columns: Name, Email, Phone, Roles, Created
- ContactForm component for create and edit (shared between both flows)
- ContactRoleBadge component with color coding per role
- ContactDetail component displaying all contact fields
- PropertyInterestsList component showing linked properties with interest types
- SWR hooks: useContacts, useContact, useCreateContact, useUpdateContact, useDeleteContact
- Search bar with debounced filtering (300ms)
- Role filter dropdown
- Pagination controls with page size selector (10, 20, 50)
- Empty state with call-to-action when no contacts exist
- Loading skeletons during data fetch
- Delete with confirmation dialog
- i18n support (en, es) for all contacts-related text
- Tests: ContactRoleBadge.test.tsx, ContactsTable.test.tsx

### Out of scope
- Lead management UI (task 0037)
- Bulk import/export
- Contact photo/avatar
- Cursor-based pagination

## Requirements
- R1: ContactsTable renders sortable columns: Name, Email, Phone, Roles, Created
- R2: Search bar filters by name or email with 300ms debounce
- R3: Role filter dropdown allows selecting one or more roles (Buyer, Seller, Tenant, Landlord, Professional)
- R4: Pagination with page size selector (10, 20, 50)
- R5: ContactForm validates required fields (firstName, lastName, email, at least one role)
- R6: ContactDetail shows all fields in a clean card layout with property interests section
- R7: ContactRoleBadge displays semantic colors per role
- R8: All data fetching via SWR with contacts-api fetch client
- R9: i18n for all user-visible text (en, es)

## Acceptance Criteria
- AC1: Contacts list page displays paginated table with Name, Email, Phone, Roles, Created columns
- AC2: Search bar filters contacts by name or email (debounced, 300ms)
- AC3: Role filter dropdown filters by selected roles
- AC4: Pagination controls navigate between pages with page size selector
- AC5: "New Contact" button opens creation form
- AC6: Creation form includes: first name, last name, email, phone, secondary phone, company, notes, roles (multi-select), preferred language
- AC7: Form validation matches backend rules (required fields, email format)
- AC8: Clicking a contact row navigates to detail page
- AC9: Detail page shows all contact fields in a clean card layout
- AC10: Detail page shows "Property Interests" section listing linked properties with interest type
- AC11: Edit button opens edit form pre-filled with current data
- AC12: Delete button with confirmation dialog soft-deletes the contact
- AC13: Empty state when no contacts exist with call-to-action
- AC14: Loading skeletons during data fetch
- AC15: ContactRoleBadge.test.tsx and ContactsTable.test.tsx pass
- AC16: i18n translations for en and es

## Constraints (non-negotiable)
- Clean Architecture layers respected.
- English-only repo content.
- No secrets in repo.
- Update walkthrough.
- TDD: write component tests first.
- Pixel-perfect Stitch design compliance.

## Proposed Approach (high-level)
1. Create SWR hooks as shared data-fetching infrastructure
2. Add i18n translations for contacts
3. Build individual components (ContactRoleBadge, ContactsTable, ContactForm, ContactDetail, PropertyInterestsList) with TDD
4. Assemble list and detail pages
5. Verify against Stitch designs

## Implementation Steps
1. Create contacts API fetch client (contactsApiFetch) in api.ts
2. Create SWR hooks: useContacts, useContact, useCreateContact, useUpdateContact, useDeleteContact
3. Add i18n translations to en.json and es.json
4. Create ContactRoleBadge component with color coding and tests
5. Create ContactsTable component with sortable columns, action menus, and tests
6. Create ContactForm component for create/edit with validation
7. Create ContactDetail component displaying all fields
8. Create PropertyInterestsList component showing linked properties
9. Create contacts list page with search, filter, pagination, empty state
10. Create contact detail page with property interests section
11. Add Contacts navigation link to AppHeader

## Files to Create / Modify
**Create:**
- `apps/web/src/app/[locale]/(dashboard)/contacts/page.tsx`
- `apps/web/src/app/[locale]/(dashboard)/contacts/[id]/page.tsx`
- `apps/web/src/components/contacts/ContactsTable.tsx`
- `apps/web/src/components/contacts/ContactForm.tsx`
- `apps/web/src/components/contacts/ContactRoleBadge.tsx`
- `apps/web/src/components/contacts/ContactDetail.tsx`
- `apps/web/src/components/contacts/PropertyInterestsList.tsx`
- `apps/web/src/hooks/useContacts.ts`
- `apps/web/src/hooks/useContact.ts`
- `apps/web/src/hooks/useCreateContact.ts`
- `apps/web/src/hooks/useUpdateContact.ts`
- `apps/web/src/hooks/useDeleteContact.ts`
- `apps/web/src/components/contacts/__tests__/ContactRoleBadge.test.tsx`
- `apps/web/src/components/contacts/__tests__/ContactsTable.test.tsx`

**Modify:**
- `apps/web/src/lib/api.ts` -- Add contactsApiFetch
- `apps/web/messages/en.json` -- Add contacts i18n keys
- `apps/web/messages/es.json` -- Add contacts i18n keys
- `apps/web/src/components/layout/AppHeader.tsx` -- Add Contacts navigation link

## Testing Plan
- Unit tests:
  - ContactRoleBadge: renders correct color and text for each role (Buyer, Seller, Tenant, Landlord, Professional)
  - ContactsTable: renders rows with correct data, handles empty state, renders action menus
- Integration tests: N/A (API integration via SWR hooks tested via mocking)
- Manual verification: Full CRUD flow through UI, visual comparison against Stitch designs

## Security & Privacy
- Contact data displayed only within authenticated, tenant-scoped context
- Delete confirmation prevents accidental data loss
- No PII stored in browser storage
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
