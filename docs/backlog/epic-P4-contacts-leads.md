# Epic P4 -- Contacts & Leads

## Overview

Build the Contacts and Leads domain for Propely, enabling real estate agents to manage people they interact with (buyers, sellers, tenants, landlords, professionals) and track leads tied to specific properties. Leads always belong to a property and follow a pipeline from New through Qualified to Converted or Lost. Lead conversion creates or merges Contacts, enabling a complete CRM workflow within the platform.

## Service Ownership

| Capability | Service |
|---|---|
| Contact & Lead domain, persistence, API | `services/contacts-api` |
| NuGet SDK client for cross-service calls | `services/contacts-api` (client package) |
| Frontend UI | `apps/web` |

## Tasks

| # | Title | Status | Dependencies |
|---|---|---|---|
| 4.1 | Contact Domain Model | PENDING | -- |
| 4.2 | Lead Domain Model | PENDING | 4.1 |
| 4.3 | Contacts & Leads Persistence & API | PENDING | 4.1, 4.2 |
| 4.4 | Lead Conversion Flow | PENDING | 4.3 |
| 4.5 | Contacts-API NuGet SDK Client | PENDING | 4.3 |
| 4.6 | Frontend Contacts List & Detail | PENDING | 4.3 |
| 4.7 | Frontend Lead Pipeline | PENDING | 4.3, 4.4 |

---

## Task 4.1 -- Contact Domain Model

**Status:** PENDING
**Dependencies:** None

### Scope

**In scope:**
- `Contact` aggregate root entity in `ContactsApi.Domain`
- Contact roles: `Buyer`, `Seller`, `Tenant`, `Landlord`, `Professional` (a contact can have multiple roles)
- Contact fields: first name, last name, email, phone, secondary phone, company, notes, preferred language, source
- `ContactPropertyInterest` entity linking a contact to a property with interest type (Buying, Renting, Selling)
- Domain events: `ContactCreatedV1`, `ContactUpdatedV1`, `ContactDeletedV1`
- Value objects: `ContactRole` (enum flags or collection), `InterestType` enum
- Business rules: email uniqueness within tenant, at least one role required
- Soft-delete support via `ISoftDeletable`

**Out of scope:**
- Persistence/EF Core mapping (task 4.3)
- API endpoints (task 4.3)
- Lead model (task 4.2)

### Acceptance Criteria

- [ ] `Contact` entity inherits from `Entity` and implements `ISoftDeletable`
- [ ] Contact has required fields: `FirstName`, `LastName`, `Email`, `TenantId`
- [ ] Contact has optional fields: `Phone`, `SecondaryPhone`, `Company`, `Notes`, `PreferredLanguage`, `Source`
- [ ] Contact has a `Roles` collection of `ContactRole` values (at least one required)
- [ ] `ContactRole` enum includes: `Buyer`, `Seller`, `Tenant`, `Landlord`, `Professional`
- [ ] `ContactPropertyInterest` links `ContactId` + `PropertyId` + `InterestType` + optional `Notes`
- [ ] `InterestType` enum includes: `Buying`, `Renting`, `Selling`
- [ ] `Contact.Create()` factory method validates required fields and raises `ContactCreatedV1`
- [ ] `Contact.Update()` validates fields and raises `ContactUpdatedV1`
- [ ] `Contact.Delete()` sets soft-delete fields and raises `ContactDeletedV1`
- [ ] `Contact.AddPropertyInterest()` links a property with deduplication (no duplicate contact+property+interestType)
- [ ] `Contact.RemovePropertyInterest()` removes the link
- [ ] Domain validation: email format, first/last name not empty, at least one role
- [ ] All domain events include `ContactId`, `TenantId`, `OccurredAt`
- [ ] Unit tests cover all factory methods, validation rules, and domain events

### Implementation Steps

1. Create `ContactRole` enum in `Domain/Contacts/`
2. Create `InterestType` enum in `Domain/Contacts/`
3. Create `ContactPropertyInterest` entity in `Domain/Contacts/`
4. Create `Contact` aggregate root with private constructor, factory methods, and business rules
5. Create domain events: `ContactCreatedV1`, `ContactUpdatedV1`, `ContactDeletedV1` in `Domain/Contacts/Events/`
6. Create `ContactValidationException` in `Domain/Contacts/Exceptions/`
7. Write comprehensive unit tests for all domain logic

### Files to Create/Modify

