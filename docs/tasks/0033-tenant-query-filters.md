# Task: 0033 - EF Core Global Query Filters for Tenant Isolation

## Metadata
- ID: 0033
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-07
- GitHub Issue: #185
- Epic: `docs/backlog/epic-003-multi-tenancy.md`
- Old Issue: #17
- Dependencies: 0032 (TenantId on entities)

## Goal
Configure EF Core global query filters to automatically scope all database queries to the current tenant (organization), making cross-tenant data access impossible at the data layer.

## Context
After task 0032 adds OrgId to entities, we need automatic filtering. The codebase already uses global query filters for soft delete (`HasQueryFilter(e => !e.IsDeleted)`). We need to combine soft delete filters with tenant filters. EF Core supports combining multiple filters using `&&` in a single `HasQueryFilter` call.

### Current Filter Pattern
```csharp
// In entity configuration:
builder.HasQueryFilter(o => !o.IsDeleted);
```

### Target Filter Pattern
```csharp
builder.HasQueryFilter(w => !w.IsDeleted && w.OrgId == _tenantAccessor.GetCurrentOrgId());
```

## Scope
### In scope
- Create `ITenantAccessor` interface (if not already from 0032) in Application layer
- Implement `HttpContextTenantAccessor` in Api/Infrastructure layer
- Add tenant filter to `WorkItem` and `WorkItemRead` in ai-api AppDbContext
- Combine with existing soft delete filter (single `HasQueryFilter` expression)
- Ensure tenant context is available during SaveChanges (auto-set OrgId)
- Handle admin/system scenarios where tenant filter should be bypassed

### Out of scope
- Cross-tenant leakage tests (task 0034)
- Multi-database tenant isolation (we use shared DB with row-level filters)

## Requirements
- R1: All tenant-scoped queries are automatically filtered by OrgId
- R2: Tenant filter works alongside existing soft delete filter
- R3: OrgId is resolved from the current HTTP context (JWT claims)
- R4: Background jobs/system operations can bypass tenant filter when needed
- R5: No manual `.Where(x => x.OrgId == ...)` needed in application code

## Acceptance Criteria
- AC1: Query for WorkItems only returns items belonging to current tenant
- AC2: Creating a WorkItem auto-sets OrgId from tenant context
- AC3: Soft delete filter still works (deleted items not returned)
- AC4: Combined filter: only returns non-deleted items for current tenant
- AC5: Admin endpoint can bypass filter using `IgnoreQueryFilters()` if needed
- AC6: `dotnet build` and `dotnet test` pass for both services

## Constraints (non-negotiable)
- Clean Architecture: ITenantAccessor in Application, implementation in Infrastructure/Api
- Must compose with existing soft delete filter (not replace it)
- English-only repo content
- Update walkthrough

## Implementation Steps

### ai-api Service

1. **Create ITenantAccessor** (if not done in 0032) (`services/ai-api/src/SaasTemplate.AiApi.Application/Common/Interfaces/ITenantAccessor.cs`)
   ```csharp
   public interface ITenantAccessor
   {
       Guid? GetCurrentOrgId();
   }
   ```

2. **Implement TenantAccessor** (`services/ai-api/src/SaasTemplate.AiApi.Api/Services/TenantAccessor.cs`)
   - Inject `IHttpContextAccessor`
   - Read OrgId from JWT claim `org_id` or from `X-Org-Id` header
   - Return null if no tenant context (system/background operations)

3. **Update AppDbContext** (`services/ai-api/src/SaasTemplate.AiApi.Infrastructure/Persistence/AppDbContext.cs`)
   - Inject `ITenantAccessor` via constructor
   - Store `private Guid? _currentOrgId`
   - In `OnModelCreating`, configure combined filter:
     ```csharp
     builder.Entity<WorkItem>()
         .HasQueryFilter(w => !w.IsDeleted && (_currentOrgId == null || w.OrgId == _currentOrgId));
     ```
   - The `_currentOrgId == null` check allows system operations to see all data

4. **Override SaveChangesAsync** (`services/ai-api/src/SaasTemplate.AiApi.Infrastructure/Persistence/AppDbContext.cs`)
   - Before saving, auto-set OrgId on new entities that implement a tenant interface
   - This ensures OrgId is always set, even if the command handler forgets

5. **Move entity-level filter from Configuration to DbContext** if needed
   - EF Core allows query filters in both `OnModelCreating` and configuration classes
   - For tenant filter with injected accessor, it must be in `OnModelCreating` (has access to `_currentOrgId` field)
   - May need to move soft delete filter too, or keep them separate

6. **Register TenantAccessor** in DI (`services/ai-api/src/SaasTemplate.AiApi.Infrastructure/DependencyInjection.cs`)
   - `services.AddScoped<ITenantAccessor, TenantAccessor>()`

### orgs-api Service

7. **Apply same pattern** for any tenant-scoped entities in orgs-api
   - AuditLog with OrgId (if added in 0032)
   - Other entities as needed

### Testing

8. **Unit tests for TenantAccessor**
   - Returns OrgId from JWT claims
   - Returns null when no HTTP context

9. **Integration tests for query filter**
   - Create work items under org A and org B
   - Set tenant context to org A -> query returns only org A items
   - Set tenant context to org B -> query returns only org B items
   - No tenant context (null) -> returns all items (system mode)

## Files to Create / Modify

### Create
- `services/ai-api/src/SaasTemplate.AiApi.Application/Common/Interfaces/ITenantAccessor.cs` (if not from 0032)
- `services/ai-api/src/SaasTemplate.AiApi.Api/Services/TenantAccessor.cs` (if not from 0032)
- `docs/walkthroughs/0033-tenant-query-filters.md`

### Modify
- `services/ai-api/src/SaasTemplate.AiApi.Infrastructure/Persistence/AppDbContext.cs` (inject ITenantAccessor, add filter)
- `services/ai-api/src/SaasTemplate.AiApi.Infrastructure/Persistence/Configurations/WorkItemConfiguration.cs` (may move filter to DbContext)
- `services/ai-api/src/SaasTemplate.AiApi.Infrastructure/DependencyInjection.cs` (register ITenantAccessor)
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/AppDbContext.cs` (if applicable)

## Testing Plan
- Unit tests: TenantAccessor
- Integration tests: Multi-tenant query isolation, combined with soft delete
- Manual test: Create data under different orgs, verify isolation via API

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes (both services)
- [x] Tests added/updated and pass
- [x] Tenant filter composes with soft delete filter
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
