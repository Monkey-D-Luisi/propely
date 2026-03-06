# Task: 0034-lead-conversion-flow

## Metadata
- ID: 0034
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-03-06
- Related docs:
  - Walkthrough: `docs/walkthroughs/0034-lead-conversion-flow.md`
  - Epic: `docs/backlog/epic-P4-contacts-leads.md` (Task 4.4)

## Goal
Implement the lead conversion flow that converts a Qualified lead into a Contact (create new or merge with existing), exposed via a dedicated API endpoint. This completes the lead lifecycle from inquiry to contact.

## Context
The contacts and leads persistence and API (task 0033) are complete. Lead conversion is the critical business workflow that bridges the lead pipeline and the contacts CRM. When a lead reaches Qualified status, an agent can convert it into a Contact -- either creating a new Contact or merging with an existing one that shares the same email. This operation is atomic and must handle both paths within a single transaction.

## Scope
### In scope
- `ConvertLeadCommand` MediatR command in Application layer
- `ConvertLeadCommandHandler` orchestrating the conversion logic
- `ConvertLeadCommandValidator` for input validation
- `ConvertLeadRequest` and `ConvertLeadResponse` DTOs
- Conversion logic: check for existing contact by email within tenant, create new or merge
- New contact creation: uses lead data (name, email, phone) with specified role
- Merge with existing: adds property interest to existing contact, links lead
- `Lead.Convert(contactId)` sets status to Converted, sets ContactId, emits `LeadConvertedV1`
- API endpoint: `POST /api/leads/{id}/convert`
- Atomic transaction via IUnitOfWork
- 6 handler unit tests covering both conversion paths and error cases

### Out of scope
- Automatic conversion (always user-initiated)
- Bulk conversion of multiple leads
- Undo conversion (Converted is a terminal status)
- Frontend conversion UI (task 0037)

## Requirements
- R1: Only leads in Qualified status can be converted
- R2: If contact with same email exists in tenant, merge (add property interest, link lead)
- R3: If no matching contact, create new Contact from lead data with specified role
- R4: Lead status changes to Converted and ContactId is set
- R5: LeadConvertedV1 event emitted with LeadId, ContactId, PropertyId, WasNewContact flag
- R6: Entire operation is atomic (single database transaction)
- R7: Response includes both the updated lead and the contact (created or existing)

## Acceptance Criteria
- AC1: `POST /api/leads/{id}/convert` accepts optional `{ "role": "Buyer", "notes": "..." }`
- AC2: When no contact exists with the lead's email: creates new Contact with lead data and specified role
- AC3: When contact exists with same email: links lead to existing contact, adds property interest
- AC4: Lead status changes to Converted and ContactId is set
- AC5: `LeadConvertedV1` event emitted with `LeadId`, `ContactId`, `PropertyId`, `WasNewContact`
- AC6: Attempting to convert a lead not in Qualified status returns 400
- AC7: Attempting to convert an already-converted lead returns 409
- AC8: Entire operation runs in a single database transaction (atomic)
- AC9: Response includes both the updated lead and the contact
- AC10: Authorization: only admin or assigned agent can convert
- AC11: 6 handler unit tests pass

## Constraints (non-negotiable)
- Clean Architecture layers respected.
- English-only repo content.
- No secrets in repo.
- Update walkthrough.
- TDD: write tests for both conversion paths first.

## Proposed Approach (high-level)
Create the ConvertLeadCommand with a handler that checks for existing contacts by email, then either creates a new Contact or adds a property interest to the existing one. The handler calls `Lead.Convert(contactId)` and saves within a single UnitOfWork transaction. Write tests for both paths (new contact, merge) before implementing.

## Implementation Steps
1. Create `ConvertLeadCommand` record in `Application/Leads/Commands/ConvertLead/`
2. Create `ConvertLeadCommandValidator` (lead must exist, lead must be Qualified)
3. Create `ConvertLeadCommandHandler` with conversion logic
4. In handler: check for existing contact by email within tenant via `IContactReadRepository`
5. If contact exists: add property interest to existing contact, link lead
6. If not exists: create new Contact from lead data with specified role (default Buyer)
7. Call `Lead.Convert(contactId)` to update status and emit `LeadConvertedV1`
8. Save within `IUnitOfWork` transaction
9. Create `ConvertLeadRequest` DTO (role, notes)
10. Create `ConvertLeadResponse` DTO (lead details + contact details + wasNewContact flag)
11. Add `POST /api/leads/{id}/convert` endpoint to LeadsController
12. Write 6 unit tests for handler (new contact, merge, invalid status, already converted, not found, unauthorized)

## Files to Create / Modify
**Create:**
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Commands/ConvertLead/ConvertLeadCommand.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Commands/ConvertLead/ConvertLeadCommandHandler.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Commands/ConvertLead/ConvertLeadCommandValidator.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Dtos/ConvertLeadRequest.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Dtos/ConvertLeadResponse.cs`
- `services/contacts-api/tests/Propely.ContactsApi.UnitTests/Application/Leads/Commands/ConvertLeadCommandHandlerTests.cs`

**Modify:**
- `services/contacts-api/src/Propely.ContactsApi.Api/Controllers/LeadsController.cs` -- Add convert endpoint

## Testing Plan
- Unit tests:
  - Convert lead with no existing contact creates new Contact with correct role and data
  - Convert lead with existing contact merges and adds property interest
  - Convert lead not in Qualified status throws validation error
  - Convert already-converted lead throws conflict error
  - Convert non-existent lead throws not found
  - Transaction rollback on failure
- Integration tests: N/A (covered by existing endpoint test patterns)
- Manual verification: Convert lead via API, verify contact and lead state in database

## Security & Privacy
- Conversion creates or modifies Contact PII; must be within same tenant
- Merge logic must not expose data from other tenants
- Event `LeadConvertedV1` contains only IDs, not PII
- Authorization: only admin or assigned agent can convert

## Observability
- Logs: Conversion events logged via MediatR pipeline
- Metrics: N/A
- Traces: OpenTelemetry auto-instrumentation for the transaction

## Rollback Plan
Revert the commit. No new database migrations involved (uses existing schema from task 0033).

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass (6 handler tests)
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
