# Walkthrough: 0013-e2e-permission-tests

## Summary
This task adds comprehensive E2E tests for the permission system, covering the full lifecycle (grant override, verify, remove, verify revocation), owner immunity, cross-tenant isolation, and Playwright-based frontend testing.

## Architecture Decisions

### Extending Existing IntegrationTests vs New E2E Project
Decision: Add tests to the existing `Propely.OrgsApi.IntegrationTests` project rather than creating a separate E2E project. The existing project already has all required infrastructure (WebApplicationFactory, Testcontainers, JWT auth). A separate project would duplicate this setup with no added value.

### Multi-User Test Pattern
The existing integration tests use a single authenticated user. For permission lifecycle tests, we need multiple users with different roles. The approach:
1. Register owner user, create org
2. Invite a second user as agent/admin
3. Register second user, accept invite
4. Switch HTTP client between users to verify different access levels

### Playwright Test Scope
The Playwright spec verifies the permission management UI renders and is navigable. Full toggle interaction requires a running API which may not be available in all CI environments, so the spec focuses on navigation and rendering verification.

## Files Changed

### Backend Integration Tests
- `PermissionLifecycleTests.cs` — Full override lifecycle: set grant, verify, remove, verify revocation
- `OwnerImmunityTests.cs` — Owner permissions always granted, override attempts rejected
- `CrossTenantPermissionTests.cs` — Users from different orgs cannot access each other's permissions

### Frontend E2E Tests
- `permission-management.spec.ts` — Navigate to permission management, verify member list renders

## Test Scenarios

### PermissionLifecycleTests
1. Agent default permissions (only LeadsManage granted)
2. Grant override on agent → permission becomes granted with Override source
3. Remove override → permission returns to Role Default
4. Deny override on agent for LeadsManage → permission becomes denied with Override source
5. Remove deny override on agent for LeadsManage → permission returns to Role Default

### OwnerImmunityTests
1. Owner permissions all granted as Role Default
2. Setting override on owner returns 400

### CrossTenantPermissionTests
1. User from Org A cannot view Org B permissions (403/404)
2. User from Org A cannot set overrides on Org B members
