// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Infrastructure.Persistence.Entities;

/// <summary>
/// Tracks processed events to ensure idempotent projections.
/// </summary>
public sealed class ProcessedEvent
{
    /// <summary>
    /// The unique event ID from the domain event.
    /// </summary>
    public Guid EventId { get; private set; }

    /// <summary>
    /// The type of event that was processed.
    /// </summary>
    public string EventType { get; private set; } = string.Empty;

    /// <summary>
    /// UTC timestamp when the event was processed.
    /// </summary>
    public DateTime ProcessedAtUtc { get; private set; }

    private ProcessedEvent() { }

    /// <summary>
    /// Creates a new processed event record.
    /// </summary>
    public static ProcessedEvent Create(Guid eventId, string eventType)
    {
        return new ProcessedEvent
        {
            EventId = eventId,
            EventType = eventType,
            ProcessedAtUtc = DateTime.UtcNow
        };
    }
}
