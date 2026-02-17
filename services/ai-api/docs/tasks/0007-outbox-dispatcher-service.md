# Task: 0007-outbox-dispatcher-service

## Metadata
- ID: 0007
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-01-29
- Related docs:
  - Walkthrough: `docs/walkthroughs/0007-outbox-dispatcher-service.md`
  - Epic: `docs/backlog/work-item-management-epic.md`
  - Architecture: `docs/architecture/vertical-slice.md`

## Goal
Implement a background service that publishes outbox messages to RabbitMQ, ensuring reliable event delivery with the Outbox pattern.

## Context
- The outbox table (`outbox_messages`) is already populated when domain events occur (during SaveChangesAsync in AppDbContext)
- Domain events like `WorkItemCreatedV1` are stored as JSON in the outbox table
- RabbitMQ is configured in docker-compose.yml but not yet integrated with the application
- The application needs a background service to poll unprocessed messages and publish them to RabbitMQ

## Scope
### In scope
- OutboxDispatcherService (BackgroundService) that polls the outbox table
- RabbitMqPublisher implementation for publishing messages
- IMessagePublisher interface in Application layer
- Extension of IOutboxRepository with query methods
- RabbitMQ configuration and DI registration
- Unit tests for dispatcher logic
- Integration tests for message flow

### Out of scope
- Event consumers (Task 0008)
- Read model projections (Task 0008)
- Cache invalidation (Task 0009)
- Dead-letter queue handling (simplified for now)

## Requirements
- R1: Create `OutboxDispatcherService : BackgroundService` that polls outbox table at configurable interval
- R2: For each unprocessed message, publish to RabbitMQ exchange `workitems.events` with routing key = event type
- R3: Mark messages as processed after successful publish (set ProcessedAtUtc)
- R4: Create `IRabbitMqPublisher` / `IMessagePublisher` interface in Application layer
- R5: Implement `RabbitMqPublisher` in Infrastructure layer
- R6: Configure RabbitMQ connection from environment variables
- R7: Declare exchange on startup
- R8: Handle failures gracefully with logging (retry logic)

## Acceptance Criteria
- AC1: Dispatcher polls at configured interval (default: 5 seconds)
- AC2: Messages published to correct exchange with routing key = EventType
- AC3: Processed messages not re-published (ProcessedAtUtc is set)
- AC4: Service handles RabbitMQ connection failures gracefully
- AC5: Integration tests verify message flow
- AC6: Unit tests cover dispatcher logic with mocked dependencies

## Constraints (non-negotiable)
- Clean Architecture layers respected (interfaces in Application, implementations in Infrastructure)
- English-only repo content
- No secrets in repo (use environment variables)
- Update walkthrough with implementation details

## Proposed Approach (high-level)
1. Add RabbitMQ.Client NuGet package to Infrastructure project
2. Create IMessagePublisher interface in Application layer
3. Extend IOutboxRepository with GetUnprocessedMessagesAsync and UpdateAsync methods
4. Implement RabbitMqPublisher with connection management and exchange declaration
5. Implement OutboxDispatcherService that:
   - Runs on a timer (configurable poll interval)
   - Fetches batch of unprocessed messages
   - Publishes each to RabbitMQ
   - Marks as processed on success
   - Logs errors but continues processing
6. Register services in DI container
7. Add configuration for RabbitMQ connection string

## Implementation Steps
1. Add RabbitMQ.Client NuGet package
2. Create IMessagePublisher interface
3. Extend IOutboxRepository interface
4. Update OutboxRepository implementation
5. Create RabbitMqConfiguration class
6. Implement RabbitMqPublisher
7. Implement OutboxDispatcherService
8. Register services in Program.cs
9. Write unit tests
10. Write integration tests

## Files to Create / Modify
- `src/SaasTemplate.AiApi.Application/Common/Interfaces/IMessagePublisher.cs` (create)
- `src/SaasTemplate.AiApi.Application/WorkItems/Interfaces/IOutboxRepository.cs` (modify)
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Repositories/OutboxRepository.cs` (modify)
- `src/SaasTemplate.AiApi.Infrastructure/Messaging/OutboxDispatcherService.cs` (create)
- `src/SaasTemplate.AiApi.Infrastructure/Messaging/RabbitMqPublisher.cs` (create)
- `src/SaasTemplate.AiApi.Infrastructure/Messaging/Configuration/RabbitMqConfiguration.cs` (create)
- `src/SaasTemplate.AiApi.Api/Program.cs` (modify)
- `tests/SaasTemplate.AiApi.UnitTests/Infrastructure/Messaging/OutboxDispatcherServiceTests.cs` (create)
- `tests/SaasTemplate.AiApi.IntegrationTests/Messaging/OutboxDispatcherTests.cs` (create)

## Testing Plan
- Unit tests: OutboxDispatcherService with mocked IOutboxRepository and IMessagePublisher
- Integration tests: Create outbox message, verify published to RabbitMQ (Testcontainers)
- Integration tests: Verify message marked as processed after publish
- Manual: Run dev-up.sh, create work item, check RabbitMQ management UI for message

## Security & Privacy
- RabbitMQ credentials from environment variables, not hardcoded
- No PII in outbox message payloads (only IDs and metadata)
- Connection string validation before use

## Observability
- Logs: Info when dispatcher starts/stops, Debug for each poll cycle, Error on publish failures
- Metrics: (deferred to Task 0010) outbox_messages_pending gauge
- Traces: (deferred to Task 0010) correlation ID propagation

## Rollback Plan
- Remove OutboxDispatcherService registration from Program.cs
- Messages remain in outbox (not lost)
- No database changes required

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
