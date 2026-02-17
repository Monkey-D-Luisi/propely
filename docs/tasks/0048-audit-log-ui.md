# Task: 0048 - Audit Log Viewer UI + Export

## Metadata
- ID: 0048
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-07
- GitHub Issue: #200
- Epic: `docs/backlog/epic-007-admin-dashboards.md`
- Old Issue: #63
- Milestone: v1.0

## Goal
Create an admin page to view audit log entries with filtering, pagination, and CSV/JSON export.

## Context
The audit logging infrastructure is complete (task 0017): `AuditLog` entity captures who/what/when for all mutations, stored in `audit_logs` table. However, there's no UI to view these logs. Admins need a way to search and export audit trails for compliance and debugging.

### Existing Backend
- `AuditLog` entity in Domain: Id, UserId, Action, EntityType, EntityId, Changes, CorrelationId, CreatedAtUtc
- Audit logs written automatically via SaveChanges override in AppDbContext
- No GET endpoint for audit logs exists yet

## Scope
### In scope
- Backend: `GET /admin/audit-logs` endpoint with pagination and filtering
- Filters: date range, user ID, action type, entity type, entity ID
- Admin page: `/admin/audit-logs`
- Table display with all audit fields
- Expandable row to show changes JSON
- Pagination controls
- CSV export and JSON export buttons
- Admin-only access
- i18n strings (EN + ES)

### Out of scope
- Real-time audit log streaming
- Audit log retention policies
- Audit log archival

## Requirements
- R1: Admins can view all audit log entries
- R2: Entries can be filtered by date range, user, action, entity type
- R3: Entries are paginated (50 per page)
- R4: Entries can be exported as CSV or JSON
- R5: Changes field is displayed as formatted JSON
- R6: Non-admin users cannot access

## Acceptance Criteria
- AC1: `GET /admin/audit-logs` returns paginated audit entries
- AC2: Filtering by date range works
- AC3: Filtering by entity type works
- AC4: CSV export downloads a file with all matching entries
- AC5: JSON export downloads a file with all matching entries
- AC6: Admin page renders with all columns
- AC7: `dotnet build` and `dotnet test` pass
- AC8: `npm run build` and `npm test` pass

## Constraints (non-negotiable)
- Clean Architecture layers respected
- Admin authorization required
- Pagination for performance
- i18n for all strings
- Update walkthrough

## Implementation Steps

### Backend (orgs-api)

1. **Create GetAuditLogsQuery** (`services/orgs-api/src/SaasTemplate.OrgsApi.Application/AuditLogs/Queries/GetAuditLogs/`)
   - Properties: Page, PageSize, DateFrom, DateTo, UserId, Action, EntityType, EntityId
   - Handler: query audit_logs table with filters, return PagedResult<AuditLogDto>

2. **Create AuditLogDto** (`services/orgs-api/src/SaasTemplate.OrgsApi.Application/AuditLogs/DTOs/AuditLogDto.cs`)

3. **Create IAuditLogRepository** and implement (or query directly in handler)

4. **Create ExportAuditLogsQuery** - same filters but returns all matching entries (no pagination)
   - Used for CSV/JSON export

5. **Create AuditLogsController** (`services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/AuditLogsController.cs`)
   - `GET /admin/audit-logs` -> GetAuditLogsQuery (paginated)
   - `GET /admin/audit-logs/export?format=csv` -> ExportAuditLogsQuery -> CSV response
   - `GET /admin/audit-logs/export?format=json` -> ExportAuditLogsQuery -> JSON response
   - All endpoints require admin role

### Frontend (apps/web)

6. **Create audit logs page** (`apps/web/src/app/[locale]/admin/audit-logs/page.tsx`)

7. **Create AuditLogTable component** with expandable rows

8. **Create AuditLogFilters component** (date pickers, dropdowns)

9. **Create useAuditLogs hook** (`apps/web/src/hooks/audit-logs.ts`)

10. **Add export buttons** (CSV, JSON) that trigger downloads

11. **Add i18n strings**

### Testing

12. **Backend tests**: Query handler with filters, export format
13. **Frontend tests**: Table rendering, filter application, export buttons

## Files to Create / Modify

### Create
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/AuditLogs/Queries/GetAuditLogs/GetAuditLogsQuery.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/AuditLogs/Queries/GetAuditLogs/GetAuditLogsQueryHandler.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/AuditLogs/DTOs/AuditLogDto.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/AuditLogsController.cs`
- `apps/web/src/app/[locale]/admin/audit-logs/page.tsx`
- `apps/web/src/components/admin/AuditLogTable.tsx`
- `apps/web/src/components/admin/AuditLogFilters.tsx`
- `apps/web/src/hooks/audit-logs.ts`
- `docs/walkthroughs/0048-audit-log-ui.md`

### Modify
- `apps/web/src/components/layout/AppHeader.tsx` (add admin link)
- `apps/web/src/lib/schemas.ts`
- `apps/web/messages/en.json`
- `apps/web/messages/es.json`

## Testing Plan
- Unit tests: GetAuditLogsQueryHandler with various filters
- Integration tests: API endpoint with pagination and filters
- Frontend tests: Component rendering, filtering, export

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] i18n strings added (EN + ES)
- [x] Formatting/analyzers pass
- [x] Walkthrough updated
