# Walkthrough: 0009-authorization-policies-middleware

## Task Reference
- Task: `docs/tasks/0009-authorization-policies-middleware.md`
- Walkthrough: `docs/walkthroughs/0009-authorization-policies-middleware.md`
- Branch/PR: `feat/authorization-policies-middleware` / TBD
- Date: `2026-02-22`

## Summary
Wired the permission domain model (from task 0008) into the ASP.NET Core authorization pipeline. This includes EF Core persistence for `PermissionOverride`, a cached permission evaluator decorator using Redis with 30-second TTL, dynamic authorization policies with `[RequirePermission]` attribute, CQRS commands/queries for managing overrides, and REST API endpoints for permission management.

## Context
- Background: Task 0008 established the pure domain model: `Permission` enum, `PermissionOverride` entity, `DefaultPermissionMatrix`, and `PermissionEvaluator`. However, none of this was wired into persistence, DI, or the HTTP pipeline.
- Problem statement: `IPermissionOverrideRepository` had no implementation, `IPermissionEvaluator` was not registered in DI, and no authorization handlers or API endpoints existed.
- Constraints: Clean Architecture layers, TDD, Redis caching with 30s TTL.

## Decisions & Trade-offs
- **Decision:** Decorator pattern for caching
  - Options considered: (1) Cache inside `PermissionEvaluator` directly, (2) Decorator wrapping inner evaluator, (3) MediatR pipeline behavior
  - Why this choice: Decorator keeps the original evaluator pure and testable. Cache logic is isolated and can be swapped independently.
  - Consequences / risks: Slightly more DI wiring. `CachedPermissionEvaluator` is registered as concrete type for cache invalidation access.

- **Decision:** Dynamic policy provider instead of static policies
  - Options considered: (1) Register one static policy per permission, (2) Dynamic `IAuthorizationPolicyProvider`
  - Why this choice: Dynamic provider scales automatically as new permissions are added to the enum without DI registration changes.
  - Consequences / risks: Policy names must follow the `Permission:` prefix convention.

- **Decision:** Replace existing override on update (delete + create) instead of in-place mutation
  - Options considered: (1) Mutate `Granted` property on existing entity, (2) Delete old + create new
  - Why this choice: `PermissionOverride.Granted` has a private setter and the entity raises domain events on creation. Replacing ensures proper event semantics.

## Implementation Notes
- Key changes:
  - EF Core: `PermissionOverrideConfiguration` with composite unique index on `(UserId, OrganizationId, Permission)`, Permission stored as string
  - Repository: `PermissionOverrideRepository` implements `IPermissionOverrideRepository` with standard CRUD operations
  - Caching: `CachedPermissionEvaluator` wraps `PermissionEvaluator`, caches effective permissions keyed by `permissions:{userId}:{orgId}` with 30s TTL
  - Authorization: `PermissionRequirement` + `PermissionAuthorizationHandler` + `PermissionPolicyProvider` enable `[RequirePermission(Permission.X)]` attribute on controllers
  - CQRS: `GetUserPermissionsQuery`, `SetPermissionOverrideCommand`, `RemovePermissionOverrideCommand` with authorization checks (admin/owner only for modifications)
  - Controller: `PermissionsController` at `/api/organizations/{orgId}/permissions`

- Edge cases handled:
  - Owner permissions cannot be overridden (fail-fast with 400)
  - Non-member target returns 400
  - Cache invalidation on every override change
  - Remove of non-existent override succeeds silently
  - Invalid permission names return 400

- Known limitations:
  - Cache TTL is 30 seconds, so permission changes may take up to 30 seconds to propagate (acceptable trade-off)
  - Cache-hit unit tests limited due to private nested `CachedPermissions` class

## Data / Schema / Migrations
- DB changes: `permission_overrides` table with columns: `id` (PK), `user_id`, `organization_id`, `permission` (varchar(50)), `granted`, `granted_by`, `granted_at_utc`
- Composite unique index: `ix_permission_overrides_user_org_permission` on `(user_id, organization_id, permission)`
- Migration: `AddPermissionOverrides` (EF Core code-first)
- Backward compatibility: Additive only, no breaking changes

## Commands Run
```bash
# Create migration
dotnet ef migrations add AddPermissionOverrides \
  --project services/orgs-api/src/Propely.OrgsApi.Infrastructure \
  --startup-project services/orgs-api/src/Propely.OrgsApi.Api

# Build
dotnet build services/orgs-api/Propely.OrgsApi.sln

# Run unit tests (646 passed)
dotnet test services/orgs-api/tests/Propely.OrgsApi.UnitTests/Propely.OrgsApi.UnitTests.csproj

# Run integration tests (173 passed)
dotnet test services/orgs-api/tests/Propely.OrgsApi.IntegrationTests/Propely.OrgsApi.IntegrationTests.csproj
```

## Files Changed