**Create:**
- `services/contacts-api/src/Propely.ContactsApi.Domain/Contacts/Contact.cs`
- `services/contacts-api/src/Propely.ContactsApi.Domain/Contacts/ContactRole.cs`
- `services/contacts-api/src/Propely.ContactsApi.Domain/Contacts/ContactPropertyInterest.cs`
- `services/contacts-api/src/Propely.ContactsApi.Domain/Contacts/InterestType.cs`
- `services/contacts-api/src/Propely.ContactsApi.Domain/Contacts/Events/ContactCreatedV1.cs`
- `services/contacts-api/src/Propely.ContactsApi.Domain/Contacts/Events/ContactUpdatedV1.cs`
- `services/contacts-api/src/Propely.ContactsApi.Domain/Contacts/Events/ContactDeletedV1.cs`
- `services/contacts-api/src/Propely.ContactsApi.Domain/Contacts/Exceptions/ContactValidationException.cs`
- `services/contacts-api/tests/Propely.ContactsApi.Domain.Tests/Contacts/ContactTests.cs`
- `services/contacts-api/tests/Propely.ContactsApi.Domain.Tests/Contacts/ContactPropertyInterestTests.cs`

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `Contact.Create()` with valid data raises event | xUnit + FluentAssertions |
| Unit | `Contact.Create()` with missing first name throws | xUnit |
| Unit | `Contact.Create()` with invalid email throws | xUnit |
| Unit | `Contact.Create()` with no roles throws | xUnit |
| Unit | `Contact.Update()` raises `ContactUpdatedV1` | xUnit |
| Unit | `Contact.Delete()` sets `IsDeleted` and raises event | xUnit |
| Unit | `Contact.AddPropertyInterest()` prevents duplicates | xUnit |
| Unit | `Contact.RemovePropertyInterest()` removes correctly | xUnit |
| Unit | All domain events contain correct IDs and timestamp | xUnit |

### Security & Privacy

- Contact data is PII (names, emails, phones); must be tenant-scoped at the domain level via `TenantId`
- Email addresses must be validated at domain level (format only, not deliverability)
- Notes field may contain free-form PII; handle with care in logging

### TDD Reminder

Write all domain unit tests first, defining expected behavior for each factory method and business rule. Then implement the `Contact` aggregate to make tests pass.

---

## Task 4.2 -- Lead Domain Model

**Status:** PENDING
**Dependencies:** 4.1 (uses `Contact` for conversion target)

### Scope

**In scope:**
- `Lead` entity in `ContactsApi.Domain`, ALWAYS linked to a property via `PropertyId`
- Lead fields: name, email, phone, message, source (portal name or "manual"), `PropertyId`, `AssignedAgentId`, `ContactId` (nullable, set on conversion)
- Status pipeline: `New` -> `Contacted` -> `Qualified` -> `Converted` | `Lost`
- Deduplication rule: unique constraint on `Email` + `PropertyId` within tenant (no duplicate leads per property)
- Domain events: `LeadCreatedV1`, `LeadStatusChangedV1`, `LeadConvertedV1`, `LeadDeletedV1`
- Business rules: cannot revert from `Converted`/`Lost`, conversion requires a `ContactId`
- Soft-delete support

**Out of scope:**
- Persistence/EF Core (task 4.3)
- Conversion logic merging contacts (task 4.4)
- Lead scoring/ranking

### Acceptance Criteria

- [ ] `Lead` entity inherits from `Entity` and implements `ISoftDeletable`
- [ ] `Lead` has required fields: `Name`, `Email`, `PropertyId`, `TenantId`
- [ ] `Lead` has optional fields: `Phone`, `Message`, `Source`, `AssignedAgentId`, `ContactId`
- [ ] `LeadStatus` enum: `New`, `Contacted`, `Qualified`, `Converted`, `Lost`
- [ ] `Lead.Create()` factory method sets status to `New` and raises `LeadCreatedV1`
- [ ] `Lead.ChangeStatus(newStatus)` validates transitions and raises `LeadStatusChangedV1`
- [ ] Valid transitions: New->Contacted, Contacted->Qualified, Qualified->Converted, Qualified->Lost, New->Lost, Contacted->Lost
- [ ] Invalid transitions throw `DomainException` (e.g., Converted->New, Lost->Qualified)
- [ ] `Lead.Convert(contactId)` sets status to `Converted`, sets `ContactId`, raises `LeadConvertedV1`
- [ ] `Lead.Convert()` throws if already converted or lost
- [ ] `Lead.AssignAgent(agentId)` sets the assigned agent
- [ ] Deduplication is enforced at the domain level with a static validation method `Lead.IsDuplicate(email, propertyId)`
- [ ] All domain events include `LeadId`, `PropertyId`, `TenantId`, `OccurredAt`
- [ ] Unit tests cover all status transitions, valid and invalid

### Implementation Steps

1. Create `LeadStatus` enum in `Domain/Leads/`
2. Create `Lead` entity with private constructor, factory methods, and status transition logic
3. Create domain events in `Domain/Leads/Events/`
4. Create `LeadValidationException` for domain rule violations
5. Implement status transition state machine in `Lead.ChangeStatus()`
6. Write unit tests for every transition path (valid and invalid)

### Files to Create/Modify

**Create:**
- `services/contacts-api/src/Propely.ContactsApi.Domain/Leads/Lead.cs`
- `services/contacts-api/src/Propely.ContactsApi.Domain/Leads/LeadStatus.cs`
- `services/contacts-api/src/Propely.ContactsApi.Domain/Leads/Events/LeadCreatedV1.cs`
- `services/contacts-api/src/Propely.ContactsApi.Domain/Leads/Events/LeadStatusChangedV1.cs`
- `services/contacts-api/src/Propely.ContactsApi.Domain/Leads/Events/LeadConvertedV1.cs`
- `services/contacts-api/src/Propely.ContactsApi.Domain/Leads/Events/LeadDeletedV1.cs`
- `services/contacts-api/src/Propely.ContactsApi.Domain/Leads/Exceptions/LeadValidationException.cs`
- `services/contacts-api/tests/Propely.ContactsApi.Domain.Tests/Leads/LeadTests.cs`
- `services/contacts-api/tests/Propely.ContactsApi.Domain.Tests/Leads/LeadStatusTransitionTests.cs`

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `Lead.Create()` sets status New, raises event | xUnit + FluentAssertions |
| Unit | `Lead.Create()` with missing PropertyId throws | xUnit |
| Unit | `Lead.Create()` with missing email throws | xUnit |
| Unit | Every valid status transition succeeds and raises event | xUnit, parameterized `[Theory]` |
| Unit | Every invalid status transition throws `DomainException` | xUnit, parameterized `[Theory]` |
| Unit | `Lead.Convert()` with no contactId throws | xUnit |
| Unit | `Lead.Convert()` when already converted throws | xUnit |
| Unit | `Lead.AssignAgent()` sets agent correctly | xUnit |
| Unit | All domain events contain correct property/tenant IDs | xUnit |

