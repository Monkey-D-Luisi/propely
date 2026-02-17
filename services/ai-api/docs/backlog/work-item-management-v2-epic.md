# Epic: Work Item Management V2 (Advanced Features)

## Overview

This epic extends the Work Item Management feature set to reach full maturity. It adds full CRUD capabilities, advanced data retrieval patterns (pagination, filtering), and a robust security layer with authentication.

### Goals
1.  **Full Logic**: Enable modification and removal of work items.
2.  **Scalable Access**: Allow clients to efficiently query large datasets.
3.  **Security**: Protect endpoints with standard JWT authentication.

## Tasks

### Task 0015 — Work Item Updates & Deletion
**Status:** DONE
**Priority:** High
**Dependencies:** None

#### Goal
Implement `PUT` and `DELETE` operations for Work Items, completing the CRUD cycle.

#### Requirements
- **Domain**:
  - Add `Update(title, description, status)` method to `WorkItem`.
  - Add `Delete()` method to `WorkItem`.
  - Create events: `WorkItemUpdatedV1`, `WorkItemDeletedV1`.
- **Application**:
  - `UpdateWorkItemCommand` / Handler.
  - `DeleteWorkItemCommand` / Handler.
  - Validation: Ensure ID exists, Status transitions are valid.
- **Infrastructure**:
  - Repository updates.
  - Outbox: Persist new events.
- **Projector**:
  - Handle `WorkItemUpdatedV1`: Update read model.
  - Handle `WorkItemDeletedV1`: Remove from read model.
  - Invalidate cache `workitem:{id}`.
- **API**:
  - `PUT /v1/work-items/{id}`
  - `DELETE /v1/work-items/{id}`

#### Acceptance Criteria
- [ ] Work Item details can be updated via PUT.
- [ ] Work Item can be deleted via DELETE.
- [ ] Read model reflects changes immediately (after projection).
- [ ] Cache is invalidated/updated on change.
- [ ] Events `Updated` and `Deleted` are published.
- [ ] Unit & Integration tests cover new commands.

---

### Task 0016 — Listing, Filtering & Pagination
**Status:** DONE
**Priority:** Medium
**Dependencies:** 0015

#### Goal
Provide a query endpoint to list work items with pagination and filtering.

#### Requirements
- **Application**:
  - `ListWorkItemsQuery`:
    - `Page` (int, default 1)
    - `PageSize` (int, default 10, max 50)
    - `Status` (optional, filter)
    - `SortBy` (optional, default CreatedAtDesc)
  - `PagedResult<T>` wrapper class.
- **Infrastructure**:
  - Implement efficient query in `WorkItemReadRepository` (using Read Model).
  - Use `Skip` / `Take` for pagination.
- **API**:
  - `GET /v1/work-items`
  - Query Params: `page`, `pageSize`, `status`
  - Response 200: `{ items: [...], totalCount: 100, page: 1, pageSize: 10 }`

#### Acceptance Criteria
- [ ] Can fetch a paginated list of work items.
- [ ] Can filter list by Status.
- [ ] Default sorting is by creation date (descending).
- [ ] Invalid page/pageSize returns 400 Bad Request.

---

### Task 0017 — Authentication (JWT)
**Status:** IN_PROGRESS
**Priority:** High
**Dependencies:** 0016

#### Goal
Secure the API using JWT Bearer authentication.

#### Requirements
- **Infrastructure**:
  - Configure `JwtBearer` authentication in `Program.cs`.
  - Use `dotnet user-jwts` for local development simplicity (no external IdP dependency required for dev).
- **API**:
  - Apply `[Authorize]` to all Work Item endpoints.
  - Update Swagger/OpenAPI to support Bearer token input.
- **Identity**:
  - Add `UserId` (Guid) to `WorkItem` entity (audit field).
  - Add `UserId` to Commands (extracted from `ClaimsPrincipal`).

#### Acceptance Criteria
- [ ] Unauthenticated requests return 401 Unauthorized.
- [ ] Authenticated requests with valid token succeed.
- [ ] Swagger UI allows inputting Bearer token.
- [ ] Local development workflow using `dotnet user-jwts` is documented in QUICKSTART.md.

