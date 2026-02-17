# Walkthrough: cr-0036-tenant-leakage-tests-pr-review

## Task Reference
- Task: `docs/tasks/cr-0036-tenant-leakage-tests-pr-review.md`
- PR: #233 (`feat/0034-tenant-leakage-tests`)
- Branch: `feat/0034-tenant-leakage-tests`
- Date: `2026-02-09`

## Summary
Address PR #233 review feedback from GitHub Copilot. Primary improvements: replace flaky `Task.Delay` waits with deterministic polling, add missing status code assertions, and make sentinel value check case-insensitive.

## Changes Made

### 1. Replace Task.Delay with polling (SHOULD_FIX)
- Created `WaitForProjectionAsync(tenantId, titlePrefix, expectedCount)` helper method
- Polls list endpoint every 200ms with 10-second timeout until expected items appear
- Replaced all three `Task.Delay(1500)` calls in: `List_TenantB_CannotSee_TenantA_Items`, `Pagination_RespectsTenantFilter`, `NoOrgIdClaim_ReturnsEmptyList_FailClosed`
- Tests are now faster (return immediately once projection completes) and more robust (no fixed sleep)

### 2. Add status code assertions before deserialization (SHOULD_FIX)
- Added `response.StatusCode.Should().Be(HttpStatusCode.OK)` before `ReadFromJsonAsync` in:
  - `Update_TenantB_Cannot_Modify_TenantA_Item` (verify step)
  - `Create_SetsCorrectOrgId_ForTenant` (response check)
  - `Pagination_RespectsTenantFilter` (both Tenant A and Tenant B list responses)
  - `TenantA_CanStill_CRUD_OwnItems` (verify update step)
- If a request fails, test errors now show HTTP status code instead of misleading NullReference

### 3. Case-insensitive "none" sentinel check (SUGGESTION)
- Changed `orgIdHeader is not "none"` to `!string.Equals(orgIdHeader, "none", StringComparison.OrdinalIgnoreCase)`
- Handles `None`, `NONE`, etc. robustly

### 4. GUID validation in DevAuthenticationHandler (OUT_OF_SCOPE)
- Declined: downstream `HttpTenantAccessor` already validates via `Guid.TryParse` with fail-closed behavior
- Adding validation here would duplicate logic and complicate the `none` sentinel handling

## Validation Results
- `dotnet build services/ai-api/SaasTemplate.AiApi.sln` — 0 errors
- `dotnet test services/ai-api/SaasTemplate.AiApi.sln` — 162 tests pass (5 arch + 82 unit + 75 integration)

## Process Deviations
- None
