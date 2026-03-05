# Task: 0013-e2e-permission-tests

## Metadata
- ID: 0013
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-03-05
- Related docs:
  - Walkthrough: `docs/walkthroughs/0013-e2e-permission-tests.md`
  - Epic: `docs/backlog/epic-P1-agency-permissions.md` (Task 1.7)

## Goal
Create a comprehensive E2E test suite verifying the permission system works correctly from API enforcement through SDK client consumption, plus Playwright tests for the frontend permission management UI.

## Context
Tasks 1.1-1.6 built the permission domain model, authorization middleware, SDK client, and frontend UI. The existing `PermissionsEndpointTests.cs` covers basic endpoint reachability but lacks multi-user lifecycle tests (agent gets blocked, grant override, verify access). This task validates the full permission lifecycle end-to-end.

## Scope
### In scope
- Backend integration tests: permission lifecycle (set override, verify access, remove override, verify revocation)
- Backend integration tests: owner immunity (owner always has full access regardless of overrides)
- Backend integration tests: multi-user scenarios (agent with/without overrides)
- Backend integration tests: cross-tenant isolation
- Playwright E2E tests: permission management UI navigation and toggle interaction
- Test data factories for creating members with specific roles

### Out of scope
- Performance/load testing (separate task)
- Penetration testing
- Tests for services not yet built (properties, contacts, etc.)

## Requirements
- R1: All permission lifecycle scenarios covered by automated tests
- R2: Tests use Testcontainers PostgreSQL (no external dependencies)
- R3: Playwright tests use existing auth fixture pattern
- R4: All tests pass in CI consistently

## Acceptance Criteria
- AC1: E2E test: agent without override gets correct default permissions; after grant override, permission changes to granted
- AC2: E2E test: admin with deny override loses the permission; after override removal, permission returns to default
- AC3: E2E test: owner always has all permissions granted regardless of any override attempts
- AC4: E2E test: cross-tenant isolation validates user in Org A cannot access Org B permissions
- AC5: Playwright test: navigate to permission management page, verify member list renders
- AC6: All new tests pass alongside existing test suite

## Constraints (non-negotiable)
- Clean Architecture layers respected.
- English-only repo content.
- No secrets in repo.
- Update walkthrough.
- TDD: write failing tests first, then verify existing implementation makes them pass.

## Proposed Approach (high-level)
Extend the existing `PermissionsEndpointTests` in the IntegrationTests project with multi-user lifecycle tests using invite member flow. Add a Playwright spec for the permission management frontend flow.

## Implementation Steps
1. Add `PermissionLifecycleTests.cs` to existing IntegrationTests project with multi-user permission scenarios
2. Add `OwnerImmunityTests.cs` for owner-always-has-access scenarios
3. Add `CrossTenantPermissionTests.cs` for tenant isolation verification
4. Add `apps/web/e2e/permission-management.spec.ts` Playwright test
5. Run all tests & verify no regressions

## Files to Create / Modify
- `services/orgs-api/tests/Propely.OrgsApi.IntegrationTests/Api/Permissions/PermissionLifecycleTests.cs`
- `services/orgs-api/tests/Propely.OrgsApi.IntegrationTests/Api/Permissions/OwnerImmunityTests.cs`
- `services/orgs-api/tests/Propely.OrgsApi.IntegrationTests/Api/Permissions/CrossTenantPermissionTests.cs`
- `apps/web/e2e/permission-management.spec.ts`

## Testing Plan
- Integration tests: PermissionLifecycleTests (4+ scenarios), OwnerImmunityTests (2+ scenarios), CrossTenantPermissionTests (2+ scenarios)
- E2E frontend: Playwright permission management spec (1-2 scenarios)
- Manual verification: run full test suite, verify CI pass

## Security & Privacy
- Test data uses synthetic users and orgs only
- Tests run in isolated Testcontainers databases
- Cross-tenant tests are critical security verification

## Observability
- N/A for test code

## Rollback Plan
Remove test files — no production code changes.

## Definition of Done Checklist
- [ ] Acceptance criteria met
- [ ] Build passes
- [ ] Tests added/updated and pass
- [ ] No secrets committed
- [ ] Walkthrough updated
