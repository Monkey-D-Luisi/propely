# Event-Driven Architecture Audit (2026-01-31)

## Scope
- Review event standards (naming, schema versioning, required metadata fields).
- Inspect event definitions and usage in `src/` for required metadata fields (`EventId`, `OccurredAtUtc`, `CorrelationId`, `CausationId`, `Producer`, `SchemaVersion`).
- Verify outbox pattern implementation for DB-write → publish flows.
- Confirm consumer idempotency handling and retry safety.
- Validate RabbitMQ configuration and routing conventions.

## Standards Reviewed
- `.agent.md` event-driven baseline (event naming, schema versioning, required metadata fields, outbox, idempotency).

## Findings Summary
| Area | Status | Notes |
| --- | --- | --- |
| Event standards & schema versioning | Compliant | `IDomainEvent` enforces required metadata and `WorkItemCreatedV1` follows naming/versioning conventions. |
| Required metadata presence in event payloads | Partially compliant | Domain events include required metadata, but the consumer envelope ignores `CausationId` and `Producer` in the payload contract. |
| Outbox pattern after DB writes | Compliant | Domain events are persisted to the outbox in `SaveChangesAsync`, with dispatcher publishing and marking processed. |
| Consumer idempotency & retry safety | Partially compliant | Idempotency is enforced via `processed_events`, but failures are NACKed to DLQ without retry/backoff. |
| RabbitMQ configuration & routing conventions | Compliant | Work item events publish to `workitems.events` exchange with routing key = event type; consumer binds queue with `#` routing. |

## Detailed Findings
### 1) Event Standards (Naming, Schema Versioning, Required Metadata)
- `.agent.md` mandates event naming like `SomethingHappenedV1`, versioning by suffix, and required metadata (`EventId`, `OccurredAtUtc`, `CorrelationId`, `CausationId`, `Producer`, `SchemaVersion`).
- `IDomainEvent` defines these required fields and `WorkItemCreatedV1` implements them with `SchemaVersion = 1` and `Producer = "WorkItemApi"`, matching the standard.

### 2) Required Metadata Presence in Event Payloads
- Domain events are serialized directly into the outbox payload, which includes the required metadata fields defined on `IDomainEvent`.
- The consumer uses `EventEnvelope<T>` and `WorkItemCreatedPayload`, which capture `EventId`, `OccurredAtUtc`, `CorrelationId`, and `SchemaVersion`, but the envelope omits `CausationId` and `Producer` entirely. This means those fields exist in the payload but are not deserialized or logged by the consumer.

### 3) Outbox Pattern Implementation (DB-write → publish)
- `AppDbContext.SaveChangesAsync` collects domain events, writes them to the outbox, and saves them in the same transaction.
- `OutboxDispatcherService` publishes outbox messages to RabbitMQ and marks them as processed, acknowledging that failures after publish can yield at-least-once delivery semantics.
- `OutboxRepository.GetUnprocessedMessagesAsync` uses `FOR UPDATE SKIP LOCKED` to avoid duplicate publishing across multiple dispatchers.

### 4) Consumer Idempotency & Retry Safety
- `WorkItemEventProjector` enforces idempotency by checking `processed_events` for `EventId` before writing, and records successful processing.
- `WorkItemProjectorService` ACKs on success and NACKs with `requeue: false` on errors, pushing failures directly to the DLQ without a retry policy.

### 5) RabbitMQ Configuration & Routing Conventions
- Publisher declares a durable topic exchange using `WorkItemsExchange` (default `workitems.events`) and publishes with routing key = `EventType`.
- The projector declares a durable queue and binds with routing key `#` so it receives all work item events.
- Prefetch and DLQ configuration are in place to control throughput and handle failed messages.

## Remediation Steps
1. **Consumer metadata completeness**: Extend `EventEnvelope<T>` (and logging scope) to include `CausationId` and `Producer`, then validate or log them for traceability.
2. **Retry policy before DLQ**: Add a retry strategy (e.g., delayed retries with backoff or a retry queue) for transient failures before dead-lettering.
3. **Document routing conventions**: Add a short section in operational docs describing the exchange name, routing key convention (event type), and queue binding pattern for new consumers.

## Evidence
- Event standards: `.agent.md`.
- Required metadata contract: `src/SaasTemplate.AiApi.Domain/Common/IDomainEvent.cs`.
- Example event implementation: `src/SaasTemplate.AiApi.Domain/WorkItems/Events/WorkItemCreatedV1.cs`.
- Outbox persistence and transaction boundary: `src/SaasTemplate.AiApi.Infrastructure/Persistence/AppDbContext.cs`.
- Outbox dispatcher: `src/SaasTemplate.AiApi.Infrastructure/Messaging/OutboxDispatcherService.cs`.
- Outbox repository `SKIP LOCKED` behavior: `src/SaasTemplate.AiApi.Infrastructure/Persistence/Repositories/OutboxRepository.cs`.
- Consumer idempotency and DLQ handling: `src/SaasTemplate.AiApi.Infrastructure/Messaging/WorkItemEventProjector.cs`, `src/SaasTemplate.AiApi.Infrastructure/Messaging/WorkItemProjectorService.cs`.
- RabbitMQ publisher and routing conventions: `src/SaasTemplate.AiApi.Infrastructure/Messaging/RabbitMqPublisher.cs`, `src/SaasTemplate.AiApi.Infrastructure/Messaging/Configuration/RabbitMqConfiguration.cs`.