### Security & Privacy

- Lead data is PII (name, email, phone, message); tenant-scoped via `TenantId`
- Lead source may reveal portal affiliation; no special treatment needed
- Message field may contain free-form text; no logging of content

### TDD Reminder

Write parameterized tests for ALL status transition combinations (5x5 matrix) first. Define which are valid and which should throw. Then implement the state machine.

---

## Task 4.3 -- Contacts & Leads Persistence & API

**Status:** PENDING
**Dependencies:** 4.1, 4.2 (domain models must exist)

### Scope

**In scope:**
- EF Core configurations for `Contact`, `ContactPropertyInterest`, and `Lead`
- Database migration for contacts and leads tables
- Repository interfaces and implementations
- CRUD API endpoints for contacts: `GET /api/contacts`, `GET /api/contacts/{id}`, `POST /api/contacts`, `PUT /api/contacts/{id}`, `DELETE /api/contacts/{id}`
- CRUD API endpoints for leads: `GET /api/leads`, `GET /api/leads/{id}`, `POST /api/leads`, `PUT /api/leads/{id}`, `DELETE /api/leads/{id}`
- Lead assignment endpoint: `PUT /api/leads/{id}/assign`
- Lead status change endpoint: `PUT /api/leads/{id}/status`
- Authorization: agent sees own contacts/leads, admin sees all within tenant
- Pagination, sorting, filtering on list endpoints
- Tenant query filter on all queries

**Out of scope:**
- Lead conversion (task 4.4)
- Frontend (tasks 4.6, 4.7)
- Cross-service SDK (task 4.5)

### Acceptance Criteria

- [ ] `ContactConfiguration` maps `Contact` to `contacts` table with correct column types and indexes
- [ ] `ContactPropertyInterestConfiguration` maps with composite unique index on `ContactId` + `PropertyId` + `InterestType`
- [ ] `LeadConfiguration` maps `Lead` to `leads` table with unique index on `Email` + `PropertyId` + `TenantId`
- [ ] `IContactRepository` and `ILeadRepository` interfaces defined in Application layer
- [ ] Repository implementations in Infrastructure layer with tenant filtering
- [ ] Contacts CRUD endpoints return appropriate DTOs (not domain entities)
- [ ] Leads CRUD endpoints return appropriate DTOs
- [ ] `GET /api/contacts` supports: `?page=1&pageSize=20&search=term&role=Buyer&sortBy=lastName&sortDir=asc`
- [ ] `GET /api/leads` supports: `?page=1&pageSize=20&status=New&propertyId=guid&assignedAgentId=guid&sortBy=createdAt`
- [ ] `PUT /api/leads/{id}/assign` accepts `{ "agentId": "guid" }` and returns updated lead
- [ ] `PUT /api/leads/{id}/status` accepts `{ "status": "Contacted" }` and validates transition
- [ ] Authorization: agents see only their assigned leads and contacts they created; admins see all in tenant
- [ ] `POST /api/leads` enforces deduplication (409 Conflict if email+propertyId exists)
- [ ] All endpoints require authentication
- [ ] Soft-deleted records are excluded from queries
- [ ] Migration runs cleanly on a fresh database
- [ ] Integration tests cover all CRUD operations and authorization rules

### Implementation Steps

1. Create `IContactRepository` interface in `Application/Contacts/Interfaces/`
2. Create `IContactReadRepository` interface for queries
3. Create `ILeadRepository` interface in `Application/Leads/Interfaces/`
4. Create `ILeadReadRepository` interface for queries
5. Create Contact DTOs: `ContactDto`, `ContactListItemDto`, `CreateContactRequest`, `UpdateContactRequest`
6. Create Lead DTOs: `LeadDto`, `LeadListItemDto`, `CreateLeadRequest`, `UpdateLeadRequest`, `AssignLeadRequest`, `ChangeLeadStatusRequest`
7. Create MediatR commands: `CreateContactCommand`, `UpdateContactCommand`, `DeleteContactCommand`
8. Create MediatR commands: `CreateLeadCommand`, `UpdateLeadCommand`, `DeleteLeadCommand`, `AssignLeadCommand`, `ChangeLeadStatusCommand`
9. Create MediatR queries: `GetContactByIdQuery`, `ListContactsQuery`, `GetLeadByIdQuery`, `ListLeadsQuery`
10. Create command/query handlers with validation
11. Create EF Core configurations: `ContactConfiguration`, `ContactPropertyInterestConfiguration`, `LeadConfiguration`
12. Create repository implementations
13. Add migration
14. Create `ContactsController` and `LeadsController` in API layer
15. Register new services in DI
16. Write integration tests for all endpoints

