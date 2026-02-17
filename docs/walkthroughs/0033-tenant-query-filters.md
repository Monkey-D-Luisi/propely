# Walkthrough: 0033-tenant-query-filters

## Task Reference
- Task: `docs/tasks/0033-tenant-query-filters.md`
- Walkthrough: `docs/walkthroughs/0033-tenant-query-filters.md`
- Branch/PR: `feat/0033-tenant-query-filters` / TBD
- Date: `2026-02-09`

## Summary
Configured EF Core global query filters to automatically scope all database queries to the current tenant (organization) in the ai-api service. The tenant is resolved from JWT `org_id` claims via `ITenantAccessor`. Query filters combine soft-delete and tenant isolation in a single expression, making cross-tenant data access impossible at the data layer without explicit opt-out via `IgnoreQueryFilters()`.

## Context
- Background: Task 0032 added OrgId columns to WorkItem and WorkItemRead entities but did not add automatic query filtering. Application code was manually extracting OrgId from JWT claims in controllers.
- Problem statement: Without global query filters, every query in the application must manually include `.Where(w => w.OrgId == orgId)`, which is error-prone and could lead to cross-tenant data leakage.
- Constraints: Must compose with existing soft-delete filter. Clean Architecture must be maintained (interface in Application, implementation in Api).

## Decisions & Trade-offs
- **Decision:** Combine soft-delete and tenant filters in a single `HasQueryFilter` expression in `AppDbContext.OnModelCreating`
  - Options considered: (a) Separate filters in configuration classes, (b) Combined filter in AppDbContext
  - Why this choice: EF Core supports only one `HasQueryFilter` per entity; the last call wins. Placing the combined filter in AppDbContext gives access to the `_currentOrgId` field from the injected `ITenantAccessor`.
  - Consequences: Removed the old soft-delete-only filter from `WorkItemConfiguration` and replaced with a comment pointing to AppDbContext.

- **Decision:** Use `_currentOrgId == null` check to allow system/background operations to bypass tenant filter
  - Options considered: (a) Separate admin context, (b) Null-check bypass
  - Why this choice: When no tenant context is available (background jobs, system operations), `ITenantAccessor.GetCurrentOrgId()` returns null. The expression `_currentOrgId == null || w.OrgId == _currentOrgId` naturally passes all rows when null.
  - Consequences: Code creating `AppDbContext` without `ITenantAccessor` (parameterless constructor) also sees all data. This is safe because the DI container always injects `ITenantAccessor` for HTTP-scoped contexts.

- **Decision:** Skip orgs-api tenant filters for now
  - Options considered: (a) Add tenant filters to Membership/Invitation/AuditLog, (b) Defer to future task
  - Why this choice: The orgs-api is the management layer for multi-tenancy. Its entities (Membership, Invitation) are inherently queried across organizations (e.g., "list my organizations"). Adding global tenant filters would break these cross-org queries.
  - Consequences: Tenant query filtering is applied only in ai-api where entities are strictly org-scoped.

- **Decision:** Auto-set OrgId on new WorkItem entities in `SaveChangesAsync` as a safety net
  - Options considered: (a) Rely entirely on command handlers, (b) Auto-set in SaveChangesAsync
  - Why this choice: Defense-in-depth. If a command handler forgets to set OrgId, the DbContext catches it before persistence. Only applies to entities with `OrgId == Guid.Empty` (unset).

## Implementation Notes
- Key changes:
  - `ITenantAccessor` interface created in Application layer with single `GetCurrentOrgId()` method
  - `HttpTenantAccessor` created in Api layer, reads `org_id` from JWT claims via `IHttpContextAccessor`
  - `AppDbContext` now accepts optional `ITenantAccessor` via second constructor; captures `_currentOrgId` at construction time
  - Combined query filter: `w => !w.IsDeleted && (_currentOrgId == null || w.OrgId == _currentOrgId)` for WorkItem
  - Tenant-only filter: `w => _currentOrgId == null || w.OrgId == _currentOrgId` for WorkItemRead (no soft-delete on read model)
  - `SaveChangesAsync` auto-sets OrgId on new WorkItem entities where OrgId is `Guid.Empty`
  - `AddHttpContextAccessor()` added to Api DI (required by `HttpTenantAccessor`)
  - `PostgresFixture` updated with `CreateContext(Guid orgId)` overload for tenant-scoped test contexts

- Edge cases handled:
  - No HTTP context (background jobs): `HttpTenantAccessor.GetCurrentOrgId()` returns null, bypassing tenant filter
  - No `ITenantAccessor` injected (parameterless constructor): `_currentOrgId` stays null, bypassing tenant filter
  - `IgnoreQueryFilters()` available for admin scenarios that need cross-tenant access

## Files Changed

### Created
| File | Purpose |
|------|---------|
| `services/ai-api/src/SaasTemplate.AiApi.Application/Common/Interfaces/ITenantAccessor.cs` | Tenant accessor interface |
| `services/ai-api/src/SaasTemplate.AiApi.Api/Services/HttpTenantAccessor.cs` | JWT-based implementation |
| `services/ai-api/tests/SaasTemplate.AiApi.UnitTests/Api/Services/HttpTenantAccessorTests.cs` | Unit tests (5 tests) |
| `services/ai-api/tests/SaasTemplate.AiApi.IntegrationTests/Persistence/TenantQueryFilterTests.cs` | Integration tests (6 tests) |
| `docs/walkthroughs/0033-tenant-query-filters.md` | This walkthrough |

### Modified
| File | Change |
|------|--------|
| `services/ai-api/src/SaasTemplate.AiApi.Infrastructure/Persistence/AppDbContext.cs` | Added ITenantAccessor injection, combined query filters, OrgId auto-set in SaveChangesAsync |
| `services/ai-api/src/SaasTemplate.AiApi.Infrastructure/Persistence/Configurations/WorkItemConfiguration.cs` | Removed soft-delete-only HasQueryFilter (now in AppDbContext) |
| `services/ai-api/src/SaasTemplate.AiApi.Api/DependencyInjection.cs` | Added AddHttpContextAccessor() and ITenantAccessor registration |
| `services/ai-api/tests/SaasTemplate.AiApi.IntegrationTests/Fixtures/PostgresFixture.cs` | Added tenant-scoped CreateContext(Guid orgId) overload and TestTenantAccessor |

## Testing
- **Unit tests (6):** HttpTenantAccessor with valid claim, no context, no claim, invalid claim, empty claim, Guid.Empty claim
- **Integration tests (6):** Tenant A isolation, tenant B isolation, no-tenant sees all, combined with soft-delete, IgnoreQueryFilters bypass, read model tenant isolation
- **ai-api tests pass:** 82 unit + 65 integration + architecture tests
- **orgs-api tests pass:** 176 unit + 114 integration + 5 architecture tests (no regressions)

## Acceptance Criteria Verification
- [x] AC1: Query for WorkItems only returns items belonging to current tenant
- [x] AC2: Creating a WorkItem auto-sets OrgId from tenant context (SaveChangesAsync safety net)
- [x] AC3: Soft delete filter still works (deleted items not returned)
- [x] AC4: Combined filter: only returns non-deleted items for current tenant
- [x] AC5: Admin endpoint can bypass filter using `IgnoreQueryFilters()` if needed
- [x] AC6: `dotnet build` and `dotnet test` pass for both services
