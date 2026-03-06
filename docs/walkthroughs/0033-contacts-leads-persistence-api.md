# Walkthrough: 0033-contacts-leads-persistence-api

## Task Reference
- Task: `docs/tasks/0033-contacts-leads-persistence-api.md`
- Walkthrough: `docs/walkthroughs/0033-contacts-leads-persistence-api.md`
- Branch/PR: `feat/p4-contacts-leads`
- Date: `2026-03-06`

## Summary
Implemented the full CQRS stack for contacts and leads in the contacts-api service: Application layer (repository interfaces, DTOs, mappers, MediatR command/query handlers, FluentValidation validators), Infrastructure layer (EF Core configurations with JSONB roles, repositories with tenant filtering, database migration), and API layer (ContactsController, LeadsController, ClaimsPrincipalExtensions, request DTOs). This is the largest task in Epic P4, delivering 185 tests across unit, architecture, and integration layers.

## Context
- Background: Contact (task 0031) and Lead (task 0032) domain models were complete but had no persistence, business logic orchestration, or API exposure.
- Problem statement: The domain entities needed to be connected to the database, wrapped in CQRS handlers for business logic orchestration, and exposed via REST API endpoints.
- Constraints: Clean Architecture, CQRS with MediatR, tenant isolation, TDD, JSONB for roles storage, deduplication enforcement.

## Decisions & Trade-offs
- **Decision: JSONB for Contact Roles storage**
  - Options considered: (1) Separate `contact_roles` join table, (2) JSONB array column, (3) Comma-separated string
  - Why this choice: JSONB is native to PostgreSQL, supports indexing and querying, and avoids the overhead of a join table for a small, bounded set of enum values.
  - Consequences: EF Core value converter needed to serialize/deserialize `List<ContactRole>` to JSONB.

- **Decision: Separate read and write repositories**
  - Options considered: (1) Single repository per entity, (2) CQRS-style split (IContactRepository for writes, IContactReadRepository for reads)
  - Why this choice: Follows CQRS principle. Read repositories can optimize for query patterns (projections, includes) without affecting write side.
  - Consequences: More interfaces to maintain, but cleaner separation of concerns.

- **Decision: ClaimsPrincipalExtensions for user/tenant extraction**
  - Why: Centralized claim extraction avoids repetitive string-based claim access in controllers. Consistent with the pattern in orgs-api.

- **Decision: Deduplication via 409 Conflict response**
  - Why: HTTP 409 is the standard status for resource conflicts. The unique index on Email+PropertyId+TenantId catches duplicates at the database level, and the handler returns a typed conflict response.

## Implementation Notes
- Key changes:
  - Full Application layer with 8 commands, 4 queries, their handlers and validators
  - ContactMapper and LeadMapper for entity-to-DTO mapping
  - EF Core configurations: ContactConfiguration (JSONB roles, snake_case columns), ContactPropertyInterestConfiguration (composite unique index), LeadConfiguration (unique index on email+propertyId+tenantId)
  - Four repository implementations with tenant-scoped queries
  - AppDbContext updated with DbSet<Contact>, DbSet<Lead>, DbSet<ContactPropertyInterest>
  - ContactsController: GET /api/contacts, GET /api/contacts/{id}, POST /api/contacts, PUT /api/contacts/{id}, DELETE /api/contacts/{id}
  - LeadsController: GET /api/leads, GET /api/leads/{id}, POST /api/leads, DELETE /api/leads/{id}, PUT /api/leads/{id}/assign, PUT /api/leads/{id}/status
  - ClaimsPrincipalExtensions for GetUserId(), GetTenantId(), IsAdmin()
- Edge cases handled:
  - Deduplication returns 409 Conflict with descriptive error message
  - Not found returns 404 with entity type and ID
  - Invalid status transition returns 400 with current and attempted status
  - Tenant isolation: all queries filtered by TenantId from JWT claims
  - Soft-deleted records excluded via global query filter
- Known limitations:
  - No cursor-based pagination (offset-based only)
  - No full-text search (LIKE-based search on name/email fields)
  - Authorization is role-based (admin/agent); no fine-grained permission checks yet

## Data / Schema / Migrations
- DB changes:
  - New table `contacts` with columns: id, first_name, last_name, email, phone, secondary_phone, company, notes, preferred_language, source, roles (jsonb), tenant_id, created_at_utc, updated_at_utc, is_deleted, deleted_at_utc
  - New table `contact_property_interests` with columns: id, contact_id (FK), property_id, interest_type, notes, created_at_utc
  - Composite unique index on contact_property_interests: contact_id + property_id + interest_type
  - New table `leads` with columns: id, name, email, phone, message, source, property_id, tenant_id, assigned_agent_id, contact_id, status, created_at_utc, updated_at_utc, is_deleted, deleted_at_utc
  - Unique index on leads: email + property_id + tenant_id (filtered by is_deleted = false)
  - Global query filters for tenant isolation and soft-delete
- Migration: Applied cleanly on fresh database
- Backward compatibility: N/A (new tables only)

