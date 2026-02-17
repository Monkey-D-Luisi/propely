# Code Review Task: cr-0036-tenant-leakage-tests-pr-review

## Metadata
- PR: #233 (`feat/0034-tenant-leakage-tests`)
- Target branch: `main`
- CI status: All checks passed (AI API Build & Test SUCCESS, Orgs API SKIPPED, Web SKIPPED)

## Changed Files
- `docs/backlog/epic-003-multi-tenancy.md`
- `docs/tasks/0034-tenant-leakage-tests.md`
- `docs/walkthroughs/0034-tenant-leakage-tests.md`
- `services/ai-api/src/SaasTemplate.AiApi.Api/Configuration/DevAuthenticationHandler.cs`
- `services/ai-api/tests/SaasTemplate.AiApi.IntegrationTests/TenantIsolation/WorkItemTenantIsolationTests.cs`

## Review Threads

### Source 1: Inline review comments (8 total — 6 Copilot, 2 Gemini)
### Source 2: General reviews (2 total — 1 Copilot summary, 1 Gemini summary)
### Source 3: Issue comments (6 total — 5 Codex usage-limit notices, 1 Gemini error notice)

## Comment Resolution Plan

### SHOULD_FIX

- [x] **Replace Task.Delay with polling for eventual consistency** (copilot #2783522737, #2783522749, #2783522755; gemini #2783556687)
  - Three tests use `Task.Delay(1500)` to wait for outbox projection
  - Polling with a bounded timeout is more robust and faster when projection is quick
  - Note: existing `WorkItemsControllerTests` also uses `Task.Delay(1500)` — this fix improves on the established pattern
  - **Fix**: Create a `WaitForProjectionAsync` helper that polls until expected items appear, use in all three tests

- [x] **Add status code assertions before response deserialization** (copilot #2783522765)
  - Several `verifyResponse` calls deserialize without first checking status code
  - If a request fails, the test error will be misleading (NullReference vs. 500)
  - **Fix**: Add `StatusCode.Should().Be(HttpStatusCode.OK)` before all `ReadFromJsonAsync` calls on verification requests

### SUGGESTION

- [x] **Case-insensitive "none" sentinel check** (copilot #2783522785)
  - Current check `orgIdHeader is not "none"` is case-sensitive
  - Trivial fix for robustness
  - **Fix**: Use `StringComparison.OrdinalIgnoreCase`

### OUT_OF_SCOPE

- [ ] **Validate X-Test-User-Id / X-Test-Org-Id as GUIDs in DevAuthenticationHandler** (copilot #2783522794)
  - `DevAuthenticationHandler` is a dev/test-only handler, never runs in production
  - Downstream `HttpTenantAccessor.GetCurrentOrgId()` already uses `Guid.TryParse` and returns `Guid.Empty` for invalid values (fail-closed)
  - Adding GUID validation in the handler would duplicate existing downstream validation
  - The `none` sentinel value is intentionally non-GUID — adding GUID validation here would require extra carve-out logic
  - Invalid test header values will produce clear fail-closed behavior, not security issues

- [ ] **Add environment safeguards and header name constants to DevAuthenticationHandler** (gemini #2783556680, security-high)
  - Gemini claims the handler lacks environment safeguards and could allow impersonation in production
  - This is already mitigated by a three-layer defense in `DependencyInjection.cs`:
    1. `Security:AllowAnonymous` defaults to `false` in production (`appsettings.json`)
    2. Explicit `InvalidOperationException` if enabled outside Development/Testing (lines 71-74)
    3. Handler is only conditionally registered when `AllowAnonymous = true`
  - Header name constants: style preference — strings are used once each and are self-documenting

### QUESTION

- None

## Behavioral Parity Checks

- [x] Redirect parity: N/A — no auth entry points changed
- [x] Locale source correctness: N/A — no localized flows changed
- [x] API/UI contract parity: N/A — no DTO or frontend changes
- [x] Test parity: Tests are the primary deliverable of this PR; all CRUD paths covered
