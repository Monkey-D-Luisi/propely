# Walkthrough: 0008-permission-domain-model

## Task Reference
- Task: `docs/tasks/0008-permission-domain-model.md`
- Walkthrough: `docs/walkthroughs/0008-permission-domain-model.md`
- Branch/PR: `feat/permission-domain-model` / TBD
- Date: `2026-02-22`

## Summary
Implemented the Permission domain model for orgs-api, including the `Permission` enum (8 granular permissions), `PermissionOverride` entity with factory method and domain events, `DefaultPermissionMatrix` mapping roles to default permission sets, and `PermissionEvaluator` service that resolves effective permissions by combining role defaults with overrides. This is a pure domain/application layer task with no persistence or API changes.

## Context
- Background: The Agency entity and hierarchy (Task 1.1/0007) are complete. The permission system is the next foundational piece needed before authorization policies (Task 1.3) can be built.
- Problem statement: Need a flexible permission model that supports both role-based defaults and per-user overrides. Owners always have full access, admins have all permissions by default, agents only have LeadsManage, and viewers have no special permissions.
- Constraints: Pure domain/application layer -- no persistence, API, or middleware in this task.

## Decisions & Trade-offs
- **Decision:** `EffectivePermissionDto` placed in `Permissions.DTOs` namespace matching its folder location (`Permissions/DTOs/`).
  - Options considered: Co-location in `Permissions.Interfaces` namespace vs. separate `DTOs` namespace matching folder
  - Why this choice: Follows the established codebase convention where DTOs are placed in separate `DTOs` folders with matching namespaces (e.g., `Agencies.DTOs`). Namespace was corrected during code review (cr-0010).
  - Consequences: Requires explicit `using` in `IPermissionEvaluator.cs` and `PermissionEvaluator.cs`, but consistent with all other DTOs in the codebase
- **Decision:** Admin role gets all permissions by default (same as Owner for defaults, but can be restricted via deny overrides).
  - Why this choice: Per the epic specification, admins have full branch access. The difference from Owner is that admin permissions CAN be restricted via deny overrides, while Owner is immune to all overrides.
- **Decision:** `PermissionOverride` entity does not implement `ISoftDeletable` -- instead it has a `Revoke()` method that raises a domain event. Physical deletion is deferred to the repository implementation in Task 1.3.
  - Why this choice: Overrides are simple toggle entities; soft-delete adds complexity without clear benefit here. The `Revoke()` method provides the domain event trail.

## Implementation Notes
- Key changes:
  - `Permission` enum with 8 values covering all feature domains
  - `PermissionOverride` entity with `Create()` factory (raises granted/denied events) and `Revoke()` method (raises revoked event)
  - `DefaultPermissionMatrix` static class with immutable, pre-computed permission sets per role
  - `PermissionEvaluator` implements `IPermissionEvaluator` -- resolves permissions via membership lookup + override lookup
  - Three domain events following the existing `IDomainEvent` pattern with `*Data` companion records
- Edge cases handled:
  - No membership returns false for all permissions
  - Deleted (soft-deleted) membership returns false for all permissions
  - Owner always returns true regardless of any overrides
  - Invalid role enum value throws `ArgumentOutOfRangeException`
- Known limitations:
  - No caching layer yet (will be added in Task 1.3 with `CachedPermissionEvaluator`)
  - No persistence yet (`IPermissionOverrideRepository` is interface only)

## Data / Schema / Migrations
- DB changes: None (domain model only)
- Migration strategy: N/A
- Backward compatibility: N/A

## Commands Run
```bash
dotnet build services/orgs-api/Propely.OrgsApi.sln
dotnet test services/orgs-api/Propely.OrgsApi.sln
```

## Files Changed
### Created
- `services/orgs-api/src/Propely.OrgsApi.Domain/Permissions/Permission.cs` -- Permission enum with 8 values
- `services/orgs-api/src/Propely.OrgsApi.Domain/Permissions/PermissionOverride.cs` -- Entity with Create() factory and Revoke() method
- `services/orgs-api/src/Propely.OrgsApi.Domain/Permissions/DefaultPermissionMatrix.cs` -- Static role-to-permissions mapping
- `services/orgs-api/src/Propely.OrgsApi.Domain/Permissions/Events/PermissionOverrideGrantedV1.cs` -- Domain event for grant overrides
- `services/orgs-api/src/Propely.OrgsApi.Domain/Permissions/Events/PermissionOverrideDeniedV1.cs` -- Domain event for deny overrides
- `services/orgs-api/src/Propely.OrgsApi.Domain/Permissions/Events/PermissionOverrideRevokedV1.cs` -- Domain event for revoked overrides
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/Interfaces/IPermissionOverrideRepository.cs` -- Repository interface
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/Interfaces/IPermissionEvaluator.cs` -- Evaluator interface
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/DTOs/EffectivePermissionDto.cs` -- DTO for effective permission results
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/Services/PermissionEvaluator.cs` -- Evaluator implementation
- `services/orgs-api/tests/Propely.OrgsApi.UnitTests/Domain/Permissions/PermissionOverrideTests.cs` -- 9 tests for PermissionOverride entity
- `services/orgs-api/tests/Propely.OrgsApi.UnitTests/Domain/Permissions/DefaultPermissionMatrixTests.cs` -- 38 tests for DefaultPermissionMatrix
- `services/orgs-api/tests/Propely.OrgsApi.UnitTests/Application/Permissions/PermissionEvaluatorTests.cs` -- 30+ tests for PermissionEvaluator

## Tests
### Unit
- What was added/updated:
  - `PermissionOverrideTests` (9 tests): Create with granted=true/false, domain events, Revoke(), all 8 permission values, unique IDs, event metadata
  - `DefaultPermissionMatrixTests` (38 tests): Each role returns correct defaults, parameterized per-permission checks, invalid role, return type verification
  - `PermissionEvaluatorTests` (30+ tests): All role x permission x override combinations, deleted membership, no membership, cancellation token propagation, GetEffectivePermissions for all roles
- How to run: `dotnet test services/orgs-api/Propely.OrgsApi.sln`
- Results: 793 total tests pass (624 unit + 164 integration + 5 architecture), 0 failures

### Integration
- N/A (pure domain model task)

### Manual
- N/A

## Observability
- Logs added/updated: None
- Traces/metrics added/updated: None

## Security
- Validation: Owner permissions cannot be restricted by overrides (enforced in PermissionEvaluator)
- AuthN/AuthZ impact: Foundation for task 1.3 authorization policies
- Sensitive data handling: None
- `GrantedBy` field on PermissionOverride creates audit trail for who granted/denied each override

## Follow-ups / Backlog
- [ ] Task 1.3: Authorization Policies & Middleware (EF Core persistence, API endpoints, caching)
- [ ] Task 1.4: OrgsApi SDK Client for Permissions

## Checklist
- [x] Task scope matches `docs/tasks/0008-permission-domain-model.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
