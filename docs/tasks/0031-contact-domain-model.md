# Task: 0031-contact-domain-model

## Metadata
- ID: 0031
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-03-06
- Related docs:
  - Walkthrough: `docs/walkthroughs/0031-contact-domain-model.md`
  - Epic: `docs/backlog/epic-P4-contacts-leads.md` (Task 4.1)

## Goal
Create the Contact aggregate root in the contacts-api Domain layer with roles, property interests, domain events, and comprehensive validation. This establishes the core Contact entity for Propely's CRM functionality.

## Context
Propely needs a Contacts & Leads module to enable real estate agents to manage people they interact with (buyers, sellers, tenants, landlords, professionals). The Contact entity is the foundational aggregate in the contacts-api service. No domain model exists yet in this service -- only the scaffolded project structure from Epic P0. This is the first task in Epic P4 (Contacts & Leads).

## Scope
### In scope
- `Contact` aggregate root entity with private constructor and factory methods
- Contact fields: FirstName, LastName, Email, Phone, SecondaryPhone, Company, Notes, PreferredLanguage, Source, TenantId
- `ContactRole` enum: Buyer, Seller, Tenant, Landlord, Professional
- `ContactPropertyInterest` entity linking a Contact to a Property with an interest type
- `InterestType` enum: Buying, Renting, Selling
- `ContactSource` enum for lead origin tracking
- Domain events: `ContactCreatedV1`, `ContactUpdatedV1`, `ContactDeletedV1`
- `ContactValidationException` for domain rule violations
- Soft-delete support via `ISoftDeletable`
- Business rules: email format validation, first/last name required, at least one role required
- `Contact.AddPropertyInterest()` with deduplication (no duplicate contact+property+interestType)
- `Contact.RemovePropertyInterest()` to unlink a property interest
- 20+ unit tests covering Create, Update, Delete, PropertyInterest, validation

### Out of scope
- Persistence / EF Core mapping (task 0033)
- API endpoints (task 0033)
- Lead model (task 0032)
- Frontend (task 0036)

## Requirements
- R1: Contact aggregate root follows existing Entity/ISoftDeletable patterns from the shared kernel
- R2: Email validated at domain level (format only, not deliverability)
- R3: At least one ContactRole required on creation
- R4: ContactPropertyInterest enforces uniqueness on ContactId + PropertyId + InterestType
- R5: Domain events raised on create, update, and delete with ContactId, TenantId, OccurredAt
- R6: All domain logic is pure (no framework dependencies)

## Acceptance Criteria
- AC1: `Contact` entity inherits from `Entity` and implements `ISoftDeletable`
- AC2: Contact has required fields: `FirstName`, `LastName`, `Email`, `TenantId`
- AC3: Contact has optional fields: `Phone`, `SecondaryPhone`, `Company`, `Notes`, `PreferredLanguage`, `Source`
- AC4: Contact has a `Roles` collection of `ContactRole` values (at least one required)
- AC5: `ContactRole` enum includes: `Buyer`, `Seller`, `Tenant`, `Landlord`, `Professional`
- AC6: `ContactPropertyInterest` links `ContactId` + `PropertyId` + `InterestType` + optional `Notes`
- AC7: `InterestType` enum includes: `Buying`, `Renting`, `Selling`
- AC8: `Contact.Create()` factory method validates required fields and raises `ContactCreatedV1`
- AC9: `Contact.Update()` validates fields and raises `ContactUpdatedV1`
- AC10: `Contact.Delete()` sets soft-delete fields and raises `ContactDeletedV1`
- AC11: `Contact.AddPropertyInterest()` prevents duplicate contact+property+interestType
- AC12: `Contact.RemovePropertyInterest()` removes the link correctly
- AC13: Domain validation: email format, first/last name not empty, at least one role
- AC14: All domain events include `ContactId`, `TenantId`, `OccurredAt`
- AC15: 20+ unit tests pass covering all factory methods, validation rules, and domain events

