# Vertical Slice Definition

> **Note:** This document was written as the initial design spec for the first vertical slice. The implementation has since evolved. Key differences from the original spec are noted inline.

## Overview
This document defines the first vertical slice to be implemented. A vertical slice traverses all architectural layers for a single feature, demonstrating the complete technical stack.

## The Slice: Work Item Management

### Domain Concept
A **Work Item** is the minimal domain entity representing a trackable unit of work.

### Work Item Entity
```
WorkItem
├── Id: Guid (identity)
├── OrgId: Guid (tenant isolation)
├── UserId: Guid (creator)
├── Title: string (required, 1-200 chars)
├── Description: string (optional, max 2000 chars)
├── Status: WorkItemStatus (enum: Pending, Active, Deleted, Deactivated, Expired)
├── IsDeleted: bool (soft delete)
├── DeletedAtUtc: DateTime? (soft delete timestamp)
├── CreatedAtUtc: DateTime
├── UpdatedAtUtc: DateTime
└── Version: int (optimistic concurrency)
```

> **Implementation note:** The original spec used `Draft, Active, Completed` as status values. The actual implementation uses `Pending = 0, Active = 1, Deleted = 2, Deactivated = 3, Expired = 4`. Entity creation uses the static factory method `WorkItem.Create(orgId, userId, title, description)`.

### Domain Events
- `WorkItemCreatedV1` — emitted when a work item is created
- `WorkItemUpdatedV1` — emitted when a work item is modified (future)

## API Endpoints

### Create Work Item
```
POST /v1/work-items
Content-Type: application/json

{
  "title": "string",
  "description": "string (optional)"
}

Response 201 Created:
{
  "id": "guid",
  "title": "string",
  "status": "Draft",
  "createdAtUtc": "datetime"
}
```

### Get Work Item by ID
```
GET /v1/work-items/{id}

Response 200 OK:
{
  "id": "guid",
  "title": "string",
  "description": "string",
  "status": "Draft|Active|Completed",
  "createdAtUtc": "datetime",
  "updatedAtUtc": "datetime"
}

Response 404 Not Found (if not exists)
```

## Data Flow

### Write Path (Create)
```
┌─────────────────────────────────────────────────────────────────────┐
│ 1. HTTP POST /v1/work-items                                         │
│    ↓                                                                │
│ 2. Controller validates request DTO                                 │
│    ↓                                                                │
│ 3. CreateWorkItemCommand dispatched                                 │
│    ↓                                                                │
│ 4. Handler creates WorkItem entity (domain)                         │
│    ↓                                                                │
│ 5. Single transaction:                                              │
│    ├─ Insert WorkItem to Postgres                                   │
│    └─ Insert WorkItemCreatedV1 to Outbox table                      │
│    ↓                                                                │
│ 6. Return Created response with ID                                  │
└─────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│ 7. Outbox Dispatcher (background service)                           │
│    ├─ Polls Outbox table                                            │
│    ├─ Publishes WorkItemCreatedV1 to RabbitMQ                       │
│    └─ Marks message as processed                                    │
└─────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│ 8. Consumer (worker)                                                │
│    ├─ Receives WorkItemCreatedV1 from RabbitMQ                      │
│    ├─ Writes/updates read model (projection)                        │
│    └─ Idempotent (checks EventId)                                   │
└─────────────────────────────────────────────────────────────────────┘
```

### Read Path (Get by ID)
```
┌─────────────────────────────────────────────────────────────────────┐
│ 1. HTTP GET /v1/work-items/{id}                                     │
│    ↓                                                                │
│ 2. Controller dispatches GetWorkItemByIdQuery                       │
│    ↓                                                                │
│ 3. Handler checks Redis cache                                       │
│    ├─ Cache hit → return cached DTO                                 │
│    └─ Cache miss → query Postgres read model                        │
│        ├─ Not found → return 404                                    │
│        └─ Found → cache result, return DTO                          │
└─────────────────────────────────────────────────────────────────────┘
```

## Database Schema

### Write Model (Postgres)
```sql
CREATE TABLE work_items (
    id UUID PRIMARY KEY,
    title VARCHAR(200) NOT NULL,
    description VARCHAR(2000),
    status VARCHAR(20) NOT NULL DEFAULT 'Draft',
    created_at_utc TIMESTAMP NOT NULL,
    updated_at_utc TIMESTAMP NOT NULL,
    version INT NOT NULL DEFAULT 1
);

CREATE INDEX idx_work_items_status ON work_items(status);
```

### Outbox Table (Postgres)
```sql
CREATE TABLE outbox_messages (
    id UUID PRIMARY KEY,
    event_type VARCHAR(200) NOT NULL,
    payload JSONB NOT NULL,
    occurred_at_utc TIMESTAMP NOT NULL,
    processed_at_utc TIMESTAMP,
    correlation_id UUID,
    causation_id UUID,
    CONSTRAINT chk_event_type CHECK (event_type <> '')
);

CREATE INDEX idx_outbox_unprocessed ON outbox_messages(occurred_at_utc)
    WHERE processed_at_utc IS NULL;
```

### Read Model (Postgres, same DB for simplicity)
```sql
CREATE TABLE work_items_read (
    id UUID PRIMARY KEY,
    title VARCHAR(200) NOT NULL,
    description VARCHAR(2000),
    status VARCHAR(20) NOT NULL,
    created_at_utc TIMESTAMP NOT NULL,
    updated_at_utc TIMESTAMP NOT NULL
);
```

## Event Schema

### WorkItemCreatedV1
```json
{
  "eventId": "guid",
  "eventType": "WorkItemCreatedV1",
  "schemaVersion": 1,
  "occurredAtUtc": "2025-01-27T10:00:00Z",
  "correlationId": "guid",
  "causationId": "guid",
  "producer": "WorkItemApi",
  "data": {
    "workItemId": "guid",
    "title": "string",
    "description": "string",
    "status": "Draft",
    "createdAtUtc": "2025-01-27T10:00:00Z"
  }
}
```

## Caching Strategy

### Redis Keys
```
workitem:{id} → JSON serialized WorkItemReadDto
TTL: 5 minutes
```

### Cache Invalidation
- Consumer invalidates cache when processing events
- Or: let TTL expire (eventual consistency acceptable for this slice)

## Observability Requirements

### Traces
- API request → Command handler → Repository → Outbox (single trace)
- Outbox dispatcher → RabbitMQ publish (linked trace via correlation ID)
- Consumer → Read model update (linked trace via correlation ID)

### Metrics
- `workitems_created_total` (counter)
- `workitems_query_duration_seconds` (histogram)
- `outbox_messages_pending` (gauge)

### Logs
- Structured JSON
- Include: correlation_id, work_item_id, operation
- No PII in logs

## Security Notes
- JWT bearer authentication with policy-based authorization (`[Authorize(Policy = AuthorizationPolicies.CanCreateWorkItem)]`)
- DevAuthenticationHandler auto-authenticates in Development environment
- Input validation at API edge (FluentValidation)
- Tenant isolation via OrgId claim
- Parameterized queries only

## Implementation Tasks (suggested order)
1. Task 0002: Docker Compose infrastructure
2. Task 0003: Domain layer (WorkItem entity, events)
3. Task 0004: Application layer (commands, queries, handlers)
4. Task 0005: Infrastructure layer (Postgres, repositories, outbox)
5. Task 0006: Presentation layer (API endpoints)
6. Task 0007: Outbox dispatcher service
7. Task 0008: Event consumer and read model
8. Task 0009: Redis caching
9. Task 0010: OpenTelemetry integration
