# Task: 0009-authorization-policies-middleware

## Metadata
- ID: 0009
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-22
- Related docs:
  - Walkthrough: `docs/walkthroughs/0009-authorization-policies-middleware.md`
  - Epic: `docs/backlog/epic-P1-agency-permissions.md` (Task 1.3)

## Goal
Wire the permission system into the ASP.NET Core authorization pipeline. Create dynamic authorization policies that evaluate permissions at request time using `IPermissionEvaluator`, persist `PermissionOverride` entities with EF Core, and expose API endpoints for managing overrides.

## Context
Task 1.2 (0008) established the permission domain model: `Permission` enum, `PermissionOverride` entity, `DefaultPermissionMatrix`, and `PermissionEvaluator`. However, none of this is wired into persistence, DI, or the HTTP pipeline. The `IPermissionOverrideRepository` has no implementation, `IPermissionEvaluator` is not registered in DI, and no authorization handlers or API endpoints exist. This task bridges the domain model to a fully enforced, runtime-evaluated permission system.

## Scope
### In scope
- EF Core configuration for `PermissionOverride` entity, migration for `PermissionOverrides` table
- `PermissionOverrideRepository` implementation in Infrastructure
- `PermissionAuthorizationHandler` implementing `IAuthorizationHandler` that calls `IPermissionEvaluator`
- `RequirePermissionAttribute` custom attribute for declarative policy enforcement
- `PermissionRequirement` implementing `IAuthorizationRequirement`
- `PermissionPolicyProvider` for dynamic authorization policies
- Registration of all services in DI (`Program.cs` / `DependencyInjection.cs`)
- `CachedPermissionEvaluator` decorator using Redis with 30-second TTL
- API endpoints for permission management:
  - `GET /api/organizations/{orgId}/permissions/{userId}` (list effective permissions)
  - `PUT /api/organizations/{orgId}/permissions/{userId}/{permission}` (set override)
  - `DELETE /api/organizations/{orgId}/permissions/{userId}/{permission}` (remove override)
- CQRS commands/queries: `GetUserPermissionsQuery`, `SetPermissionOverrideCommand`, `RemovePermissionOverrideCommand`

### Out of scope
- SDK client (task 1.4)
- Frontend UI (tasks 1.5, 1.6)
- Cross-service permission enforcement

## Requirements
- R1: `PermissionOverride` entities must be persisted in PostgreSQL with a composite unique index on `(UserId, OrganizationId, Permission)`
- R2: `[RequirePermission(Permission.X)]` attribute must enforce permissions via `IPermissionEvaluator` at the ASP.NET Core pipeline level
- R3: Permission evaluation must be cached in Redis with 30-second TTL, invalidated on override changes
- R4: Only admins and owners can manage permission overrides
- R5: All permission changes must be audit-logged

## Acceptance Criteria
- AC1: `PermissionOverrides` table exists with composite unique index on `(UserId, OrganizationId, Permission)`
- AC2: `[RequirePermission(Permission.PropertiesViewAll)]` on a controller action enforces the permission
- AC3: Agent without permission gets 403 on protected endpoint
- AC4: Agent with grant override gets 200 on protected endpoint
- AC5: `GET /api/organizations/{orgId}/permissions/{userId}` returns effective permissions with source
- AC6: `PUT /api/organizations/{orgId}/permissions/{userId}/PropertiesViewAll` creates/updates override (admin/owner only)
- AC7: `DELETE /api/organizations/{orgId}/permissions/{userId}/PropertiesViewAll` removes override
- AC8: Permission evaluation uses Redis cache with 30-second TTL; cache invalidated on changes
- AC9: All permission changes are audit-logged
- AC10: `dotnet test` passes all new and existing tests

## Constraints (non-negotiable)
- Clean Architecture layers respected.
- English-only repo content.
- No secrets in repo.
- Update walkthrough.
- TDD: write tests first, then implementation.

## Proposed Approach (high-level)
1. Infrastructure: EF Core config, migration, repository implementation
2. Application: CachedPermissionEvaluator decorator, CQRS commands/queries
3. API: Authorization handler, policy provider, RequirePermission attribute, PermissionsController
4. DI registration across all layers
5. Unit tests for cached evaluator, command handlers
6. Integration tests for authorization enforcement and API endpoints

