// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.ContactsApi.Infrastructure.Persistence.Entities;

/// <summary>
/// Tracks processed events to ensure idempotent projections.
/// </summary>
public sealed class ProcessedEvent
{
    public Guid EventId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public DateTime ProcessedAtUtc { get; private set; }

    private ProcessedEvent() { }

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
