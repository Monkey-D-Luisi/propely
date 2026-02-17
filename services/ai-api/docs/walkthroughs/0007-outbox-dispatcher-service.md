# Walkthrough: 0007-outbox-dispatcher-service

## Task Reference
- Task: `docs/tasks/0007-outbox-dispatcher-service.md`
- Walkthrough: `docs/walkthroughs/0007-outbox-dispatcher-service.md`
- Branch/PR: `main` (direct commit)
- Date: `2026-01-29`

## Summary
Implemented the Outbox Dispatcher Service that polls the outbox table for unprocessed domain events and publishes them to RabbitMQ. This completes the "write path" of the event-driven architecture by ensuring reliable event delivery using the Transactional Outbox pattern.

## Context
- Background: Domain events are already being stored in the outbox table during SaveChangesAsync
- Problem statement: Events need to be published to RabbitMQ for consumers to process
- Constraints: Must handle connection failures gracefully, maintain at-least-once delivery semantics

## Decisions & Trade-offs
- **Decision: Polling vs Push-based**
  - Options considered: PostgreSQL LISTEN/NOTIFY, polling
  - Why this choice: Polling is simpler, proven pattern, works across all databases
  - Consequences: Slight latency (poll interval), but predictable behavior

- **Decision: Batch size for polling**
  - Options considered: Single message, configurable batch
  - Why this choice: Configurable batch (default 100) for efficiency
  - Consequences: Good balance between throughput and memory usage

- **Decision: Error handling strategy**
  - Options considered: Immediate retry, exponential backoff, dead-letter
  - Why this choice: Log and continue for now, dead-letter in future task
  - Consequences: Failed messages remain unprocessed until next poll

- **Decision: RabbitMQ.Client 7.x async API**
  - Options considered: 6.x blocking API, 7.x async API
  - Why this choice: 7.x is current version with fully async API
  - Consequences: Modern async patterns, better scalability

## Implementation Notes
- Key changes:
  - Added `IMessagePublisher` interface in Application layer for abstraction
  - Extended `IOutboxRepository` with query and update methods
  - Implemented `RabbitMqPublisher` with connection management and topic exchange
  - Implemented `OutboxDispatcherService` as a BackgroundService with configurable polling
  - Added configuration classes for RabbitMQ and dispatcher settings
- Edge cases handled:
  - RabbitMQ connection failures (logged, continues polling)
  - Individual message publish failures (logged, other messages still processed)
  - Graceful shutdown with cancellation token
- Known limitations: No dead-letter queue yet, no exponential backoff

## Data / Schema / Migrations
- DB changes: None (outbox table already exists)
- Migration strategy: N/A
- Backward compatibility: N/A

## Commands Run
```bash
dotnet add src/SaasTemplate.AiApi.Infrastructure package RabbitMQ.Client
dotnet add src/SaasTemplate.AiApi.Infrastructure package Microsoft.Extensions.Hosting.Abstractions
dotnet add tests/SaasTemplate.AiApi.IntegrationTests package Testcontainers.RabbitMq
dotnet add tests/SaasTemplate.AiApi.IntegrationTests package Testcontainers.PostgreSql --version 4.10.0
dotnet build
dotnet test
```

## Files Changed
- `src/SaasTemplate.AiApi.Application/Common/Interfaces/IMessagePublisher.cs` — New interface for message publishing
- `src/SaasTemplate.AiApi.Application/Common/Interfaces/IOutboxRepository.cs` — Extended with GetUnprocessedMessagesAsync and UpdateAsync
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Repositories/OutboxRepository.cs` — Implemented new methods
- `src/SaasTemplate.AiApi.Infrastructure/Messaging/RabbitMqPublisher.cs` — RabbitMQ publisher implementation
- `src/SaasTemplate.AiApi.Infrastructure/Messaging/OutboxDispatcherService.cs` — Background service for polling and publishing
- `src/SaasTemplate.AiApi.Infrastructure/Messaging/Configuration/RabbitMqConfiguration.cs` — RabbitMQ connection options
- `src/SaasTemplate.AiApi.Infrastructure/Messaging/Configuration/OutboxDispatcherConfiguration.cs` — Dispatcher options
- `src/SaasTemplate.AiApi.Api/Program.cs` — Service registration
- `src/SaasTemplate.AiApi.Api/appsettings.json` — Configuration sections
- `src/SaasTemplate.AiApi.Api/appsettings.Development.json` — Development configuration
- `tests/SaasTemplate.AiApi.UnitTests/Infrastructure/Messaging/OutboxDispatcherServiceTests.cs` — Unit tests
- `tests/SaasTemplate.AiApi.IntegrationTests/Messaging/OutboxDispatcherTests.cs` — Integration tests
- `tests/SaasTemplate.AiApi.IntegrationTests/Fixtures/RabbitMqFixture.cs` — RabbitMQ testcontainer fixture

## Tests
### Unit
- What was added: 8 unit tests for OutboxDispatcherService
  - Disabled dispatcher should not poll
  - No messages should not publish
  - Messages should publish to correct exchange with routing key = event type
  - Messages should be marked as processed after publish
  - Multiple messages should all be processed
  - Publish failures should continue with next message
  - Batch size configuration should be respected
  - Routing key should match event type
- How to run: `dotnet test --filter "FullyQualifiedName~OutboxDispatcherServiceTests"`

### Integration
- What was added: 7 integration tests with Testcontainers
  - RabbitMQ publish verification
  - Message properties verification (content type, delivery mode, correlation ID)
  - OutboxRepository query and update operations
  - Batch size and ordering behavior
- How to run: `dotnet test --filter "FullyQualifiedName~OutboxDispatcherTests"`

### Manual
- What you verified: N/A (full automated test coverage)
- Steps: Run `./scripts/dev-up.sh`, start the API, create a work item, check RabbitMQ management UI

## Observability
- Logs added/updated:
  - Info: Dispatcher start/stop, connection events, processing summary
  - Debug: Each poll cycle, individual message publish
  - Error: Publish failures with full exception details
- Traces/metrics added/updated: Deferred to Task 0010
- Dashboards/alerts touched: None

## Security
- Validation: RabbitMQ connection validated on first publish attempt
- AuthN/AuthZ impact: None
- Sensitive data handling: Credentials from configuration (appsettings or env vars), not hardcoded

## Performance
- Hot paths impacted: Background polling, not in request path
- Any profiling/bench notes:
  - Poll interval configurable (default 5s)
  - Batch size configurable (default 100)
  - Connection reused across publishes (singleton publisher)

## Docs Updated
- Files updated: This walkthrough, task document
- Anything intentionally left for later: OpenTelemetry integration (Task 0010)

## Rollback Plan
- How to revert safely: Remove `AddHostedService<OutboxDispatcherService>()` from Program.cs
- Data rollback considerations: None needed, messages stay in outbox

## Follow-ups / Backlog
- [ ] Add dead-letter queue handling
- [ ] Add exponential backoff for failed publishes
- [ ] Add metrics for outbox_messages_pending gauge
- [ ] Add health check for RabbitMQ connection

## Checklist
- [x] Task scope matches `docs/tasks/0007-outbox-dispatcher-service.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
