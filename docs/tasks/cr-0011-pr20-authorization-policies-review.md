# Code Review: cr-0011-pr20-authorization-policies-review

## PR Metadata
- PR: #20 - feat(orgs): add authorization policies and permission management API (#0009)
- Branch: `feat/authorization-policies-middleware` -> `main`
- CI Status: QUEUED (build/test checks pending)
- Link: https://github.com/Monkey-D-Luisi/propely/pull/20

## Changed Files
- `docs/backlog/epic-P1-agency-permissions.md` (status update)
- `docs/tasks/0009-authorization-policies-middleware.md` (new)
- `docs/walkthroughs/0009-authorization-policies-middleware.md` (new)
- `services/orgs-api/src/Propely.OrgsApi.Api/Authorization/PermissionAuthorizationHandler.cs` (new)
- `services/orgs-api/src/Propely.OrgsApi.Api/Authorization/PermissionPolicyProvider.cs` (new)
- `services/orgs-api/src/Propely.OrgsApi.Api/Authorization/PermissionRequirement.cs` (new)
- `services/orgs-api/src/Propely.OrgsApi.Api/Authorization/RequirePermissionAttribute.cs` (new)
- `services/orgs-api/src/Propely.OrgsApi.Api/Controllers/PermissionsController.cs` (new)
- `services/orgs-api/src/Propely.OrgsApi.Api/DependencyInjection.cs` (modified)
- `services/orgs-api/src/Propely.OrgsApi.Api/Dtos/SetPermissionOverrideRequest.cs` (new)
- `services/orgs-api/src/Propely.OrgsApi.Application/DependencyInjection.cs` (modified)
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/Commands/RemovePermissionOverride/RemovePermissionOverrideCommand.cs` (new)
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/Commands/RemovePermissionOverride/RemovePermissionOverrideCommandHandler.cs` (new)
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/Commands/SetPermissionOverride/SetPermissionOverrideCommand.cs` (new)
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/Commands/SetPermissionOverride/SetPermissionOverrideCommandHandler.cs` (new)
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/Commands/SetPermissionOverride/SetPermissionOverrideCommandValidator.cs` (new)
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/Queries/GetUserPermissions/GetUserPermissionsQuery.cs` (new)
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/Queries/GetUserPermissions/GetUserPermissionsQueryHandler.cs` (new)
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/Services/CachedPermissionEvaluator.cs` (new)
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/DependencyInjection.cs` (modified)
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/Migrations/` (new migration)
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/Persistence/AppDbContext.cs` (modified)
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/Persistence/Configurations/PermissionOverrideConfiguration.cs` (new)
- `services/orgs-api/src/Propely.OrgsApi.Infrastructure/Persistence/Repositories/PermissionOverrideRepository.cs` (new)
- `services/orgs-api/tests/Propely.OrgsApi.IntegrationTests/Api/Permissions/PermissionsEndpointTests.cs` (new)
- `services/orgs-api/tests/Propely.OrgsApi.UnitTests/Application/Permissions/CachedPermissionEvaluatorTests.cs` (new)
- `services/orgs-api/tests/Propely.OrgsApi.UnitTests/Application/Permissions/RemovePermissionOverrideCommandHandlerTests.cs` (new)
- `services/orgs-api/tests/Propely.OrgsApi.UnitTests/Application/Permissions/SetPermissionOverrideCommandHandlerTests.cs` (new)

---

## Section 1: Agent Review Findings

### A.2.1: Architecture & Clean Architecture -- PASS
- Domain layer has zero framework dependencies.
- Dependencies flow inward correctly.
- Thin controllers delegating to MediatR.
- CQRS separation correct (commands mutate, queries read-only).
- One handler per command/query.
- Repository interfaces in Application, implementations in Infrastructure.
- Correct namespace convention: `Propely.OrgsApi.*`

### A.2.2: Domain Design (DDD) -- PASS
- `PermissionOverride` uses factory method with domain events.
- Private setters, private constructor.
- Domain events named with V1 suffix.
- GUID-based identity.

### A.2.3: API & Security -- ISSUES FOUND
- FluentValidation on `SetPermissionOverrideCommand`. OK.
- Authorization checks (admin/owner) in command handlers. OK.
- No secrets. OK.
- EF Core parameterized queries. OK.
- No PII in logs (only IDs). OK.
- **FINDING S1**: `Enum.TryParse` accepts numeric strings (e.g. "0") in controller and policy provider.
- **FINDING S2**: `CancellationToken.None` used in authorization handler instead of `HttpContext.RequestAborted`.

### A.2.4: Data & Persistence -- PASS
- Composite unique index on `(UserId, OrganizationId, Permission)`.
- EF Core migration present.
- Redis cache with 30s TTL.
- Cache used as optimization, not source of truth.

