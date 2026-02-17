# Epic: Work Item Management

## Overview

This epic implements the first vertical slice of the application: a complete Work Item management feature that traverses all architectural layers (Domain, Application, Infrastructure, Presentation).

### Business Goal
Enable users to create and retrieve Work Items through a REST API, with full observability, caching, and event-driven architecture.

### Technical Goals
- Demonstrate Clean Architecture with strict layer separation
- Implement CQRS pattern (Commands for writes, Queries for reads)
- Use Outbox pattern for reliable event publishing
- Integrate Redis caching for read optimization
- Full OpenTelemetry instrumentation

## Architecture Reference
See [docs/architecture/vertical-slice.md](../architecture/vertical-slice.md) for detailed technical design.

---

## Tasks

### Task 0002 — Docker Compose & Dev Scripts
**Status:** DONE
**Dependencies:** None

#### Goal
Set up the local development environment with Docker Compose for all infrastructure services.

#### Requirements
- Create `docker-compose.yml` with services:
  - PostgreSQL on port `5432`
  - RabbitMQ on ports `5672` (AMQP) and `15672` (Management UI)
  - Redis on port `6379`
- Create `.env.example` with placeholder credentials (never real secrets)
- Update `scripts/dev-up.sh` to run `docker compose up -d`
- Update `scripts/dev-down.sh` to run `docker compose down`
- Update `scripts/dev-reset.sh` to run `docker compose down -v` with confirmation prompt

#### Acceptance Criteria
- [ ] `./scripts/dev-up.sh` starts all services successfully
- [ ] `./scripts/dev-down.sh` stops services without data loss
- [ ] `./scripts/dev-reset.sh` destroys volumes (with user confirmation)
- [ ] `.env.example` committed with `CHANGEME` placeholders
- [ ] `.env` is in `.gitignore` (already should be)
- [ ] Services are accessible at documented ports

#### Files to Create/Modify
- `docker-compose.yml` (create)
- `.env.example` (create)
- `scripts/dev-up.sh` (modify)
- `scripts/dev-down.sh` (modify)
- `scripts/dev-reset.sh` (modify)

#### Testing Plan
- Manual: Run `dev-up.sh`, verify services with `docker ps`
- Manual: Connect to Postgres via psql or GUI
- Manual: Access RabbitMQ management at http://localhost:15672
- Manual: Connect to Redis via redis-cli

---

### Task 0003 — WorkItem Domain Model
**Status:** DONE
**Dependencies:** 0002

#### Goal
Define the WorkItem entity and related domain types in the Domain layer.

#### Requirements
- Create `WorkItem` entity with:
  - `Id`: Guid (identity)
  - `Title`: string (required, 1-200 characters)
  - `Description`: string (optional, max 2000 characters)
  - `Status`: WorkItemStatus enum
  - `CreatedAtUtc`: DateTime
  - `UpdatedAtUtc`: DateTime
  - `Version`: int (for optimistic concurrency)
- Create `WorkItemStatus` enum: `Draft`, `Active`, `Completed`
- Create `WorkItemCreatedV1` domain event with standard fields:
  - `EventId`, `EventType`, `SchemaVersion`, `OccurredAtUtc`
  - `CorrelationId`, `CausationId`, `Producer`
  - `Data`: { workItemId, title, description, status, createdAtUtc }
- Implement domain validation (Title not empty, within length limits)
- No external dependencies in Domain layer

#### Acceptance Criteria
- [ ] WorkItem entity enforces invariants (Title validation)
- [ ] WorkItemStatus enum has exactly 3 values
- [ ] WorkItemCreatedV1 follows event schema from vertical-slice.md
- [ ] Domain layer has zero framework dependencies
- [ ] Unit tests cover validation logic

#### Files to Create/Modify
- `src/SaasTemplate.AiApi.Domain/WorkItems/WorkItem.cs`
- `src/SaasTemplate.AiApi.Domain/WorkItems/WorkItemStatus.cs`
- `src/SaasTemplate.AiApi.Domain/WorkItems/Events/WorkItemCreatedV1.cs`
- `src/SaasTemplate.AiApi.Domain/WorkItems/Exceptions/WorkItemValidationException.cs`
- `tests/SaasTemplate.AiApi.UnitTests/Domain/WorkItems/WorkItemTests.cs`

#### Testing Plan
- Unit tests: WorkItem creation with valid/invalid titles
- Unit tests: WorkItemCreatedV1 event construction
- Unit tests: Status enum values

---

### Task 0004 — Application Layer (Commands, Queries, Interfaces)
**Status:** DONE
**Dependencies:** 0003

#### Goal
Define use cases and interfaces in the Application layer using CQRS pattern.

