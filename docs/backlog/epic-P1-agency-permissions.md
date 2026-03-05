# Epic P1 -- Agency Hierarchy & Permissions

## Overview

Extend orgs-api with an Agency hierarchy layer and a granular, override-capable permission system. This is foundational infrastructure -- every subsequent phase (properties, contacts, appointments, publishing) depends on the authorization model established here. The Agency entity introduces a parent grouping above the existing Organization (branch), enabling multi-branch real estate agencies. The permission system combines fixed roles (owner, admin, agent, viewer) with per-user overrides stored in `PermissionOverride(UserId, Permission, Granted, GrantedBy, GrantedAt)`, evaluated at runtime through `IPermissionEvaluator`.

**Target:** Agencies can create and manage branches, assign roles, and configure fine-grained permission overrides. All backend services can query permissions via the `Propely.OrgsApi.Client` SDK. Frontend screens exist for agency management and permission administration.

## Service Ownership

| Capability | Service |
|---|---|
| Agency & permission domain model | `services/orgs-api` (Domain) |
| Agency & permission persistence, authorization middleware | `services/orgs-api` (Infrastructure, Api) |
| NuGet SDK client for permission evaluation | `services/orgs-api` (Client package) |
| Agency management UI | `apps/web` |
| Permission management UI | `apps/web` |

## Organizational Hierarchy

```
Agency (new parent entity)
├── Branch A (existing Organization / tenant)
│   ├── Agent 1 (user + membership)
│   ├── Agent 2
│   └── Viewer 1
├── Branch B (Organization)
│   ├── Agent 3
│   └── Admin 1
└── Branch C (Organization)
    └── ...
```

- One `Organization` = one branch. The existing tenant isolation model is preserved.
- `Agency` is a new aggregate root that owns one or more Organizations.
- Agency owners have cross-branch visibility; all other roles are branch-scoped.

## Permission Model Summary

### Fixed Roles

| Role | Scope | Default Capabilities |
|---|---|---|
| `owner` | Agency | Full access across all branches, billing, agency settings |
| `admin` | Branch | Manage branch members, settings; see all branch data |
| `agent` | Branch | Manage own properties, contacts, appointments |
| `viewer` | Branch | Read-only access to branch data |

### Permission Overrides

`PermissionOverride(UserId, Permission, Granted: bool, GrantedBy, GrantedAt)` extends or restricts a user's base role:

| Permission | Description | Default for agent |
|---|---|---|
| `properties.view_all` | View all properties in the branch | No |
| `properties.edit_all` | Edit any property in the branch | No |
| `contacts.view_all` | View all contacts in the branch | No |
| `contacts.edit_all` | Edit any contact in the branch | No |
| `appointments.view_all` | View all appointments in the branch | No |
| `publishing.manage` | Publish/unpublish properties to portals | No |
| `leads.manage` | Manage leads (assign, convert, close) | Yes |
| `reports.view` | Access dashboards and analytics | No |

## Tasks

| # | Title | Status | Dependencies |
|---|---|---|---|
| 1.1 | Agency Entity & Hierarchy | DONE | 0.2 |
| 1.2 | Permission Domain Model | DONE | 1.1 |
| 1.3 | Authorization Policies & Middleware | DONE | 1.2 |
| 1.4 | OrgsApi SDK Client for Permissions | DONE | 1.3, 0.4 |
| 1.5 | Agency Management UI | DONE | 1.1 |
| 1.6 | Permission Management UI | DONE | 1.3, 1.5 |
| 1.7 | E2E Permission Tests | DONE | 1.3, 1.4, 1.6 |

---

## Task 1.1 -- Agency Entity & Hierarchy

**Status:** DONE
**Dependencies:** 0.2 (namespaces must be `Propely.*` before adding new domain entities)

### Goal

Introduce the `Agency` aggregate root into the orgs-api Domain layer and establish the Agency-to-Branch (Organization) parent-child relationship. This creates the organizational hierarchy that enables multi-branch real estate agencies. The Agency entity encapsulates creation, branch association, and the domain events needed for downstream processing.

### Scope

**In scope:**
- `Agency` aggregate root with: `Id`, `Name`, `Slug`, `CreatedByUserId`, `CreatedAtUtc`, `UpdatedAtUtc`, soft-delete support
- `Agency.AddBranch(Organization org)` method linking an existing Organization to the Agency
- `Agency.RemoveBranch(Guid organizationId)` method unlinking a branch
- Update `Organization` entity with optional `AgencyId` foreign key
- Domain events: `AgencyCreatedV1`, `BranchAddedToAgencyV1`, `BranchRemovedFromAgencyV1`
- Value object: `AgencySlug` with validation (lowercase, alphanumeric + hyphens, 3-50 chars)
- Update `MembershipRole` enum: rename `Member` to `Agent` to align with Propely terminology
- EF Core configuration for `Agency` entity, migration for `Agencies` table and `Organization.AgencyId` FK
- Repository interface: `IAgencyRepository` in Application layer
- Repository implementation: `AgencyRepository` in Infrastructure layer
- CQRS commands/queries: `CreateAgencyCommand`, `AddBranchToAgencyCommand`, `RemoveBranchFromAgencyCommand`, `GetAgencyByIdQuery`, `ListAgenciesForUserQuery`, `GetAgencyDetailsQuery`
- API endpoints: `POST /api/agencies`, `GET /api/agencies`, `GET /api/agencies/{id}`, `POST /api/agencies/{id}/branches`, `DELETE /api/agencies/{id}/branches/{branchId}`

**Out of scope:**
- Permission system (task 1.2)
- Agency billing or subscription management
- Branch creation (branches are existing Organizations; this task only links them to an Agency)
- Frontend screens (task 1.5)

### Acceptance Criteria

- [ ] **AC1:** `Agency` entity exists in `Domain/Agencies/` with `Id`, `Name`, `Slug`, `CreatedByUserId`, `CreatedAtUtc`, `UpdatedAtUtc`, soft-delete fields
- [ ] **AC2:** `Agency.Create(name, slug, createdByUserId)` factory method validates name (1-200 chars) and slug (3-50 chars, lowercase alphanumeric + hyphens) and raises `AgencyCreatedV1`
- [ ] **AC3:** `Agency.AddBranch(organizationId)` raises `BranchAddedToAgencyV1`; duplicate branch addition throws `DomainException`
- [ ] **AC4:** `Agency.RemoveBranch(organizationId)` raises `BranchRemovedFromAgencyV1`; removing a non-member branch throws `NotFoundException`
- [ ] **AC5:** `Organization` entity has optional `AgencyId` property; EF Core migration adds nullable FK column
- [ ] **AC6:** `MembershipRole` enum values are `Owner`, `Admin`, `Agent`, `Viewer` (renamed from `Member` to `Agent`)
- [ ] **AC7:** `POST /api/agencies` creates an agency and returns 201 with the agency DTO; creator is automatically assigned `Owner` role at the agency level
- [ ] **AC8:** `GET /api/agencies` returns agencies the authenticated user belongs to (as owner or through branch membership)
- [ ] **AC9:** `POST /api/agencies/{id}/branches` links an existing Organization to the agency; only agency owners can do this; returns 200
- [ ] **AC10:** `dotnet test` passes all new and existing tests; domain entity tests cover creation, slug validation, branch add/remove, and invariant enforcement

### Implementation Steps

