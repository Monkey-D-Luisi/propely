# Task: 0008-event-consumer-read-model

## Metadata
- ID: 0008
- Type: Standard
- Status: DOING
- Owner: Agent
- Created: 2026-01-29
- Related docs:
  - Walkthrough: `docs/walkthroughs/0008-event-consumer-read-model.md`
  - Epic: `docs/backlog/work-item-management-epic.md`

## Goal
Implement a RabbitMQ consumer that updates a read model from domain events, enabling optimized read paths.

## Context
Task 0007 implemented the Outbox Dispatcher that publishes domain events to RabbitMQ. Now we need a consumer to process those events and maintain a denormalized read model for efficient queries.

## Scope
### In scope
- WorkItemProjectorService as BackgroundService
- work_items_read table (EF migration)
- Idempotency check using EventId
- Dead-letter queue for failed messages

### Out of scope
- Redis cache invalidation (deferred to Task 0009)
- Multiple event types (only WorkItemCreatedV1)
- Consumer scaling/partitioning

## Requirements
- R1: Consumer subscribes to queue `projector.workitems` bound to exchange `workitems.events`
- R2: On `WorkItemCreatedV1`: Insert into `work_items_read` table
- R3: Duplicate events ignored using `processed_events` table with EventId
- R4: Failed messages routed to dead-letter queue
- R5: Consumer handles connection failures gracefully

## Acceptance Criteria
- [ ] Consumer receives and processes WorkItemCreatedV1 events
- [ ] Read model table updated correctly with work item data
- [ ] Duplicate events ignored (idempotent via EventId)
- [ ] Integration tests verify end-to-end flow

## Constraints (non-negotiable)
- Clean Architecture layers respected
- English-only repo content
- No secrets in repo
- Update walkthrough

## Proposed Approach
1. Create `work_items_read` and `processed_events` tables via EF migration
2. Implement `WorkItemProjectorService : BackgroundService`
3. Subscribe to queue with dead-letter exchange configuration
4. Process messages with idempotency check before projection

## Implementation Steps
1. Create `WorkItemRead` entity in Infrastructure
2. Create `ProcessedEvent` entity for idempotency
3. Add EF configurations and migration
4. Implement `WorkItemProjectorService`
5. Add integration tests
6. Update DI registration

## Files to Create / Modify
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Entities/WorkItemRead.cs`
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Entities/ProcessedEvent.cs`
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Configurations/WorkItemReadConfiguration.cs`
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Configurations/ProcessedEventConfiguration.cs`
- `src/SaasTemplate.AiApi.Infrastructure/Messaging/WorkItemProjectorService.cs`
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/AppDbContext.cs` (modify)
- `src/SaasTemplate.AiApi.Infrastructure/DependencyInjection.cs` (modify)
- `tests/SaasTemplate.AiApi.IntegrationTests/Messaging/WorkItemProjectorTests.cs`

## Testing Plan
- Unit tests: Message deserialization, idempotency logic
- Integration tests: End-to-end event processing
- Integration tests: Duplicate event handling

## Security & Privacy
- No user credentials in event payloads
- Queue access controlled via RabbitMQ configuration

## Observability
- Logs: Event received, projected, duplicates skipped
- Metrics: (deferred to Task 0010)

## Definition of Done Checklist
- [ ] Acceptance criteria met
- [ ] Build passes
- [ ] Tests added/updated and pass
- [ ] Formatting/analyzers pass
- [ ] No secrets committed
- [ ] Walkthrough updated