#### Requirements
- Create `IWorkItemRepository` interface:
  - `Task AddAsync(WorkItem workItem, CancellationToken ct)`
  - `Task<WorkItem?> GetByIdAsync(Guid id, CancellationToken ct)`
- Create `IOutboxRepository` interface:
  - `Task AddAsync(OutboxMessage message, CancellationToken ct)`
- Create `IUnitOfWork` interface for transaction management
- Create `CreateWorkItemCommand` with Title and Description
- Create `CreateWorkItemCommandHandler`:
  - Validates input
  - Creates WorkItem entity
  - Persists via repository
  - Adds WorkItemCreatedV1 to outbox
  - Returns created work item ID
- Create `GetWorkItemByIdQuery` with Id parameter
- Create `GetWorkItemByIdQueryHandler`:
  - Returns WorkItemDto or null
- Create DTOs: `WorkItemDto`, `CreateWorkItemResult`

#### Acceptance Criteria
- [ ] Interfaces defined without infrastructure dependencies
- [ ] Command handler creates entity and outbox message
- [ ] Query handler returns DTO, not entity
- [ ] All methods use CancellationToken
- [ ] Unit tests with mocked repositories

#### Files to Create/Modify
- `src/SaasTemplate.AiApi.Application/WorkItems/Interfaces/IWorkItemRepository.cs`
- `src/SaasTemplate.AiApi.Application/WorkItems/Interfaces/IOutboxRepository.cs`
- `src/SaasTemplate.AiApi.Application/Common/Interfaces/IUnitOfWork.cs`
- `src/SaasTemplate.AiApi.Application/WorkItems/Commands/CreateWorkItemCommand.cs`
- `src/SaasTemplate.AiApi.Application/WorkItems/Commands/CreateWorkItemCommandHandler.cs`
- `src/SaasTemplate.AiApi.Application/WorkItems/Queries/GetWorkItemByIdQuery.cs`
- `src/SaasTemplate.AiApi.Application/WorkItems/Queries/GetWorkItemByIdQueryHandler.cs`
- `src/SaasTemplate.AiApi.Application/WorkItems/Dtos/WorkItemDto.cs`
- `tests/SaasTemplate.AiApi.UnitTests/Application/WorkItems/CreateWorkItemCommandHandlerTests.cs`
- `tests/SaasTemplate.AiApi.UnitTests/Application/WorkItems/GetWorkItemByIdQueryHandlerTests.cs`

#### Testing Plan
- Unit tests: CreateWorkItemCommandHandler with mock repository
- Unit tests: GetWorkItemByIdQueryHandler with mock repository
- Unit tests: Verify outbox message is created on command

---

### Task 0005 — Infrastructure Layer (Persistence, EF Core, Outbox)
**Status:** DONE
**Dependencies:** 0004

#### Goal
Implement persistence using EF Core with PostgreSQL.

#### Requirements
- Create `AppDbContext` with:
  - `DbSet<WorkItem> WorkItems`
  - `DbSet<OutboxMessage> OutboxMessages`
  - Entity configurations (snake_case naming)
- Implement `WorkItemRepository : IWorkItemRepository`
- Implement `OutboxRepository : IOutboxRepository`
- Implement `UnitOfWork : IUnitOfWork`
- Create `OutboxMessage` entity:
  - Id, EventType, Payload (JSONB), OccurredAtUtc, ProcessedAtUtc, CorrelationId, CausationId
- Create EF Core migrations for:
  - `work_items` table
  - `outbox_messages` table
- Configure connection string from environment variable

#### Acceptance Criteria
- [ ] Database schema matches vertical-slice.md exactly
- [ ] snake_case naming for all DB identifiers
- [ ] Migrations can be applied to empty database
- [ ] Integration tests with Testcontainers (Postgres)
- [ ] Repository correctly persists and retrieves WorkItem

#### Files to Create/Modify
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/AppDbContext.cs`
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Configurations/WorkItemConfiguration.cs`
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Configurations/OutboxMessageConfiguration.cs`
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Repositories/WorkItemRepository.cs`
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Repositories/OutboxRepository.cs`
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/UnitOfWork.cs`
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Entities/OutboxMessage.cs`
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Migrations/*.cs`
- `tests/SaasTemplate.AiApi.IntegrationTests/Persistence/WorkItemRepositoryTests.cs`

#### Testing Plan
- Integration tests: Repository CRUD with real Postgres (Testcontainers)
- Integration tests: Verify snake_case column names
- Integration tests: Outbox message persistence

---

### Task 0006 — Presentation Layer (API Endpoints)
**Status:** DONE
**Dependencies:** 0005

**Note:** Authorization policies deferred to future task. Current implementation allows anonymous access (dev mode only).

