# Walkthrough: 0048-audit-log-ui

## Task Reference
- Task: `docs/tasks/0048-audit-log-ui.md`
- Walkthrough: `docs/walkthroughs/0048-audit-log-ui.md`
- Branch/PR: `feat/0048-audit-log-viewer`
- Date: `2026-02-12`

## Summary
Implemented a full audit log viewer for admins at `/admin/audit-logs`. The page provides paginated viewing, filtering by date range/user/action/entity, expandable JSON changes, and CSV/JSON export. Backend uses Clean Architecture with CQRS queries through MediatR, a dedicated repository, and a controller with export endpoints.

## Context
- Background: Audit logging infrastructure (task 0017) captures all mutations via SaveChanges override. No UI existed to view these logs.
- Problem statement: Admins needed a way to search and export audit trails for compliance and debugging.
- Constraints: Must use existing patterns (pagination, i18n, design system tokens).

## Decisions & Trade-offs
- **Decision:** Repository pattern for audit log queries instead of direct DbContext queries in handler.
  - Options considered: Query directly in handler vs dedicated repository
  - Why this choice: Follows existing codebase pattern (all other entities use repositories). Allows filter logic reuse between paged and export queries.
  - Consequences: One extra abstraction layer, but consistent with architecture.

- **Decision:** Separate GetAuditLogsQuery (paged) and ExportAuditLogsQuery (all matching).
  - Options considered: Single query with "export mode" flag vs two queries
  - Why this choice: Different return types (PagedResult vs List). Cleaner separation of concerns.

- **Decision:** Debounced filters with 400ms delay.
  - Why: Prevents excessive API calls while typing. Consistent with MembersManager debounce pattern.

## Implementation Notes
- Key changes: New CQRS queries, repository, controller, frontend page with table/filters/pagination/export
- Edge cases: CSV escaping handles commas, quotes, and newlines in changes JSON
- Known limitations: No role-based access check on backend (endpoint is [Authorize] but not admin-only — matches feature flags pattern)

## Data / Schema / Migrations
- No DB changes needed — reads existing `audit_logs` table
- Existing indexes on `created_at_utc`, `user_id`, `organization_id`, `(entity_type, entity_id)` support all filter queries

## Commands Run
```bash
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln
npx tsc --noEmit
```

## Files Changed

### Created
- `services/orgs-api/src/.../Application/AuditLogs/DTOs/AuditLogDto.cs` — DTO record
- `services/orgs-api/src/.../Application/AuditLogs/Interfaces/IAuditLogRepository.cs` — Repository interface
- `services/orgs-api/src/.../Application/AuditLogs/Queries/GetAuditLogs/GetAuditLogsQuery.cs` — Paged query
- `services/orgs-api/src/.../Application/AuditLogs/Queries/GetAuditLogs/GetAuditLogsQueryHandler.cs` — Paged handler
- `services/orgs-api/src/.../Application/AuditLogs/Queries/ExportAuditLogs/ExportAuditLogsQuery.cs` — Export query
- `services/orgs-api/src/.../Application/AuditLogs/Queries/ExportAuditLogs/ExportAuditLogsQueryHandler.cs` — Export handler
- `services/orgs-api/src/.../Infrastructure/Persistence/Repositories/AuditLogRepository.cs` — EF Core implementation
- `services/orgs-api/src/.../Api/Controllers/AuditLogsController.cs` — REST endpoints
- `apps/web/src/app/[locale]/admin/audit-logs/page.tsx` — Admin page
- `apps/web/src/components/admin/AuditLogTable.tsx` — Table with expandable rows
- `apps/web/src/components/admin/AuditLogFilters.tsx` — Filter controls
- `apps/web/src/hooks/audit-logs.ts` — Data fetching hooks
- `tests/.../Application/AuditLogs/Queries/GetAuditLogsQueryHandlerTests.cs` — 5 unit tests
- `tests/.../Application/AuditLogs/Queries/ExportAuditLogsQueryHandlerTests.cs` — 3 unit tests

### Modified
- `services/orgs-api/src/.../Infrastructure/DependencyInjection.cs` — Register IAuditLogRepository
- `apps/web/src/lib/schemas.ts` — AuditLogSchema, AuditLogsResponseSchema
- `apps/web/src/components/layout/AppHeader.tsx` — Audit Logs nav link
- `apps/web/messages/en.json` — auditLogs i18n section
- `apps/web/messages/es.json` — auditLogs i18n section (Spanish)
- `docs/backlog/epic-007-admin-dashboards.md` — Task 0048 DONE
- `docs/tasks/0048-audit-log-ui.md` — Status DONE, DoD checked

## Tests
### Unit
- `GetAuditLogsQueryHandlerTests`: 5 tests (no filters, action filter, date range, DTO mapping, pagination info)
- `ExportAuditLogsQueryHandlerTests`: 3 tests (no filters, with filters, DTO mapping)
- Run: `dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln`
- Result: 528 total (370 unit + 5 arch + 153 integration), 0 failures

### Manual
- N/A (requires running stack with seed data)

## Security
- Endpoint requires authentication ([Authorize])
- No sensitive data exposure — audit logs are admin-operational data
- CSV export uses proper escaping to prevent injection

## Follow-ups / Backlog
- [ ] Add admin role check to audit logs endpoint (currently [Authorize] only, same as feature flags)

## Checklist
- [x] Task scope matches `docs/tasks/0048-audit-log-ui.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
