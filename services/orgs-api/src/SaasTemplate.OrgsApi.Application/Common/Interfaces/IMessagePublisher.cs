// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace SaasTemplate.OrgsApi.Application.Common.Interfaces;

/// <summary>
/// Interface for publishing messages to a message broker.
/// </summary>
public interface IMessagePublisher
{
    /// <summary>
    /// Publishes a message to the specified exchange with the given routing key.
    /// </summary>
    /// <param name="exchange">The exchange to publish to.</param>
    /// <param name="routingKey">The routing key for the message.</param>
    /// <param name="payload">The message payload as JSON string.</param>
    /// <param name="correlationId">Optional correlation ID for distributed tracing.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PublishAsync(
        string exchange,
        string routingKey,
        string payload,
        Guid? correlationId = null,
        CancellationToken cancellationToken = default);
}
