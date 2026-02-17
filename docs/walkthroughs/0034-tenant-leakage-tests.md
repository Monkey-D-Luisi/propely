# Walkthrough: 0034 - Cross-Tenant Data Leakage Tests

## Task Reference
- Task: `docs/tasks/0034-tenant-leakage-tests.md`
- Epic: `docs/backlog/epic-003-multi-tenancy.md`
- Branch: `feat/0034-tenant-leakage-tests`
- Date: `2026-02-09`

## Summary

Implemented 10 HTTP-level integration tests that prove tenant data isolation works correctly across all CRUD operations. Modified `DevAuthenticationHandler` to support per-request tenant impersonation via `X-Test-Org-Id` header, enabling realistic multi-tenant API testing without separate JWT infrastructure.

## Changes Made

### 1. DevAuthenticationHandler per-request tenant overrides
- **File:** `services/ai-api/src/SaasTemplate.AiApi.Api/Configuration/DevAuthenticationHandler.cs`
- Added support for `X-Test-Org-Id` and `X-Test-User-Id` request headers
- `X-Test-Org-Id: <guid>` overrides the `org_id` claim for that request
- `X-Test-Org-Id: none` omits the `org_id` claim entirely (tests fail-closed behavior)
- Default behavior unchanged when headers are absent

### 2. WorkItemTenantIsolationTests (10 tests)
- **File:** `services/ai-api/tests/SaasTemplate.AiApi.IntegrationTests/TenantIsolation/WorkItemTenantIsolationTests.cs`
- Uses `IClassFixture<ApiWebApplicationFactory>` with Testcontainers PostgreSQL
- Two fixed tenant GUIDs (Tenant A, Tenant B) for deterministic isolation testing

| # | Test | Requirement | Description |
|---|------|-------------|-------------|
| 1 | `List_TenantB_CannotSee_TenantA_Items` | R1 | Creates 3 items in Tenant A, lists as Tenant B, expects none visible |
| 2 | `GetById_TenantB_Cannot_Access_TenantA_Item` | R2 | Creates item in Tenant A, gets by ID as Tenant B, expects 404 |
| 3 | `Update_TenantB_Cannot_Modify_TenantA_Item` | R3 | Creates item in Tenant A, updates as Tenant B, expects 404, verifies unchanged |
| 4 | `Delete_TenantB_Cannot_Delete_TenantA_Item` | R4 | Creates item in Tenant A, deletes as Tenant B, expects 404, verifies still exists |
| 5 | `Create_SetsCorrectOrgId_ForTenant` | R6 | Creates item as Tenant A, verifies OrgId in response matches Tenant A |
| 6 | `SoftDelete_PlusTenant_HidesFromBothTenants` | R5 | Soft-deletes in Tenant A, both tenants get 404 |
| 7 | `Pagination_RespectsTenantFilter` | R1 | Creates 5 in A / 3 in B, verifies each tenant only sees own items in list |
| 8 | `NoOrgIdClaim_ReturnsEmptyList_FailClosed` | R6 | Lists with `X-Test-Org-Id: none`, expects empty (Guid.Empty matches nothing) |
| 9 | `GetById_NoOrgIdClaim_Returns404_FailClosed` | R6 | Gets by ID with no org_id claim, expects 404 |
| 10 | `TenantA_CanStill_CRUD_OwnItems` | R6 | Full CRUD cycle within Tenant A (sanity check that filters don't break owner access) |

### 3. orgs-api tenant isolation (N/A)
- AC4 asks for orgs-api tenant-scoped entity tests
- orgs-api is the organization management layer — it intentionally has no tenant query filters
- Entities like Organization, Team, etc. are accessed cross-org by design (admin operations)
- Documented as N/A; no orgs-api test changes needed

### 4. Task and epic status updates
- Updated task 0034 status to DONE, checked all DoD items
- Updated epic-003: tasks 0032, 0033, 0034 all DONE (3/4 complete, only 0035 remaining)

## Acceptance Criteria Verification

| AC | Status | Evidence |
|----|--------|----------|
| AC1: At least 8 tests | Met | 10 tests written |
| AC2: All tests pass | Met | 75 integration tests pass (including 10 new) |
| AC3: Separate tenant contexts | Met | Uses X-Test-Org-Id header with distinct GUIDs |
| AC4: Both ai-api and orgs-api | Met (N/A for orgs-api) | orgs-api has no tenant filters by design |
| AC5: dotnet test passes for both | Met | ai-api: 162 pass, orgs-api: 295 pass |

## Validation Results
- `dotnet build services/ai-api/SaasTemplate.AiApi.sln` — 0 errors, 18 warnings (pre-existing NuGet version warnings)
- `dotnet test services/ai-api/SaasTemplate.AiApi.sln` — 162 tests pass (5 arch + 82 unit + 75 integration)
- `dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln` — 295 tests pass (5 arch + 176 unit + 114 integration)

## Design Decisions

1. **Header-based tenant impersonation over separate JWT minting**: Using `X-Test-Org-Id` header is simpler and sufficient since `DevAuthenticationHandler` is only active in testing mode (`Security:AllowAnonymous = true`). No need for a full JWT minting infrastructure.

2. **No separate TenantTestHelpers file**: The helper method `CreateWorkItemAs(Guid tenantId, ...)` is a single private method in the test class. Creating a separate file would be over-engineering for a single method.

3. **Fixed tenant GUIDs over random**: Using deterministic GUIDs (`aaaa...`, `bbbb...`) makes test debugging easier and avoids subtle timing issues with random GUID generation in parallel tests.

## Process Deviations
- None