1. Create `Domain/Agencies/Agency.cs` aggregate root with private constructor, `Create` factory, `AddBranch`, `RemoveBranch`, `Update`, `SoftDelete` methods
2. Create `Domain/Agencies/AgencySlug.cs` value object with validation regex (`^[a-z0-9][a-z0-9-]{1,48}[a-z0-9]$`)
3. Create `Domain/Agencies/Events/AgencyCreatedV1.cs` record implementing `IDomainEvent`
4. Create `Domain/Agencies/Events/BranchAddedToAgencyV1.cs` record implementing `IDomainEvent`
5. Create `Domain/Agencies/Events/BranchRemovedFromAgencyV1.cs` record implementing `IDomainEvent`
6. Update `Domain/Organizations/MembershipRole.cs`: rename `Member` to `Agent`
7. Update `Domain/Organizations/Organization.cs`: add `AgencyId` nullable property and `AssignToAgency(Guid agencyId)` method
8. Create `Application/Agencies/Interfaces/IAgencyRepository.cs` with `GetByIdAsync`, `GetBySlugAsync`, `ListForUserAsync`, `AddAsync`, `UpdateAsync`
9. Create `Application/Agencies/DTOs/AgencyDto.cs`, `AgencyDetailDto.cs`, `AgencyBranchDto.cs`
10. Create `Application/Agencies/Commands/CreateAgency/CreateAgencyCommand.cs` and handler
11. Create `Application/Agencies/Commands/CreateAgency/CreateAgencyCommandValidator.cs`
12. Create `Application/Agencies/Commands/AddBranchToAgency/AddBranchToAgencyCommand.cs` and handler
13. Create `Application/Agencies/Commands/RemoveBranchFromAgency/RemoveBranchFromAgencyCommand.cs` and handler
14. Create `Application/Agencies/Queries/GetAgencyById/GetAgencyByIdQuery.cs` and handler
15. Create `Application/Agencies/Queries/ListAgenciesForUser/ListAgenciesForUserQuery.cs` and handler
16. Create `Infrastructure/Persistence/Configurations/AgencyConfiguration.cs` EF Core config
17. Update `Infrastructure/Persistence/Configurations/OrganizationConfiguration.cs` to add `AgencyId` FK
18. Create EF Core migration: `AddAgencyEntity`
19. Create `Infrastructure/Persistence/Repositories/AgencyRepository.cs`
20. Create `Api/Controllers/AgenciesController.cs` with CRUD endpoints
21. Create `Api/Dtos/CreateAgencyRequest.cs`, `AddBranchRequest.cs`
22. Create `Api/Validators/CreateAgencyRequestValidator.cs`, `AddBranchRequestValidator.cs`
23. Update all existing references to `MembershipRole.Member` to use `MembershipRole.Agent`
24. Write unit tests for Agency entity (creation, validation, branch management, domain events)
25. Write integration tests for all API endpoints

### Files to Create/Modify

**Create:**
- `services/orgs-api/src/Propely.OrgsApi.Domain/Agencies/Agency.cs`
- `services/orgs-api/src/Propely.OrgsApi.Domain/Agencies/AgencySlug.cs`
- `services/orgs-api/src/Propely.OrgsApi.Domain/Agencies/Events/AgencyCreatedV1.cs`
- `services/orgs-api/src/Propely.OrgsApi.Domain/Agencies/Events/BranchAddedToAgencyV1.cs`
- `services/orgs-api/src/Propely.OrgsApi.Domain/Agencies/Events/BranchRemovedFromAgencyV1.cs`
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
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/Persistence/Configurations/AgencyConfiguration.cs`
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/Persistence/Repositories/AgencyRepository.cs`
- `services/orgs-api/src/Propely.OrgsApi.Api/Controllers/AgenciesController.cs`
- `services/orgs-api/src/Propely.OrgsApi.Api/Dtos/CreateAgencyRequest.cs`
- `services/orgs-api/src/Propely.OrgsApi.Api/Dtos/AddBranchRequest.cs`
- `services/orgs-api/src/Propely.OrgsApi.Api/Validators/CreateAgencyRequestValidator.cs`
- `services/orgs-api/src/Propely.OrgsApi.Api/Validators/AddBranchRequestValidator.cs`
- `services/orgs-api/tests/Propely.OrgsApi.UnitTests/Domain/Agencies/AgencyTests.cs`
- `services/orgs-api/tests/Propely.OrgsApi.UnitTests/Domain/Agencies/AgencySlugTests.cs`
- `services/orgs-api/tests/Propely.OrgsApi.IntegrationTests/Agencies/AgenciesControllerTests.cs`

**Modify:**
- `services/orgs-api/src/Propely.OrgsApi.Domain/Organizations/Organization.cs` (add `AgencyId`)
- `services/orgs-api/src/Propely.OrgsApi.Domain/Organizations/MembershipRole.cs` (rename `Member` to `Agent`)
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/Persistence/Configurations/OrganizationConfiguration.cs` (add `AgencyId` FK)
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/Persistence/OrgsApiDbContext.cs` (add `DbSet<Agency>`)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `Agency.Create` validates name length, slug format, raises `AgencyCreatedV1` | xUnit + FluentAssertions |
| Unit | `Agency.AddBranch` raises event; duplicate throws `DomainException` | xUnit + FluentAssertions |
| Unit | `Agency.RemoveBranch` raises event; missing branch throws `NotFoundException` | xUnit + FluentAssertions |
| Unit | `AgencySlug` rejects invalid formats (uppercase, special chars, too short/long) | xUnit, parameterized tests |
| Unit | `CreateAgencyCommandValidator` rejects empty name, invalid slug | xUnit + FluentAssertions |
| Integration | `POST /api/agencies` creates agency and returns 201 | `WebApplicationFactory`, Testcontainers |
| Integration | `GET /api/agencies` returns only agencies the user belongs to | `WebApplicationFactory` |
| Integration | `POST /api/agencies/{id}/branches` links org; unauthorized user gets 403 | `WebApplicationFactory` |
| Integration | `DELETE /api/agencies/{id}/branches/{branchId}` unlinks org | `WebApplicationFactory` |
| Integration | `MembershipRole.Agent` rename does not break existing membership queries | `WebApplicationFactory` |

### Security & Privacy Considerations

- Only agency owners can add/remove branches; enforce in command handler and controller
- Agency slug must be globally unique to prevent impersonation
- Agency creation should be rate-limited to prevent abuse (defer to infrastructure middleware)
- The `CreatedByUserId` field establishes the initial ownership chain; this cannot be changed after creation
- Cross-branch visibility for agency owners must be explicitly scoped; do not leak branch data to non-owner members of other branches

### TDD Reminder

Write domain entity tests first (creation, validation, events, invariants). Then write integration tests for endpoints with expected 201/200/400/403 responses. Implement domain, application, infrastructure, and API layers in that order.

---

## Task 1.2 -- Permission Domain Model

**Status:** DONE
**Dependencies:** 1.1 (Agency entity and `MembershipRole.Agent` rename must exist)

### Goal

Define the permission system in the Domain and Application layers. This includes the `Permission` enum (all granular permissions), the `PermissionOverride` entity for per-user grants/denials, and the `IPermissionEvaluator` interface that resolves a user's effective permissions by combining their base role defaults with any overrides. This is a pure domain model task -- no persistence, no API, no middleware.

### Scope

**In scope:**
- `Permission` enum listing all granular permissions (`PropertiesViewAll`, `PropertiesEditAll`, `ContactsViewAll`, `ContactsEditAll`, `AppointmentsViewAll`, `PublishingManage`, `LeadsManage`, `ReportsView`)
- `PermissionOverride` entity: `Id`, `UserId`, `OrganizationId`, `Permission`, `Granted` (bool), `GrantedBy` (userId), `GrantedAtUtc`
- `DefaultPermissionMatrix` static class mapping each `MembershipRole` to its default `Permission` set
- `IPermissionEvaluator` interface: `HasPermissionAsync(Guid userId, Guid organizationId, Permission permission)` and `GetEffectivePermissionsAsync(Guid userId, Guid organizationId)`
- `PermissionEvaluator` implementation in Application layer using `IPermissionOverrideRepository` and `IMembershipRepository`
- Resolution logic: base role defaults + overrides (grant adds, deny removes, overrides take precedence over defaults)
- Domain events: `PermissionOverrideGrantedV1`, `PermissionOverrideDeniedV1`, `PermissionOverrideRevokedV1`

**Out of scope:**
- EF Core persistence for `PermissionOverride` (task 1.3)
- ASP.NET Core authorization policies and middleware (task 1.3)
- API endpoints for managing overrides (task 1.3)
- SDK client (task 1.4)

### Acceptance Criteria

