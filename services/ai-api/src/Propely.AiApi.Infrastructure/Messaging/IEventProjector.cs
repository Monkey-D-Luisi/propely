// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Infrastructure.Messaging;

/// <summary>
/// Interface for projecting domain events to read models.
/// </summary>
public interface IEventProjector
{
    /// <summary>
    /// Gets the event types this projector can handle.
    /// </summary>
    IReadOnlyCollection<string> SupportedEventTypes { get; }

    /// <summary>
    /// Projects an event to the read model.
    /// Unknown event types should be logged but not throw exceptions.
    /// </summary>
    /// <param name="eventType">The type of event (routing key).</param>
    /// <param name="payload">The serialized event payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ProjectAsync(string eventType, string payload, CancellationToken cancellationToken);
}
