# Event-Driven Architecture Audit (2026-01-30)

## Scope
- Review event standards (naming, schema versioning, required metadata fields).
- Inspect event definitions and usage in `src/` for envelope compliance.
- Verify outbox pattern implementation when publishing after DB writes.
- Confirm consumer idempotency and retry safety in messaging handlers.
- Review RabbitMQ configuration for reliability considerations.

## Standards Reviewed
- `.agent.md` event-driven baseline (event naming, schema versioning, required metadata fields, outbox, idempotency).

## Findings Summary
| Area | Status | Notes |
| --- | --- | --- |
| Event standards & schema versioning | Compliant | `IDomainEvent` defines required metadata and `WorkItemCreatedV1` follows naming/versioning conventions. |
| Event envelope compliance in payloads | Partially compliant | Event payload includes required metadata, but the consumer DTO ignores `SchemaVersion`, `Producer`, `OccurredAtUtc`, `CorrelationId`, and `CausationId`. |
| Outbox pattern after DB writes | Compliant | Domain events are persisted to the outbox in `SaveChangesAsync`, with dispatcher publishing and marking processed. |
| Consumer idempotency & retry safety | Partially compliant | Idempotency enforced via `processed_events` table, but failures are sent to DLQ without retry/backoff. |
| RabbitMQ reliability configuration | Partially compliant | Durable exchange/queues and persistent delivery are configured, but publisher confirms/mandatory routing are not enabled. |

## Detailed Findings
### 1) Event Standards (Naming, Schema Versioning, Required Metadata)
- `IDomainEvent` declares required metadata (`EventId`, `EventType`, `SchemaVersion`, `OccurredAtUtc`, `CorrelationId`, `CausationId`, `Producer`).
- `WorkItemCreatedV1` follows the `SomethingHappenedV1` naming convention and includes the required metadata fields.

### 2) Event Envelope Compliance in `src/`
- Event payloads are serialized from domain events into the outbox, so the payload includes the required metadata fields.
- The consumer DTO (`WorkItemCreatedEventData`) only captures `EventId`, `EventType`, and `Data`; it does not validate or use the required metadata (`SchemaVersion`, `Producer`, `OccurredAtUtc`, `CorrelationId`, `CausationId`).
- This means the published envelope is compliant, but the consumer does not enforce or surface required metadata for downstream tracing and version safety.

### 3) Outbox Pattern Implementation
- `AppDbContext.SaveChangesAsync` collects domain events, writes them to the outbox, and saves them in the same transaction.
- `OutboxDispatcherService` polls, publishes to RabbitMQ, and marks messages as processed, with `FOR UPDATE SKIP LOCKED` used to avoid duplicate publishing in scaled deployments.
- The dispatcher acknowledges that publish-success but update-failure can cause duplicate delivery, matching at-least-once semantics.

### 4) Consumer Idempotency & Retry Safety
- `WorkItemProjectorService` checks `processed_events` for `EventId` and records processed events to enforce idempotency.
- Messages are ACKed after successful processing and DB write; failures are NACKed without requeue and sent to the DLQ.
- There is no automatic retry/backoff for transient failures; operators must monitor the DLQ and reprocess as needed.

### 5) RabbitMQ Reliability Considerations
- Publisher uses persistent delivery mode and declares a durable topic exchange.
- Consumer uses manual ACKs, prefetch, and configures a durable DLQ.
- Publisher confirms and mandatory routing are not enabled, so unroutable publishes or broker-side failures are not explicitly detected at publish time.

## Recommendations
1. Expand consumer DTOs to include `SchemaVersion`, `Producer`, `OccurredAtUtc`, `CorrelationId`, and `CausationId`, and log or validate them for traceability and version safety.
2. Consider enabling publisher confirms (and/or mandatory routing) to detect broker-side publish failures.
3. Define a retry policy for transient consumer failures before DLQ routing (e.g., delayed retries with backoff), or document the operational runbook for DLQ reprocessing.

## Evidence
- Event standard and required metadata definitions: `.agent.md`.
- Domain event envelope: `src/SaasTemplate.AiApi.Domain/Common/IDomainEvent.cs`, `src/SaasTemplate.AiApi.Domain/WorkItems/Events/WorkItemCreatedV1.cs`.
- Outbox persistence and dispatcher: `src/SaasTemplate.AiApi.Infrastructure/Persistence/AppDbContext.cs`, `src/SaasTemplate.AiApi.Infrastructure/Persistence/Repositories/OutboxRepository.cs`, `src/SaasTemplate.AiApi.Infrastructure/Messaging/OutboxDispatcherService.cs`.
- Consumer idempotency and DLQ handling: `src/SaasTemplate.AiApi.Infrastructure/Messaging/WorkItemProjectorService.cs`, `src/SaasTemplate.AiApi.Infrastructure/Persistence/Entities/ProcessedEvent.cs`.
- RabbitMQ publisher configuration: `src/SaasTemplate.AiApi.Infrastructure/Messaging/RabbitMqPublisher.cs`.
