# Walkthrough: 0031-contact-domain-model

## Task Reference
- Task: `docs/tasks/0031-contact-domain-model.md`
- Walkthrough: `docs/walkthroughs/0031-contact-domain-model.md`
- Branch/PR: `feat/p4-contacts-leads`
- Date: `2026-03-06`

## Summary
Implemented the Contact aggregate root in the contacts-api Domain layer with roles (Buyer, Seller, Tenant, Landlord, Professional), property interests (Buying, Renting, Selling), domain events (Created, Updated, Deleted), and comprehensive validation. This is the foundational entity for Propely's CRM contacts functionality, following Clean Architecture and TDD.

## Context
- Background: Propely's contacts-api service was scaffolded but had no domain model. Agents need to manage contacts (buyers, sellers, tenants, landlords, professionals) as part of the CRM workflow.
- Problem statement: No Contact entity existed. The domain layer needed the aggregate root, value objects, enums, and events to support the full contacts CRUD stack.
- Constraints: Clean Architecture (domain has zero framework dependencies), TDD mandatory, ISoftDeletable pattern required.

## Decisions & Trade-offs
- **Decision: Roles as a collection of enum values rather than flags enum**
  - Options considered: (1) `[Flags]` enum with bitwise operations, (2) `List<ContactRole>` collection
  - Why this choice: Collection approach is more explicit and serializes cleanly to JSON/JSONB in EF Core. Avoids bitwise complexity.
  - Consequences: Roles stored as a JSONB array in PostgreSQL (configured in task 0033).

- **Decision: ContactPropertyInterest as a child entity with deduplication**
  - Options considered: (1) Join table managed by EF Core, (2) Domain-managed child entity
  - Why this choice: Domain-managed entity allows business rules (deduplication on Contact+Property+InterestType) to live in the aggregate root, not in infrastructure.
  - Consequences: The Contact aggregate manages its own property interests collection.

- **Decision: ContactSource as an enum rather than free-text**
  - Why: Standardizes lead origin tracking (Portal, Manual, Referral, Website, etc.) for analytics. Free-text would create data quality issues.

## Implementation Notes
- Key changes:
  - `Contact` aggregate root with `Create()`, `Update()`, `Delete()`, `AddPropertyInterest()`, `RemovePropertyInterest()` factory/mutation methods
  - `ContactRole` enum: Buyer, Seller, Tenant, Landlord, Professional
  - `InterestType` enum: Buying, Renting, Selling
  - `ContactSource` enum for origin tracking
  - `ContactPropertyInterest` child entity with ContactId, PropertyId, InterestType, Notes
  - Three domain events: `ContactCreatedV1`, `ContactUpdatedV1`, `ContactDeletedV1`
  - `ContactValidationException` for domain rule violations
- Edge cases handled:
  - Duplicate property interest (same Contact+Property+InterestType) throws exception
  - Removing non-existent property interest throws exception
  - Empty roles collection throws validation exception
  - Null/empty first name, last name, email throw validation exceptions
  - Invalid email format throws validation exception
- Known limitations:
  - Email uniqueness within tenant enforced at persistence level (task 0033), not domain level

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
- `services/contacts-api/src/Propely.ContactsApi.Domain/Contacts/Contact.cs` -- Contact aggregate root with factory methods and business rules
- `services/contacts-api/src/Propely.ContactsApi.Domain/Contacts/ContactPropertyInterest.cs` -- Child entity linking Contact to Property with interest type
- `services/contacts-api/src/Propely.ContactsApi.Domain/Contacts/ContactRole.cs` -- Enum: Buyer, Seller, Tenant, Landlord, Professional
- `services/contacts-api/src/Propely.ContactsApi.Domain/Contacts/InterestType.cs` -- Enum: Buying, Renting, Selling
- `services/contacts-api/src/Propely.ContactsApi.Domain/Contacts/ContactSource.cs` -- Enum for lead origin tracking
- `services/contacts-api/src/Propely.ContactsApi.Domain/Contacts/Events/ContactCreatedV1.cs` -- Domain event
- `services/contacts-api/src/Propely.ContactsApi.Domain/Contacts/Events/ContactUpdatedV1.cs` -- Domain event
- `services/contacts-api/src/Propely.ContactsApi.Domain/Contacts/Events/ContactDeletedV1.cs` -- Domain event
- `services/contacts-api/src/Propely.ContactsApi.Domain/Contacts/Exceptions/ContactValidationException.cs` -- Domain validation exception

**Created (Tests):**
- `services/contacts-api/tests/Propely.ContactsApi.UnitTests/Domain/Contacts/ContactTests.cs` -- 15+ tests for Contact aggregate
- `services/contacts-api/tests/Propely.ContactsApi.UnitTests/Domain/Contacts/ContactPropertyInterestTests.cs` -- 5+ tests for property interest logic

## Tests
### Unit
- What was added/updated: 20+ unit tests covering Contact.Create(), Contact.Update(), Contact.Delete(), AddPropertyInterest, RemovePropertyInterest, validation rules, domain events
- How to run: `dotnet test services/contacts-api/tests/Propely.ContactsApi.UnitTests/`

### Integration
- What was added/updated: N/A (domain-only task)
- How to run: N/A

### Manual
- What you verified: N/A (domain-only task, no UI or API)
- Steps: N/A

## Observability
- Logs added/updated: N/A (domain events dispatched by existing outbox infrastructure)
- Traces/metrics added/updated: N/A

## Security
- Validation: Email format, required first/last name, at least one role
- AuthN/AuthZ impact: None (domain layer; authorization enforced at API level in task 0033)
- Sensitive data handling: Contact PII (name, email, phone) scoped by TenantId; no PII in logs

## Follow-ups / Backlog
- [x] Task 0032: Lead Domain Model
- [x] Task 0033: Contacts & Leads Persistence & API
- [x] Task 0036: Frontend Contacts UI

## Checklist
- [x] Task scope matches `docs/tasks/0031-contact-domain-model.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