- [ ] **AC1:** `Permission` enum exists with at least 8 values: `PropertiesViewAll`, `PropertiesEditAll`, `ContactsViewAll`, `ContactsEditAll`, `AppointmentsViewAll`, `PublishingManage`, `LeadsManage`, `ReportsView`
- [ ] **AC2:** `PermissionOverride` entity has `Id`, `UserId`, `OrganizationId`, `Permission`, `Granted`, `GrantedBy`, `GrantedAtUtc` properties with a private constructor and a `Create` factory method
- [ ] **AC3:** `DefaultPermissionMatrix.GetDefaults(MembershipRole.Owner)` returns all permissions granted
- [ ] **AC4:** `DefaultPermissionMatrix.GetDefaults(MembershipRole.Admin)` returns all permissions except those explicitly reserved for owners
- [ ] **AC5:** `DefaultPermissionMatrix.GetDefaults(MembershipRole.Agent)` returns only `LeadsManage` by default
- [ ] **AC6:** `DefaultPermissionMatrix.GetDefaults(MembershipRole.Viewer)` returns an empty set (read-only base access, no special permissions)
- [ ] **AC7:** `PermissionEvaluator.HasPermissionAsync` returns `true` when the user's base role grants the permission and no deny override exists
- [ ] **AC8:** `PermissionEvaluator.HasPermissionAsync` returns `true` when the user's base role does NOT grant the permission but a grant override exists
- [ ] **AC9:** `PermissionEvaluator.HasPermissionAsync` returns `false` when the user's base role grants the permission but a deny override exists
- [ ] **AC10:** Unit tests cover all combinations: each role x each permission x with/without override (at least 30 test cases)

### Implementation Steps

1. Create `Domain/Permissions/Permission.cs` enum with all 8 permission values
2. Create `Domain/Permissions/PermissionOverride.cs` entity with private constructor, `Create(userId, orgId, permission, granted, grantedBy)` factory, domain event raising
3. Create `Domain/Permissions/Events/PermissionOverrideGrantedV1.cs` record
4. Create `Domain/Permissions/Events/PermissionOverrideDeniedV1.cs` record
5. Create `Domain/Permissions/Events/PermissionOverrideRevokedV1.cs` record
6. Create `Domain/Permissions/DefaultPermissionMatrix.cs` static class with `GetDefaults(MembershipRole role)` returning `IReadOnlySet<Permission>`
7. Create `Application/Permissions/Interfaces/IPermissionOverrideRepository.cs` with `GetOverridesAsync(Guid userId, Guid organizationId)`, `GetOverrideAsync(Guid userId, Guid orgId, Permission)`, `AddAsync`, `RemoveAsync`
8. Create `Application/Permissions/Interfaces/IPermissionEvaluator.cs` interface
9. Create `Application/Permissions/Services/PermissionEvaluator.cs` implementing `IPermissionEvaluator`:
   - Load user's membership to get role
   - Load role defaults from `DefaultPermissionMatrix`
   - Load overrides from repository
   - Apply overrides: `Granted=true` adds permission, `Granted=false` removes permission
   - Owner role always gets all permissions (cannot be restricted by overrides)
10. Create `Application/Permissions/DTOs/EffectivePermissionDto.cs` (permission name, granted bool, source: "role" or "override")
11. Write unit tests for `DefaultPermissionMatrix` (every role returns expected defaults)
12. Write unit tests for `PermissionEvaluator` with mocked repositories (all role+override combinations)
13. Write unit tests for `PermissionOverride` entity (creation, validation, events)

### Files to Create/Modify

**Create:**
- `services/orgs-api/src/Propely.OrgsApi.Domain/Permissions/Permission.cs`
- `services/orgs-api/src/Propely.OrgsApi.Domain/Permissions/PermissionOverride.cs`
- `services/orgs-api/src/Propely.OrgsApi.Domain/Permissions/DefaultPermissionMatrix.cs`
- `services/orgs-api/src/Propely.OrgsApi.Domain/Permissions/Events/PermissionOverrideGrantedV1.cs`
- `services/orgs-api/src/Propely.OrgsApi.Domain/Permissions/Events/PermissionOverrideDeniedV1.cs`
- `services/orgs-api/src/Propely.OrgsApi.Domain/Permissions/Events/PermissionOverrideRevokedV1.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/Interfaces/IPermissionOverrideRepository.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/Interfaces/IPermissionEvaluator.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/Services/PermissionEvaluator.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/DTOs/EffectivePermissionDto.cs`
- `services/orgs-api/tests/Propely.OrgsApi.UnitTests/Domain/Permissions/PermissionOverrideTests.cs`
- `services/orgs-api/tests/Propely.OrgsApi.UnitTests/Domain/Permissions/DefaultPermissionMatrixTests.cs`
- `services/orgs-api/tests/Propely.OrgsApi.UnitTests/Application/Permissions/PermissionEvaluatorTests.cs`

**Modify:**
- None (pure additive task)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `DefaultPermissionMatrix` returns correct set for each of 4 roles | xUnit, parameterized `[Theory]` |
| Unit | `PermissionOverride.Create` sets all fields, raises correct event | xUnit + FluentAssertions |
| Unit | `PermissionOverride.Create` with `Granted=true` raises `PermissionOverrideGrantedV1` | xUnit |
| Unit | `PermissionOverride.Create` with `Granted=false` raises `PermissionOverrideDeniedV1` | xUnit |
| Unit | `PermissionEvaluator` owner always has all permissions regardless of overrides | xUnit, mock repos |
| Unit | `PermissionEvaluator` admin has defaults minus deny overrides | xUnit, mock repos |
| Unit | `PermissionEvaluator` agent has only `LeadsManage` by default | xUnit, mock repos |
| Unit | `PermissionEvaluator` agent with grant override gains permission | xUnit, mock repos |
| Unit | `PermissionEvaluator` admin with deny override loses permission | xUnit, mock repos |
| Unit | `PermissionEvaluator` viewer has no special permissions by default | xUnit, mock repos |

### Security & Privacy Considerations

- Owner permissions cannot be restricted by overrides (business rule: owner always has full access)
- `GrantedBy` field creates an audit trail for who granted/denied each override
- Permission enum is defined in the Domain layer to ensure it has no framework dependencies
- The evaluator must handle edge cases: user with no membership returns false for all permissions; deleted membership is treated as no membership

### TDD Reminder

Write `DefaultPermissionMatrix` tests first (each role maps to expected permission set). Then write `PermissionEvaluator` tests for every role+override combination (at least 30 cases). Implement the matrix and evaluator to make tests green. Refactor for clarity.

---

## Task 1.3 -- Authorization Policies & Middleware

**Status:** DONE
**Dependencies:** 1.2 (permission domain model and `IPermissionEvaluator` must exist)

### Goal

Wire the permission system into the ASP.NET Core authorization pipeline. Create dynamic authorization policies that evaluate permissions at request time using `IPermissionEvaluator`, persist `PermissionOverride` entities with EF Core, and expose API endpoints for managing overrides. This makes the permission system enforceable across all orgs-api endpoints and provides the management API for frontend consumption.

### Scope

**In scope:**
- EF Core configuration for `PermissionOverride` entity, migration for `PermissionOverrides` table
- `PermissionOverrideRepository` implementation in Infrastructure
- `PermissionAuthorizationHandler` implementing `IAuthorizationHandler` that calls `IPermissionEvaluator`
- `RequirePermissionAttribute` custom attribute for declarative policy enforcement on controllers/actions
- `PermissionRequirement` implementing `IAuthorizationRequirement`
- Registration of dynamic authorization policies in `Program.cs`
- Permission caching: `CachedPermissionEvaluator` decorator using Redis with short TTL (30 seconds)
- API endpoints: `GET /api/organizations/{orgId}/permissions/{userId}` (list effective permissions), `PUT /api/organizations/{orgId}/permissions/{userId}/{permission}` (set override), `DELETE /api/organizations/{orgId}/permissions/{userId}/{permission}` (remove override)
- CQRS commands/queries: `GetUserPermissionsQuery`, `SetPermissionOverrideCommand`, `RemovePermissionOverrideCommand`
- Audit logging for all permission changes

