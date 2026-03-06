# Task: 0033-contacts-leads-persistence-api

## Metadata
- ID: 0033
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-03-06
- Related docs:
  - Walkthrough: `docs/walkthroughs/0033-contacts-leads-persistence-api.md`
  - Epic: `docs/backlog/epic-P4-contacts-leads.md` (Task 4.3)

## Goal
Implement EF Core persistence, CQRS command/query handlers (MediatR), and REST API endpoints for contacts and leads in the contacts-api service. This delivers the full backend stack from Application through Infrastructure to API layers.

## Context
The Contact (task 0031) and Lead (task 0032) domain models are complete. This task creates the full CQRS stack: repository interfaces, DTOs, mappers, command/query handlers, EF Core configurations, repository implementations, database migration, and REST controllers. This is the largest task in Epic P4, tying together all layers of the Clean Architecture stack.

## Scope
### In scope
- Application layer: Repository interfaces (IContactRepository, IContactReadRepository, ILeadRepository, ILeadReadRepository)
- Application layer: DTOs and mapper for contacts and leads
- Application layer: MediatR commands -- CreateContact, UpdateContact, DeleteContact, CreateLead, DeleteLead, AssignLead, ChangeLeadStatus
- Application layer: MediatR queries -- GetContactById, ListContacts, GetLeadById, ListLeads
- Application layer: FluentValidation validators for all commands
- Infrastructure layer: EF Core configurations (ContactConfiguration with JSONB roles, ContactPropertyInterestConfiguration, LeadConfiguration with unique index)
- Infrastructure layer: Repository implementations with tenant filtering
- Infrastructure layer: AppDbContext update with new DbSets
- Infrastructure layer: EF Core migration
- API layer: ContactsController (GET list, GET by ID, POST, PUT, DELETE)
- API layer: LeadsController (GET list, GET by ID, POST, DELETE, PUT assign, PUT status)
- API layer: ClaimsPrincipalExtensions for extracting user/tenant claims
- API layer: Request DTOs for API input
- Authorization: agent sees own data, admin sees all within tenant
- Pagination, sorting, filtering on list endpoints
- Tenant query filter on all queries
- 185 total tests (unit + architecture + integration)

### Out of scope
- Lead conversion endpoint (task 0034)
- SDK client (task 0035)
- Frontend (tasks 0036, 0037)
- Bulk operations

## Requirements
- R1: ContactConfiguration maps Roles as JSONB array in PostgreSQL
- R2: LeadConfiguration has unique index on Email + PropertyId + TenantId
- R3: ContactPropertyInterestConfiguration has composite unique index on ContactId + PropertyId + InterestType
- R4: All repositories enforce tenant isolation via global query filter
- R5: Soft-deleted records excluded from all queries
- R6: List endpoints support pagination (page, pageSize), search, sorting, and type-specific filters
- R7: POST /api/leads enforces deduplication (409 Conflict if Email+PropertyId exists within tenant)
- R8: All endpoints require JWT authentication

## Acceptance Criteria
- AC1: `ContactConfiguration` maps `Contact` to `contacts` table with correct column types, indexes, and JSONB roles
- AC2: `ContactPropertyInterestConfiguration` maps with composite unique index on `ContactId` + `PropertyId` + `InterestType`
- AC3: `LeadConfiguration` maps `Lead` to `leads` table with unique index on `Email` + `PropertyId` + `TenantId`
- AC4: `IContactRepository` and `ILeadRepository` interfaces defined in Application layer
- AC5: Contacts CRUD endpoints return appropriate DTOs (not domain entities)
- AC6: Leads CRUD endpoints return appropriate DTOs
- AC7: `GET /api/contacts` supports: `?page=1&pageSize=20&search=term&role=Buyer&sortBy=lastName&sortDir=asc`
- AC8: `GET /api/leads` supports: `?page=1&pageSize=20&status=New&propertyId=guid&assignedAgentId=guid&sortBy=createdAt`
- AC9: `PUT /api/leads/{id}/assign` accepts `{ "agentId": "guid" }` and returns updated lead
- AC10: `PUT /api/leads/{id}/status` accepts `{ "status": "Contacted" }` and validates transition
- AC11: `POST /api/leads` returns 409 Conflict if email+propertyId already exists within tenant
- AC12: Authorization: agents see only their assigned leads and contacts they created; admins see all in tenant
- AC13: Soft-deleted records excluded from all queries
- AC14: Migration runs cleanly on a fresh database
- AC15: 185 total tests pass (unit, architecture, integration)

## Constraints (non-negotiable)
- Clean Architecture layers respected.
- English-only repo content.
- No secrets in repo.
- Update walkthrough.
- TDD: write tests first, then implementation.

## Proposed Approach (high-level)
Build layer by layer: Application (interfaces, DTOs, handlers) -> Infrastructure (EF Core, repositories, migration) -> API (controllers, request DTOs). Write tests at each layer before implementation.

