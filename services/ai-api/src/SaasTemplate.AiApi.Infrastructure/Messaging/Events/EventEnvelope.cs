// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace SaasTemplate.AiApi.Infrastructure.Messaging.Events;

/// <summary>
/// Standard envelope for domain events consumed from the message bus.
/// Property names match the IDomainEvent contract for correct deserialization.
/// </summary>
/// <typeparam name="TData">The type of the event-specific data payload.</typeparam>
public sealed record EventEnvelope<TData> where TData : class
{
    /// <summary>
    /// Unique identifier for this event instance.
    /// Used for idempotency checking.
    /// </summary>
    public required Guid EventId { get; init; }

    /// <summary>
    /// The type/name of the event (e.g., "WorkItemCreatedV1").
    /// Used for routing and deserialization.
    /// </summary>
    public required string EventType { get; init; }

    /// <summary>
    /// UTC timestamp when the event occurred.
    /// </summary>
    public required DateTime OccurredAtUtc { get; init; }

    /// <summary>
    /// Optional correlation ID for request tracing across services.
    /// </summary>
    public Guid? CorrelationId { get; init; }

    /// <summary>
    /// Schema version of the event payload (default: 1).
    /// Used for backward compatibility handling.
    /// </summary>
    public int SchemaVersion { get; init; } = 1;

    /// <summary>
    /// The event-specific data payload.
    /// </summary>
    public required TData Data { get; init; }
}
