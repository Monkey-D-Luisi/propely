# Task 0015: Work Item Updates & Deletion

## Goal
Implement `PUT` and `DELETE` operations for Work Items to complete the CRUD cycle.

## Requirements

### Domain Layer
- [ ] Add `Update(string title, string? description, WorkItemStatus status)` to `WorkItem`.
- [ ] Add `Delete()` to `WorkItem`.
- [ ] Create `WorkItemUpdatedV1` event.
- [ ] Create `WorkItemDeletedV1` event.

### Application Layer
- [ ] Create `UpdateWorkItemCommand` and `UpdateWorkItemCommandHandler`.
  - Validate ID exists.
  - Update entity.
  - Save changes.
- [ ] Create `DeleteWorkItemCommand` and `DeleteWorkItemCommandHandler`.
  - Validate ID exists.
  - Delete entity.
  - Save changes.
- [ ] Update `IWorkItemRepository` with `DeleteAsync` / `UpdateAsync` (if generic Repository doesn't suffice).

### Infrastructure Layer
- [ ] Implement Repository updates.
- [ ] Ensure `SoftDelete` IS used (Requirement updated). *Decision: Soft Delete using Status='Deleted'.*

### Presentation Layer (API)
- [ ] `PUT /v1/work-items/{id}`
  - Request: Title, Description, Status.
  - Response: 204 No Content (or 200 with updated DTO). *Decision: 204 No Content.*
- [ ] `DELETE /v1/work-items/{id}`
  - Response: 204 No Content.

### Background Processing (Projector)
- [ ] Update `WorkItemProjectorService` to handle `WorkItemUpdatedV1`.
  - Update read model `work_items_read`.
  - Invalidate Cache `workitem:{id}`.
- [ ] Update `WorkItemProjectorService` to handle `WorkItemDeletedV1`.
  - Update `work_items_read` status to `Deleted`.
  - Invalidate Cache `workitem:{id}`.

## Definition of Done
- [ ] All requirements implemented.
- [ ] Build succeeds.
- [ ] Unit tests for Domain & Application logic.
- [ ] Integration tests for API & Persistence.
- [ ] No secrets committed.