### A.2.5: Testing -- PASS
- 24 unit tests + 9 integration tests.
- Test naming follows `MethodName_Scenario_ExpectedResult`.
- AAA pattern used consistently.
- Happy + error paths covered.

### A.2.6: Frontend -- N/A

### A.2.7: Inter-Service Communication -- N/A

### A.2.8: Code Quality -- ISSUES FOUND
- **FINDING S3**: Self-contradicting comment in `RemoveOverride_NoExisting_ShouldReturn200` test.

### A.3: Behavioral Parity Checks
- Redirect parity: N/A
- Locale correctness: N/A
- API-to-UI contract: N/A (no frontend changes)
- Test parity: Tests exist for all new behavior. PASS

### Additional Agent Findings
- **FINDING S4**: `HasPermissionAsync` in `CachedPermissionEvaluator` doesn't populate cache on miss. Authorization handler uses `HasPermissionAsync`, so caching is mostly ineffective.
- **FINDING S5**: `GetUserPermissionsQueryHandler` doesn't verify target user is an active org member, returning 200 with empty list for non-members.
- **FINDING S6**: `RemovePermissionOverrideCommandHandler` doesn't validate target membership or check for owner targets, inconsistent with `SetPermissionOverrideCommandHandler`.

---

## Section 2: Review Comment Threads

### Source 1: Inline Review Comments (10 total)

| # | Reviewer | File | Issue | Classification |
|---|----------|------|-------|---------------|
| 1 | gemini-code-assist | PermissionAuthorizationHandler.cs:50 | CancellationToken.None -> HttpContext.RequestAborted | SHOULD_FIX |
| 2 | Copilot | CachedPermissionEvaluator.cs:48 | HasPermissionAsync doesn't populate cache on miss | SHOULD_FIX |
| 3 | Copilot | RequirePermissionAttribute.cs:16 | No usages of [RequirePermission] on any controller | OUT_OF_SCOPE |
| 4 | Copilot | PermissionPolicyProvider.cs:33 | Enum.TryParse accepts numeric values | SHOULD_FIX |
| 5 | Copilot | PermissionAuthorizationHandler.cs:51 | CancellationToken.None (duplicate of #1) | SHOULD_FIX |
| 6 | Copilot | PermissionsController.cs:63 | Enum.TryParse accepts numeric strings (PUT) | SHOULD_FIX |
| 7 | Copilot | PermissionsController.cs:95 | Enum.TryParse accepts numeric strings (DELETE) | SHOULD_FIX |
| 8 | Copilot | GetUserPermissionsQueryHandler.cs:42 | Missing target membership validation | SHOULD_FIX |
| 9 | Copilot | RemovePermissionOverrideCommandHandler.cs:52 | Missing target validation, inconsistent with Set | SHOULD_FIX |
| 10 | Copilot | PermissionsEndpointTests.cs:216 | Self-contradicting test comment | SUGGESTION |

### Source 2: General Reviews (2 total)
- gemini-code-assist: Summary only (feedback in inline comments). No additional action items.
- Copilot: Summary only (feedback in inline comments). No additional action items.

### Source 3: Issue Comments (2 total)
- chatgpt-codex-connector: Usage limit notice. No action needed.
- gemini-code-assist: Summary/changelog. No action needed.

---

## Resolution Plan

### SHOULD_FIX (implementing)

- [x] **Fix 1**: Use `HttpContext.RequestAborted` instead of `CancellationToken.None` in `PermissionAuthorizationHandler` (Agent S2 + Gemini #1 + Copilot #5)
- [x] **Fix 2**: Add `Enum.IsDefined` validation after `Enum.TryParse` in `PermissionsController` PUT and DELETE, and `PermissionPolicyProvider` (Agent S1 + Copilot #4, #6, #7)
- [x] **Fix 3**: Populate cache on miss in `HasPermissionAsync` in `CachedPermissionEvaluator` (Agent S4 + Copilot #2)
- [x] **Fix 4**: Add target membership validation in `GetUserPermissionsQueryHandler` (Agent S5 + Copilot #8)
- [x] **Fix 5**: Add target membership and owner validation in `RemovePermissionOverrideCommandHandler` for parity with Set handler (Agent S6 + Copilot #9)

### SUGGESTION (implementing)

- [x] **Fix 6**: Clean up self-contradicting comment and rename test in `PermissionsEndpointTests` (Agent S3 + Copilot #10)

### OUT_OF_SCOPE (no action)

- **No usages of [RequirePermission]** (Copilot #3): This is by design. The attribute is infrastructure for future tasks (1.4+). Other services will use it once the SDK client is built. No action needed now.

### FALSE_POSITIVE (none)

None identified.
