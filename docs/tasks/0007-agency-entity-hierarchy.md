# Task: 0007-agency-entity-hierarchy

## Metadata
- ID: 0007
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-21
- Related docs:
  - Walkthrough: `docs/walkthroughs/0007-agency-entity-hierarchy.md`
  - Epic: `docs/backlog/epic-P1-agency-permissions.md` (Task 1.1)

## Goal
Introduce the `Agency` aggregate root into the orgs-api Domain layer and establish the Agency-to-Branch (Organization) parent-child relationship. Create the full CQRS stack (commands, queries, handlers) and API endpoints for agency management. Rename `MembershipRole.Member` to `MembershipRole.Agent`.

## Context
Propely needs a multi-branch agency hierarchy. Currently the Organization entity represents a branch/tenant but has no parent grouping. Agencies are the parent entity that owns one or more Organizations (branches). This is the first task in Phase 1 (Agency Hierarchy & Permissions).

## Scope
### In scope
- `Agency` aggregate root with Id, Name, Slug, CreatedByUserId, timestamps, soft-delete
- `AgencySlug` value object with validation (lowercase, alphanumeric + hyphens, 3-50 chars)
- Domain events: `AgencyCreatedV1`, `BranchAddedToAgencyV1`, `BranchRemovedFromAgencyV1`
- `Organization.AgencyId` optional FK
- `MembershipRole.Member` → `MembershipRole.Agent` rename
- `IAgencyRepository` interface and `AgencyRepository` implementation
- CQRS commands: CreateAgency, AddBranchToAgency, RemoveBranchFromAgency
- CQRS queries: GetAgencyById, ListAgenciesForUser
- API endpoints: POST/GET /api/agencies, GET /api/agencies/{id}, POST /api/agencies/{id}/branches, DELETE /api/agencies/{id}/branches/{branchId}
- EF Core configuration and migration
- Unit tests for domain entities and handlers
- Integration tests for API endpoints

### Out of scope
- Permission system (Task 1.2)
- Agency billing or subscription management
- Branch creation (only linking existing orgs)
- Frontend screens (Task 1.5)

## Requirements
- R1: Agency aggregate root follows existing Entity/ISoftDeletable patterns
- R2: AgencySlug validated with regex `^[a-z0-9][a-z0-9-]{1,48}[a-z0-9]$`
- R3: Domain events raised on create, add branch, remove branch
- R4: Only agency owners can add/remove branches
- R5: Slug must be globally unique
- R6: MembershipRole.Member renamed to Agent without breaking existing data

## Acceptance Criteria
- AC1: `Agency` entity exists with Id, Name, Slug, CreatedByUserId, timestamps, soft-delete
- AC2: `Agency.Create(name, slug, createdByUserId)` validates and raises `AgencyCreatedV1`
- AC3: `Agency.AddBranch(organizationId)` raises event; duplicate throws `DomainException`
- AC4: `Agency.RemoveBranch(organizationId)` raises event; missing branch throws `NotFoundException`
- AC5: `Organization` has optional `AgencyId`; migration adds nullable FK
- AC6: `MembershipRole` enum has Owner, Admin, Agent, Viewer
- AC7: `POST /api/agencies` creates agency, returns 201
- AC8: `GET /api/agencies` returns agencies for authenticated user
- AC9: `POST /api/agencies/{id}/branches` links org; only owners allowed
- AC10: `dotnet test` passes all new and existing tests

## Constraints (non-negotiable)
- Clean Architecture layers respected.
- English-only repo content.
- No secrets in repo.
- Update walkthrough.
- TDD: write tests first, then production code.

## Proposed Approach (high-level)
Follow Clean Architecture layer order: Domain → Application → Infrastructure → API. Write unit tests for domain entities first, then integration tests for endpoints.

## Implementation Steps
1. Create Agency entity, AgencySlug value object, domain events
2. Update Organization with AgencyId, rename MembershipRole.Member to Agent
3. Create IAgencyRepository, DTOs, commands, queries in Application layer
4. Create AgencyConfiguration, update OrganizationConfiguration, update DbContext
5. Create AgencyRepository, register in DI
6. Create EF Core migration
7. Create AgenciesController with endpoints
8. Create API DTOs and validators
9. Write unit tests for domain and application layers
10. Write integration tests for API endpoints
11. Update all references to MembershipRole.Member

## Files to Create / Modify
- Create: Domain/Agencies/Agency.cs, AgencySlug.cs, Events/*.cs
- Create: Application/Agencies/ (interfaces, DTOs, commands, queries)
- Create: Infrastructure/Persistence/Configurations/AgencyConfiguration.cs
- Create: Infrastructure/Persistence/Repositories/AgencyRepository.cs
- Create: Api/Controllers/AgenciesController.cs, Dtos/, Validators/
- Modify: Organization.cs, MembershipRole.cs, AppDbContext.cs, OrganizationConfiguration.cs, DependencyInjection.cs

## Testing Plan
- Unit tests: Agency entity creation/validation/events, AgencySlug validation, command handlers
- Integration tests: All API endpoints with auth scenarios
- Manual verification: None required

## Security & Privacy
- Only agency owners can add/remove branches
- Agency slug globally unique to prevent impersonation
- CreatedByUserId immutable after creation
- No cross-tenant data leakage

## Observability
- Logs: Agency creation, branch management logged via audit context
- Metrics: Standard MediatR pipeline logging
- Traces: OpenTelemetry auto-instrumentation

## Rollback Plan
Revert the EF Core migration and all code changes via git revert.

## Definition of Done Checklist
- [ ] Acceptance criteria met
- [ ] Build passes
- [ ] Tests added/updated and pass
- [ ] Formatting/analyzers pass
- [ ] No secrets committed
- [ ] Walkthrough updated