## Commands Run
```bash
dotnet ef migrations add AddContactsAndLeads \
  --project services/contacts-api/src/Propely.ContactsApi.Infrastructure \
  --startup-project services/contacts-api/src/Propely.ContactsApi.Api
dotnet build services/contacts-api/Propely.ContactsApi.sln
dotnet test services/contacts-api/Propely.ContactsApi.sln
```

## Files Changed

**Created (Application -- Contacts):**
- `services/contacts-api/src/Propely.ContactsApi.Application/Contacts/Interfaces/IContactRepository.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Contacts/Interfaces/IContactReadRepository.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Contacts/Dtos/ContactDto.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Contacts/Dtos/ContactListItemDto.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Contacts/Mapping/ContactMapper.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Contacts/Commands/CreateContact/` (Command, Handler, Validator)
- `services/contacts-api/src/Propely.ContactsApi.Application/Contacts/Commands/UpdateContact/` (Command, Handler)
- `services/contacts-api/src/Propely.ContactsApi.Application/Contacts/Commands/DeleteContact/` (Command, Handler)
- `services/contacts-api/src/Propely.ContactsApi.Application/Contacts/Queries/GetContactById/` (Query, Handler)
- `services/contacts-api/src/Propely.ContactsApi.Application/Contacts/Queries/ListContacts/` (Query, Handler)

**Created (Application -- Leads):**
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Interfaces/ILeadRepository.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Interfaces/ILeadReadRepository.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Dtos/LeadDto.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Dtos/LeadListItemDto.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Mapping/LeadMapper.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Commands/CreateLead/` (Command, Handler, Validator)
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Commands/DeleteLead/` (Command, Handler)
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Commands/AssignLead/` (Command, Handler)
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Commands/ChangeLeadStatus/` (Command, Handler)
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Queries/GetLeadById/` (Query, Handler)
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Queries/ListLeads/` (Query, Handler)

**Created (Infrastructure):**
- `services/contacts-api/src/Propely.ContactsApi.Infrastructure/Persistence/Configurations/ContactConfiguration.cs` -- JSONB roles, snake_case columns
- `services/contacts-api/src/Propely.ContactsApi.Infrastructure/Persistence/Configurations/ContactPropertyInterestConfiguration.cs` -- Composite unique index
- `services/contacts-api/src/Propely.ContactsApi.Infrastructure/Persistence/Configurations/LeadConfiguration.cs` -- Unique index on email+propertyId+tenantId
- `services/contacts-api/src/Propely.ContactsApi.Infrastructure/Persistence/Repositories/ContactRepository.cs`
- `services/contacts-api/src/Propely.ContactsApi.Infrastructure/Persistence/Repositories/ContactReadRepository.cs`
- `services/contacts-api/src/Propely.ContactsApi.Infrastructure/Persistence/Repositories/LeadRepository.cs`
- `services/contacts-api/src/Propely.ContactsApi.Infrastructure/Persistence/Repositories/LeadReadRepository.cs`

**Created (API):**
- `services/contacts-api/src/Propely.ContactsApi.Api/Controllers/ContactsController.cs` -- CRUD endpoints
- `services/contacts-api/src/Propely.ContactsApi.Api/Controllers/LeadsController.cs` -- CRUD + assign + status endpoints
- `services/contacts-api/src/Propely.ContactsApi.Api/Extensions/ClaimsPrincipalExtensions.cs`
- `services/contacts-api/src/Propely.ContactsApi.Api/Dtos/` -- Request DTOs for all endpoints

**Modified:**
- `services/contacts-api/src/Propely.ContactsApi.Infrastructure/Persistence/AppDbContext.cs` -- Added DbSets
- `services/contacts-api/src/Propely.ContactsApi.Infrastructure/DependencyInjection.cs` -- Registered repositories

## Tests
### Unit
- What was added/updated: Handler tests for all 8 commands and 4 queries, validator tests, mapper tests
- How to run: `dotnet test services/contacts-api/tests/Propely.ContactsApi.UnitTests/`

### Integration
- What was added/updated: Endpoint tests for all ContactsController and LeadsController actions, deduplication (409), authorization, tenant isolation
- How to run: `dotnet test services/contacts-api/tests/Propely.ContactsApi.IntegrationTests/`

### Manual
- What you verified: CRUD operations via API, database state verification
- Steps: Start contacts-api, create/read/update/delete contacts and leads via HTTP client

## Observability
- Logs added/updated: MediatR pipeline logging for all commands/queries
- Traces/metrics added/updated: OpenTelemetry auto-instrumentation for EF Core queries

## Security
- Validation: FluentValidation on all commands (required fields, email format, enum values)
- AuthN/AuthZ impact: JWT auth required on all endpoints; admin sees all, agent sees own data
- Sensitive data handling: PII (email, phone, name) not logged; tenant isolation via global query filter

## Follow-ups / Backlog
- [x] Task 0034: Lead Conversion Flow
- [x] Task 0035: Contacts-API SDK Client
- [x] Task 0036: Frontend Contacts UI
- [x] Task 0037: Frontend Lead Pipeline

## Checklist
- [x] Task scope matches `docs/tasks/0033-contacts-leads-persistence-api.md`
- [x] Tests updated and passing (185 total)
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
