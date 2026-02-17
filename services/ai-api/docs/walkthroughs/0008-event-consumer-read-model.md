# Walkthrough: 0008-event-consumer-read-model

## Task Reference
- Task: `docs/tasks/0008-event-consumer-read-model.md`
- Walkthrough: `docs/walkthroughs/0008-event-consumer-read-model.md`
- Branch/PR: `main` (direct commit)
- Date: 2026-01-29

## Summary
Implemented a RabbitMQ consumer (`WorkItemProjectorService`) that subscribes to domain events and maintains a denormalized read model for optimized queries. Added `work_items_read` and `processed_events` tables with idempotency support.

## Context
- Background: Task 0007 implemented the Outbox Dispatcher that publishes domain events to RabbitMQ
- Problem statement: Events need to be consumed and projected to a read model for efficient queries
- Constraints: Must be idempotent to handle duplicate event delivery (at-least-once semantics)

## Decisions & Trade-offs
- **Decision:** Used `processed_events` table for idempotency instead of in-memory tracking
  - Options: In-memory deduplication, distributed cache, database table
  - Why this choice: Database provides durability and works across service restarts
  - Consequences: Slight overhead per event, but guarantees idempotency

- **Decision:** Configured dead-letter queue for failed message handling
  - Why: Prevents poison messages from blocking the queue
  - Benefit: Failed messages can be inspected and replayed manually

## Implementation Notes
- WorkItemProjectorService subscribes to `projector.workitems` queue bound to `workitems.events` exchange
- Uses `#` routing key to receive all event types
- Deserializes `WorkItemCreatedV1` events and projects to `work_items_read` table
- Records processed events to prevent duplicate projections

## Data / Schema / Migrations
- New tables:
  - `work_items_read`: Denormalized read model with id, title, description, status, created_at_utc, last_projected_at_utc
  - `processed_events`: Idempotency tracking with event_id (PK), event_type, processed_at_utc
- Migration: `20260129110000_AddWorkItemsReadAndProcessedEvents.cs`

## Commands Run
```bash
dotnet build
dotnet test tests/SaasTemplate.AiApi.UnitTests --no-build
```

## Files Changed
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Entities/WorkItemRead.cs` — Read model entity
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Entities/ProcessedEvent.cs` — Idempotency tracking entity
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Configurations/WorkItemReadConfiguration.cs` — EF config
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Configurations/ProcessedEventConfiguration.cs` — EF config
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/AppDbContext.cs` — Added new DbSets
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Migrations/*` — New migration files
- `src/SaasTemplate.AiApi.Infrastructure/Messaging/Configuration/ProjectorConfiguration.cs` — Config class
- `src/SaasTemplate.AiApi.Infrastructure/Messaging/WorkItemProjectorService.cs` — RabbitMQ consumer
- `src/SaasTemplate.AiApi.Api/Program.cs` — DI registration
- `tests/SaasTemplate.AiApi.IntegrationTests/Messaging/WorkItemProjectorTests.cs` — Integration tests

## Tests
### Unit
- All 45 existing unit tests pass

### Integration
- `WorkItemProjectorTests.cs` — Tests for read model, idempotency, and RabbitMQ infrastructure

## Checklist
- [x] Task scope matches `docs/tasks/0008-event-consumer-read-model.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed
