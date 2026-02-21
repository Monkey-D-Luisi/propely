# Walkthrough: 0007-agency-entity-hierarchy

## Task Reference
- Task: `docs/tasks/0007-agency-entity-hierarchy.md`
- Walkthrough: `docs/walkthroughs/0007-agency-entity-hierarchy.md`
- Branch/PR: `feat/0007-agency-entity-hierarchy` / TBD
- Date: `2026-02-21`

## Summary
Introduced the `Agency` aggregate root into the orgs-api Domain layer, establishing the Agency-to-Branch (Organization) parent-child hierarchy for multi-branch real estate agencies. Renamed `MembershipRole.Member` to `MembershipRole.Agent` to align with Propely terminology. Created the full CQRS stack (commands, queries, handlers) and REST API endpoints for agency CRUD and branch management. Added EF Core migration for the `agencies` table and `Organization.AgencyId` FK.

## Context
- Background: Propely requires agencies to group multiple branches (organizations) under a single parent entity, enabling multi-branch real estate agencies.
- Problem statement: The Organization entity existed in isolation with no parent grouping. There was no way to represent that multiple branches belong to the same agency.
- Constraints: Clean Architecture layers, domain event patterns, snake_case DB columns, TDD.

## Decisions & Trade-offs
- **Decision:** Store `BranchIds` as a derived collection loaded from `Organization.AgencyId` FK rather than a separate join table.
  - Options considered: (1) Join table `agency_branches`, (2) FK on Organization pointing to Agency
  - Why this choice: Simpler schema, single source of truth (Organization.AgencyId), no join table maintenance
  - Consequences: The `Agency.BranchIds` list is loaded separately in the repository by querying Organizations with matching AgencyId. Domain events raised during loading are cleared.

- **Decision:** `AgencySlug` as a value object with regex validation `^[a-z0-9][a-z0-9-]{1,48}[a-z0-9]$`.
  - Why: Enforces URL-safe, lowercase slugs between 3-50 characters. Stored as a string column with EF Core value conversion.

- **Decision:** Authorization by `CreatedByUserId` equality for owner-only actions.
  - Why: Simplest approach before the full permission system (Task 1.2). Will be superseded by `IPermissionEvaluator` in later tasks.

## Implementation Notes
- Key changes:
  - `Agency` aggregate root with `Create`, `AddBranch`, `RemoveBranch`, `Update`, `SoftDelete` methods
  - `AgencySlug` value object with `GeneratedRegex` for validation
  - Three domain events: `AgencyCreatedV1`, `BranchAddedToAgencyV1`, `BranchRemovedFromAgencyV1`
  - `Organization.AgencyId` nullable FK, `AssignToAgency()` and `RemoveFromAgency()` methods
  - `MembershipRole.Member` renamed to `MembershipRole.Agent` across the entire codebase
  - Full CQRS stack: CreateAgency, AddBranchToAgency, RemoveBranchFromAgency commands; GetAgencyById, ListAgenciesForUser queries
  - `AgenciesController` with REST endpoints: POST/GET /api/agencies, GET /api/agencies/{id}, POST /api/agencies/{id}/branches, DELETE /api/agencies/{id}/branches/{branchId}
- Edge cases handled:
  - Duplicate branch addition throws `DomainException`
  - Removing non-member branch throws `NotFoundException`
  - Organization already assigned to another agency throws `DomainException`
  - Access control: only agency creator (owner) can add/remove branches
  - Agency listing includes both owned agencies and agencies where user is a branch member
- Known limitations:
  - Owner is determined by `CreatedByUserId` only (no role-based ownership yet -- deferred to Permission tasks)
  - No agency deletion endpoint yet (will be added with Agency Management UI task 1.5)

## Data / Schema / Migrations
- DB changes:
  - New table `agencies` with columns: `id`, `name`, `slug`, `created_by_user_id`, `created_at_utc`, `updated_at_utc`, `is_deleted`, `deleted_at_utc`
  - Unique index `ix_agencies_slug_unique` on `slug` with filter `is_deleted = false`
  - New nullable column `agency_id` (uuid) on `organizations` table
  - Data migration: `UPDATE memberships SET role = 'Agent' WHERE role = 'Member'` and same for `invitations`
- Migration: `20260221163758_AddAgencyEntity`
- Backward compatibility: The role rename migration handles existing data. The `agency_id` column is nullable so existing organizations are unaffected.

## Commands Run
```bash
dotnet ef migrations add AddAgencyEntity \
  --project services/orgs-api/src/Propely.OrgsApi.Infrastructure \
  --startup-project services/orgs-api/src/Propely.OrgsApi.Api
dotnet build services/orgs-api/Propely.OrgsApi.sln
dotnet test services/orgs-api/tests/Propely.OrgsApi.UnitTests/
dotnet test services/orgs-api/tests/Propely.OrgsApi.ArchitectureTests/
```

## Files Changed