**Out of scope:**
- SDK client (task 1.4)
- Frontend UI (tasks 1.5, 1.6)
- Cross-service permission enforcement (handled by SDK client in task 1.4)

### Acceptance Criteria

- [ ] **AC1:** `PermissionOverrides` table exists in the database with columns: `Id`, `UserId`, `OrganizationId`, `Permission`, `Granted`, `GrantedBy`, `GrantedAtUtc`; composite unique index on `(UserId, OrganizationId, Permission)`
- [ ] **AC2:** `[RequirePermission(Permission.PropertiesViewAll)]` attribute on a controller action enforces the permission via `PermissionAuthorizationHandler`
- [ ] **AC3:** A request from an agent without `PropertiesViewAll` permission to a `[RequirePermission(Permission.PropertiesViewAll)]` endpoint returns 403 Forbidden
- [ ] **AC4:** A request from an agent with a `PropertiesViewAll` grant override to the same endpoint returns 200
- [ ] **AC5:** `GET /api/organizations/{orgId}/permissions/{userId}` returns all effective permissions with source (role default or override)
- [ ] **AC6:** `PUT /api/organizations/{orgId}/permissions/{userId}/PropertiesViewAll` with `{ "granted": true }` creates/updates an override; only admins and owners can do this
- [ ] **AC7:** `DELETE /api/organizations/{orgId}/permissions/{userId}/PropertiesViewAll` removes the override, reverting to role default
- [ ] **AC8:** Permission evaluation results are cached in Redis with 30-second TTL; cache is invalidated when overrides change
- [ ] **AC9:** All permission changes (set/remove override) are recorded in the audit log with actor, target user, permission, old value, new value
- [ ] **AC10:** `dotnet test` passes all new and existing tests; integration tests verify end-to-end authorization enforcement

### Implementation Steps

1. Create `Infrastructure/Persistence/Configurations/PermissionOverrideConfiguration.cs` with EF Core config: table name, column types, unique index on `(UserId, OrganizationId, Permission)`
2. Update `OrgsApiDbContext` with `DbSet<PermissionOverride>`
3. Create EF Core migration: `AddPermissionOverrides`
4. Create `Infrastructure/Persistence/Repositories/PermissionOverrideRepository.cs` implementing `IPermissionOverrideRepository`
5. Create `Api/Authorization/PermissionRequirement.cs` implementing `IAuthorizationRequirement`
6. Create `Api/Authorization/RequirePermissionAttribute.cs` as `AuthorizeAttribute` subclass that sets `Policy` to `Permission:<name>`
7. Create `Api/Authorization/PermissionAuthorizationHandler.cs` implementing `AuthorizationHandler<PermissionRequirement>`:
   - Extract `UserId` and `OrganizationId` from claims/route
   - Call `IPermissionEvaluator.HasPermissionAsync`
   - Succeed or fail the requirement
8. Create `Api/Authorization/PermissionPolicyProvider.cs` implementing `IAuthorizationPolicyProvider` to dynamically create policies from permission names
9. Create `Application/Permissions/Services/CachedPermissionEvaluator.cs` decorator:
   - Cache key: `permissions:{userId}:{orgId}`
   - TTL: 30 seconds
   - Invalidate on override changes
10. Create `Application/Permissions/Queries/GetUserPermissions/GetUserPermissionsQuery.cs` and handler
11. Create `Application/Permissions/Commands/SetPermissionOverride/SetPermissionOverrideCommand.cs` and handler (with audit logging)
12. Create `Application/Permissions/Commands/RemovePermissionOverride/RemovePermissionOverrideCommand.cs` and handler (with audit logging)
13. Create `Api/Controllers/PermissionsController.cs` with endpoints
14. Create `Api/Dtos/SetPermissionOverrideRequest.cs`
15. Register authorization services in `Program.cs`: `PermissionAuthorizationHandler`, `PermissionPolicyProvider`, `CachedPermissionEvaluator`
16. Write integration tests for authorization enforcement (agent blocked, agent with override allowed, admin allowed)
17. Write integration tests for permission management endpoints

### Files to Create/Modify

**Create:**
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/Persistence/Configurations/PermissionOverrideConfiguration.cs`
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/Persistence/Repositories/PermissionOverrideRepository.cs`
- `services/orgs-api/src/Propely.OrgsApi.Api/Authorization/PermissionRequirement.cs`
- `services/orgs-api/src/Propely.OrgsApi.Api/Authorization/RequirePermissionAttribute.cs`
- `services/orgs-api/src/Propely.OrgsApi.Api/Authorization/PermissionAuthorizationHandler.cs`
- `services/orgs-api/src/Propely.OrgsApi.Api/Authorization/PermissionPolicyProvider.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/Services/CachedPermissionEvaluator.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/Queries/GetUserPermissions/GetUserPermissionsQuery.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/Queries/GetUserPermissions/GetUserPermissionsQueryHandler.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/Commands/SetPermissionOverride/SetPermissionOverrideCommand.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/Commands/SetPermissionOverride/SetPermissionOverrideCommandHandler.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/Commands/SetPermissionOverride/SetPermissionOverrideCommandValidator.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/Commands/RemovePermissionOverride/RemovePermissionOverrideCommand.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/Commands/RemovePermissionOverride/RemovePermissionOverrideCommandHandler.cs`
- `services/orgs-api/src/Propely.OrgsApi.Api/Controllers/PermissionsController.cs`
- `services/orgs-api/src/Propely.OrgsApi.Api/Dtos/SetPermissionOverrideRequest.cs`
- `services/orgs-api/tests/Propely.OrgsApi.UnitTests/Application/Permissions/CachedPermissionEvaluatorTests.cs`
- `services/orgs-api/tests/Propely.OrgsApi.IntegrationTests/Permissions/PermissionAuthorizationTests.cs`
- `services/orgs-api/tests/Propely.OrgsApi.IntegrationTests/Permissions/PermissionsControllerTests.cs`

**Modify:**
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/Persistence/OrgsApiDbContext.cs` (add `DbSet<PermissionOverride>`)
- `services/orgs-api/src/Propely.OrgsApi.Api/Program.cs` (register authorization services)
- `services/orgs-api/src/Propely.OrgsApi.Api/DependencyInjection.cs` (register `CachedPermissionEvaluator`)
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/DependencyInjection.cs` (register `PermissionOverrideRepository`)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `CachedPermissionEvaluator` returns cached result within TTL | xUnit, mock `ICacheService`, mock `IPermissionEvaluator` |
| Unit | `CachedPermissionEvaluator` invalidates cache on override change | xUnit, mock `ICacheService` |
| Unit | `SetPermissionOverrideCommandValidator` rejects invalid permission names, missing userId | xUnit + FluentAssertions |
| Unit | `SetPermissionOverrideCommandHandler` creates override and publishes audit log | xUnit, mock repos |
| Integration | Agent without permission gets 403 on protected endpoint | `WebApplicationFactory`, Testcontainers |
| Integration | Agent with grant override gets 200 on protected endpoint | `WebApplicationFactory` |
| Integration | Admin can set/remove overrides via API | `WebApplicationFactory` |
| Integration | Agent cannot set overrides (403) | `WebApplicationFactory` |
| Integration | `GET /permissions/{userId}` returns effective permissions with sources | `WebApplicationFactory` |
| Integration | Override removal reverts behavior to role default | `WebApplicationFactory` |

### Security & Privacy Considerations

- Only admins and owners can view/modify permission overrides; enforce at both handler and controller level
- Admins cannot modify owner permissions (owners are always fully privileged)
- Permission changes must be audit-logged with who made the change, what changed, and when
- Redis cache TTL of 30 seconds means a revoked permission could still be active for up to 30 seconds; this is an acceptable tradeoff for performance
- The `PermissionPolicyProvider` must validate that the policy name maps to a real `Permission` enum value to prevent injection of arbitrary policy names
- Rate limit the permission management endpoints to prevent abuse

### TDD Reminder

Write integration tests first: create a test that expects 403 for an agent accessing a protected endpoint, then implement the authorization handler to make it pass. Add tests for the override flow (set override, verify access changes). Then implement the management endpoints.

---