### Files to Create/Modify

**Create:**
- `services/contacts-api/src/Propely.ContactsApi.Application/Contacts/Interfaces/IContactRepository.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Contacts/Interfaces/IContactReadRepository.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Contacts/Dtos/ContactDto.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Contacts/Dtos/ContactListItemDto.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Contacts/Dtos/CreateContactRequest.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Contacts/Dtos/UpdateContactRequest.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Contacts/Commands/CreateContact/CreateContactCommand.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Contacts/Commands/CreateContact/CreateContactCommandHandler.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Contacts/Commands/CreateContact/CreateContactCommandValidator.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Contacts/Commands/UpdateContact/UpdateContactCommand.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Contacts/Commands/UpdateContact/UpdateContactCommandHandler.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Contacts/Commands/DeleteContact/DeleteContactCommand.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Contacts/Commands/DeleteContact/DeleteContactCommandHandler.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Contacts/Queries/GetContactById/GetContactByIdQuery.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Contacts/Queries/GetContactById/GetContactByIdQueryHandler.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Contacts/Queries/ListContacts/ListContactsQuery.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Contacts/Queries/ListContacts/ListContactsQueryHandler.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Contacts/Queries/ListContacts/ListContactsQueryValidator.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Interfaces/ILeadRepository.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Interfaces/ILeadReadRepository.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Dtos/LeadDto.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Dtos/LeadListItemDto.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Dtos/CreateLeadRequest.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Dtos/UpdateLeadRequest.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Dtos/AssignLeadRequest.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Dtos/ChangeLeadStatusRequest.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Commands/CreateLead/CreateLeadCommand.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Commands/CreateLead/CreateLeadCommandHandler.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Commands/CreateLead/CreateLeadCommandValidator.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Commands/UpdateLead/UpdateLeadCommand.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Commands/UpdateLead/UpdateLeadCommandHandler.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Commands/DeleteLead/DeleteLeadCommand.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Commands/DeleteLead/DeleteLeadCommandHandler.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Commands/AssignLead/AssignLeadCommand.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Commands/AssignLead/AssignLeadCommandHandler.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Commands/ChangeLeadStatus/ChangeLeadStatusCommand.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Commands/ChangeLeadStatus/ChangeLeadStatusCommandHandler.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Queries/GetLeadById/GetLeadByIdQuery.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Queries/GetLeadById/GetLeadByIdQueryHandler.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Queries/ListLeads/ListLeadsQuery.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Queries/ListLeads/ListLeadsQueryHandler.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Queries/ListLeads/ListLeadsQueryValidator.cs`
- `services/contacts-api/src/Propely.ContactsApi.Infrastructure/Persistence/Configurations/ContactConfiguration.cs`
- `services/contacts-api/src/Propely.ContactsApi.Infrastructure/Persistence/Configurations/ContactPropertyInterestConfiguration.cs`
- `services/contacts-api/src/Propely.ContactsApi.Infrastructure/Persistence/Configurations/LeadConfiguration.cs`
- `services/contacts-api/src/Propely.ContactsApi.Infrastructure/Persistence/Repositories/ContactRepository.cs`
- `services/contacts-api/src/Propely.ContactsApi.Infrastructure/Persistence/Repositories/ContactReadRepository.cs`
- `services/contacts-api/src/Propely.ContactsApi.Infrastructure/Persistence/Repositories/LeadRepository.cs`
- `services/contacts-api/src/Propely.ContactsApi.Infrastructure/Persistence/Repositories/LeadReadRepository.cs`
- `services/contacts-api/src/Propely.ContactsApi.Api/Controllers/ContactsController.cs`
- `services/contacts-api/src/Propely.ContactsApi.Api/Controllers/LeadsController.cs`
- `services/contacts-api/tests/Propely.ContactsApi.Application.Tests/Contacts/Commands/CreateContactCommandHandlerTests.cs`
- `services/contacts-api/tests/Propely.ContactsApi.Application.Tests/Leads/Commands/CreateLeadCommandHandlerTests.cs`
- `services/contacts-api/tests/Propely.ContactsApi.Api.Tests/Controllers/ContactsControllerTests.cs`
- `services/contacts-api/tests/Propely.ContactsApi.Api.Tests/Controllers/LeadsControllerTests.cs`

**Modify:**
- `services/contacts-api/src/Propely.ContactsApi.Infrastructure/Persistence/OrgsApiDbContext.cs` (add `DbSet<Contact>`, `DbSet<Lead>`, etc.)
- `services/contacts-api/src/Propely.ContactsApi.Infrastructure/DependencyInjection.cs` (register repositories)
- `services/contacts-api/src/Propely.ContactsApi.Api/Program.cs` (if controller registration needed)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | All command validators (required fields, email format, enum values) | xUnit + FluentAssertions |
| Unit | All command handlers (create, update, delete, assign, status change) | xUnit, mock repositories |
| Unit | All query handlers (list with filters, get by ID, not found) | xUnit, mock read repositories |
| Integration | Contact CRUD via HTTP endpoints | `WebApplicationFactory`, in-memory DB |
| Integration | Lead CRUD via HTTP endpoints | `WebApplicationFactory`, in-memory DB |
| Integration | Lead deduplication returns 409 | `WebApplicationFactory` |
| Integration | Authorization: agent cannot see other agent's leads | `WebApplicationFactory`, different auth contexts |
| Integration | Tenant isolation: tenant A cannot see tenant B contacts | `WebApplicationFactory` |
| Manual | Create contacts and leads through API, verify database state | Dev environment |

