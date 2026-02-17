# Task: 0032 - Add TenantId (OrgId) to Core Entities

## Metadata
- ID: 0032
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-07
- GitHub Issue: #184
- Epic: `docs/backlog/epic-003-multi-tenancy.md`
- Old Issue: #16

## Goal
Add TenantId (OrgId) column to all tenant-scoped entities so data can be isolated per organization.

## Context
Currently, entities like WorkItem in the ai-api have no organization context. Any authenticated user can potentially see all work items. For proper multi-tenancy, every tenant-scoped entity needs an OrgId (TenantId) to enable automatic data isolation via EF Core query filters (task 0033). This task adds the column and backfills existing data.

### Tenant-Scoped Entities
- **ai-api**: `WorkItem` (primary entity that needs tenant isolation)
- **orgs-api**: Most entities already scoped by org (Membership, Invitation have OrgId). Notification may need OrgId. AuditLog should get OrgId for org-level audit trails.

## Scope
### In scope
- Add `OrgId` (Guid) to `WorkItem` entity in ai-api Domain
- Add `OrgId` to `WorkItemRead` read model in ai-api Infrastructure
- Add `OrgId` to `AuditLog` entity in orgs-api Domain (optional, for org-scoped audit)
- Update all constructors and factory methods to require `OrgId`
- Create EF Core migrations for both services
- Backfill existing data (set OrgId to a default or require manual assignment)
- Update DTOs and API contracts to include OrgId
- Update command handlers to set OrgId from authenticated user's context

### Out of scope
- Query filter implementation (task 0033)
- Cross-tenant leakage tests (task 0034)
- Tenant resolution middleware

## Requirements
- R1: `WorkItem` has a non-nullable `OrgId` column
- R2: All new WorkItem creation requires an OrgId from the authenticated user's context
- R3: Migration is backward-compatible (additive: new nullable column, then backfill, then make non-nullable)
- R4: API responses include OrgId where relevant
- R5: Existing data is handled gracefully during migration

## Acceptance Criteria
- AC1: `WorkItem` table has `org_id` column (non-nullable after backfill)
- AC2: `POST /work-items` requires and stores OrgId from user context
- AC3: `GET /work-items` responses include OrgId
- AC4: Migration applies cleanly on fresh and existing databases
- AC5: `dotnet build` passes for both services
- AC6: `dotnet test` passes for both services

