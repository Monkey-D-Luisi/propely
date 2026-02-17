# Walkthrough: 0032-tenant-id-entities

## Task Reference
- Task: `docs/tasks/0032-tenant-id-entities.md`
- Walkthrough: `docs/walkthroughs/0032-tenant-id-entities.md`
- Branch/PR: `feat/tenant-id-entities-0032` / TBD
- Date: `2026-02-09`

## Summary
Added OrgId (TenantId) column to WorkItem entity in ai-api and OrganizationId to AuditLog in orgs-api to enable multi-tenant data isolation. Updated all layers following Clean Architecture: domain entities, application commands/DTOs, infrastructure EF configurations and migrations, API responses and controllers.

## Context
- Background: WorkItem entity had no organization context, making tenant isolation impossible.
- Problem statement: Any authenticated user could potentially see all work items across all organizations.
- Constraints: Migration must be backward-compatible. ITenantAccessor pattern deferred to task 0033.

## Decisions & Trade-offs
- **Decision:** Use `OrgId` (Guid) in ai-api, `OrganizationId` (Guid?) in orgs-api
  - Options considered: Uniform naming vs service-specific conventions
  - Why this choice: ai-api uses shorter `OrgId` since it doesn't reference the Organization entity. orgs-api uses `OrganizationId` to match existing Membership/Invitation convention.
  - Consequences: Minor naming difference between services, but each is internally consistent.

- **Decision:** Extract OrgId from JWT claims in controllers (not via ITenantAccessor)
  - Options considered: Create ITenantAccessor now vs direct claims extraction
  - Why this choice: ITenantAccessor is explicitly scoped to task 0033. Direct extraction mirrors existing UserId pattern.
  - Consequences: Controller code will be refactored in task 0033 to use ITenantAccessor.

- **Decision:** Migration strategy: nullable column -> backfill with default -> alter to non-nullable
  - Options considered: Single non-nullable migration with default vs explicit 3-step approach
  - Why this choice: Explicit 3-step is clearer and handles existing data gracefully per R3.

## Implementation Notes
- Key changes:
  - `WorkItem.Create()` factory method now requires `orgId` as first parameter (breaking change for callers)
  - `WorkItemCreatedV1` domain event data includes `OrgId` for event-driven projections
  - `WorkItemRead` read model includes `OrgId` for query-side tenant isolation
  - `AuditLog.Create()` now accepts `organizationId` parameter, resolved from entity being audited via reflection
  - `DevAuthenticationHandler` includes `org_id` claim with value `00000000-0000-0000-0000-000000000001`
- Edge cases handled:
  - Existing data backfilled with dev org GUID (`00000000-0000-0000-0000-000000000001`)
  - AuditLog `TryGetOrganizationId` gracefully returns null for entities without OrganizationId
  - WorkItemReadRepository fallback from read model to write model both include OrgId
- Known limitations:
  - No tenant-scoped query filter yet (task 0033)
  - OrgId claim hardcoded in dev auth handler; real JWT mapping pending auth system updates

## Data / Schema / Migrations
- DB changes:
  - `work_items` table: added `org_id` (uuid, NOT NULL) column with index `idx_work_items_org_id`
  - `work_items_read` table: added `org_id` (uuid, NOT NULL) column
  - `audit_logs` table: added `organization_id` (uuid, nullable) column with index `idx_audit_logs_organization_id`
- Migration strategy: 3-step for ai-api (nullable -> backfill -> non-nullable), single additive step for orgs-api (nullable column)
- Backward compatibility: Existing rows backfilled with default org GUID. No data loss.

## Commands Run
```bash
# EF Core migrations
dotnet ef migrations add AddOrgIdToWorkItems --project services/ai-api/src/SaasTemplate.AiApi.Infrastructure --startup-project services/ai-api/src/SaasTemplate.AiApi.Api
dotnet ef migrations add AddOrganizationIdToAuditLog --project services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure --startup-project services/orgs-api/src/SaasTemplate.OrgsApi.Api

# Build
dotnet build services/ai-api/SaasTemplate.AiApi.sln    # 0 errors
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln  # 0 errors

# Tests
dotnet test services/ai-api/SaasTemplate.AiApi.sln     # 140 passed (76 unit + 59 integration + 5 architecture)
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln  # 286 passed (176 unit + 110 integration)
```