### Security & Privacy

- Tenant isolation enforced via global query filter on `TenantId`
- Agent-level authorization: agents see own data, admins see all within tenant
- Deduplication check must be tenant-scoped (same email+property in different tenants is OK)
- PII fields (email, phone, name) must not appear in logs
- Soft-deleted records excluded from all queries but retained in database

### TDD Reminder

Write integration tests for each endpoint first (expected request/response pairs). Then implement controllers, handlers, and persistence layer to make them pass.

---

## Task 4.4 -- Lead Conversion Flow

**Status:** PENDING
**Dependencies:** 4.3 (persistence and API must exist)

### Scope

**In scope:**
- `POST /api/leads/{id}/convert` endpoint
- Convert a qualified lead into a contact (or link to existing contact)
- If contact with same email already exists in tenant: merge lead data into existing contact, add property interest
- If no matching contact: create new contact from lead data with `Buyer` or `Tenant` role
- Set `Lead.ContactId` and change status to `Converted`
- Emit `LeadConvertedV1` domain event
- MediatR handler orchestrating the flow within a single transaction

**Out of scope:**
- Automatic conversion (always user-initiated)
- Bulk conversion
- Undo conversion

### Acceptance Criteria

- [ ] `POST /api/leads/{id}/convert` accepts optional `{ "role": "Buyer", "notes": "..." }`
- [ ] When no contact exists with the lead's email: creates new Contact with lead data and specified role
- [ ] When contact exists with same email: links lead to existing contact, adds property interest
- [ ] Lead status changes to `Converted` and `ContactId` is set
- [ ] `LeadConvertedV1` event is emitted with `LeadId`, `ContactId`, `PropertyId`, `WasNewContact`
- [ ] Attempting to convert a lead not in `Qualified` status returns 400
- [ ] Attempting to convert an already-converted lead returns 409
- [ ] Entire operation runs in a single database transaction (atomic)
- [ ] Response includes both the updated lead and the contact (created or existing)
- [ ] Authorization: only admin or assigned agent can convert
- [ ] Unit tests cover: new contact creation, merge with existing, invalid status, already converted
- [ ] Integration test covers full conversion flow

### Implementation Steps

1. Create `ConvertLeadCommand` in `Application/Leads/Commands/ConvertLead/`
2. Create `ConvertLeadCommandValidator` (lead must exist, lead must be Qualified)
3. Create `ConvertLeadCommandHandler` with conversion logic
4. In handler: check for existing contact by email within tenant
5. If exists: add property interest, update contact if lead has newer/additional data
6. If not exists: create new Contact from lead data with specified role
7. Call `Lead.Convert(contactId)` to update status and emit event
8. Save within `IUnitOfWork` transaction
9. Create `ConvertLeadResponse` DTO with lead and contact details
10. Add endpoint to `LeadsController`
11. Write unit tests for handler (both paths)
12. Write integration test covering conversion flow

### Files to Create/Modify

