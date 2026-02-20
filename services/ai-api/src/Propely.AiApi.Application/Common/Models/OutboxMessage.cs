// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Application.Common.Models;

/// <summary>
/// Represents a message stored in the outbox for reliable event publishing.
/// </summary>
public sealed class OutboxMessage
{
    public Guid Id { get; private set; }
    public string EventType { get; private set; } = null!;
    public string Payload { get; private set; } = null!;
    public DateTime OccurredAtUtc { get; private set; }
    public DateTime? ProcessedAtUtc { get; private set; }
    public Guid? CorrelationId { get; private set; }
    public Guid? CausationId { get; private set; }

    // Required for ORM materialization
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

    /// <summary>
    /// Creates a new outbox message.
    /// </summary>
    /// <param name="eventId">The unique event identifier.</param>
    /// <param name="eventType">The type of event.</param>
    /// <param name="payload">The serialized event payload (JSON).</param>
    /// <param name="occurredAtUtc">When the event occurred.</param>
    /// <param name="correlationId">Optional correlation ID for tracing.</param>
    /// <param name="causationId">Optional causation ID for tracing.</param>
    /// <returns>A new OutboxMessage instance.</returns>
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

    /// <summary>
    /// Marks the message as processed.
    /// </summary>
    /// <param name="processedAtUtc">The timestamp when the message was processed.</param>
    public void MarkAsProcessed(DateTime processedAtUtc)
    {
        ProcessedAtUtc = processedAtUtc;
    }
}
