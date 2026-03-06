# Walkthrough: 0036-frontend-contacts-ui

## Task Reference
- Task: `docs/tasks/0036-frontend-contacts-ui.md`
- Walkthrough: `docs/walkthroughs/0036-frontend-contacts-ui.md`
- Branch/PR: `feat/p4-contacts-leads`
- Date: `2026-03-06`

## Summary
Implemented the frontend Contacts UI in the Next.js web app: contacts list page with search, role filter, and pagination; contact detail page with property interests; create/edit contact form; and all supporting components (ContactsTable, ContactRoleBadge, ContactForm, ContactDetail, PropertyInterestsList). Backed by 5 SWR data-fetching hooks and fully i18n-supported (en/es). Added Contacts navigation link to AppHeader.

## Context
- Background: The contacts-api REST endpoints (task 0033) were complete. Agents needed a frontend interface to manage contacts within the web application.
- Problem statement: No frontend existed for contacts management. Agents needed list, detail, create, edit, and delete views.
- Constraints: SWR-based data-fetching (consistent with existing pattern), TDD, i18n (en/es), Stitch design compliance, responsive layout.

## Decisions & Trade-offs
- **Decision: SWR hooks for contacts data fetching**
  - Options considered: React Query, SWR, custom fetch hooks
  - Why this choice: SWR already established as the project's data-fetching library (properties hooks, work-items hooks). Consistency across the codebase.
  - Consequences: All hooks in separate files (useContacts, useContact, useCreateContact, useUpdateContact, useDeleteContact) for clear import paths.

- **Decision: Separate hooks per mutation (useCreateContact, useUpdateContact, useDeleteContact)**
  - Options considered: (1) Single useContactMutations hook, (2) Separate hooks
  - Why this choice: Separate hooks are simpler to test, import, and compose. Each page only imports the mutations it needs.
  - Consequences: More files but cleaner dependency graph.

- **Decision: ContactRoleBadge with semantic colors per role**
  - Color mapping: Buyer=blue, Seller=green, Tenant=purple, Landlord=amber, Professional=slate
  - Why: Consistent with PropertyStatusBadge pattern, enables quick visual identification of contact roles.

## Implementation Notes
- Key changes:
  - contactsApiFetch added to api.ts for cross-origin fetch to contacts-api (port 5050)
  - 5 SWR hooks: useContacts (list + pagination + search + role filter), useContact (by ID), useCreateContact, useUpdateContact, useDeleteContact
  - ContactsTable with columns: Name, Email, Phone, Roles, Created; action menu per row (view, edit, delete)
  - ContactForm shared between create and edit; validates firstName, lastName, email, at least one role
  - ContactDetail displays all fields in card layout with editable sections
  - PropertyInterestsList shows linked properties with interest type badges and links to property detail
  - ContactRoleBadge renders color-coded pills per role
  - Contacts list page with search (debounced 300ms), role filter dropdown, pagination (10/20/50), empty state
  - Contact detail page with property interests section
  - Contacts nav link added to AppHeader
  - i18n keys added for en and es
- Edge cases handled:
  - Empty state when no contacts exist with "Create your first contact" CTA
  - Loading skeletons during data fetch
  - Delete confirmation dialog before soft-delete
  - Form validation with inline error messages
- Known limitations:
  - No cursor-based pagination (offset-based only, consistent with properties)
  - No contact photo/avatar

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
- `apps/web/src/components/contacts/ContactsTable.tsx` -- Paginated table with action menus
- `apps/web/src/components/contacts/ContactForm.tsx` -- Create/edit form with validation
- `apps/web/src/components/contacts/ContactRoleBadge.tsx` -- Color-coded role pills
- `apps/web/src/components/contacts/ContactDetail.tsx` -- Full contact card layout
- `apps/web/src/components/contacts/PropertyInterestsList.tsx` -- Property interests with type badges

**Created (Hooks):**
- `apps/web/src/hooks/useContacts.ts` -- List with pagination, search, role filter
- `apps/web/src/hooks/useContact.ts` -- Single contact by ID
- `apps/web/src/hooks/useCreateContact.ts` -- Create mutation
- `apps/web/src/hooks/useUpdateContact.ts` -- Update mutation
- `apps/web/src/hooks/useDeleteContact.ts` -- Delete mutation

**Created (Pages):**
- `apps/web/src/app/[locale]/(dashboard)/contacts/page.tsx` -- Contacts list page
- `apps/web/src/app/[locale]/(dashboard)/contacts/[id]/page.tsx` -- Contact detail page

**Created (Tests):**
- `apps/web/src/components/contacts/__tests__/ContactRoleBadge.test.tsx` -- Role badge rendering tests
- `apps/web/src/components/contacts/__tests__/ContactsTable.test.tsx` -- Table rendering and interaction tests

**Modified:**
- `apps/web/src/lib/api.ts` -- Added contactsApiFetch for contacts-api
- `apps/web/messages/en.json` -- Added contacts i18n keys
- `apps/web/messages/es.json` -- Added contacts i18n keys
- `apps/web/src/components/layout/AppHeader.tsx` -- Added Contacts navigation link

## Tests
### Unit
- What was added/updated:
  - ContactRoleBadge: tests for each role (Buyer, Seller, Tenant, Landlord, Professional) with correct CSS classes and text
  - ContactsTable: tests for row rendering, empty state, action menu visibility
- How to run: `cd apps/web && npx vitest run --filter "Contact"`

### Integration
- What was added/updated: N/A (API integration via SWR mocking)
- How to run: N/A

### Manual
- What you verified: Full CRUD flow -- create contact, view in list, open detail, edit, delete with confirmation
- Steps: Start dev server, navigate to /contacts, create a contact, verify list/detail/edit/delete flows

## Observability
- Logs added/updated: SWR error handler logs failed API calls to console
- Traces/metrics added/updated: N/A (frontend-only)

## Security
- Validation: Form validation for required fields (firstName, lastName, email, roles)
- AuthN/AuthZ impact: Authenticated fetch with tenant context headers
- Sensitive data handling: No PII stored in browser storage; contact data displayed only within authenticated context

## Follow-ups / Backlog
- [x] Task 0037: Frontend Lead Pipeline (adds Leads nav link alongside Contacts)

## Checklist
- [x] Task scope matches `docs/tasks/0036-frontend-contacts-ui.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
