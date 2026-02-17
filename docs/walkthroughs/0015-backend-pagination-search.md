# Walkthrough: 0015-backend-pagination-search

## Task Reference
- Task: `docs/tasks/0015-backend-pagination-search.md`
- Walkthrough: `docs/walkthroughs/0015-backend-pagination-search.md`
- Branch/PR: `feat/0015-backend-pagination-search`
- Date: `2026-02-06`

## Summary
Added offset-based pagination to both list endpoints (`GET /orgs/mine` and `GET /orgs/{id}/members`) and search filtering (by name/email) to the members endpoint. Updated the frontend with paginated hooks, a reusable `Pagination` component, debounced search input, and i18n support. Also normalized `user_id` to `userId` across the frontend to match .NET camelCase JSON serialization.

## Context
- Background: List endpoints returned all records without pagination. The existing `PagedResult<T>` model was already in the Application layer but unused by the orgs-api.
- Problem statement: Unbounded list responses won't scale. Members endpoint lacked search functionality.
- Constraints: Clean Architecture layers respected; the existing N+1 query pattern (iterate memberships, fetch users individually) was replaced with JOIN queries.

## Decisions & Trade-offs
- **Offset-based pagination over cursor-based:**
  - Offset pagination (Skip/Take) was chosen for simplicity. The dataset sizes for orgs and members are small enough that offset performance is acceptable.

- **JOIN queries in repository:**
  - The previous implementation fetched memberships, then looped to fetch each user individually (N+1 pattern). The new implementation uses EF Core LINQ JOINs to fetch data in a single query with `Skip/Take`.

- **`user_id` to `userId` rename:**
  - The old API used a `MemberResponse` DTO with `[JsonPropertyName("user_id")]` for snake_case. Since `PagedResult<MemberDto>` is returned directly (no mapping DTO), the JSON serialization uses .NET default camelCase (`userId`). The frontend was updated to match.

- **Search debounce (300ms):**
  - Search input in MembersManager is debounced to avoid firing API calls on every keystroke.

## Implementation Notes

### Backend (orgs-api)

**Modified files:**
- `Application/Organizations/Queries/GetMyOrgs/GetMyOrgsQuery.cs` — Changed to `IRequest<PagedResult<OrgWithRole>>`, added `Page=1, PageSize=20` defaults, removed `GetMyOrgsResult` record
- `Application/Organizations/Queries/GetMyOrgs/GetMyOrgsQueryHandler.cs` — Returns `PagedResult<OrgWithRole>`, delegates to `GetOrgsPagedAsync`, validates page/pageSize with `Math.Max/Math.Clamp`
- `Application/Organizations/Queries/GetMembers/GetMembersQuery.cs` — Changed to `IRequest<PagedResult<MemberDto>>`, added `Page, PageSize, Search, RequestingUserId` params, removed `GetMembersResult` record
- `Application/Organizations/Queries/GetMembers/GetMembersQueryHandler.cs` — Returns `PagedResult<MemberDto>`, delegates to `GetMembersPagedAsync`, verifies requesting user's membership (IDOR fix), removed N+1 query pattern and `IUserRepository` dependency
- `Application/Organizations/Interfaces/IMembershipRepository.cs` — Added `GetOrgsPagedAsync` and `GetMembersPagedAsync` methods
- `Infrastructure/Persistence/Repositories/MembershipRepository.cs` — Implemented paginated JOIN queries with `Skip/Take`, case-insensitive search via `EF.Functions.ILike()`
- `Api/Controllers/OrgsController.cs` — Added `[FromQuery]` parameters for `page`, `pageSize`, `search`; extracts userId and passes to GetMembersQuery for authorization; returns `PagedResult` directly; removed `MemberResponse` DTO and `System.Text.Json.Serialization` import

**Existing model used:**
- `Application/Common/Models/PagedResult.cs` — Already existed with `Items`, `PageNumber`, `TotalPages`, `TotalCount`, `HasPreviousPage`, `HasNextPage`

### Frontend (apps/web)

**New files:**
- `src/components/ui/pagination.tsx` — Reusable `Pagination` component with Previous/Next buttons, page indicator, and `aria-label` for accessibility

**Modified files:**
- `src/lib/schemas.ts` — `MemberSchema.user_id` renamed to `userId`; added `pagedResponseSchema()` factory and `PagedResponse<T>` type; updated `MyOrgsResponseSchema` and `MembersResponseSchema` to use paginated format; restored accidentally deleted `CsrfResponseSchema`
- `src/hooks/orgs.ts` — `useMyOrgs(page, pageSize)` and `useMembers(orgId, page, pageSize, search)` now accept pagination params, build URL query strings, return `pagination` state; added `PaginationState` type; used object destructuring for pagination data; fixed `user_id` to `userId`
- `src/components/orgs/MembersManager.tsx` — Added `page`, `search`, `debouncedSearch` state; debounced search with 300ms `setTimeout`; added search input and pagination component; preserved `currentMember` across pagination pages; fixed `user_id` to `userId`
- `src/components/orgs/MembersTable.tsx` — Fixed `user_id` to `userId`
- `src/app/[locale]/orgs/mine/page.tsx` — Added `page` state and `Pagination` component
- `messages/en.json` — Added `common.pagination.*` keys (previous, next, pageOf, showing, search)
- `messages/es.json` — Added Spanish translations for pagination keys

**Test files updated:**
- `src/hooks/__tests__/orgs.test.ts` — Updated mock responses to paginated format; `user_id` to `userId`; added `pagedResponse()` helper
- `src/lib/__tests__/schemas.test.ts` — Updated schema tests for paginated response format; `user_id` to `userId`
- `src/components/orgs/__tests__/MembersManager.test.tsx` — Added `pagination` to mock `useMembers` return; `user_id` to `userId`
- `src/components/orgs/__tests__/MembersTable.test.tsx` — `user_id` to `userId`
- `src/components/orgs/__tests__/LeaveOrgButton.test.tsx` — `user_id` to `userId`

## Verification
- `dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln` — 0 errors, 0 warnings
- `dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln` — 13 tests passed (8 unit + 5 architecture)
- `npm run build` (apps/web) — Compiled successfully
- `npx vitest run` (apps/web) — 184 tests passed across 14 test files
