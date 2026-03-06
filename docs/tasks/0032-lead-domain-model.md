# Task: 0032-lead-domain-model

## Metadata
- ID: 0032
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-03-06
- Related docs:
  - Walkthrough: `docs/walkthroughs/0032-lead-domain-model.md`
  - Epic: `docs/backlog/epic-P4-contacts-leads.md` (Task 4.2)

## Goal
Create the Lead entity in the contacts-api Domain layer with a status pipeline state machine, domain events, and comprehensive validation. Leads are always linked to a property and follow a pipeline from New through Qualified to Converted or Lost.

## Context
The Contact aggregate root (task 0031) is complete. Leads represent potential clients who have expressed interest in a specific property. A Lead always belongs to a property via `PropertyId` and follows a strict status pipeline. Once converted, a Lead is linked to a Contact. This is the second task in Epic P4.

## Scope
### In scope
- `Lead` entity with private constructor and factory methods
- Lead fields: Name, Email, Phone, Message, Source, PropertyId, TenantId, AssignedAgentId (nullable), ContactId (nullable, set on conversion)
- `LeadStatus` enum: New, Contacted, Qualified, Converted, Lost
- Status transition state machine with strict validation of allowed transitions
- Valid transitions: New->Contacted, Contacted->Qualified, Qualified->Converted, Qualified->Lost, New->Lost, Contacted->Lost
- Domain events: `LeadCreatedV1`, `LeadStatusChangedV1`, `LeadConvertedV1`, `LeadDeletedV1`
- `LeadValidationException` for domain rule violations
- `Lead.Convert(contactId)` sets status to Converted and links to Contact
- `Lead.AssignAgent(agentId)` sets the assigned agent
- Soft-delete support via `ISoftDeletable`
- 47 unit tests including parameterized transition tests for all valid/invalid paths

### Out of scope
- Persistence / EF Core mapping (task 0033)
- API endpoints (task 0033)
- Lead conversion orchestration (task 0034)
- Lead scoring/ranking

## Requirements
- R1: Lead entity follows existing Entity/ISoftDeletable patterns
- R2: PropertyId is required on every Lead (a Lead always belongs to a property)
- R3: Status transitions follow a strict state machine -- invalid transitions throw DomainException
- R4: Conversion requires a ContactId; cannot convert from non-Qualified status
- R5: Cannot revert from Converted or Lost status
- R6: Domain events raised on create, status change, conversion, and delete
- R7: Deduplication rule: unique Email + PropertyId within tenant (enforced at persistence level, validated at domain level)

## Acceptance Criteria
- AC1: `Lead` entity inherits from `Entity` and implements `ISoftDeletable`
- AC2: Lead has required fields: `Name`, `Email`, `PropertyId`, `TenantId`
- AC3: Lead has optional fields: `Phone`, `Message`, `Source`, `AssignedAgentId`, `ContactId`
- AC4: `LeadStatus` enum: `New`, `Contacted`, `Qualified`, `Converted`, `Lost`
- AC5: `Lead.Create()` sets status to `New` and raises `LeadCreatedV1`
- AC6: `Lead.ChangeStatus(newStatus)` validates transitions and raises `LeadStatusChangedV1`
- AC7: Valid transitions: New->Contacted, Contacted->Qualified, Qualified->Converted, Qualified->Lost, New->Lost, Contacted->Lost
- AC8: Invalid transitions throw `DomainException` (e.g., Converted->New, Lost->Qualified)
- AC9: `Lead.Convert(contactId)` sets status to Converted, sets ContactId, raises `LeadConvertedV1`
- AC10: `Lead.Convert()` throws if already converted or lost
- AC11: `Lead.AssignAgent(agentId)` sets the assigned agent correctly
- AC12: All domain events include `LeadId`, `PropertyId`, `TenantId`, `OccurredAt`
- AC13: 47 unit tests pass covering all status transitions (valid and invalid), creation, conversion, agent assignment

## Constraints (non-negotiable)
- Clean Architecture layers respected.
- English-only repo content.
- No secrets in repo.
- Update walkthrough.
- TDD: write parameterized tests for ALL status transition combinations (5x5 matrix) first.

## Proposed Approach (high-level)
Write parameterized `[Theory]` tests covering all 25 status transition combinations (5 current statuses x 5 target statuses), defining which are valid and which should throw. Then implement the Lead entity with a state machine to make all tests pass.

## Implementation Steps
1. Create `LeadStatus` enum in `Domain/Leads/`
2. Create domain events: `LeadCreatedV1`, `LeadStatusChangedV1`, `LeadConvertedV1`, `LeadDeletedV1` in `Domain/Leads/Events/`
3. Create `LeadValidationException` in `Domain/Leads/Exceptions/`
4. Create `Lead` entity with private constructor, `Create()`, `ChangeStatus()`, `Convert()`, `AssignAgent()`, `Delete()`
5. Implement status transition state machine in `Lead.ChangeStatus()` with allowed transition map
6. Write `LeadTests.cs` covering creation, assignment, deletion, conversion
7. Write `LeadStatusTransitionTests.cs` with parameterized tests for all 25 transition combinations

## Files to Create / Modify
**Create:**
- `services/contacts-api/src/Propely.ContactsApi.Domain/Leads/Lead.cs`
- `services/contacts-api/src/Propely.ContactsApi.Domain/Leads/LeadStatus.cs`
- `services/contacts-api/src/Propely.ContactsApi.Domain/Leads/Events/LeadCreatedV1.cs`
- `services/contacts-api/src/Propely.ContactsApi.Domain/Leads/Events/LeadStatusChangedV1.cs`
- `services/contacts-api/src/Propely.ContactsApi.Domain/Leads/Events/LeadConvertedV1.cs`
- `services/contacts-api/src/Propely.ContactsApi.Domain/Leads/Events/LeadDeletedV1.cs`
- `services/contacts-api/src/Propely.ContactsApi.Domain/Leads/Exceptions/LeadValidationException.cs`
- `services/contacts-api/tests/Propely.ContactsApi.UnitTests/Domain/Leads/LeadTests.cs`
- `services/contacts-api/tests/Propely.ContactsApi.UnitTests/Domain/Leads/LeadStatusTransitionTests.cs`

## Testing Plan
- Unit tests:
  - `Lead.Create()` sets status New, raises `LeadCreatedV1` (valid data)
  - `Lead.Create()` with missing PropertyId throws
  - `Lead.Create()` with missing email throws
  - `Lead.Create()` with missing name throws
  - Every valid status transition succeeds and raises `LeadStatusChangedV1` (parameterized `[Theory]`)
  - Every invalid status transition throws `DomainException` (parameterized `[Theory]`)
  - `Lead.Convert()` with valid contactId sets Converted status and ContactId
  - `Lead.Convert()` with no contactId throws
  - `Lead.Convert()` when already converted throws
  - `Lead.Convert()` when Lost throws
  - `Lead.AssignAgent()` sets agent correctly
  - `Lead.Delete()` sets soft-delete fields and raises `LeadDeletedV1`
  - All domain events contain correct LeadId, PropertyId, TenantId
- Integration tests: None (domain-only task)
- Manual verification: None required

## Security & Privacy
- Lead data is PII (name, email, phone, message); tenant-scoped via `TenantId`
- Lead source may reveal portal affiliation; no special treatment needed
- Message field may contain free-form text; no logging of content

## Observability
- Logs: Domain events dispatched via outbox (existing infrastructure)
- Metrics: N/A (domain-only task)
- Traces: N/A (domain-only task)

## Rollback Plan
Revert the commit. No database migrations or infrastructure changes involved.

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass (47 unit tests)
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