## Implementation Steps
1. Create `PermissionOverrideConfiguration.cs` EF Core config with unique index
2. Add `DbSet<PermissionOverride>` to `AppDbContext`
3. Create EF Core migration `AddPermissionOverrides`
4. Create `PermissionOverrideRepository` implementing `IPermissionOverrideRepository`
5. Create `CachedPermissionEvaluator` decorator wrapping `PermissionEvaluator`
6. Create `PermissionRequirement` implementing `IAuthorizationRequirement`
7. Create `RequirePermissionAttribute` as `AuthorizeAttribute` subclass
8. Create `PermissionAuthorizationHandler` implementing `AuthorizationHandler<PermissionRequirement>`
9. Create `PermissionPolicyProvider` implementing `IAuthorizationPolicyProvider`
10. Create `GetUserPermissionsQuery` and handler
11. Create `SetPermissionOverrideCommand` and handler with audit logging
12. Create `RemovePermissionOverrideCommand` and handler with audit logging
13. Create `PermissionsController` with endpoints
14. Register all services in DI
15. Write unit tests for CachedPermissionEvaluator, command handlers
16. Write integration tests for authorization enforcement and API endpoints

## Files to Create / Modify
### Create
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/Persistence/Configurations/PermissionOverrideConfiguration.cs`
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/Persistence/Repositories/PermissionOverrideRepository.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/Services/CachedPermissionEvaluator.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/Queries/GetUserPermissions/GetUserPermissionsQuery.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/Queries/GetUserPermissions/GetUserPermissionsQueryHandler.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/Commands/SetPermissionOverride/SetPermissionOverrideCommand.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/Commands/SetPermissionOverride/SetPermissionOverrideCommandHandler.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/Commands/SetPermissionOverride/SetPermissionOverrideCommandValidator.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/Commands/RemovePermissionOverride/RemovePermissionOverrideCommand.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/Commands/RemovePermissionOverride/RemovePermissionOverrideCommandHandler.cs`
- `services/orgs-api/src/Propely.OrgsApi.Api/Authorization/PermissionRequirement.cs`
- `services/orgs-api/src/Propely.OrgsApi.Api/Authorization/RequirePermissionAttribute.cs`
- `services/orgs-api/src/Propely.OrgsApi.Api/Authorization/PermissionAuthorizationHandler.cs`
- `services/orgs-api/src/Propely.OrgsApi.Api/Authorization/PermissionPolicyProvider.cs`
- `services/orgs-api/src/Propely.OrgsApi.Api/Controllers/PermissionsController.cs`
- `services/orgs-api/src/Propely.OrgsApi.Api/Dtos/SetPermissionOverrideRequest.cs`
- `services/orgs-api/tests/Propely.OrgsApi.UnitTests/Application/Permissions/CachedPermissionEvaluatorTests.cs`
- `services/orgs-api/tests/Propely.OrgsApi.IntegrationTests/Permissions/PermissionAuthorizationTests.cs`
- `services/orgs-api/tests/Propely.OrgsApi.IntegrationTests/Permissions/PermissionsControllerTests.cs`

### Modify
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/Persistence/AppDbContext.cs` (add `DbSet<PermissionOverride>`, add config in `OnModelCreating`)
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/DependencyInjection.cs` (register `PermissionOverrideRepository`)
- `services/orgs-api/src/Propely.OrgsApi.Application/DependencyInjection.cs` (register `PermissionEvaluator`, `CachedPermissionEvaluator`)
- `services/orgs-api/src/Propely.OrgsApi.Api/DependencyInjection.cs` (register authorization handler, policy provider)

## Testing Plan
- Unit tests:
  - `CachedPermissionEvaluator` returns cached result within TTL
  - `CachedPermissionEvaluator` invalidates cache on override change
  - `SetPermissionOverrideCommandHandler` creates override and validates authorization
  - `RemovePermissionOverrideCommandHandler` removes override
  - `SetPermissionOverrideCommandValidator` rejects invalid inputs
- Integration tests:
  - Agent without permission gets 403 on protected endpoint
  - Agent with grant override gets 200 on protected endpoint
  - Admin can set/remove overrides via API
  - Agent cannot set overrides (403)
  - `GET /permissions/{userId}` returns effective permissions with sources
  - Override removal reverts behavior to role default
- Manual verification: None required

## Security & Privacy
- Only admins and owners can view/modify permission overrides
- Admins cannot modify owner permissions (owners always fully privileged)
- Permission changes audit-logged with actor, target, permission, old/new value
- Redis cache TTL of 30 seconds: acceptable tradeoff for performance
- `PermissionPolicyProvider` validates policy names against `Permission` enum
- Fail-closed: if permission evaluation fails, deny access

## Observability
- Logs: Permission override creation/removal logged at Information level
- Metrics: None (defer to later task)
- Traces: Permission evaluation included in request traces via OpenTelemetry

## Rollback Plan
Revert the EF Core migration with `dotnet ef database update <previous-migration>`. Remove all new files and DI registrations. The system reverts to the previous authentication-only authorization model.

## Definition of Done Checklist
- [ ] Acceptance criteria met
- [ ] Build passes
- [ ] Tests added/updated and pass
- [ ] Formatting/analyzers pass
- [ ] No secrets committed
- [ ] Walkthrough updated