### Created
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/Persistence/Configurations/PermissionOverrideConfiguration.cs` -- EF Core entity configuration with composite unique index
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/Persistence/Repositories/PermissionOverrideRepository.cs` -- `IPermissionOverrideRepository` implementation
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/Services/CachedPermissionEvaluator.cs` -- Redis-backed caching decorator for `PermissionEvaluator`
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/Queries/GetUserPermissions/GetUserPermissionsQuery.cs` -- CQRS query record
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/Queries/GetUserPermissions/GetUserPermissionsQueryHandler.cs` -- Query handler with authorization
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/Commands/SetPermissionOverride/SetPermissionOverrideCommand.cs` -- CQRS command record
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/Commands/SetPermissionOverride/SetPermissionOverrideCommandHandler.cs` -- Command handler with admin/owner authorization
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/Commands/SetPermissionOverride/SetPermissionOverrideCommandValidator.cs` -- FluentValidation validator
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/Commands/RemovePermissionOverride/RemovePermissionOverrideCommand.cs` -- CQRS command record
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/Commands/RemovePermissionOverride/RemovePermissionOverrideCommandHandler.cs` -- Command handler with cache invalidation
- `services/orgs-api/src/Propely.OrgsApi.Api/Authorization/PermissionRequirement.cs` -- `IAuthorizationRequirement` for permissions
- `services/orgs-api/src/Propely.OrgsApi.Api/Authorization/RequirePermissionAttribute.cs` -- `[RequirePermission(Permission.X)]` attribute
- `services/orgs-api/src/Propely.OrgsApi.Api/Authorization/PermissionAuthorizationHandler.cs` -- `AuthorizationHandler<PermissionRequirement>` that calls `IPermissionEvaluator`
- `services/orgs-api/src/Propely.OrgsApi.Api/Authorization/PermissionPolicyProvider.cs` -- Dynamic `IAuthorizationPolicyProvider` for permission-prefixed policies
- `services/orgs-api/src/Propely.OrgsApi.Api/Controllers/PermissionsController.cs` -- REST API for permission management
- `services/orgs-api/src/Propely.OrgsApi.Api/Dtos/SetPermissionOverrideRequest.cs` -- Request DTO
- `services/orgs-api/tests/Propely.OrgsApi.UnitTests/Application/Permissions/CachedPermissionEvaluatorTests.cs` -- 8 unit tests for caching behavior
- `services/orgs-api/tests/Propely.OrgsApi.UnitTests/Application/Permissions/SetPermissionOverrideCommandHandlerTests.cs` -- 9 unit tests for set override handler
- `services/orgs-api/tests/Propely.OrgsApi.UnitTests/Application/Permissions/RemovePermissionOverrideCommandHandlerTests.cs` -- 7 unit tests for remove override handler
- `services/orgs-api/tests/Propely.OrgsApi.IntegrationTests/Api/Permissions/PermissionsEndpointTests.cs` -- 9 integration tests for API endpoints

### Modified
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/Persistence/AppDbContext.cs` -- Added `DbSet<PermissionOverride>` and applied configuration
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/DependencyInjection.cs` -- Registered `IPermissionOverrideRepository`
- `services/orgs-api/src/Propely.OrgsApi.Application/DependencyInjection.cs` -- Registered `PermissionEvaluator`, `CachedPermissionEvaluator`, `IPermissionEvaluator`
- `services/orgs-api/src/Propely.OrgsApi.Api/DependencyInjection.cs` -- Registered `PermissionAuthorizationHandler`, `PermissionPolicyProvider`

## Tests
### Unit
- `CachedPermissionEvaluatorTests` (8 tests): cache miss delegation, cache population via SetAsync, cache invalidation via RemoveAsync, correct cache key format, cancellation token propagation
- `SetPermissionOverrideCommandHandlerTests` (9 tests): authorization checks (no membership, agent, viewer), owner target rejection, non-member target rejection, admin/owner success, existing override replacement, cache invalidation
- `RemovePermissionOverrideCommandHandlerTests` (7 tests): authorization checks, existing override removal, no-op for missing override, cache invalidation
- How to run: `dotnet test services/orgs-api/tests/Propely.OrgsApi.UnitTests`

### Integration
- `PermissionsEndpointTests` (9 tests): GET permissions (200, all granted for owner, 401 unauthenticated), PUT override (400 invalid permission, 400 owner target, 401 unauthenticated), DELETE override (400 invalid permission, 401 unauthenticated, 200 no existing override)
- How to run: `dotnet test services/orgs-api/tests/Propely.OrgsApi.IntegrationTests --filter "FullyQualifiedName~Permissions"`

### Manual
- None required

## Observability
- Logs added/updated: Permission override set/removed logged at Information level with actor, target, permission, and org IDs
- Traces/metrics added/updated: Permission evaluation included in existing OpenTelemetry pipeline via standard middleware tracing

## Security
- Validation: Permission enum values validated via `Enum.TryParse`, userId/orgId validated as GUIDs via route constraints
- AuthN/AuthZ impact: New `[RequirePermission]` attribute enables fine-grained permission enforcement; fail-closed (deny on evaluation failure)
- Sensitive data handling: No PII in permission data; audit entries created automatically by `AppDbContext.SaveChangesAsync`

## Follow-ups / Backlog
- [ ] Task 1.4: OrgsApi SDK Client for Permissions (expose `IPermissionEvaluator` to other services)
- [ ] Task 1.6: Permission Management UI
- [ ] Consider making `CachedPermissions` internal + `InternalsVisibleTo` for more thorough cache-hit unit tests

## Checklist
- [x] Task scope matches `docs/tasks/0009-authorization-policies-middleware.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
