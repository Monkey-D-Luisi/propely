# Task 0016: Listing, Filtering & Pagination

## Goal
Provide a query endpoint to list work items with pagination and filtering.

## Requirements

### Application Layer
- [ ] Create `ListWorkItemsQuery` (Page, PageSize, Status, SortBy).
- [ ] Create `PagedResult<T>` wrapper.
- [ ] Implement `ListWorkItemsQueryHandler`.

### Infrastructure Layer
- [ ] Update `IWorkItemReadRepository` with `ListAsync` method.
- [ ] Implement efficient query in `WorkItemReadRepository` (Postgres).
  - Use `Skip` / `Take`.
  - Apply filters (Status).
  - Apply sorting.

### Presentation Layer (API)
- [ ] Add `GET /v1/work-items` endpoint.
- [ ] Add Query Parameters: `page`, `pageSize`, `status`.
- [ ] Return 200 OK with `PagedResult<WorkItemDto>`.
- [ ] Validate pagination inputs (Page > 0, PageSize > 0 & <= 50).

## Definition of Done
- [ ] All requirements implemented.
- [ ] Build succeeds.
- [ ] Integration tests verify pagination and filtering.
- [ ] Unit tests for Query Handler.
- [ ] Swagger documentation shows parameters.
