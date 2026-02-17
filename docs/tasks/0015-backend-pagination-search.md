# Task: 0015 - Backend Pagination and Search/Filtering

## Metadata
- ID: 0015
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-04
- Related docs:
  - Epic: `docs/backlog/epic-001-professional-saas-refinement.md`
  - Walkthrough: `docs/walkthroughs/0015-backend-pagination-search.md`

## Goal
Add offset-based pagination to all list endpoints (GetMembers, GetMyOrgs) and add search/filtering by name and email on the members endpoint. Update frontend to consume pagination metadata.

## Context
List endpoints currently return all records without pagination. This works for small datasets but won't scale. The AI API already has pagination on ListWorkItems which can serve as a reference pattern.

## Scope
### In scope
- Add pagination to `GET /orgs/mine` (page, pageSize, total)
- Add pagination to `GET /orgs/{orgId}/members` (page, pageSize, total)
- Add search parameter to members endpoint (search by name or email)
- Create a shared `PagedResponse<T>` DTO
- Update frontend hooks to support pagination parameters
- Update frontend UI with pagination controls (simple prev/next)

### Out of scope
- Cursor-based pagination (offset is fine for now)
- Full-text search (simple LIKE/ILIKE is sufficient)
- Backend for AI API (already has pagination)

## Requirements
- R1: Pagination uses query params: `?page=1&pageSize=20`
- R2: Default pageSize is 20, max is 100
- R3: Response includes: `{ items: T[], page: number, pageSize: number, totalCount: number, totalPages: number }`
- R4: Search uses `?search=term` and matches against name OR email (case-insensitive)
- R5: Frontend shows pagination controls when totalPages > 1

## Acceptance Criteria
- AC1: `GET /orgs/mine?page=1&pageSize=10` returns paginated response
- AC2: `GET /orgs/{id}/members?page=1&search=john` returns filtered, paginated results
- AC3: Frontend pagination controls navigate between pages
- AC4: `dotnet build` succeeds for orgs-api
- AC5: `dotnet test` passes for orgs-api
- AC6: `npm run build` succeeds
- AC7: ESLint passes

## Constraints (non-negotiable)
- Clean Architecture layers respected.
- English-only repo content.
- No secrets in repo.
- Update walkthrough.

## Implementation Steps
1. Create `PagedResponse<T>` in Application layer
2. Create `PaginationParams` value object
3. Update `GetMyOrgsQuery` to accept pagination params
4. Update `GetMyOrgsQueryHandler` to use `.Skip()` / `.Take()` and `.CountAsync()`
5. Update `GetMembersQuery` to accept pagination + search params
6. Update `GetMembersQueryHandler` with ILIKE search and pagination
7. Update controller endpoints to accept query params
8. Update frontend `useMyOrgs` and `useMembers` hooks
9. Create simple Pagination component in frontend
10. Add pagination to org list and members list pages

## Files to Create / Modify
### Backend (orgs-api)
- `Application/Common/PagedResponse.cs` (create)
- `Application/Common/PaginationParams.cs` (create)
- `Application/Orgs/Queries/GetMyOrgs/` (modify)
- `Application/Orgs/Queries/GetMembers/` (modify)
- `Api/Controllers/OrgsController.cs` (modify)

### Frontend
- `apps/web/src/components/ui/pagination.tsx` (create)
- `apps/web/src/hooks/orgs.ts` (modify)
- `apps/web/src/app/[locale]/orgs/mine/page.tsx` (modify)
- `apps/web/src/components/orgs/MembersManager.tsx` (modify)

## Testing Plan
- Unit tests: Handler tests with pagination
- Integration tests: API endpoint tests with query params
- Manual verification: Navigate between pages in UI

## Security & Privacy
- Search input must be sanitized (parameterized queries prevent SQL injection)
- Page/pageSize validated (prevent negative values or huge page sizes)

## Observability
- Log page/pageSize in request traces

## Rollback Plan
Revert to non-paginated endpoints. Frontend ignores pagination metadata gracefully.

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
