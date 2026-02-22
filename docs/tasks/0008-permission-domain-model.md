# Task: 0008-permission-domain-model

## Metadata
- ID: 0008
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-22
- Related docs:
  - Walkthrough: `docs/walkthroughs/0008-permission-domain-model.md`
  - Epic: `docs/backlog/epic-P1-agency-permissions.md` (Task 1.2)

## Goal
Define the permission system in the Domain and Application layers of orgs-api. This includes the `Permission` enum, `PermissionOverride` entity, `DefaultPermissionMatrix`, `IPermissionEvaluator` interface and implementation. Pure domain model -- no persistence, no API, no middleware.

## Context
The Agency entity and hierarchy (Task 1.1/0007) are complete. The next step is to define the granular permission system that combines fixed roles (Owner, Admin, Agent, Viewer) with per-user permission overrides. This is foundational for all subsequent authorization work in tasks 1.3-1.7.

## Scope
### In scope
- `Permission` enum with 8 permission values
- `PermissionOverride` entity with factory method and domain events
- `DefaultPermissionMatrix` static class mapping each `MembershipRole` to default permissions
- `IPermissionEvaluator` interface and `PermissionEvaluator` implementation in Application layer
- `IPermissionOverrideRepository` interface in Application layer
- `EffectivePermissionDto` in Application layer
- Domain events: `PermissionOverrideGrantedV1`, `PermissionOverrideDeniedV1`, `PermissionOverrideRevokedV1`
- Unit tests for all of the above (TDD)

### Out of scope
- EF Core persistence for `PermissionOverride` (task 1.3)
- ASP.NET Core authorization policies and middleware (task 1.3)
- API endpoints for managing overrides (task 1.3)
- SDK client (task 1.4)

## Requirements
- R1: `Permission` enum lists all 8 granular permissions
- R2: `PermissionOverride` entity follows the existing Entity pattern with factory method and domain events (uses Revoke() instead of soft-delete)
- R3: `DefaultPermissionMatrix` maps each of 4 roles to their default permission set
- R4: `PermissionEvaluator` resolves effective permissions by combining base role defaults with overrides
- R5: Owner role always has all permissions regardless of overrides
- R6: Grant overrides add permissions beyond role defaults; deny overrides remove permissions from role defaults

## Acceptance Criteria
- AC1: `Permission` enum exists with 8 values: `PropertiesViewAll`, `PropertiesEditAll`, `ContactsViewAll`, `ContactsEditAll`, `AppointmentsViewAll`, `PublishingManage`, `LeadsManage`, `ReportsView`
- AC2: `PermissionOverride` entity has `Id`, `UserId`, `OrganizationId`, `Permission`, `Granted`, `GrantedBy`, `GrantedAtUtc` with private constructor and `Create` factory
- AC3: `DefaultPermissionMatrix.GetDefaults(MembershipRole.Owner)` returns all permissions
- AC4: `DefaultPermissionMatrix.GetDefaults(MembershipRole.Admin)` returns all permissions (same defaults as Owner; difference is Admin can be restricted via deny overrides)
- AC5: `DefaultPermissionMatrix.GetDefaults(MembershipRole.Agent)` returns only `LeadsManage`
- AC6: `DefaultPermissionMatrix.GetDefaults(MembershipRole.Viewer)` returns empty set
- AC7: `PermissionEvaluator.HasPermissionAsync` returns true when role grants and no deny override
- AC8: `PermissionEvaluator.HasPermissionAsync` returns true when role does NOT grant but grant override exists
- AC9: `PermissionEvaluator.HasPermissionAsync` returns false when role grants but deny override exists
- AC10: Unit tests cover all role x permission x override combinations (30+ test cases)

## Constraints (non-negotiable)
- Clean Architecture layers respected.
- English-only repo content.
- No secrets in repo.
- Update walkthrough.

## Proposed Approach (high-level)
Follow TDD (Red-Green-Refactor) approach:
1. Write Domain layer tests first, then implement: Permission enum, PermissionOverride entity, DefaultPermissionMatrix, domain events
2. Write Application layer tests first, then implement: IPermissionOverrideRepository, IPermissionEvaluator, PermissionEvaluator, DTOs

## Implementation Steps
1. Create `Domain/Permissions/Permission.cs` enum
2. Create `Domain/Permissions/PermissionOverride.cs` entity with factory, domain events
3. Create `Domain/Permissions/DefaultPermissionMatrix.cs` static class
4. Create `Domain/Permissions/Events/PermissionOverrideGrantedV1.cs`
5. Create `Domain/Permissions/Events/PermissionOverrideDeniedV1.cs`
6. Create `Domain/Permissions/Events/PermissionOverrideRevokedV1.cs`
7. Create `Application/Permissions/Interfaces/IPermissionOverrideRepository.cs`
8. Create `Application/Permissions/Interfaces/IPermissionEvaluator.cs`
9. Create `Application/Permissions/Services/PermissionEvaluator.cs`
10. Create `Application/Permissions/DTOs/EffectivePermissionDto.cs`
11. Write unit tests for DefaultPermissionMatrix (parameterized per role)
12. Write unit tests for PermissionOverride entity (creation, events)
13. Write unit tests for PermissionEvaluator (all role+override combinations)

## Files to Create / Modify
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

## Testing Plan
- Unit tests: DefaultPermissionMatrix (4 role cases), PermissionOverride entity (creation, validation, events), PermissionEvaluator (30+ combinations of role x permission x override)
- Integration tests: None (pure domain model task)
- Manual verification: None

## Security & Privacy
- Owner permissions cannot be restricted by overrides
- `GrantedBy` field creates audit trail for who granted/denied each override
- Permission enum is in Domain layer with no framework dependencies
- Evaluator handles edge case: user with no membership returns false for all permissions

## Observability
- Logs: None (domain layer)
- Metrics: None (domain layer)
- Traces: None (domain layer)

## Rollback Plan
Revert the commit. No database migrations or external dependencies involved.

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
