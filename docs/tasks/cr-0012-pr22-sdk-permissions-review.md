# Code Review: cr-0012-pr22-sdk-permissions-review

## Metadata
- PR: [#22](https://github.com/Monkey-D-Luisi/propely/pull/22)
- Branch: `feat/orgsapi-sdk-permissions` -> `main`
- CI: Orgs API Build & Test: SUCCESS; other services queued/in-progress
- Date: 2026-02-23

## Changed Files
- `docs/backlog/epic-P1-agency-permissions.md` (status updates)
- `docs/roadmap.md` (status updates)
- `docs/tasks/0010-orgsapi-sdk-permissions.md` (new task file)
- `docs/walkthroughs/0010-orgsapi-sdk-permissions.md` (new walkthrough)
- `services/orgs-api/src/Propely.OrgsApi.Client/Agencies/IAgenciesApi.cs` (new)
- `services/orgs-api/src/Propely.OrgsApi.Client/Dtos/AddBranchRequest.cs` (new)
- `services/orgs-api/src/Propely.OrgsApi.Client/Dtos/AgencyDetailResponse.cs` (new)
- `services/orgs-api/src/Propely.OrgsApi.Client/Dtos/AgencyResponse.cs` (new)
- `services/orgs-api/src/Propely.OrgsApi.Client/Dtos/BranchResponse.cs` (new)
- `services/orgs-api/src/Propely.OrgsApi.Client/Dtos/CreateAgencyRequest.cs` (new)
- `services/orgs-api/src/Propely.OrgsApi.Client/Dtos/CreateAgencyResponse.cs` (new)
- `services/orgs-api/src/Propely.OrgsApi.Client/Dtos/EffectivePermissionResponse.cs` (new)
- `services/orgs-api/src/Propely.OrgsApi.Client/Dtos/SetPermissionOverrideRequest.cs` (new)
- `services/orgs-api/src/Propely.OrgsApi.Client/Permissions/ForbiddenException.cs` (new)
- `services/orgs-api/src/Propely.OrgsApi.Client/Permissions/IPermissionGuard.cs` (new)
- `services/orgs-api/src/Propely.OrgsApi.Client/Permissions/IPermissionsApi.cs` (new)
- `services/orgs-api/src/Propely.OrgsApi.Client/Permissions/PermissionGuard.cs` (new)
- `services/orgs-api/src/Propely.OrgsApi.Client/ServiceCollectionExtensions.cs` (modified)
- `services/orgs-api/tests/Propely.OrgsApi.IntegrationTests/Client/OrgsApiClientIntegrationTests.cs` (modified)
- `services/orgs-api/tests/Propely.OrgsApi.UnitTests/Client/PermissionGuardTests.cs` (new)

---

## Section 1: Agent Review Findings

### F-1: Missing `JsonStringEnumConverter` for API enum serialization (MUST_FIX)
- **File**: `services/orgs-api/src/Propely.OrgsApi.Api/Program.cs` (API-wide config)
- **Severity**: MUST_FIX
- **Category**: API / Inter-Service Communication
- **Description**: The `PermissionsController` returns `EffectivePermissionDto` where `Permission` is the `Permission` enum. The API has no `JsonStringEnumConverter` configured globally. By default, `System.Text.Json` serializes enums as integers (0, 1, 2...). The client DTO `EffectivePermissionResponse.Permission` is `string`. This means the client will receive `0` instead of `"PropertiesViewAll"`, causing `PermissionGuard` string comparison to always fail, effectively denying all permissions.
- **Fix**: Add `JsonStringEnumConverter` to the API's JSON serializer options so enums serialize as strings consistently across all endpoints.

### F-2: `HasPermissionAsync` swallows `OperationCanceledException` (SHOULD_FIX)
- **File**: `PermissionGuard.cs:42`
- **Severity**: SHOULD_FIX
- **Category**: Code Quality
- **Description**: The catch-all `catch (Exception ex)` block in `HasPermissionAsync` also catches `OperationCanceledException` when the actual `CancellationToken` is cancelled (e.g., HTTP request aborted). Genuine cancellations should propagate, not be silently swallowed as permission denials.
- **Fix**: Check `ct.IsCancellationRequested` and rethrow `OperationCanceledException` in that case.

### F-3: Task doc references non-existent files and mismatches actual implementation (SHOULD_FIX)
- **File**: `docs/tasks/0010-orgsapi-sdk-permissions.md`
- **Severity**: SHOULD_FIX
- **Category**: Documentation
- **Description**: The task file references `PermissionCheckResult.cs` (never created), `PermissionsApiContractTests.cs`, and `AgenciesApiContractTests.cs` (not created). The "Files to Create" list also omits DTOs that were actually created (`CreateAgencyResponse.cs`, `SetPermissionOverrideRequest.cs`). AC9 requires contract tests that don't exist.
- **Fix**: Update the file list to match actual implementation. Update AC9 to reflect that contract tests were deferred (no `WebApplicationFactory` contract tests in this PR).

### F-4: "CRUD" description overstates agencies client capabilities (NIT)
- **File**: `docs/walkthroughs/0010-orgsapi-sdk-permissions.md`, PR description
- **Severity**: NIT
- **Category**: Documentation
- **Description**: The walkthrough and PR body describe `IAgenciesApi` as "CRUD + branch management", but there's no update or delete agency endpoint. It's create + list + get + branch management.
- **Fix**: Adjust wording to "create, list, get" instead of "CRUD".

### F-5: Roadmap/epic status numbering discrepancy (NIT)
- **File**: `docs/roadmap.md`, `docs/backlog/epic-P1-agency-permissions.md`
- **Severity**: NIT
- **Category**: Documentation
- **Description**: Roadmap task 1.5 is marked DONE but the corresponding epic task 1.4 is IN_PROGRESS. The different numbering between roadmap and epic creates confusion. The roadmap status should be IN_PROGRESS since the epic task isn't complete until merged.
- **Fix**: Set roadmap task 1.5 to IN_PROGRESS until this PR merges.

---

## Section 2: Review Comment Threads

### C-1 (Copilot #2839373836): EffectivePermissionResponse.Permission should match wire format
- **Mapped to**: F-1 (MUST_FIX)
- **Assessment**: Correct. The API has no `JsonStringEnumConverter`. Copilot's suggestion to add a client-side enum is one option; registering `JsonStringEnumConverter` globally in the API is the better fix (applies to all enum properties across all endpoints).

### C-2 (Copilot #2839373861): Rethrow OperationCanceledException on genuine cancellation
- **Mapped to**: F-2 (SHOULD_FIX)
- **Assessment**: Correct. Swallowing genuine cancellations is a subtle bug.

### C-3 (Copilot #2839373873): Task file references non-existent tests/DTO
- **Mapped to**: F-3 (SHOULD_FIX)
- **Assessment**: Correct. Documentation should match implementation.

### C-4 (Copilot #2839373885): IAgenciesApi doesn't fully support CRUD
- **Mapped to**: F-4 (NIT)
- **Assessment**: Correct. The API itself doesn't have update/delete endpoints yet.

### C-5 (Copilot #2839373890): Walkthrough "CRUD" overstates capabilities
- **Mapped to**: F-4 (NIT)
- **Assessment**: Same issue as C-4, different file.

### C-6 (Gemini #2839375150): Missing contract tests and inaccurate file list
- **Mapped to**: F-3 (SHOULD_FIX)
- **Assessment**: Correct. Same issue as C-3.

### C-7 (Copilot suppressed): Roadmap/epic status inconsistency
- **Mapped to**: F-5 (NIT)
- **Assessment**: Correct.

---

## Resolution Plan

### MUST_FIX
- [x] F-1: Add `JsonStringEnumConverter` to orgs-api JSON serializer options

### SHOULD_FIX
- [x] F-2: Rethrow `OperationCanceledException` when `ct.IsCancellationRequested`
- [x] F-3: Fix task file to match actual implementation, defer contract tests explicitly

### NIT
- [x] F-4: Fix "CRUD" wording in walkthrough
- [x] F-5: Fix roadmap task 1.5 status to IN_PROGRESS