## Task 1.4 -- OrgsApi SDK Client for Permissions

**Status:** DONE
**Dependencies:** 1.3 (permission API endpoints must exist), 0.4 (SDK client infrastructure pattern must exist)

### Goal

Extend the `Propely.OrgsApi.Client` NuGet SDK package with Refit interfaces for permission evaluation. Other microservices (properties-api, contacts-api, appointments-api, publishing-api) will use this client to enforce authorization without direct database access to orgs-api. The client exposes `HasPermissionAsync`, `GetEffectivePermissionsAsync`, and agency/branch hierarchy queries.

### Scope

**In scope:**
- `IPermissionsApi` Refit interface with: `HasPermissionAsync(Guid userId, Guid orgId, Permission permission)`, `GetEffectivePermissionsAsync(Guid userId, Guid orgId)`
- `IAgenciesApi` Refit interface with: `GetAgencyByIdAsync`, `ListAgenciesForUserAsync`, `GetBranchesForAgencyAsync`
- Permission-related DTOs in the client package: `PermissionCheckResponse`, `EffectivePermissionResponse`, `AgencyResponse`, `BranchResponse`
- `IPermissionGuard` helper class that wraps `IPermissionsApi` with a simpler API for common authorization checks (throws `ForbiddenException` on denial)
- Registration via `AddOrgsApiClient()` extension (extend existing registration to include new interfaces)
- Retry policy for permission checks: 2 retries with short backoff (permission checks are latency-sensitive)
- Fallback policy: if orgs-api is unreachable, deny all permissions (fail-closed)
- Contract tests verifying SDK behavior matches API behavior

**Out of scope:**
- Modifying orgs-api endpoints (already done in task 1.3)
- Caching in the consuming services (each service decides its own caching strategy)
- Authentication token management (handled by existing `DelegatingHandler`)

### Acceptance Criteria

- [ ] **AC1:** `IPermissionsApi` Refit interface has `[Get("/api/organizations/{orgId}/permissions/{userId}")]` returning `EffectivePermissionResponse`
- [ ] **AC2:** `IPermissionsApi` has a `HasPermissionAsync` convenience method that calls the API and returns `bool`
- [ ] **AC3:** `IAgenciesApi` Refit interface has `GetAgencyByIdAsync`, `ListAgenciesForUserAsync`, `GetBranchesForAgencyAsync`
- [ ] **AC4:** `IPermissionGuard.RequirePermissionAsync(userId, orgId, permission)` throws `ForbiddenException` when permission is denied
- [ ] **AC5:** `services.AddOrgsApiClient(options => ...)` registers `IPermissionsApi`, `IAgenciesApi`, and `IPermissionGuard` in DI
- [ ] **AC6:** Polly retry policy retries permission checks twice on transient failures (5xx, 408)
- [ ] **AC7:** Polly fallback policy denies all permissions when orgs-api is unreachable (fail-closed)
- [ ] **AC8:** `Propely.OrgsApi.Client` package builds without warnings
- [ ] **AC9:** Contract tests verify: SDK `HasPermissionAsync` returns `true` for owner, `false` for agent without override, `true` for agent with grant override
- [ ] **AC10:** `dotnet test` passes all new and existing client tests

### Implementation Steps

1. Create `IPermissionsApi` Refit interface in the client project:
   ```csharp
   public interface IPermissionsApi
   {
       [Get("/api/organizations/{orgId}/permissions/{userId}")]
       Task<List<EffectivePermissionDto>> GetEffectivePermissionsAsync(
           Guid orgId, Guid userId, CancellationToken ct = default);

       [Get("/api/organizations/{orgId}/permissions/{userId}/{permission}")]
       Task<PermissionCheckResponse> CheckPermissionAsync(
           Guid orgId, Guid userId, string permission, CancellationToken ct = default);
   }
   ```
2. Create `IAgenciesApi` Refit interface:
   ```csharp
   public interface IAgenciesApi
   {
       [Get("/api/agencies/{id}")]
       Task<AgencyResponse> GetAgencyByIdAsync(Guid id, CancellationToken ct = default);

       [Get("/api/agencies")]
       Task<List<AgencyResponse>> ListAgenciesForUserAsync(CancellationToken ct = default);

       [Get("/api/agencies/{id}/branches")]
       Task<List<BranchResponse>> GetBranchesForAgencyAsync(Guid id, CancellationToken ct = default);
   }
   ```
3. Create DTO classes: `PermissionCheckResponse`, `EffectivePermissionDto`, `AgencyResponse`, `BranchResponse`
4. Create `IPermissionGuard` interface and `PermissionGuard` implementation:
   - `RequirePermissionAsync` calls `IPermissionsApi.CheckPermissionAsync` and throws `ForbiddenException` on denial
   - `HasPermissionAsync` returns `bool` without throwing
5. Update `ServiceCollectionExtensions.AddOrgsApiClient()` to register `IPermissionsApi`, `IAgenciesApi`, `IPermissionGuard`
6. Configure Polly retry: 2 retries, 200ms/400ms backoff for permission checks
7. Configure Polly fallback: on circuit-breaker open or timeout, return `PermissionCheckResponse { Granted = false }`
8. Write unit tests for `PermissionGuard` (throws on denial, passes on grant)
9. Write unit tests for Polly policy configuration (retries, fallback)
10. Write contract tests using `WebApplicationFactory` of orgs-api to verify SDK behavior end-to-end

### Files to Create/Modify

**Create:**
- `services/orgs-api/src/Propely.OrgsApi.Client/Permissions/IPermissionsApi.cs`
- `services/orgs-api/src/Propely.OrgsApi.Client/Permissions/IPermissionGuard.cs`
- `services/orgs-api/src/Propely.OrgsApi.Client/Permissions/PermissionGuard.cs`
- `services/orgs-api/src/Propely.OrgsApi.Client/Permissions/Models/PermissionCheckResponse.cs`
- `services/orgs-api/src/Propely.OrgsApi.Client/Permissions/Models/EffectivePermissionDto.cs`
- `services/orgs-api/src/Propely.OrgsApi.Client/Agencies/IAgenciesApi.cs`
- `services/orgs-api/src/Propely.OrgsApi.Client/Agencies/Models/AgencyResponse.cs`
- `services/orgs-api/src/Propely.OrgsApi.Client/Agencies/Models/BranchResponse.cs`
- `services/orgs-api/tests/Propely.OrgsApi.Client.Tests/Permissions/PermissionGuardTests.cs`
- `services/orgs-api/tests/Propely.OrgsApi.Client.Tests/Permissions/PermissionsApiContractTests.cs`
- `services/orgs-api/tests/Propely.OrgsApi.Client.Tests/Agencies/AgenciesApiContractTests.cs`

**Modify:**
- `services/orgs-api/src/Propely.OrgsApi.Client/ServiceCollectionExtensions.cs` (register new interfaces)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `PermissionGuard.RequirePermissionAsync` throws `ForbiddenException` when API returns denied | xUnit, mock `IPermissionsApi` |
| Unit | `PermissionGuard.HasPermissionAsync` returns `false` without throwing when denied | xUnit, mock `IPermissionsApi` |
| Unit | Polly retry triggers on 500, 502, 503, 408 status codes | xUnit, mock `HttpMessageHandler` |
| Unit | Polly fallback returns `Granted=false` when orgs-api unreachable | xUnit, mock handler with timeout |
| Unit | DI registration resolves `IPermissionsApi`, `IAgenciesApi`, `IPermissionGuard` | xUnit, build `ServiceProvider` |
| Contract | Owner has all permissions | `WebApplicationFactory` of orgs-api, call through SDK |
| Contract | Agent without override lacks `PropertiesViewAll` | `WebApplicationFactory`, call through SDK |
| Contract | Agent with grant override has `PropertiesViewAll` | `WebApplicationFactory`, set override, call through SDK |
| Contract | `ListAgenciesForUserAsync` returns only user's agencies | `WebApplicationFactory`, call through SDK |

### Security & Privacy Considerations