## Implementation Steps
1. Create `IContactRepository` and `IContactReadRepository` interfaces in Application layer
2. Create `ILeadRepository` and `ILeadReadRepository` interfaces in Application layer
3. Create Contact DTOs (ContactDto, ContactListItemDto) and mapper
4. Create Lead DTOs (LeadDto, LeadListItemDto) and mapper
5. Create MediatR commands: CreateContactCommand, UpdateContactCommand, DeleteContactCommand with handlers and validators
6. Create MediatR commands: CreateLeadCommand, DeleteLeadCommand, AssignLeadCommand, ChangeLeadStatusCommand with handlers and validators
7. Create MediatR queries: GetContactByIdQuery, ListContactsQuery with handlers
8. Create MediatR queries: GetLeadByIdQuery, ListLeadsQuery with handlers
9. Create EF Core configurations: ContactConfiguration (JSONB roles), ContactPropertyInterestConfiguration, LeadConfiguration
10. Create repository implementations with tenant filtering
11. Update AppDbContext with new DbSets and configurations
12. Generate EF Core migration
13. Create ClaimsPrincipalExtensions for user/tenant claim extraction
14. Create ContactsController with CRUD endpoints
15. Create LeadsController with CRUD + assign + status endpoints
16. Create API request DTOs
17. Register all new services in DI
18. Write unit tests for all handlers and validators
19. Write architecture tests
20. Write integration tests for all endpoints

## Files to Create / Modify
**Create (Application):**
- `services/contacts-api/src/Propely.ContactsApi.Application/Contacts/Interfaces/IContactRepository.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Contacts/Interfaces/IContactReadRepository.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Contacts/Dtos/ContactDto.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Contacts/Dtos/ContactListItemDto.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Contacts/Mapping/ContactMapper.cs`
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
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Interfaces/ILeadRepository.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Interfaces/ILeadReadRepository.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Dtos/LeadDto.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Dtos/LeadListItemDto.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Mapping/LeadMapper.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Commands/CreateLead/CreateLeadCommand.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Commands/CreateLead/CreateLeadCommandHandler.cs`
- `services/contacts-api/src/Propely.ContactsApi.Application/Leads/Commands/CreateLead/CreateLeadCommandValidator.cs`
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

**Create (Infrastructure):**
- `services/contacts-api/src/Propely.ContactsApi.Infrastructure/Persistence/Configurations/ContactConfiguration.cs`
- `services/contacts-api/src/Propely.ContactsApi.Infrastructure/Persistence/Configurations/ContactPropertyInterestConfiguration.cs`
- `services/contacts-api/src/Propely.ContactsApi.Infrastructure/Persistence/Configurations/LeadConfiguration.cs`
- `services/contacts-api/src/Propely.ContactsApi.Infrastructure/Persistence/Repositories/ContactRepository.cs`
- `services/contacts-api/src/Propely.ContactsApi.Infrastructure/Persistence/Repositories/ContactReadRepository.cs`
- `services/contacts-api/src/Propely.ContactsApi.Infrastructure/Persistence/Repositories/LeadRepository.cs`
- `services/contacts-api/src/Propely.ContactsApi.Infrastructure/Persistence/Repositories/LeadReadRepository.cs`

**Create (API):**
- `services/contacts-api/src/Propely.ContactsApi.Api/Controllers/ContactsController.cs`
- `services/contacts-api/src/Propely.ContactsApi.Api/Controllers/LeadsController.cs`
- `services/contacts-api/src/Propely.ContactsApi.Api/Extensions/ClaimsPrincipalExtensions.cs`
- `services/contacts-api/src/Propely.ContactsApi.Api/Dtos/CreateContactRequest.cs`
- `services/contacts-api/src/Propely.ContactsApi.Api/Dtos/UpdateContactRequest.cs`
- `services/contacts-api/src/Propely.ContactsApi.Api/Dtos/CreateLeadRequest.cs`
- `services/contacts-api/src/Propely.ContactsApi.Api/Dtos/AssignLeadRequest.cs`
- `services/contacts-api/src/Propely.ContactsApi.Api/Dtos/ChangeLeadStatusRequest.cs`

**Modify:**
- `services/contacts-api/src/Propely.ContactsApi.Infrastructure/Persistence/AppDbContext.cs` -- Add DbSets for Contact, Lead, ContactPropertyInterest
- `services/contacts-api/src/Propely.ContactsApi.Infrastructure/DependencyInjection.cs` -- Register repositories

**Create (Tests):**
- Multiple test files across UnitTests, ArchitectureTests, and IntegrationTests projects (185 total tests)

## Testing Plan
- Unit tests: All command validators, all command handlers (mock repositories), all query handlers (mock read repositories)
- Architecture tests: Layer dependency validation (domain has no infrastructure references, etc.)
- Integration tests: Contact CRUD via HTTP, Lead CRUD via HTTP, deduplication 409, authorization, tenant isolation
- Manual verification: Create contacts and leads through API, verify database state

## Security & Privacy
- Tenant isolation enforced via global query filter on TenantId
- Agent-level authorization: agents see own data, admins see all within tenant
- Deduplication check is tenant-scoped
- PII fields (email, phone, name) must not appear in logs
- Soft-deleted records excluded from all queries but retained in database
- JWT authentication required on all endpoints

## Observability
- Logs: MediatR pipeline logging for all commands/queries
- Metrics: Standard request metrics via middleware
- Traces: OpenTelemetry auto-instrumentation for EF Core queries

## Rollback Plan
Revert the EF Core migration and all code changes via git revert. Run `dotnet ef database update <previous-migration>` to roll back the schema.

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass (185 total)
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
