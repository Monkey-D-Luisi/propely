# Walkthrough - Task 0015: Work Item Updates & Deletion

I have implemented `PUT` (Update) and `DELETE` (Soft Delete) operations for Work Items, along with a comprehensive status lifecycle update.

## Changes

### 1. Domain Layer
- **Refactored `WorkItemStatus`**: expanded to `Pending`, `Active`, `Deleted`, `Deactivated`, `Expired`.
- **Updated `WorkItem`**:
    - Initial status is now `Pending`.
    - Added `Update(title, description, status)` method.
    - Added `Delete()` method which sets status to `Deleted`.
    - Added `WorkItemUpdatedV1` and `WorkItemDeletedV1` domain events.

### 2. Application Layer
- **Commands**:
    - `UpdateWorkItemCommand` & handler.
    - `DeleteWorkItemCommand` & handler.
- **Repository Interface**: Added `UpdateAsync`.

### 3. Infrastructure Layer
- **Repository**: Implemented `UpdateAsync` in `WorkItemRepository`.
- **Read Model**: Updated `WorkItemReadRepository` to handle new statuses and fallback to `Pending`.
- **Projector**: Updated `WorkItemEventProjector` to handle `WorkItemUpdatedV1` and `WorkItemDeletedV1`:
    - Updates Read Model in Postgres.
    - Invalidates Redis Cache.
    - Soft deleted items remain in Read Model but with `Status = "Deleted"`.

### 4. API Layer
- **Endpoints**:
    - `PUT /v1/work-items/{id}`: Updates work item. Returns `204 No Content`.
    - `DELETE /v1/work-items/{id}`: Soft deletes work item. Returns `204 No Content`.
- **Validation**: Added `UpdateWorkItemRequestValidator`.
- **Security**: Added `CanUpdateWorkItem` and `CanDeleteWorkItem` policies.

## Verification Results

### Automated Tests
Ran full test suite: `dotnet test`
- **Total Tests**: 106
- **Passed**: 106
- **Failed**: 0

### Key Test Coverages
- **Unit Tests**:
    - `UpdateWorkItemCommandHandlerTests`: Verifies update logic and repository calls.
    - `DeleteWorkItemCommandHandlerTests`: Verifies soft delete transition.
    - `WorkItemTests`: Verifies domain state transitions and event generation.
- **Integration Tests**:
    - `WorkItemsControllerTests`: Verifies API endpoints (PUT/DELETE) and status codes (204, 404).
    - `WorkItemProjectorTests`: Verifies Read Model updates from events.

## Next Steps
- Verify end-to-end in staging environment if needed.
- Monitor `work_items_read` table for accumulation of "Deleted" items (retention policy might be needed in future).