- Fail-closed policy: when orgs-api is down, all permission checks deny access rather than granting it
- The SDK client must propagate the `X-Tenant-Id` header via `TenantDelegatingHandler` for all requests
- Permission check responses should not leak internal error details; use generic error messages
- The `IPermissionGuard` must not cache results; caching is the responsibility of the consuming service to avoid stale permissions
- Log permission check failures at Warning level for security monitoring (without logging the full request body)

### TDD Reminder

Write contract tests first: set up a test orgs-api instance, create users with specific roles and overrides, then verify the SDK returns the expected permission results. Then implement the SDK interfaces and guard. Write unit tests for Polly policies separately.

---

## Task 1.5 -- Agency Management UI

**Status:** DONE
**Dependencies:** 1.1 (agency API endpoints must exist)

### Goal

Build the frontend screens for agency management: creating an agency, viewing agency details with branches, switching between branches in the header, and managing agency settings. This is the first user-facing feature of the agency hierarchy and establishes the multi-branch navigation pattern used throughout the application.

### Scope

**In scope:**
- Use case definition for agency management screens
- Stitch MCP design generation for all new screens
- Agency creation flow: form with name and slug input, create agency, redirect to agency dashboard
- Agency dashboard: list branches with member counts, status, and quick actions
- Branch switcher in the application header: dropdown showing branches the user belongs to, current branch highlighted, agency name shown above branches
- Agency settings page: update name, manage slug, danger zone (delete agency)
- API integration: call agency endpoints via fetch/axios
- Responsive design (mobile branch switcher as slide-out panel)

**Out of scope:**
- Permission management UI (task 1.6)
- Branch creation (branches are created via the existing org creation flow)
- Billing/subscription UI
- Invitation management (existing feature, no changes needed)

### Acceptance Criteria

- [ ] **AC1:** "Create Agency" button appears on the dashboard for users without an agency; clicking opens a creation form
- [ ] **AC2:** Agency creation form has name (required, max 200 chars) and slug (required, 3-50 chars, auto-generated from name with manual edit) inputs with real-time validation
- [ ] **AC3:** Successful agency creation redirects to the agency dashboard showing the new agency with zero branches
- [ ] **AC4:** Agency dashboard displays a card list of branches with: name, member count, creation date; "Add Branch" button for owners
- [ ] **AC5:** Branch switcher in the header shows agency name and a dropdown of branches; clicking a branch navigates to that branch's context
- [ ] **AC6:** Current branch is visually highlighted in the branch switcher dropdown
- [ ] **AC7:** Agency settings page allows updating the agency name (slug is read-only after creation)
- [ ] **AC8:** Agency settings page has a danger zone with "Delete Agency" action requiring confirmation
- [ ] **AC9:** All new screens have corresponding Stitch designs in the project; implementation matches designs pixel-for-pixel
- [ ] **AC10:** All components have unit tests (Vitest + React Testing Library); form validation tests, API call tests, navigation tests

### Implementation Steps

1. Define use cases for: create agency, view agency dashboard, switch branch, edit agency settings, delete agency
2. Generate Stitch MCP designs for: agency creation form, agency dashboard, branch switcher dropdown, agency settings page
3. Download Stitch HTML to `.stitch-html/agency-create.html`, `.stitch-html/agency-dashboard.html`, `.stitch-html/branch-switcher.html`, `.stitch-html/agency-settings.html`
4. Create `useAgencies` hook for listing agencies
5. Create `useAgency` hook for single agency details
6. Create `useCreateAgency` mutation hook
7. Create `useUpdateAgency` mutation hook
8. Create `useDeleteAgency` mutation hook
9. Create `useAddBranch` mutation hook
10. Create `AgencyContext` React context for current agency state (shared with branch switcher)
11. Create `AgencyCreateForm` component matching Stitch design
12. Create `AgencyDashboard` component with branch card list
13. Create `BranchSwitcher` component for the header dropdown
14. Create `AgencySettingsPage` component with update form and danger zone
15. Add routes: `/agencies/new`, `/agencies/[id]`, `/agencies/[id]/settings`
16. Update header layout to include `BranchSwitcher`
17. Write component tests for all new components
18. Verify against Stitch designs

### Files to Create/Modify

**Create:**
- `apps/web/src/hooks/useAgencies.ts`
- `apps/web/src/hooks/useAgency.ts`
- `apps/web/src/hooks/useCreateAgency.ts`
- `apps/web/src/hooks/useUpdateAgency.ts`
- `apps/web/src/hooks/useDeleteAgency.ts`
- `apps/web/src/hooks/useAddBranch.ts`
- `apps/web/src/contexts/AgencyContext.tsx`
- `apps/web/src/components/agencies/AgencyCreateForm.tsx`
- `apps/web/src/components/agencies/AgencyDashboard.tsx`
- `apps/web/src/components/agencies/BranchCard.tsx`
- `apps/web/src/components/agencies/BranchSwitcher.tsx`
- `apps/web/src/components/agencies/AgencySettingsForm.tsx`
- `apps/web/src/app/[locale]/(dashboard)/agencies/new/page.tsx`
- `apps/web/src/app/[locale]/(dashboard)/agencies/[id]/page.tsx`
- `apps/web/src/app/[locale]/(dashboard)/agencies/[id]/settings/page.tsx`
- `apps/web/src/components/agencies/__tests__/AgencyCreateForm.test.tsx`
- `apps/web/src/components/agencies/__tests__/AgencyDashboard.test.tsx`
- `apps/web/src/components/agencies/__tests__/BranchSwitcher.test.tsx`
- `apps/web/src/components/agencies/__tests__/AgencySettingsForm.test.tsx`
- `.stitch-html/agency-create.html`
- `.stitch-html/agency-dashboard.html`
- `.stitch-html/branch-switcher.html`
- `.stitch-html/agency-settings.html`

**Modify:**
- `apps/web/src/components/layout/Header.tsx` (add `BranchSwitcher`)
- `apps/web/src/messages/en.json` (add agency translation keys)
- `apps/web/src/messages/es.json` (add agency translation keys)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `AgencyCreateForm` validates name length and slug format | Vitest + RTL |
| Unit | `AgencyCreateForm` calls API on submit and handles success/error | Vitest + RTL, mock fetch |
| Unit | `AgencyDashboard` renders branch cards from API data | Vitest + RTL, mock fetch |
| Unit | `BranchSwitcher` renders branches, highlights current, navigates on click | Vitest + RTL |
| Unit | `AgencySettingsForm` pre-fills current values, validates, submits | Vitest + RTL, mock fetch |
| Unit | `AgencyContext` provides agency state to children | Vitest + RTL |
| Unit | Slug auto-generation from name (lowercase, replace spaces with hyphens) | Vitest |
| Manual | Full flow: create agency, view dashboard, add branch, switch branch | Dev environment |
| Manual | Visual comparison against Stitch designs | Dev environment |
| Manual | Responsive layout on mobile viewport | Dev environment |

### Security & Privacy Considerations

- Agency deletion requires explicit confirmation (type agency name to confirm pattern)
- Only agency owners see the "Settings" and "Delete" options; hide UI elements for non-owners
- Branch switcher only shows branches the user has membership in; do not show all agency branches to non-owner users
- Slug uniqueness validation should call the API for real-time feedback during creation

### TDD Reminder

Write component tests first with mock API responses. Define expected renders for: empty state (no agencies), agency with branches, form validation errors, API error states. Then implement components to pass the tests. Verify against Stitch designs last.

---

## Task 1.6 -- Permission Management UI

**Status:** DONE
**Dependencies:** 1.3 (permission API endpoints must exist), 1.5 (agency management UI and branch context must exist)

### Goal

Build the frontend screens for viewing and managing user permissions within a branch. Admins and owners can see each member's effective permissions (base role + overrides) and toggle individual permission overrides. This gives agency administrators fine-grained control over what each team member can do without changing their role.

### Scope

**In scope:**
- Use case definition for permission management screens
- Stitch MCP design generation for all new screens
- Member permissions page: list of branch members with their role and a summary of overrides
- Permission detail view: for a selected member, show all permissions with their effective state (granted/denied), source (role default or override), and toggle controls
- Toggle control: switch that sets or removes a permission override; visual distinction between role defaults and overrides
- Confirmation dialog for deny overrides (revoking a permission an admin has by default)
- Permission change history (last 10 changes per user, from audit log)
- Responsive design