**Create:**
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Commands/ConvertLead/ConvertLeadCommand.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Commands/ConvertLead/ConvertLeadCommandHandler.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Commands/ConvertLead/ConvertLeadCommandValidator.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Dtos/ConvertLeadRequest.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Dtos/ConvertLeadResponse.cs`
- `services/contacts-api/tests/Propely.ContactsApi.Application.Tests/Leads/Commands/ConvertLeadCommandHandlerTests.cs`
- `services/contacts-api/tests/Propely.ContactsApi.Api.Tests/Controllers/LeadsController_ConvertTests.cs`

**Modify:**
- `services/contacts-api/src/Propely.ContactsApi.Api/Controllers/LeadsController.cs` (add convert endpoint)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | Convert lead with no existing contact creates new Contact | xUnit, mock repos |
| Unit | Convert lead with existing contact merges and adds interest | xUnit, mock repos |
| Unit | Convert lead not in Qualified status throws | xUnit |
| Unit | Convert already-converted lead throws | xUnit |
| Unit | Transaction rollback on failure | xUnit, mock UnitOfWork |
| Integration | Full conversion endpoint with new contact | `WebApplicationFactory` |
| Integration | Full conversion endpoint with existing contact merge | `WebApplicationFactory` |
| Integration | Authorization: non-assigned agent cannot convert | `WebApplicationFactory` |
| Manual | Convert lead via API, verify contact and lead state in DB | Dev environment |

### Security & Privacy

- Conversion creates or modifies Contact PII; must be within same tenant
- Merge logic must not expose data from other tenants
- Event `LeadConvertedV1` may be consumed by other services; include only IDs, not PII

### TDD Reminder

Write tests for both conversion paths (new contact vs. merge) first. Define expected Contact state after each path. Then implement the handler.

---

## Task 4.5 -- Contacts-API NuGet SDK Client

**Status:** PENDING
**Dependencies:** 4.3 (API endpoints must be defined)

### Scope

**In scope:**
- `Propely.ContactsApi.Client` NuGet package for cross-service communication
- `IContactsApi` Refit interface: CRUD operations for contacts
- `ILeadsApi` Refit interface: CRUD, assign, status change, convert
- Shared DTO models matching API response schemas
- `AddContactsApiClient(this IServiceCollection, Uri baseUrl)` extension method
- Retry policies (Polly) for transient failures
- Primary consumer: `publishing-api` webhooks creating leads from portal inquiries

**Out of scope:**
- Publishing the package to NuGet.org (CI/CD task)
- Authentication token management (existing infrastructure)

### Acceptance Criteria

- [ ] `IContactsApi` has methods for: `GetContactsAsync`, `GetContactByIdAsync`, `CreateContactAsync`, `UpdateContactAsync`, `DeleteContactAsync`
- [ ] `ILeadsApi` has methods for: `GetLeadsAsync`, `GetLeadByIdAsync`, `CreateLeadAsync`, `UpdateLeadAsync`, `DeleteLeadAsync`, `AssignLeadAsync`, `ChangeLeadStatusAsync`, `ConvertLeadAsync`
- [ ] All DTOs have XML doc comments
- [ ] `AddContactsApiClient(this IServiceCollection, Uri baseUrl)` registers all interfaces
- [ ] Retry policy: 3 retries with exponential backoff for 5xx and 408
- [ ] Timeout policy: 30 seconds
- [ ] Package builds without warnings
- [ ] Unit tests verify DI registration and Polly policies

### Implementation Steps

1. Create `Propely.ContactsApi.Client` class library project
2. Add contact DTOs: `ContactResponse`, `CreateContactRequest`, `UpdateContactRequest`, `ContactListResponse`
3. Add lead DTOs: `LeadResponse`, `CreateLeadRequest`, `UpdateLeadRequest`, `AssignLeadRequest`, `ChangeStatusRequest`, `ConvertLeadRequest`, `ConvertLeadResponse`, `LeadListResponse`
4. Create `IContactsApi` Refit interface with route attributes
5. Create `ILeadsApi` Refit interface with route attributes
6. Create `ServiceCollectionExtensions.AddContactsApiClient` with Polly policies
7. Write unit tests for DI registration and policies

### Files to Create/Modify

**Create:**
- `services/contacts-api/src/Propely.ContactsApi.Client/Propely.ContactsApi.Client.csproj`
- `services/contacts-api/src/Propely.ContactsApi.Client/IContactsApi.cs`
- `services/contacts-api/src/Propely.ContactsApi.Client/ILeadsApi.cs`
- `services/contacts-api/src/Propely.ContactsApi.Client/Models/ContactResponse.cs`
- `services/contacts-api/src/Propely.ContactsApi.Client/Models/CreateContactRequest.cs`
- `services/contacts-api/src/Propely.ContactsApi.Client/Models/UpdateContactRequest.cs`
- `services/contacts-api/src/Propely.ContactsApi.Client/Models/ContactListResponse.cs`
- `services/contacts-api/src/Propely.ContactsApi.Client/Models/LeadResponse.cs`
- `services/contacts-api/src/Propely.ContactsApi.Client/Models/CreateLeadRequest.cs`
- `services/contacts-api/src/Propely.ContactsApi.Client/Models/UpdateLeadRequest.cs`
- `services/contacts-api/src/Propely.ContactsApi.Client/Models/AssignLeadRequest.cs`
- `services/contacts-api/src/Propely.ContactsApi.Client/Models/ChangeStatusRequest.cs`
- `services/contacts-api/src/Propely.ContactsApi.Client/Models/ConvertLeadRequest.cs`
- `services/contacts-api/src/Propely.ContactsApi.Client/Models/ConvertLeadResponse.cs`
- `services/contacts-api/src/Propely.ContactsApi.Client/Models/LeadListResponse.cs`
- `services/contacts-api/src/Propely.ContactsApi.Client/ServiceCollectionExtensions.cs`
- `services/contacts-api/tests/Propely.ContactsApi.Client.Tests/ServiceCollectionExtensionsTests.cs`

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | All Refit interfaces resolve from DI | xUnit, `ServiceProvider` |
| Unit | Polly retry triggers on 500, 502, 503, 408 | xUnit, mock `HttpMessageHandler` |
| Unit | Polly timeout at 30s | xUnit, delayed mock handler |
| Unit | DTO serialization round-trip | xUnit, `System.Text.Json` |
| Integration | Client calls to running orgs-api | Manual, dev environment |

### Security & Privacy

- Client transmits PII (contact/lead data); ensure HTTPS in production
- No secrets stored in the client package
- Base URL configured at registration time

### TDD Reminder

Write DI resolution tests first. Then implement registration extension. Write Polly policy tests, then configure policies.

---

## Task 4.6 -- Frontend Contacts List & Detail

**Status:** PENDING
**Dependencies:** 4.3 (API endpoints must be available)

### Scope

**In scope:**
- Contacts list page at `/[locale]/(dashboard)/contacts`
- Contact detail page at `/[locale]/(dashboard)/contacts/[id]`
- Create contact modal/page
- Edit contact modal/page
- Search, filter by role, pagination
- Contact detail shows property interests with links to properties
- Stitch design for all screens
- i18n support (en, es)

**Out of scope:**
- Lead management UI (task 4.7)
- Bulk operations (import/export)
- Contact photo/avatar

### Acceptance Criteria

- [ ] Contacts list page displays paginated table with columns: Name, Email, Phone, Roles, Created
- [ ] Search bar filters contacts by name or email (debounced, 300ms)
- [ ] Role filter dropdown allows selecting one or more roles
- [ ] Pagination controls with page size selector (10, 20, 50)
- [ ] "New Contact" button opens a creation form
- [ ] Creation form includes: first name, last name, email, phone, secondary phone, company, notes, roles (multi-select), preferred language
- [ ] Form validation matches backend rules (required fields, email format)
- [ ] Clicking a contact row navigates to detail page
- [ ] Detail page shows all contact fields in a clean card layout
- [ ] Detail page shows "Property Interests" section listing linked properties with interest type
- [ ] Edit button opens edit form pre-filled with current data
- [ ] Delete button with confirmation dialog, soft-deletes the contact
- [ ] Empty state when no contacts exist with call-to-action
- [ ] Loading skeletons during data fetch
- [ ] Error states with retry option
- [ ] Stitch designs exist for: contacts list, contact detail, create/edit form
- [ ] All components tested with Vitest + RTL
- [ ] Responsive layout >= 375px

### Implementation Steps

1. Create Stitch designs for contacts list, detail, and create/edit form
2. Download Stitch HTML to `.stitch-html/`
3. Create `useContacts` hook (list with pagination, search, role filter)
4. Create `useContact` hook (single contact by ID)
5. Create `useCreateContact` hook (mutation)
6. Create `useUpdateContact` hook (mutation)
7. Create `useDeleteContact` hook (mutation)
8. Create `ContactsListPage` component
9. Create `ContactDetailPage` component with property interests section
10. Create `ContactForm` component (shared between create and edit)
11. Create `PropertyInterestsList` component
12. Add routes to Next.js app router
13. Add i18n keys for contacts
14. Write component tests
15. Verify against Stitch designs

### Files to Create/Modify

**Create:**
- `apps/web/src/app/[locale]/(dashboard)/contacts/page.tsx`
- `apps/web/src/app/[locale]/(dashboard)/contacts/[id]/page.tsx`
- `apps/web/src/components/contacts/ContactsTable.tsx`
- `apps/web/src/components/contacts/ContactForm.tsx`
- `apps/web/src/components/contacts/ContactDetail.tsx`
- `apps/web/src/components/contacts/PropertyInterestsList.tsx`
- `apps/web/src/components/contacts/ContactRoleBadge.tsx`
- `apps/web/src/hooks/useContacts.ts`
- `apps/web/src/hooks/useContact.ts`
- `apps/web/src/hooks/useCreateContact.ts`
- `apps/web/src/hooks/useUpdateContact.ts`
- `apps/web/src/hooks/useDeleteContact.ts`
- `apps/web/src/schemas/contactSchema.ts`
- `apps/web/src/components/contacts/__tests__/ContactsTable.test.tsx`
- `apps/web/src/components/contacts/__tests__/ContactForm.test.tsx`
- `apps/web/src/components/contacts/__tests__/ContactDetail.test.tsx`
- `.stitch-html/contacts-list.html`
- `.stitch-html/contact-detail.html`
- `.stitch-html/contact-form.html`

**Modify:**
- `apps/web/src/messages/en.json` (add contacts i18n keys)
- `apps/web/src/messages/es.json` (add contacts i18n keys)
- `apps/web/src/components/layout/DashboardSidebar.tsx` (add Contacts nav item)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `ContactsTable` renders rows, handles empty state | Vitest + RTL |
| Unit | `ContactForm` validates required fields, submits correctly | Vitest + RTL |
| Unit | `ContactDetail` displays all fields, property interests | Vitest + RTL |
| Unit | `ContactRoleBadge` renders correct color per role | Vitest + RTL |
| Unit | Hooks handle loading, success, error states | Vitest, mock fetch |
| Unit | `contactSchema` validates required fields, email format | Vitest |
| Manual | Full CRUD flow through UI | Dev environment |
| Manual | Visual comparison against Stitch designs | Dev environment |

### Security & Privacy

- Contact data displayed only within authenticated, tenant-scoped context
- Delete confirmation prevents accidental data loss
- No PII stored in browser storage

### TDD Reminder

Write schema validation tests and component render tests first. Then implement the form schema, components, and pages.

---

## Task 4.7 -- Frontend Lead Pipeline

**Status:** PENDING
**Dependencies:** 4.3 (API endpoints), 4.4 (conversion endpoint)

### Scope

**In scope:**
- Lead pipeline page at `/[locale]/(dashboard)/leads`
- Dual view: kanban board (by status columns) and list view (table)
- Kanban columns: New, Contacted, Qualified, Converted, Lost
- Drag-and-drop to change lead status (with validation for allowed transitions)
- Lead card shows: name, property reference, source, assigned agent, age
- Click lead card to open detail panel/modal
- Lead detail shows all fields, property link, assigned agent, conversion history
- Conversion modal: triggered from Qualified leads, shows contact preview, role selector
- Filter by: property, assigned agent, source, date range
- Stitch design for all screens
- i18n support (en, es)

**Out of scope:**
- Lead scoring
- Automated lead assignment
- Email integration (sending emails from lead detail)

### Acceptance Criteria

- [ ] Lead pipeline page has toggle between Kanban and List views
- [ ] Kanban view shows 5 columns: New, Contacted, Qualified, Converted, Lost
- [ ] Lead cards display: name, property short reference, source badge, assigned agent avatar/initials, days since creation
- [ ] Drag-and-drop a card between columns triggers status change API call
- [ ] Invalid transitions (e.g., Converted -> New) show error toast and revert card position
- [ ] List view shows sortable table with columns: Name, Email, Property, Status, Source, Agent, Created
- [ ] Clicking a lead opens a detail panel (slide-over or modal)
- [ ] Detail panel shows all lead fields with links to property and contact (if converted)
- [ ] "Convert" button appears only on Qualified leads
- [ ] Conversion modal shows: contact preview (what will be created/merged), role selector, notes field
- [ ] After successful conversion, lead card moves to Converted column and shows contact link
- [ ] Filters: property dropdown, agent dropdown, source text input, date range picker
- [ ] Filters persist in URL query params for shareable links
- [ ] Empty state per column ("No leads in this status")
- [ ] Loading skeletons for initial load and column-level loading for drag operations
- [ ] Stitch designs exist for: kanban board, list view, lead detail, conversion modal
- [ ] All components tested with Vitest + RTL
- [ ] Responsive: kanban scrolls horizontally on mobile, list view remains usable

### Implementation Steps

1. Create Stitch designs for kanban board, list view, lead detail, conversion modal
2. Download Stitch HTML to `.stitch-html/`
3. Create `useLeads` hook (list with filters and pagination)
4. Create `useLead` hook (single lead by ID)
5. Create `useChangeLeadStatus` hook (mutation)
6. Create `useConvertLead` hook (mutation)
7. Install and configure `@dnd-kit/core` for drag-and-drop
8. Create `LeadPipelinePage` component with view toggle
9. Create `LeadKanbanBoard` component with status columns
10. Create `LeadKanbanCard` component
11. Create `LeadListView` component (table)
12. Create `LeadDetailPanel` component (slide-over)
13. Create `LeadConversionModal` component
14. Create `LeadStatusBadge` component
15. Create `LeadSourceBadge` component
16. Create `LeadFilters` component
17. Add routes to Next.js app router
18. Add i18n keys
19. Write component tests
20. Verify against Stitch designs

### Files to Create/Modify

**Create:**
- `apps/web/src/app/[locale]/(dashboard)/leads/page.tsx`
- `apps/web/src/components/leads/LeadKanbanBoard.tsx`
- `apps/web/src/components/leads/LeadKanbanCard.tsx`
- `apps/web/src/components/leads/LeadKanbanColumn.tsx`
- `apps/web/src/components/leads/LeadListView.tsx`
- `apps/web/src/components/leads/LeadDetailPanel.tsx`
- `apps/web/src/components/leads/LeadConversionModal.tsx`
- `apps/web/src/components/leads/LeadStatusBadge.tsx`
- `apps/web/src/components/leads/LeadSourceBadge.tsx`
- `apps/web/src/components/leads/LeadFilters.tsx`
- `apps/web/src/hooks/useLeads.ts`
- `apps/web/src/hooks/useLead.ts`
- `apps/web/src/hooks/useChangeLeadStatus.ts`
- `apps/web/src/hooks/useConvertLead.ts`
- `apps/web/src/components/leads/__tests__/LeadKanbanBoard.test.tsx`
- `apps/web/src/components/leads/__tests__/LeadKanbanCard.test.tsx`
- `apps/web/src/components/leads/__tests__/LeadListView.test.tsx`
- `apps/web/src/components/leads/__tests__/LeadConversionModal.test.tsx`
- `apps/web/src/components/leads/__tests__/LeadFilters.test.tsx`
- `.stitch-html/lead-kanban.html`
- `.stitch-html/lead-list.html`
- `.stitch-html/lead-detail.html`
- `.stitch-html/lead-conversion-modal.html`

**Modify:**
- `apps/web/src/messages/en.json` (add leads i18n keys)
- `apps/web/src/messages/es.json` (add leads i18n keys)
- `apps/web/src/components/layout/DashboardSidebar.tsx` (add Leads nav item)
- `apps/web/package.json` (add `@dnd-kit/core`, `@dnd-kit/sortable`)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `LeadKanbanBoard` renders correct columns | Vitest + RTL |
| Unit | `LeadKanbanCard` displays lead info correctly | Vitest + RTL |
| Unit | Drag-and-drop triggers status change callback | Vitest + RTL + `@dnd-kit` test utils |
| Unit | Invalid transition shows error and reverts | Vitest + RTL |
| Unit | `LeadListView` renders sortable table | Vitest + RTL |
| Unit | `LeadConversionModal` submits with role and notes | Vitest + RTL |
| Unit | `LeadFilters` updates URL params | Vitest + RTL |
| Unit | Hooks handle loading, success, error states | Vitest, mock fetch |
| Manual | Full kanban drag-and-drop flow | Dev environment |
| Manual | Lead conversion end-to-end | Dev environment |
| Manual | Visual comparison against Stitch designs | Dev environment |

### Security & Privacy

- Lead data displayed only within authenticated, tenant-scoped context
- Conversion modal handles PII creation; no extra client-side storage
- URL query params may contain filter values but no PII (only IDs and enum values)

### TDD Reminder

Write component render tests and interaction tests first. Define expected behavior for drag-and-drop transitions (valid and invalid). Then implement components.