#### Goal
Expose REST API endpoints for Work Item operations.

#### Requirements
- Create `POST /v1/work-items` endpoint:
  - Request body: `{ "title": "string", "description": "string?" }`
  - Response 201: `{ "id": "guid", "title": "string", "status": "Draft", "createdAtUtc": "datetime" }`
  - FluentValidation: Title required (1-200 chars), Description optional (max 2000)
- Create `GET /v1/work-items/{id}` endpoint:
  - Response 200: Full WorkItem DTO
  - Response 404: If not found
- ~~Configure authorization policies~~ (deferred - see Task 0006 scope)
- Add security headers middleware
- Add rate limiting (100 req/min default)

#### Acceptance Criteria
- [ ] POST returns 201 with created resource
- [ ] POST returns 400 on validation failure
- [ ] GET returns 200 with correct data
- [ ] GET returns 404 for non-existent ID
- [ ] Authorization policies are applied
- [ ] Rate limiting is active
- [ ] Security headers present in responses

#### Files to Create/Modify
- `src/SaasTemplate.AiApi.Api/Controllers/WorkItemsController.cs`
- `src/SaasTemplate.AiApi.Api/Dtos/CreateWorkItemRequest.cs`
- `src/SaasTemplate.AiApi.Api/Dtos/WorkItemResponse.cs`
- `src/SaasTemplate.AiApi.Api/Validators/CreateWorkItemRequestValidator.cs`
- `src/SaasTemplate.AiApi.Api/Configuration/AuthorizationConfiguration.cs`
- `src/SaasTemplate.AiApi.Api/Configuration/SecurityConfiguration.cs`
- `src/SaasTemplate.AiApi.Api/Program.cs` (modify)
- `tests/SaasTemplate.AiApi.IntegrationTests/Api/WorkItemsControllerTests.cs`

#### Testing Plan
- Integration tests: POST with valid/invalid data
- Integration tests: GET existing/non-existing item
- Integration tests: Verify response structure
- Manual: Check security headers with curl

---

### Task 0007 — Outbox Dispatcher Service
**Status:** DONE
**Dependencies:** 0006

#### Goal
Implement background service that publishes outbox messages to RabbitMQ.

#### Requirements
- Create `OutboxDispatcherService : BackgroundService`
  - Poll outbox table every N seconds (configurable)
  - For each unprocessed message:
    - Publish to RabbitMQ exchange `workitems.events`
    - Routing key = event type (e.g., `WorkItemCreatedV1`)
    - Mark message as processed (set ProcessedAtUtc)
  - Handle failures gracefully (retry logic)
- Create `IRabbitMqPublisher` interface in Application
- Implement `RabbitMqPublisher` in Infrastructure
- Configure RabbitMQ connection from environment variables
- Declare exchange on startup

#### Acceptance Criteria
- [ ] Dispatcher polls at configured interval
- [ ] Messages published to correct exchange/routing key
- [ ] Processed messages not re-published
- [ ] Service handles RabbitMQ connection failures
- [ ] Integration tests verify message flow

#### Files to Create/Modify
- `src/SaasTemplate.AiApi.Infrastructure/Messaging/OutboxDispatcherService.cs`
- `src/SaasTemplate.AiApi.Infrastructure/Messaging/RabbitMqPublisher.cs`
- `src/SaasTemplate.AiApi.Application/Common/Interfaces/IMessagePublisher.cs`
- `src/SaasTemplate.AiApi.Infrastructure/Messaging/Configuration/RabbitMqConfiguration.cs`
- `tests/SaasTemplate.AiApi.IntegrationTests/Messaging/OutboxDispatcherTests.cs`

#### Testing Plan
- Integration tests: Create outbox message, verify published to RabbitMQ
- Integration tests: Verify message marked as processed
- Unit tests: Dispatcher logic with mocked publisher

---

### Task 0008 — Event Consumer & Read Model Projection
**Status:** DONE
**Dependencies:** 0007

#### Goal
Implement consumer that updates read model from domain events.

#### Requirements
- Create `WorkItemProjectorService : BackgroundService`
  - Subscribe to queue `projector.workitems`
  - Bind to exchange `workitems.events`
  - On `WorkItemCreatedV1`: Insert into `work_items_read` table
- Create `work_items_read` table (EF migration)
- Implement idempotency check using EventId
- Invalidate Redis cache on projection update
- Handle consumer failures with dead-letter queue

#### Acceptance Criteria
- [ ] Consumer receives and processes events
- [ ] Read model table updated correctly
- [ ] Duplicate events ignored (idempotent)
- [ ] Cache invalidated after projection
- [ ] Integration tests verify end-to-end flow