**Out of scope:**
- Role changes (existing feature, no changes needed)
- Bulk permission changes across multiple users
- Permission templates or presets
- Cross-branch permission management (each branch managed independently)

### Acceptance Criteria

- [ ] **AC1:** Members page shows a table of branch members with columns: name, email, role, override count; sortable by role
- [ ] **AC2:** Clicking a member row opens the permission detail view for that user
- [ ] **AC3:** Permission detail view shows all permissions in a categorized list (Properties, Contacts, Appointments, Publishing, Leads, Reports) with toggle switches
- [ ] **AC4:** Each permission row shows: permission name, description, effective state (granted/denied), source badge ("Role Default" or "Override"), and a toggle switch
- [ ] **AC5:** Toggling a permission that is a role default to "off" creates a deny override; toggling a non-default permission to "on" creates a grant override
- [ ] **AC6:** Toggling a permission back to its role default state removes the override (returns to "Role Default" source)
- [ ] **AC7:** Creating a deny override shows a confirmation dialog: "This will revoke [permission] for [user]. They will lose access even though their role normally grants it."
- [ ] **AC8:** Permission changes are reflected immediately in the UI without page reload
- [ ] **AC9:** Only admins and owners see the permission management UI; agents and viewers see their own permissions as read-only
- [ ] **AC10:** All components have unit tests; Stitch designs exist for: members list, permission detail view, confirmation dialog

### Implementation Steps

1. Define use cases for: view member permissions list, view permission detail for user, toggle permission override, view permission change history
2. Generate Stitch MCP designs for: members permission list, permission detail panel, override confirmation dialog
3. Download Stitch HTML to `.stitch-html/permission-members-list.html`, `.stitch-html/permission-detail.html`, `.stitch-html/permission-confirm-dialog.html`
4. Create `useUserPermissions` hook calling `GET /api/organizations/{orgId}/permissions/{userId}`
5. Create `useSetPermissionOverride` mutation hook calling `PUT /api/organizations/{orgId}/permissions/{userId}/{permission}`
6. Create `useRemovePermissionOverride` mutation hook calling `DELETE /api/organizations/{orgId}/permissions/{userId}/{permission}`
7. Create `PermissionMembersList` component with sortable table
8. Create `PermissionDetailPanel` component with categorized permission list and toggle switches
9. Create `PermissionToggle` component with visual distinction for defaults vs. overrides
10. Create `OverrideConfirmDialog` component for deny override confirmation
11. Create `PermissionSourceBadge` component ("Role Default" / "Override")
12. Add routes: `/organizations/[orgId]/permissions`, `/organizations/[orgId]/permissions/[userId]`
13. Integrate into the branch settings navigation (add "Permissions" tab)
14. Write component tests for all new components
15. Verify against Stitch designs

### Files to Create/Modify

**Create:**
- `apps/web/src/hooks/useUserPermissions.ts`
- `apps/web/src/hooks/useSetPermissionOverride.ts`
- `apps/web/src/hooks/useRemovePermissionOverride.ts`
- `apps/web/src/components/permissions/PermissionMembersList.tsx`
- `apps/web/src/components/permissions/PermissionDetailPanel.tsx`
- `apps/web/src/components/permissions/PermissionToggle.tsx`
- `apps/web/src/components/permissions/PermissionSourceBadge.tsx`
- `apps/web/src/components/permissions/OverrideConfirmDialog.tsx`
- `apps/web/src/components/permissions/PermissionCategoryGroup.tsx`
- `apps/web/src/app/[locale]/(dashboard)/organizations/[orgId]/permissions/page.tsx`
- `apps/web/src/app/[locale]/(dashboard)/organizations/[orgId]/permissions/[userId]/page.tsx`
- `apps/web/src/components/permissions/__tests__/PermissionMembersList.test.tsx`
- `apps/web/src/components/permissions/__tests__/PermissionDetailPanel.test.tsx`
- `apps/web/src/components/permissions/__tests__/PermissionToggle.test.tsx`
- `apps/web/src/components/permissions/__tests__/OverrideConfirmDialog.test.tsx`
- `.stitch-html/permission-members-list.html`
- `.stitch-html/permission-detail.html`
- `.stitch-html/permission-confirm-dialog.html`

**Modify:**
- `apps/web/src/messages/en.json` (add permission translation keys)
- `apps/web/src/messages/es.json` (add permission translation keys)
- `apps/web/src/components/layout/BranchSettingsNav.tsx` (or equivalent, add "Permissions" tab)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `PermissionMembersList` renders member rows with correct role and override count | Vitest + RTL |
| Unit | `PermissionDetailPanel` renders all permissions categorized with correct toggle states | Vitest + RTL, mock data |
| Unit | `PermissionToggle` calls set-override API when toggling on a non-default permission | Vitest + RTL, mock fetch |
| Unit | `PermissionToggle` calls remove-override API when toggling back to role default | Vitest + RTL, mock fetch |
| Unit | `OverrideConfirmDialog` shows warning text and calls callback on confirm | Vitest + RTL |
| Unit | `PermissionSourceBadge` shows "Role Default" or "Override" with correct styling | Vitest + RTL |
| Unit | Permission categories group permissions correctly | Vitest + RTL |
| Manual | Full flow: navigate to permissions, select member, toggle override, verify immediate update | Dev environment |
| Manual | Visual comparison against Stitch designs | Dev environment |
| Manual | Responsive layout on mobile viewport | Dev environment |

### Security & Privacy Considerations

- Permission management UI must only be accessible to admins and owners; the route should redirect non-authorized users
- Toggle controls must be disabled (read-only) for viewers and agents viewing their own permissions
- The confirmation dialog for deny overrides prevents accidental permission revocation
- Permission changes should optimistically update the UI but revert on API error

### TDD Reminder

Write component tests first with mock permission data covering: all permissions granted (owner), mixed defaults (admin), minimal permissions (agent), override states. Then implement components to pass the tests. Write toggle interaction tests that verify correct API calls are made.

---

## Task 1.7 -- E2E Permission Tests

**Status:** DONE
**Dependencies:** 1.3 (authorization middleware), 1.4 (SDK client), 1.6 (frontend UI)

### Goal

Create a comprehensive end-to-end test suite that verifies the entire permission system works correctly from UI interaction through API enforcement to SDK client consumption. This validates that permission changes made in the UI are immediately effective across the system, including cross-service permission checks via the SDK client.

### Scope

**In scope:**
- E2E test scenarios covering the complete permission lifecycle: create agency, add branches, assign roles, set overrides, verify access, remove overrides, verify revocation
- Backend integration tests simulating cross-service authorization: service A calls orgs-api SDK to check permissions before allowing an action
- Frontend E2E tests (Playwright) for: agency creation, branch management, permission override toggle, verification of UI state changes
- Load test for permission evaluation: verify `IPermissionEvaluator` performs within latency budget under concurrent access
- Negative test cases: unauthorized access attempts, invalid permission names, cross-tenant access attempts
- Test data factories for creating agencies, branches, members, and overrides programmatically

**Out of scope:**
- Performance benchmarking (separate task)
- Penetration testing
- Tests for features not yet built (properties, contacts, etc.)

### Acceptance Criteria

- [ ] **AC1:** E2E test: create agency with 2 branches, add users with different roles, verify each user sees only their authorized data
- [ ] **AC2:** E2E test: agent without `PropertiesViewAll` override cannot access the "view all properties" endpoint; after override is granted, access succeeds
- [ ] **AC3:** E2E test: admin with `PropertiesViewAll` deny override loses access to "view all properties" endpoint; after override is removed, access returns
- [ ] **AC4:** E2E test: owner always has full access regardless of any overrides
- [ ] **AC5:** E2E test: SDK client `HasPermissionAsync` returns correct results for all role+override combinations
- [ ] **AC6:** E2E test: SDK client `PermissionGuard.RequirePermissionAsync` throws `ForbiddenException` for unauthorized access
- [ ] **AC7:** E2E test: permission cache invalidation -- after setting an override, the next permission check (within 30s) reflects the change
- [ ] **AC8:** E2E test: cross-tenant isolation -- user in Agency A cannot access Agency B's branches or modify Agency B's permissions
- [ ] **AC9:** Playwright test: navigate to permission management, toggle an override, verify UI updates immediately
- [ ] **AC10:** All E2E tests run in CI and pass consistently (no flaky tests)

