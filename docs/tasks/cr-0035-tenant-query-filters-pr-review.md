# Code Review Task: cr-0035-tenant-query-filters-pr-review

## Metadata
- PR: #232 (`feat/0033-tenant-query-filters`)
- Target branch: `main`
- CI status: All checks passed (AI API Build & Test SUCCESS, Orgs API SKIPPED, Web SKIPPED)

## Changed Files
- `docs/tasks/0033-tenant-query-filters.md`
- `docs/walkthroughs/0033-tenant-query-filters.md`
- `services/ai-api/src/SaasTemplate.AiApi.Api/DependencyInjection.cs`
- `services/ai-api/src/SaasTemplate.AiApi.Api/Services/HttpTenantAccessor.cs`
- `services/ai-api/src/SaasTemplate.AiApi.Application/Common/Interfaces/ITenantAccessor.cs`
- `services/ai-api/src/SaasTemplate.AiApi.Infrastructure/Persistence/AppDbContext.cs`
- `services/ai-api/src/SaasTemplate.AiApi.Infrastructure/Persistence/Configurations/WorkItemConfiguration.cs`
- `services/ai-api/tests/SaasTemplate.AiApi.IntegrationTests/Fixtures/PostgresFixture.cs`
- `services/ai-api/tests/SaasTemplate.AiApi.IntegrationTests/Persistence/TenantQueryFilterTests.cs`
- `services/ai-api/tests/SaasTemplate.AiApi.UnitTests/Api/Services/HttpTenantAccessorTests.cs`

## Review Threads

### Source 1: Inline review comments (10 total)
### Source 2: General reviews (2 total)
### Source 3: Issue comments (2 total — 1 informational summary, 1 unrelated usage-limit notice)

## Comment Resolution Plan

### MUST_FIX

- [x] **Fail-closed tenant filter** (gemini #2783320969, #2783320973; copilot #2783351923)
  - `HttpTenantAccessor` returns `null` when HttpContext exists but `org_id` claim is missing/invalid
  - Combined with `_currentOrgId == null` bypass in query filter, this allows authenticated users without org_id to see all tenants' data
  - **Fix**: Distinguish between "no HttpContext" (system mode → null) and "HttpContext exists but claim missing" (fail-closed → `Guid.Empty`)
  - `Guid.Empty` will never match any real OrgId, effectively returning zero rows

- [x] **Test cleanup bug: DisposeAsync missing IgnoreQueryFilters()** (copilot #2783352012)
  - `DisposeAsync` queries `context.WorkItems.ToListAsync()` which applies soft-delete filter
  - Soft-deleted items from tests won't be cleaned up, leaking between tests
  - **Fix**: Use `context.WorkItems.IgnoreQueryFilters().ToListAsync()`

### SHOULD_FIX

- [x] **Enhance SetTenantIdOnNewEntities to validate OrgId** (gemini #2783320978)
  - Current logic only sets OrgId when `Guid.Empty`; doesn't prevent cross-tenant injection
  - **Fix**: When `_currentOrgId` is set, validate that new WorkItem OrgId matches (or throw if mismatched)
  - ITenantScoped interface suggestion deferred (OUT_OF_SCOPE — only one tenant entity exists)

- [x] **DoD/walkthrough accuracy** (copilot #2783352046, #2783352077)
  - Claimed "both services" tested but orgs-api tests weren't run
  - **Fix**: Run orgs-api tests, update DoD and walkthrough to reflect actual results

### SUGGESTION

- [x] **Explicit null initialization in parameterless ctor** (copilot #2783351888)
  - Copilot claims readonly field will fail compilation — this is incorrect (Guid? defaults to null)
  - **Fix**: Add explicit `_currentOrgId = null` for clarity despite being unnecessary

### OUT_OF_SCOPE

- [ ] **ITenantScoped interface** (gemini #2783320978)
  - Good future improvement but premature — only WorkItem is tenant-scoped in ai-api currently
  - Defer to when more tenant-scoped entities are added

### QUESTION

- None

### Tests to Update

- [x] Update `HttpTenantAccessorTests` — missing/invalid/empty claim tests should now assert `Guid.Empty` instead of `null`
- [x] Update integration test `Query_WithNoTenant_ShouldReturnAllItems` — system mode (null accessor) still works
- [x] Add new test: authenticated request with no org_id returns empty result set

## Behavioral Parity Checks

- [x] Redirect parity: N/A — no auth entry points changed
- [x] Locale source correctness: N/A — no localized flows changed
- [x] API/UI contract parity: N/A — no DTO or frontend changes
- [x] Test parity: Unit + integration tests updated for new fail-closed behavior