**Created (Domain):**
- `services/orgs-api/src/Propely.OrgsApi.Domain/Agencies/Agency.cs` -- Agency aggregate root
- `services/orgs-api/src/Propely.OrgsApi.Domain/Agencies/AgencySlug.cs` -- Slug value object
- `services/orgs-api/src/Propely.OrgsApi.Domain/Agencies/Events/AgencyCreatedV1.cs` -- Domain event
- `services/orgs-api/src/Propely.OrgsApi.Domain/Agencies/Events/BranchAddedToAgencyV1.cs` -- Domain event
- `services/orgs-api/src/Propely.OrgsApi.Domain/Agencies/Events/BranchRemovedFromAgencyV1.cs` -- Domain event

**Created (Application):**
- `services/orgs-api/src/Propely.OrgsApi.Application/Agencies/Interfaces/IAgencyRepository.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Agencies/DTOs/AgencyDto.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Agencies/DTOs/AgencyDetailDto.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Agencies/DTOs/AgencyBranchDto.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Agencies/Commands/CreateAgency/CreateAgencyCommand.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Agencies/Commands/CreateAgency/CreateAgencyCommandHandler.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Agencies/Commands/CreateAgency/CreateAgencyCommandValidator.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Agencies/Commands/AddBranchToAgency/AddBranchToAgencyCommand.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Agencies/Commands/AddBranchToAgency/AddBranchToAgencyCommandHandler.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Agencies/Commands/RemoveBranchFromAgency/RemoveBranchFromAgencyCommand.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Agencies/Commands/RemoveBranchFromAgency/RemoveBranchFromAgencyCommandHandler.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Agencies/Queries/GetAgencyById/GetAgencyByIdQuery.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Agencies/Queries/GetAgencyById/GetAgencyByIdQueryHandler.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Agencies/Queries/ListAgenciesForUser/ListAgenciesForUserQuery.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Agencies/Queries/ListAgenciesForUser/ListAgenciesForUserQueryHandler.cs`

**Created (Infrastructure):**
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/Persistence/Configurations/AgencyConfiguration.cs`
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/Persistence/Repositories/AgencyRepository.cs`
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/Migrations/20260221163758_AddAgencyEntity.cs` (+ Designer)

**Created (API):**
- `services/orgs-api/src/Propely.OrgsApi.Api/Controllers/AgenciesController.cs`
- `services/orgs-api/src/Propely.OrgsApi.Api/Dtos/CreateAgencyRequest.cs`
- `services/orgs-api/src/Propely.OrgsApi.Api/Dtos/AddBranchRequest.cs`
- `services/orgs-api/src/Propely.OrgsApi.Api/Validators/CreateAgencyRequestValidator.cs`
- `services/orgs-api/src/Propely.OrgsApi.Api/Validators/AddBranchRequestValidator.cs`

**Created (Tests):**
- `services/orgs-api/tests/Propely.OrgsApi.UnitTests/Domain/Agencies/AgencyTests.cs` -- 14 tests
- `services/orgs-api/tests/Propely.OrgsApi.UnitTests/Domain/Agencies/AgencySlugTests.cs` -- 10 tests

**Modified:**
- `services/orgs-api/src/Propely.OrgsApi.Domain/Organizations/Organization.cs` -- Added AgencyId, AssignToAgency(), RemoveFromAgency()
- `services/orgs-api/src/Propely.OrgsApi.Domain/Organizations/MembershipRole.cs` -- Renamed Member to Agent
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/Persistence/AppDbContext.cs` -- Added DbSet<Agency>, AgencyConfiguration
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/Persistence/Configurations/OrganizationConfiguration.cs` -- Added AgencyId column
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/DependencyInjection.cs` -- Registered IAgencyRepository
- 19 test files -- Updated MembershipRole.Member to MembershipRole.Agent references

## Tests
### Unit
- 24 new tests added (14 AgencyTests + 10 AgencySlugTests)
- All 486 unit tests pass
- All 5 architecture tests pass
- Run: `dotnet test services/orgs-api/tests/Propely.OrgsApi.UnitTests/`

### Integration
- Updated string literals from "Member" to "Agent" in integration tests
- Integration tests not run locally (require Docker containers)

### Manual
- N/A

## Observability
- Domain events automatically dispatched to outbox (existing infrastructure)
- Audit logging automatic via AppDbContext for all Agency entity changes

## Security
- Validation: Name 1-200 chars, Slug 3-50 chars lowercase alphanumeric+hyphens
- AuthN/AuthZ: JWT auth required, owner-only checks for branch management
- Sensitive data: No PII stored in Agency entity beyond CreatedByUserId

## Follow-ups / Backlog
- [ ] Task 1.2: Permission Domain Model (depends on this task)
- [ ] Task 1.5: Agency Management UI (depends on this task)

## Checklist
- [x] Task scope matches `docs/tasks/0007-agency-entity-hierarchy.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