#### Files to Create/Modify
- `src/SaasTemplate.AiApi.Infrastructure/Messaging/WorkItemProjectorService.cs`
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Configurations/WorkItemReadConfiguration.cs`
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Entities/WorkItemRead.cs`
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Migrations/*_AddWorkItemsReadTable.cs`
- `tests/SaasTemplate.AiApi.IntegrationTests/Messaging/WorkItemProjectorTests.cs`

#### Testing Plan
- Integration tests: Publish event, verify read model updated
- Integration tests: Duplicate event processing
- Integration tests: Cache invalidation

---

### Task 0009 — Redis Caching Integration
**Status:** DONE
**Dependencies:** 0008

#### Goal
Add Redis caching to the read path for improved performance.

#### Requirements
- Create `ICacheService` interface in Application
- Implement `RedisCacheService` in Infrastructure
- Modify `GetWorkItemByIdQueryHandler`:
  - Check cache first: key `workitem:{id}`
  - On miss: query DB, cache result with 5-minute TTL
  - Return cached or fresh data
- Configure Redis connection from environment variable
- Add cache invalidation in projector (Task 0008)

#### Acceptance Criteria
- [ ] Cache hit returns data without DB query
- [ ] Cache miss queries DB and populates cache
- [ ] TTL is 5 minutes
- [ ] Cache keys follow naming convention
- [ ] Projector invalidates cache on event

#### Files to Create/Modify
- `src/SaasTemplate.AiApi.Application/Common/Interfaces/ICacheService.cs`
- `src/SaasTemplate.AiApi.Infrastructure/Caching/RedisCacheService.cs`
- `src/SaasTemplate.AiApi.Infrastructure/Caching/Configuration/RedisConfiguration.cs`
- `src/SaasTemplate.AiApi.Application/WorkItems/Queries/GetWorkItemByIdQueryHandler.cs` (modify)
- `tests/SaasTemplate.AiApi.IntegrationTests/Caching/RedisCacheServiceTests.cs`

#### Testing Plan
- Integration tests: Cache hit scenario
- Integration tests: Cache miss and population
- Integration tests: TTL expiration
- Unit tests: Handler with mocked cache

---

### Task 0010 — OpenTelemetry Integration
**Status:** DONE
**Dependencies:** 0009

#### Goal
Add distributed tracing, metrics, and health endpoints.

#### Requirements
- Configure OpenTelemetry for:
  - Traces: API requests, DB queries, RabbitMQ operations
  - Metrics: Custom counters and histograms
- Add custom metrics:
  - `workitems_created_total` (counter)
  - `workitems_query_duration_seconds` (histogram)
  - `outbox_messages_pending` (gauge)
- Propagate correlation IDs across all operations
- Add health endpoints:
  - `/health/live` — Process liveness
  - `/health/ready` — Dependencies (DB, RabbitMQ, Redis)
- Configure structured logging with correlation IDs

#### Acceptance Criteria
- [ ] Traces visible in collector/dashboard
- [ ] Metrics exported correctly
- [ ] Correlation ID propagated across services
- [ ] Health endpoints return correct status
- [ ] Logs include correlation ID

#### Files to Create/Modify
- `src/SaasTemplate.AiApi.Api/Configuration/TelemetryConfiguration.cs`
- `src/SaasTemplate.AiApi.Api/Configuration/HealthChecksConfiguration.cs`
- `src/SaasTemplate.AiApi.Infrastructure/Telemetry/Metrics/WorkItemMetrics.cs`
- `src/SaasTemplate.AiApi.Api/Program.cs` (modify)
- `tests/SaasTemplate.AiApi.IntegrationTests/Health/HealthEndpointTests.cs`

#### Testing Plan
- Integration tests: Health endpoints
- Manual: Verify traces in collector
- Manual: Verify metrics exported
- Unit tests: Metrics recording

---

## Summary

| Task | Name | Status | Dependencies |
|------|------|--------|--------------|
| 0002 | Docker Compose & Dev Scripts | DONE | - |
| 0003 | WorkItem Domain Model | DONE | 0002 |
| 0004 | Application Layer | DONE | 0003 |
| 0005 | Infrastructure Layer | DONE | 0004 |
| 0006 | Presentation Layer (API) | DONE | 0005 |
| 0007 | Outbox Dispatcher Service | DONE | 0006 |
| 0008 | Event Consumer & Read Model | DONE | 0007 |
| 0009 | Redis Caching Integration | DONE | 0008 |
| 0010 | OpenTelemetry Integration | DONE | 0009 |

## Future Epics (Not in Scope)
- User authentication with external IdP
- Work Item updates and deletion
- Listing and filtering Work Items
- AI Client integration for content analysis
