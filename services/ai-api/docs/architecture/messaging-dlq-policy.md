# Messaging & Dead Letter Queue Policy

## Overview

This document describes the messaging architecture, event envelope format, and dead letter queue (DLQ) policy for the AI API Template.

## Event Envelope Format

All domain events use a standard envelope format for consistency and observability:

```json
{
  "EventId": "550e8400-e29b-41d4-a716-446655440000",
  "EventType": "WorkItemCreatedV1",
  "OccurredAtUtc": "2026-01-31T10:30:00Z",
  "CorrelationId": "abc123de-f456-7890-abcd-ef1234567890",
  "SchemaVersion": 1,
  "Data": {
    // Event-specific payload
  }
}
```

### Envelope Fields

| Field | Required | Description |
|-------|----------|-------------|
| `EventId` | Yes | Unique identifier for idempotency checking |
| `EventType` | Yes | Event type name with version suffix (e.g., `WorkItemCreatedV1`) |
| `OccurredAtUtc` | Yes | UTC timestamp when the event occurred |
| `CorrelationId` | No | Request correlation ID (GUID) for distributed tracing |
| `SchemaVersion` | No | Schema version of the event payload (default: 1) |
| `Data` | Yes | Event-specific data payload |

## Message Flow

```
┌─────────────┐     ┌──────────────────┐     ┌─────────────────┐
│   API       │────>│  Outbox Table    │────>│  RabbitMQ       │
│   (Write)   │     │  (Transactional) │     │  Exchange       │
└─────────────┘     └──────────────────┘     └────────┬────────┘
                                                      │
                           ┌──────────────────────────┼──────────────────────────┐
                           │                          │                          │
                    ┌──────▼──────┐           ┌───────▼───────┐          ┌───────▼───────┐
                    │ Projector   │           │ Other         │          │ Future        │
                    │ Queue       │           │ Consumer      │          │ Consumer      │
                    └──────┬──────┘           └───────────────┘          └───────────────┘
                           │
              ┌────────────┼────────────┐
              │ Success    │ Failure    │
              │            │            │
         ┌────▼────┐  ┌────▼────┐
         │  ACK    │  │  NACK   │
         │ (Done)  │  │ (->DLQ) │
         └─────────┘  └────┬────┘
                           │
                    ┌──────▼──────┐
                    │ Dead Letter │
                    │ Queue (DLQ) │
                    └─────────────┘
```

## Dead Letter Queue Policy

### Configuration

| Setting | Value | Description |
|---------|-------|-------------|
| Main Queue | `projector.workitems` | Primary consumer queue |
| DLX Exchange | `workitems.events.dlx` | Dead letter exchange |
| DLQ | `projector.workitems.dlq` | Dead letter queue |
| Prefetch Count | `10` | Messages prefetched per consumer |

### When Messages Go to DLQ

Messages are routed to the dead letter queue when:

1. **Processing Exception** - Unhandled exception during projection
2. **NACK without Requeue** - Consumer explicitly rejects the message
3. **Queue TTL Exceeded** - Message exceeds queue-level TTL (if configured)

### What Does NOT Go to DLQ

These scenarios result in ACK (message removed from queue):

1. **Unknown Event Type** - Logged as warning, ACK'd to prevent infinite retry
2. **Invalid Payload** - Logged as warning, ACK'd (malformed messages can't be fixed by retry)
3. **Duplicate Event** - Already processed (idempotency check), ACK'd

### Retry Policy

**Current Implementation: No Automatic Retry**

Messages are either:
- Successfully processed → ACK
- Failed with exception → NACK → DLQ

**Rationale:**
- Simple, predictable behavior
- DLQ provides audit trail of failures
- Manual intervention for investigation
- Prevents poison message loops

### DLQ Monitoring & Handling

#### Monitoring

Monitor the DLQ for messages that require investigation:

```bash
# Check DLQ message count via RabbitMQ Management API
# Use credentials from your environment (do NOT use defaults in production)
curl -u $RabbitMQ__Username:$RabbitMQ__Password http://localhost:15672/api/queues/%2F/projector.workitems.dlq

# For local development (credentials from monorepo root .env)
curl -u saastemplate:saastemplate_dev_password http://localhost:15672/api/queues/%2F/projector.workitems.dlq

# Via RabbitMQ CLI
rabbitmqctl list_queues name messages | grep dlq
```

#### Handling DLQ Messages

1. **Investigate** - Examine message payload and logs
2. **Fix Root Cause** - Deploy fix if code issue
3. **Replay** - Manually republish to main queue if recoverable
4. **Purge** - Remove if not recoverable

```bash
# Republish DLQ message to main queue (via shovel or manual)
# This is a manual operation requiring investigation first
```

### Future Enhancements

Potential improvements for production:

1. **Automatic Retry with Backoff** - Use delayed message plugins
2. **Retry Counter Header** - Track retry attempts in message headers
3. **DLQ Alerting** - Alert when DLQ depth exceeds threshold
4. **Automatic Replay** - Scheduled job to retry DLQ messages

## Idempotency

All event handlers must be idempotent:

1. Check `ProcessedEvents` table before processing
2. Record `EventId` after successful processing
3. Use database transaction for atomicity

```csharp
// Example idempotency check
var alreadyProcessed = await dbContext.ProcessedEvents
    .AnyAsync(pe => pe.EventId == envelope.EventId);

if (alreadyProcessed)
{
    _logger.LogDebug("Event {EventId} already processed, skipping", envelope.EventId);
    return;
}

// ... process event ...

var processedEvent = ProcessedEvent.Create(envelope.EventId, envelope.EventType);
dbContext.ProcessedEvents.Add(processedEvent);
await dbContext.SaveChangesAsync();
```

## Observability

### Log Enrichment

All event processing logs include:

- `EventId` - Unique event identifier
- `EventType` - Event type name
- `CorrelationId` - Request correlation (if provided)
- `EventTimestamp` - When event was created
- `EventVersion` - Schema version

### Structured Logging Example

```
[INF] Projected WorkItemCreatedV1 for WorkItem abc123
      EventId=550e8400... EventType=WorkItemCreatedV1 CorrelationId=xyz789
```

## Configuration Reference

> **Note:** The projector configuration is loaded programmatically via `ProjectorConfiguration.cs`, not from `appsettings.json`. The JSON below represents the conceptual settings:

```json
{
  "Projector": {
    "Enabled": true,
    "QueueName": "projector.workitems",
    "DeadLetterExchange": "workitems.events.dlx",
    "DeadLetterQueue": "projector.workitems.dlq",
    "PrefetchCount": 10
  }
}
```
