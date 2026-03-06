# Walkthrough: 0034-lead-conversion-flow

## Task Reference
- Task: `docs/tasks/0034-lead-conversion-flow.md`
- Walkthrough: `docs/walkthroughs/0034-lead-conversion-flow.md`
- Branch/PR: `feat/p4-contacts-leads`
- Date: `2026-03-06`

## Summary
Implemented the lead conversion flow that converts a Qualified lead into a Contact, supporting both new contact creation and merge with existing contacts. The handler checks for an existing contact by email within the tenant, either creates a new Contact or adds a property interest to the existing one, then calls `Lead.Convert(contactId)` within a single atomic transaction. Exposed via `POST /api/leads/{id}/convert` endpoint with 6 unit tests.

## Context
- Background: The contacts and leads persistence and API (task 0033) were complete. Lead conversion is the key business workflow that bridges the lead pipeline and contacts CRM.
- Problem statement: When a lead reaches Qualified status, agents needed a way to convert it into a Contact -- either creating a new one or merging with an existing contact that shares the same email.
- Constraints: Atomic transaction, both conversion paths (new + merge) must be handled, TDD mandatory.

## Decisions & Trade-offs
- **Decision: Single handler for both conversion paths (new vs. merge)**
  - Options considered: (1) Separate commands for "convert to new" and "convert to existing", (2) Single command that auto-detects by email
  - Why this choice: Auto-detection by email is more user-friendly. The agent does not need to know whether a contact already exists -- the system figures it out.
  - Consequences: The handler has branching logic, but it is well-contained and well-tested.

- **Decision: Default role is Buyer if not specified**
  - Why: Most leads in real estate are buyers. Requiring a role in every conversion would add friction with minimal benefit.
  - Consequences: Optional `role` field in ConvertLeadRequest; defaults to `ContactRole.Buyer`.

- **Decision: WasNewContact flag in response and domain event**
  - Why: The caller (and downstream event consumers) need to know whether the conversion created a new contact or merged with an existing one, for UI feedback and analytics.

## Implementation Notes
- Key changes:
  - `ConvertLeadCommand(Guid LeadId, ContactRole? Role, string? Notes, Guid TenantId, Guid UserId)` MediatR command
  - `ConvertLeadCommandHandler` orchestrates: lookup lead, check email in contacts, create-or-merge, convert, save
  - `ConvertLeadCommandValidator` validates lead existence and Qualified status
  - `ConvertLeadResponse` includes LeadDto, ContactDto, and WasNewContact flag
  - `POST /api/leads/{id}/convert` endpoint added to LeadsController
- Edge cases handled:
  - Lead not in Qualified status returns 400 Bad Request
  - Lead already converted returns 409 Conflict
  - Lead not found returns 404 Not Found
  - Merge path adds property interest with InterestType.Buying (matching the lead's property context)
  - New contact path uses lead's Name (split into first/last), Email, Phone
- Known limitations:
  - Name splitting is basic (first token = first name, rest = last name). Complex names may not split correctly.
  - No undo conversion (by design -- Converted is terminal)

## Data / Schema / Migrations
- DB changes: None (uses existing schema from task 0033)
- Migration strategy: N/A
- Backward compatibility: N/A

## Commands Run
```bash
dotnet build services/contacts-api/Propely.ContactsApi.sln
dotnet test services/contacts-api/tests/Propely.ContactsApi.UnitTests/
```

## Files Changed

**Created:**
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Commands/ConvertLead/ConvertLeadCommand.cs` -- MediatR command record
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Commands/ConvertLead/ConvertLeadCommandHandler.cs` -- Conversion orchestration logic
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Commands/ConvertLead/ConvertLeadCommandValidator.cs` -- Input validation
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Dtos/ConvertLeadRequest.cs` -- API request DTO
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Dtos/ConvertLeadResponse.cs` -- API response DTO with WasNewContact flag
- `services/contacts-api/tests/Propely.ContactsApi.UnitTests/Application/Leads/Commands/ConvertLeadCommandHandlerTests.cs` -- 6 handler tests

**Modified:**
- `services/contacts-api/src/Propely.ContactsApi.Api/Controllers/LeadsController.cs` -- Added POST /api/leads/{id}/convert endpoint

## Tests
### Unit
- What was added/updated: 6 tests for ConvertLeadCommandHandler
  - Convert_WithNoExistingContact_CreatesNewContact
  - Convert_WithExistingContact_MergesAndAddsInterest
  - Convert_NotQualifiedStatus_ThrowsValidationException
  - Convert_AlreadyConverted_ThrowsConflictException
  - Convert_LeadNotFound_ThrowsNotFoundException
  - Convert_VerifiesAtomicTransaction
- How to run: `dotnet test services/contacts-api/tests/Propely.ContactsApi.UnitTests/ --filter "ConvertLead"`

### Integration
- What was added/updated: N/A
- How to run: N/A

### Manual
- What you verified: Full conversion flow via API -- create lead, qualify it, convert it, verify contact created
- Steps: POST lead, PUT status to Contacted, PUT status to Qualified, POST convert, verify response

## Observability
- Logs added/updated: MediatR pipeline logging for ConvertLeadCommand
- Traces/metrics added/updated: OpenTelemetry auto-instrumentation for the transaction

## Security
- Validation: Lead must exist, must be in Qualified status, must be within tenant
- AuthN/AuthZ impact: Only admin or assigned agent can convert; enforced at controller level
- Sensitive data handling: LeadConvertedV1 event contains only IDs (no PII)

## Follow-ups / Backlog
- [x] Task 0037: Frontend Lead Pipeline (conversion modal)

## Checklist
- [x] Task scope matches `docs/tasks/0034-lead-conversion-flow.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
