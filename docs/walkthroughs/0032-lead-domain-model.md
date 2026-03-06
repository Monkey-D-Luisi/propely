# Walkthrough: 0032-lead-domain-model

## Task Reference
- Task: `docs/tasks/0032-lead-domain-model.md`
- Walkthrough: `docs/walkthroughs/0032-lead-domain-model.md`
- Branch/PR: `feat/p4-contacts-leads`
- Date: `2026-03-06`

## Summary
Implemented the Lead entity in the contacts-api Domain layer with a strict status pipeline state machine (New -> Contacted -> Qualified -> Converted/Lost), domain events, conversion logic, and agent assignment. The implementation includes 47 unit tests with parameterized transition tests covering all 25 possible status transition combinations (valid and invalid paths).

## Context
- Background: The Contact aggregate root (task 0031) was complete. Leads represent potential clients interested in specific properties and need a strict pipeline workflow to track engagement.
- Problem statement: No Lead entity existed. The domain needed a state machine to enforce valid status transitions and support conversion to Contact.
- Constraints: Clean Architecture, TDD mandatory (parameterized tests first), ISoftDeletable pattern, PropertyId always required.

## Decisions & Trade-offs
- **Decision: Status transition map as a static dictionary**
  - Options considered: (1) Switch/case in ChangeStatus, (2) Static `Dictionary<LeadStatus, HashSet<LeadStatus>>` mapping, (3) State pattern with individual state classes
  - Why this choice: Dictionary map is concise, easy to read, and easy to test exhaustively with parameterized tests. State pattern would be over-engineered for 5 states.
  - Consequences: All valid transitions defined in one place; adding a new status requires updating the map.

- **Decision: Convert() as a separate method from ChangeStatus()**
  - Options considered: (1) `ChangeStatus(Converted)` with ContactId parameter, (2) Separate `Convert(contactId)` method
  - Why this choice: Conversion is a distinct business operation with its own validation (requires ContactId) and domain event (`LeadConvertedV1`). Separating it from generic status changes makes the API clearer.
  - Consequences: Two paths to Converted status are prevented -- `ChangeStatus(Converted)` throws; only `Convert(contactId)` is valid.

- **Decision: Deduplication as a static validation method**
  - Why: `Lead.IsDuplicate(email, propertyId)` provides domain-level awareness of the rule, but actual enforcement happens at the persistence layer (unique index on Email+PropertyId+TenantId in task 0033).

## Implementation Notes
- Key changes:
  - `Lead` entity with `Create()`, `ChangeStatus()`, `Convert()`, `AssignAgent()`, `Delete()` methods
  - `LeadStatus` enum: New, Contacted, Qualified, Converted, Lost
  - Status transition map: `{ New: [Contacted, Lost], Contacted: [Qualified, Lost], Qualified: [Converted, Lost], Converted: [], Lost: [] }`
  - Four domain events: `LeadCreatedV1`, `LeadStatusChangedV1`, `LeadConvertedV1`, `LeadDeletedV1`
  - `LeadValidationException` for domain rule violations
- Edge cases handled:
  - `ChangeStatus(Converted)` blocked -- must use `Convert(contactId)` instead
  - `Convert()` with null/empty contactId throws
  - `Convert()` when already Converted throws (idempotency guard)
  - `Convert()` when Lost throws (terminal state)
  - `ChangeStatus()` on terminal states (Converted, Lost) throws for any target
  - Self-transition (e.g., New->New) throws
- Known limitations:
  - No undo for conversion (by design -- Converted is terminal)
  - No status history tracking (only current status stored; history via domain events)

## Data / Schema / Migrations
- DB changes: None (domain-only task; persistence in task 0033)
- Migration strategy: N/A
- Backward compatibility: N/A

## Commands Run
```bash
dotnet build services/contacts-api/Propely.ContactsApi.sln
dotnet test services/contacts-api/tests/Propely.ContactsApi.UnitTests/
```

## Files Changed

**Created (Domain):**
- `services/contacts-api/src/Propely.ContactsApi.Domain/Leads/Lead.cs` -- Lead entity with state machine
- `services/contacts-api/src/Propely.ContactsApi.Domain/Leads/LeadStatus.cs` -- Status enum (New, Contacted, Qualified, Converted, Lost)
- `services/contacts-api/src/Propely.ContactsApi.Domain/Leads/Events/LeadCreatedV1.cs` -- Domain event
- `services/contacts-api/src/Propely.ContactsApi.Domain/Leads/Events/LeadStatusChangedV1.cs` -- Domain event with OldStatus/NewStatus
- `services/contacts-api/src/Propely.ContactsApi.Domain/Leads/Events/LeadConvertedV1.cs` -- Domain event with ContactId
- `services/contacts-api/src/Propely.ContactsApi.Domain/Leads/Events/LeadDeletedV1.cs` -- Domain event
- `services/contacts-api/src/Propely.ContactsApi.Domain/Leads/Exceptions/LeadValidationException.cs` -- Domain validation exception

**Created (Tests):**
- `services/contacts-api/tests/Propely.ContactsApi.UnitTests/Domain/Leads/LeadTests.cs` -- Tests for Create, Convert, AssignAgent, Delete
- `services/contacts-api/tests/Propely.ContactsApi.UnitTests/Domain/Leads/LeadStatusTransitionTests.cs` -- Parameterized tests for all 25 transition combinations

## Tests
### Unit
- What was added/updated: 47 unit tests total
  - LeadTests: Create (valid, missing name, missing email, missing propertyId), Convert (valid, no contactId, already converted, from Lost), AssignAgent, Delete, domain event verification
  - LeadStatusTransitionTests: 6 valid transitions as `[Theory]` with `[InlineData]`, 19 invalid transitions as `[Theory]` with `[InlineData]` covering all remaining combinations
- How to run: `dotnet test services/contacts-api/tests/Propely.ContactsApi.UnitTests/`

### Integration
- What was added/updated: N/A (domain-only task)
- How to run: N/A

### Manual
- What you verified: N/A (domain-only task)
- Steps: N/A

## Observability
- Logs added/updated: N/A (domain events dispatched by existing outbox infrastructure)
- Traces/metrics added/updated: N/A

## Security
- Validation: Required Name, Email, PropertyId; status transition enforcement
- AuthN/AuthZ impact: None (domain layer; authorization enforced at API level in task 0033)
- Sensitive data handling: Lead PII (name, email, phone, message) scoped by TenantId; no PII in logs

## Follow-ups / Backlog
- [x] Task 0033: Contacts & Leads Persistence & API
- [x] Task 0034: Lead Conversion Flow
- [x] Task 0037: Frontend Lead Pipeline

## Checklist
- [x] Task scope matches `docs/tasks/0032-lead-domain-model.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
