// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Infrastructure.Persistence.Entities;

public sealed class ProcessedWebhookEvent
{
    public string EventId { get; private set; } = string.Empty;
    public string EventType { get; private set; } = string.Empty;
    public DateTime ProcessedAtUtc { get; private set; }

    private ProcessedWebhookEvent() { }

    public static ProcessedWebhookEvent Create(string eventId, string eventType)
    {
        return new ProcessedWebhookEvent
        {
            EventId = eventId,
            EventType = eventType,
            ProcessedAtUtc = DateTime.UtcNow
        };
    }
}