### Implementation Steps

1. Create test data factory classes:
   - `AgencyTestFactory`: creates agency with branches and members
   - `PermissionTestFactory`: sets up permission overrides for test scenarios
   - `AuthenticatedClientFactory`: creates HTTP clients with specific user identities
2. Create `tests/Propely.OrgsApi.E2ETests/` project in the orgs-api solution
3. Write agency lifecycle E2E tests:
   - Create agency, verify 201
   - Add branch, verify branch appears in agency details
   - List agencies for user, verify correct results
4. Write permission enforcement E2E tests:
   - Set up agent user, verify 403 on protected endpoint
   - Grant override, verify 200 on same endpoint
   - Remove override, verify 403 again
5. Write owner immunity E2E tests:
   - Create deny override for owner, verify owner still has access
6. Write SDK client E2E tests:
   - Start orgs-api via `WebApplicationFactory`
   - Create `IPermissionsApi` client pointing at test server
   - Verify `HasPermissionAsync` results for all combinations
7. Write `IPermissionGuard` E2E tests:
   - Verify `RequirePermissionAsync` throws for unauthorized users
   - Verify it passes for authorized users
8. Write cross-tenant isolation tests:
   - Create two agencies with separate users
   - Verify user from Agency A cannot access Agency B data
9. Write Playwright E2E tests for frontend:
   - Navigate to `/agencies/new`, create agency, verify redirect
   - Navigate to `/organizations/{orgId}/permissions`, toggle override, verify toggle state
10. Write permission evaluation latency test:
    - Run 100 concurrent `HasPermissionAsync` calls
    - Assert p95 latency < 50ms (with cache)
11. Set up CI configuration for E2E test execution
12. Create test data cleanup utilities

### Files to Create/Modify

**Create:**
- `services/orgs-api/tests/Propely.OrgsApi.E2ETests/Propely.OrgsApi.E2ETests.csproj`
- `services/orgs-api/tests/Propely.OrgsApi.E2ETests/Factories/AgencyTestFactory.cs`
- `services/orgs-api/tests/Propely.OrgsApi.E2ETests/Factories/PermissionTestFactory.cs`
- `services/orgs-api/tests/Propely.OrgsApi.E2ETests/Factories/AuthenticatedClientFactory.cs`
- `services/orgs-api/tests/Propely.OrgsApi.E2ETests/AgencyLifecycleTests.cs`
- `services/orgs-api/tests/Propely.OrgsApi.E2ETests/PermissionEnforcementTests.cs`
- `services/orgs-api/tests/Propely.OrgsApi.E2ETests/OwnerImmunityTests.cs`
- `services/orgs-api/tests/Propely.OrgsApi.E2ETests/SdkClientPermissionTests.cs`
- `services/orgs-api/tests/Propely.OrgsApi.E2ETests/PermissionGuardTests.cs`
- `services/orgs-api/tests/Propely.OrgsApi.E2ETests/CrossTenantIsolationTests.cs`
- `services/orgs-api/tests/Propely.OrgsApi.E2ETests/PermissionLatencyTests.cs`
- `apps/web/e2e/agency-management.spec.ts`
- `apps/web/e2e/permission-management.spec.ts`
- `apps/web/e2e/fixtures/agency-fixtures.ts`

**Modify:**
- `services/orgs-api/Propely.OrgsApi.sln` (add E2E test project)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| E2E (backend) | Full agency lifecycle: create, add branches, list, delete | `WebApplicationFactory`, Testcontainers |
| E2E (backend) | Permission enforcement: agent blocked, agent with override allowed | `WebApplicationFactory`, Testcontainers |
| E2E (backend) | Owner immunity: deny overrides do not affect owner | `WebApplicationFactory`, Testcontainers |
| E2E (backend) | SDK client permission evaluation matches direct API calls | `WebApplicationFactory`, SDK client |
| E2E (backend) | Cross-tenant isolation: Agency A user cannot access Agency B | `WebApplicationFactory`, multiple users |
| E2E (backend) | Cache invalidation: override change reflected in < 30s | `WebApplicationFactory`, timed assertions |
| E2E (backend) | Permission evaluation latency: p95 < 50ms under load | `WebApplicationFactory`, concurrent calls |
| E2E (frontend) | Agency creation flow: form, submit, redirect | Playwright |
| E2E (frontend) | Permission toggle: click toggle, verify API call, verify UI update | Playwright |
| E2E (frontend) | Cross-branch navigation via branch switcher | Playwright |

### Security & Privacy Considerations

- Test data must use synthetic users and agencies; no real data
- E2E tests must run in isolated test databases (Testcontainers) to prevent data leakage
- Cross-tenant isolation tests are critical security tests; they must cover: API-level isolation, data-level isolation, and UI-level isolation
- Playwright tests must verify that unauthorized UI elements (settings, delete buttons) are not rendered for non-admin users
- Test cleanup must remove all created data after each test run to prevent state leakage between tests

### TDD Reminder

Write the test scenarios and expected outcomes first as failing tests. For each scenario, clearly document the setup (which users, roles, overrides), the action (API call or UI interaction), and the expected result (status code, UI state). Then verify existing implementations make them pass. Fix any discovered bugs as part of this task.

---

## Summary

| Task | Title | Status | Dependencies |
|---|---|---|---|
| 1.1 | Agency Entity & Hierarchy | DONE | 0.2 |
| 1.2 | Permission Domain Model | DONE | 1.1 |
| 1.3 | Authorization Policies & Middleware | DONE | 1.2 |
| 1.4 | OrgsApi SDK Client for Permissions | DONE | 1.3, 0.4 |
| 1.5 | Agency Management UI | DONE | 1.1 |
| 1.6 | Permission Management UI | DONE | 1.3, 1.5 |
| 1.7 | E2E Permission Tests | DONE | 1.3, 1.4, 1.6 |

## Dependency Graph

```
Phase 0 (prerequisites)
  0.2 Rename ──────────── 0.4 NuGet SDK Client Infrastructure
       │                       │
       ▼                       │
  1.1 Agency Entity            │
   ├────────────────┐          │
   ▼                ▼          │
  1.2 Permission   1.5 Agency  │
  Domain Model     Mgmt UI    │
   │                │          │
   ▼                │          │
  1.3 Authorization │          │
  Policies &        │          │
  Middleware        │          │
   ├────────┬───────┘          │
   │        │                  │
   │        ▼                  │
   │       1.6 Permission      │
   │       Mgmt UI            │
   │        │                  │
   ▼        │                  ▼
  1.4 OrgsApi SDK ◄────────── 0.4
  Client for Perms
   │        │
   ▼        ▼
  1.7 E2E Permission Tests
```

## Completion Criteria

Phase 1 is complete when:
1. `Agency` aggregate root exists with full branch management lifecycle
2. `Permission` enum and `PermissionOverride` entity exist in the Domain layer
3. `DefaultPermissionMatrix` correctly maps all 4 roles to their default permissions
4. `IPermissionEvaluator` resolves effective permissions (role defaults + overrides) with 100% test coverage for all combinations
5. ASP.NET Core authorization pipeline enforces permissions dynamically via `[RequirePermission]` attribute
6. Permission evaluation is cached in Redis with 30-second TTL
7. `Propely.OrgsApi.Client` SDK exposes `IPermissionsApi`, `IAgenciesApi`, and `IPermissionGuard` for cross-service authorization
8. Frontend agency management screens (create, dashboard, settings, branch switcher) are implemented and match Stitch designs
9. Frontend permission management screens (member list, permission detail, toggle controls) are implemented and match Stitch designs
10. E2E tests verify the complete permission lifecycle including cross-tenant isolation and SDK client behavior