## Constraints (non-negotiable)
- Clean Architecture layers respected
- Migration must be additive (not destructive)
- OrgId comes from JWT claims (user's current organization), not from request body
- English-only repo content
- Update walkthrough

## Implementation Steps

### ai-api Service

1. **Create ITenantAccessor interface** (`services/ai-api/src/SaasTemplate.AiApi.Application/Common/Interfaces/ITenantAccessor.cs`)
   - `Guid? GetCurrentOrgId()` - resolves OrgId from HTTP context (JWT claims or header)
   - This will be used by command handlers to set OrgId

2. **Implement TenantAccessor** (`services/ai-api/src/SaasTemplate.AiApi.Infrastructure/Services/TenantAccessor.cs` or `Api/Services/`)
   - Read OrgId from JWT claim `org_id` or from `X-Org-Id` header
   - Register as Scoped in DI

3. **Update WorkItem entity** (`services/ai-api/src/SaasTemplate.AiApi.Domain/WorkItems/WorkItem.cs`)
   - Add `public Guid OrgId { get; private set; }`
   - Update `Create()` factory method to require `orgId` parameter
   - Add `OrgId` to constructor

4. **Update WorkItem EF configuration** (`services/ai-api/src/SaasTemplate.AiApi.Infrastructure/Persistence/Configurations/WorkItemConfiguration.cs`)
   - Add `builder.Property(w => w.OrgId).HasColumnName("org_id").IsRequired()`
   - Add index: `builder.HasIndex(w => w.OrgId)`

5. **Update WorkItemRead** (`services/ai-api/src/SaasTemplate.AiApi.Infrastructure/Persistence/Entities/WorkItemRead.cs`)
   - Add `OrgId` property
   - Update its configuration similarly

6. **Create migration** (two-step for existing data)
   - Step 1: Add nullable `org_id` column
   - Step 2: Backfill with default OrgId (or system org)
   - Step 3: Alter column to non-nullable
   - Command: `dotnet ef migrations add AddTenantIdToWorkItems --project services/ai-api/src/SaasTemplate.AiApi.Infrastructure --startup-project services/ai-api/src/SaasTemplate.AiApi.Api`

7. **Update CreateWorkItemCommand** (`services/ai-api/src/SaasTemplate.AiApi.Application/WorkItems/Commands/CreateWorkItem/CreateWorkItemCommand.cs`)
   - Handler: get OrgId from ITenantAccessor -> pass to WorkItem.Create()

8. **Update ListWorkItemsQuery** (`services/ai-api/src/SaasTemplate.AiApi.Application/WorkItems/Queries/ListWorkItems/ListWorkItemsQuery.cs`)
   - Handler: filter by current OrgId (preparation for task 0033 query filters)

9. **Update WorkItemDto** (`services/ai-api/src/SaasTemplate.AiApi.Application/WorkItems/DTOs/WorkItemDto.cs`)
   - Add `OrgId` property

10. **Update WorkItemsController** (`services/ai-api/src/SaasTemplate.AiApi.Api/Controllers/WorkItemsController.cs`)
    - Update response mapping to include OrgId

11. **Register ITenantAccessor** in DI (`services/ai-api/src/SaasTemplate.AiApi.Infrastructure/DependencyInjection.cs`)

### orgs-api Service (if applicable)

12. **Add OrgId to AuditLog** (`services/orgs-api/src/SaasTemplate.OrgsApi.Domain/Common/AuditLog.cs`)
    - Add `public Guid? OrgId { get; private set; }` (nullable since some audits are system-level)
    - Update AuditLogConfiguration

13. **Create orgs-api migration**
    - `dotnet ef migrations add AddOrgIdToAuditLog --project services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure --startup-project services/orgs-api/src/SaasTemplate.OrgsApi.Api`

### Testing

14. **Update existing tests** that create WorkItems to include OrgId
15. **Add tests** for ITenantAccessor resolution

## Files to Create / Modify

### Create
- `services/ai-api/src/SaasTemplate.AiApi.Application/Common/Interfaces/ITenantAccessor.cs`
- `services/ai-api/src/SaasTemplate.AiApi.Api/Services/TenantAccessor.cs` (or Infrastructure)
- `docs/walkthroughs/0032-tenant-id-entities.md`

### Modify
- `services/ai-api/src/SaasTemplate.AiApi.Domain/WorkItems/WorkItem.cs`
- `services/ai-api/src/SaasTemplate.AiApi.Infrastructure/Persistence/Configurations/WorkItemConfiguration.cs`
- `services/ai-api/src/SaasTemplate.AiApi.Infrastructure/Persistence/Entities/WorkItemRead.cs`
- `services/ai-api/src/SaasTemplate.AiApi.Application/WorkItems/Commands/CreateWorkItem/CreateWorkItemCommand.cs`
- `services/ai-api/src/SaasTemplate.AiApi.Application/WorkItems/Commands/CreateWorkItem/CreateWorkItemCommandHandler.cs`
- `services/ai-api/src/SaasTemplate.AiApi.Application/WorkItems/Queries/ListWorkItems/ListWorkItemsQueryHandler.cs`
- `services/ai-api/src/SaasTemplate.AiApi.Application/WorkItems/DTOs/WorkItemDto.cs`
- `services/ai-api/src/SaasTemplate.AiApi.Api/Controllers/WorkItemsController.cs`
- `services/ai-api/src/SaasTemplate.AiApi.Infrastructure/DependencyInjection.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Domain/Common/AuditLog.cs` (if adding OrgId)
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Configurations/AuditLogConfiguration.cs`

## Testing Plan
- Unit tests: WorkItem.Create() with OrgId, ITenantAccessor mock
- Integration tests: Create work item with OrgId, list work items includes OrgId
- Migration test: Apply on clean DB and on DB with existing data

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes (both services)
- [x] Tests added/updated and pass (both services)
- [x] Migration applies cleanly
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