## Constraints (non-negotiable)
- Clean Architecture layers respected.
- English-only repo content.
- No secrets in repo.
- Update walkthrough.
- TDD: write tests first, then production code.

## Proposed Approach (high-level)
Follow Clean Architecture domain-first approach: define enums, value objects, and entity contracts via TDD. Write all unit tests first (Red), then implement the Contact aggregate root to make tests pass (Green), then refactor.

## Implementation Steps
1. Create `ContactRole` enum in `Domain/Contacts/`
2. Create `InterestType` enum in `Domain/Contacts/`
3. Create `ContactSource` enum in `Domain/Contacts/`
4. Create `ContactPropertyInterest` entity in `Domain/Contacts/`
5. Create domain events: `ContactCreatedV1`, `ContactUpdatedV1`, `ContactDeletedV1` in `Domain/Contacts/Events/`
6. Create `ContactValidationException` in `Domain/Contacts/Exceptions/`
7. Create `Contact` aggregate root with private constructor, `Create()`, `Update()`, `Delete()`, `AddPropertyInterest()`, `RemovePropertyInterest()`
8. Write unit tests for all domain logic (ContactTests.cs, ContactPropertyInterestTests.cs)

## Files to Create / Modify
**Create:**
- `services/contacts-api/src/Propely.ContactsApi.Domain/Contacts/Contact.cs`
- `services/contacts-api/src/Propely.ContactsApi.Domain/Contacts/ContactPropertyInterest.cs`
- `services/contacts-api/src/Propely.ContactsApi.Domain/Contacts/ContactRole.cs`
- `services/contacts-api/src/Propely.ContactsApi.Domain/Contacts/InterestType.cs`
- `services/contacts-api/src/Propely.ContactsApi.Domain/Contacts/ContactSource.cs`
- `services/contacts-api/src/Propely.ContactsApi.Domain/Contacts/Events/ContactCreatedV1.cs`
- `services/contacts-api/src/Propely.ContactsApi.Domain/Contacts/Events/ContactUpdatedV1.cs`
- `services/contacts-api/src/Propely.ContactsApi.Domain/Contacts/Events/ContactDeletedV1.cs`
- `services/contacts-api/src/Propely.ContactsApi.Domain/Contacts/Exceptions/ContactValidationException.cs`
- `services/contacts-api/tests/Propely.ContactsApi.UnitTests/Domain/Contacts/ContactTests.cs`
- `services/contacts-api/tests/Propely.ContactsApi.UnitTests/Domain/Contacts/ContactPropertyInterestTests.cs`

## Testing Plan
- Unit tests:
  - `Contact.Create()` with valid data raises `ContactCreatedV1`
  - `Contact.Create()` with missing first name throws `ContactValidationException`
  - `Contact.Create()` with missing last name throws `ContactValidationException`
  - `Contact.Create()` with invalid email throws `ContactValidationException`
  - `Contact.Create()` with no roles throws `ContactValidationException`
  - `Contact.Update()` validates fields and raises `ContactUpdatedV1`
  - `Contact.Delete()` sets `IsDeleted` and raises `ContactDeletedV1`
  - `Contact.AddPropertyInterest()` adds interest and prevents duplicates
  - `Contact.RemovePropertyInterest()` removes interest correctly
  - All domain events contain correct ContactId, TenantId, OccurredAt
  - ContactPropertyInterest creation and validation
- Integration tests: None (domain-only task)
- Manual verification: None required

## Security & Privacy
- Contact data is PII (names, emails, phones); tenant-scoped via `TenantId` at the domain level
- Email addresses validated for format only (not deliverability)
- Notes field may contain free-form PII; must be handled with care in logging (no PII in logs)

## Observability
- Logs: Domain events dispatched via outbox (existing infrastructure)
- Metrics: N/A (domain-only task)
- Traces: N/A (domain-only task)

## Rollback Plan
Revert the commit. No database migrations or infrastructure changes involved.

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass (20+ unit tests)
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
