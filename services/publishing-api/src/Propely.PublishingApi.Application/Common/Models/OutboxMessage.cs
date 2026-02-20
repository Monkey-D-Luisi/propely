// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.PublishingApi.Application.Common.Models;

public sealed class OutboxMessage
{
    public Guid Id { get; private set; }
    public string EventType { get; private set; } = null!;
    public string Payload { get; private set; } = null!;
    public DateTime OccurredAtUtc { get; private set; }
    public DateTime? ProcessedAtUtc { get; private set; }
    public Guid? CorrelationId { get; private set; }
    public Guid? CausationId { get; private set; }

    private OutboxMessage() { }

    private OutboxMessage(
        Guid id,
        string eventType,
        string payload,
        DateTime occurredAtUtc,
        Guid? correlationId,
        Guid? causationId)
    {
        Id = id;
        EventType = eventType;
        Payload = payload;
        OccurredAtUtc = occurredAtUtc;
        CorrelationId = correlationId;
        CausationId = causationId;
    }

    public static OutboxMessage Create(
        Guid eventId,
        string eventType,
        string payload,
        DateTime occurredAtUtc,
        Guid? correlationId = null,
        Guid? causationId = null)
    {
        return new OutboxMessage(
            eventId,
            eventType,
            payload,
            occurredAtUtc,
            correlationId,
            causationId);
    }

    public void MarkAsProcessed(DateTime processedAtUtc)
    {
        ProcessedAtUtc = processedAtUtc;
    }
}
