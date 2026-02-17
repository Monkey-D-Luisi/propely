# Task: 0034 - Cross-Tenant Data Leakage Tests

## Metadata
- ID: 0034
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-07
- GitHub Issue: #186
- Epic: `docs/backlog/epic-003-multi-tenancy.md`
- Old Issue: #18
- Dependencies: 0033 (EF Core tenant query filters)

## Goal
Write comprehensive integration tests that prove tenant data isolation works correctly and no cross-tenant data leakage is possible.

## Context
After tasks 0032 (TenantId) and 0033 (query filters), we need confidence that the isolation is complete. These tests create data under Tenant A and verify Tenant B cannot see, modify, or delete it. The project uses Testcontainers with PostgreSQL for integration tests.

### Current Test Infrastructure
- Test project: `services/ai-api/tests/SaasTemplate.AiApi.IntegrationTests/` (or similar)
- Uses Testcontainers for PostgreSQL
- Has `PostgresFixture` for test database management
- Tests run via `dotnet test`

## Scope
### In scope
- Integration tests for all CRUD operations proving tenant isolation
- Test that query filters cannot be bypassed via direct SQL/raw queries from application layer
- Test all tenant-scoped entities (WorkItem in ai-api, relevant entities in orgs-api)
- Test that listing, getting by ID, updating, and deleting across tenants fails
- Edge case: test that tenant filter + soft delete filter work together

### Out of scope
- Performance/load testing of tenant filters
- DB-level row security policies (we use application-level EF filters)

## Requirements
- R1: Create data in Tenant A and prove Tenant B cannot list it
- R2: Create data in Tenant A and prove Tenant B cannot get it by ID
- R3: Create data in Tenant A and prove Tenant B cannot update it
- R4: Create data in Tenant A and prove Tenant B cannot delete it
- R5: Test combined soft-delete + tenant filter (soft-deleted items in Tenant A not visible to Tenant A)
- R6: All tests use realistic HTTP client calls (not direct DbContext queries)

## Acceptance Criteria
- AC1: Test suite has at least 8 tests covering all CRUD + edge cases
- AC2: All tests pass with green status
- AC3: Tests use separate tenant contexts (different JWTs with different OrgIds)
- AC4: Tests cover both ai-api and orgs-api tenant-scoped entities
- AC5: `dotnet test` passes for both services

## Constraints (non-negotiable)
- Tests must use integration test infrastructure (Testcontainers, real DB)
- Tests must authenticate as different users in different orgs
- English-only repo content
- Update walkthrough

## Implementation Steps

1. **Create test base/fixture** for multi-tenant tests
   - Helper to create a user in a specific organization and get a JWT for that user
   - Helper to set up two isolated tenants (Org A, Org B) with users in each

2. **ai-api: WorkItem isolation tests** (`services/ai-api/tests/.../TenantIsolation/WorkItemTenantIsolationTests.cs`)
   - **Test: List isolation** - Create work items in Org A -> List as Org B user -> expect empty list
   - **Test: Get by ID isolation** - Create work item in Org A -> Get by ID as Org B user -> expect 404
   - **Test: Update isolation** - Create work item in Org A -> Update as Org B user -> expect 404 or 403
   - **Test: Delete isolation** - Create work item in Org A -> Delete as Org B user -> expect 404 or 403
   - **Test: Create sets correct OrgId** - Create as Org A user -> verify OrgId matches Org A
   - **Test: Soft delete + tenant combo** - Soft-delete in Org A -> list as Org A -> not visible -> list as Org B -> not visible

3. **orgs-api: Notification/AuditLog isolation tests** (if these entities have OrgId)
   - Similar pattern as above

4. **Edge case tests**
   - **Test: Empty OrgId rejected** - Attempt to create without tenant context -> expect error
   - **Test: Pagination respects tenant** - Create 10 items in Org A, 5 in Org B -> paginate as Org A -> total count = 10

5. **Test helper utilities**
   - `CreateTestOrganization()` -> returns orgId
   - `CreateTestUser(orgId)` -> returns userId + JWT
   - `AuthenticateAs(jwt)` -> configure HttpClient with JWT

## Files to Create / Modify

### Create
- `services/ai-api/tests/SaasTemplate.AiApi.IntegrationTests/TenantIsolation/WorkItemTenantIsolationTests.cs`
- `services/ai-api/tests/SaasTemplate.AiApi.IntegrationTests/TenantIsolation/TenantTestHelpers.cs`
- `services/orgs-api/tests/SaasTemplate.OrgsApi.IntegrationTests/TenantIsolation/` (if applicable)
- `docs/walkthroughs/0034-tenant-leakage-tests.md`

### Modify
- Test project files if needed for new test infrastructure

## Testing Plan
- This task IS the testing plan
- All tests must pass: `dotnet test`
- Minimum 8 distinct test cases

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] All tenant isolation tests pass
- [x] Tests cover list, get, update, delete operations
- [x] Tests cover combined soft-delete + tenant filter
- [x] Build passes
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