## Files Changed
### ai-api
- `src/SaasTemplate.AiApi.Domain/WorkItems/WorkItem.cs` — Added `OrgId` property, updated constructor and `Create()` factory
- `src/SaasTemplate.AiApi.Domain/WorkItems/Events/WorkItemCreatedV1.cs` — Added `OrgId` to event data record
- `src/SaasTemplate.AiApi.Application/WorkItems/Commands/CreateWorkItemCommand.cs` — Added `OrgId` to command and result records
- `src/SaasTemplate.AiApi.Application/WorkItems/Commands/CreateWorkItemCommandHandler.cs` — Pass `OrgId` to factory and result
- `src/SaasTemplate.AiApi.Application/WorkItems/Dtos/WorkItemDto.cs` — Added `OrgId` to DTO record
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Configurations/WorkItemConfiguration.cs` — Mapped `org_id` column + index
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Entities/WorkItemRead.cs` — Added `OrgId` property and factory parameter
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Configurations/WorkItemReadConfiguration.cs` — Mapped `org_id` column
- `src/SaasTemplate.AiApi.Infrastructure/Messaging/WorkItemEventProjector.cs` — Updated projection and payload DTO
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Repositories/WorkItemReadRepository.cs` — Updated DTO construction
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Migrations/20260209105314_AddOrgIdToWorkItems.cs` — 3-step migration
- `src/SaasTemplate.AiApi.Api/Dtos/WorkItemResponse.cs` — Added `OrgId` to response record
- `src/SaasTemplate.AiApi.Api/Controllers/WorkItemsController.cs` — Extract `org_id` claim from JWT
- `src/SaasTemplate.AiApi.Api/Configuration/DevAuthenticationHandler.cs` — Added `org_id` claim

### orgs-api
- `src/SaasTemplate.OrgsApi.Domain/Common/AuditLog.cs` — Added `OrganizationId` property and factory parameter
- `src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Configurations/AuditLogConfiguration.cs` — Mapped `organization_id` column + index
- `src/SaasTemplate.OrgsApi.Infrastructure/Persistence/AppDbContext.cs` — Added `TryGetOrganizationId()` helper, updated audit entry creation
- `src/SaasTemplate.OrgsApi.Infrastructure/Migrations/20260209105406_AddOrganizationIdToAuditLog.cs` — Additive migration

### Tests
- `tests/SaasTemplate.AiApi.UnitTests/Domain/WorkItems/WorkItemTests.cs` — Updated all `Create()` calls with orgId
- `tests/SaasTemplate.AiApi.UnitTests/Application/WorkItems/CreateWorkItemCommandHandlerTests.cs` — Updated command construction
- `tests/SaasTemplate.AiApi.UnitTests/Application/WorkItems/UpdateWorkItemCommandHandlerTests.cs` — Updated `Create()` calls
- `tests/SaasTemplate.AiApi.UnitTests/Application/WorkItems/DeleteWorkItemCommandHandlerTests.cs` — Updated `Create()` calls
- `tests/SaasTemplate.AiApi.UnitTests/Application/WorkItems/GetWorkItemByIdQueryHandlerTests.cs` — Updated DTO construction
- `tests/SaasTemplate.AiApi.IntegrationTests/Persistence/WorkItemRepositoryTests.cs` — Updated `Create()` calls, added `org_id` column assertion
- `tests/SaasTemplate.AiApi.IntegrationTests/Messaging/WorkItemProjectorTests.cs` — Updated `FromCreatedEvent()` calls

## Tests
### Unit
- What was added/updated: All `WorkItem.Create()` and `CreateWorkItemCommand` callsites updated with OrgId parameter. All `WorkItemDto` constructions updated. No new test classes added; existing tests updated to cover OrgId flow.
- How to run: `dotnet test services/ai-api/SaasTemplate.AiApi.sln`

### Integration
- What was added/updated: `WorkItemRepositoryTests.WorkItemTable_ShouldUseSnakeCaseNaming` now asserts `org_id` column. `WorkItemProjectorTests` updated with orgId in `FromCreatedEvent()` calls. Controller tests pass via DevAuthenticationHandler with `org_id` claim.
- How to run: `dotnet test services/ai-api/SaasTemplate.AiApi.sln`

### Manual
- No manual verification required.

## Observability
- Logs added/updated: OrgId included in domain events (WorkItemCreatedV1) which flow through outbox/messaging
- Traces/metrics added/updated: None

## Security
- Validation: OrgId extracted from JWT `org_id` claim, not request body (prevents tenant spoofing)
- AuthN/AuthZ impact: Missing `org_id` claim returns 401 Unauthorized from CreateWorkItem endpoint
- Sensitive data handling: No PII involved

## Follow-ups / Backlog
- [ ] Task 0033: Create ITenantAccessor and EF Core global query filters for tenant isolation
- [ ] Task 0034: Cross-tenant data leakage tests

## Checklist
- [x] Task scope matches `docs/tasks/0032-tenant-id-entities.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
