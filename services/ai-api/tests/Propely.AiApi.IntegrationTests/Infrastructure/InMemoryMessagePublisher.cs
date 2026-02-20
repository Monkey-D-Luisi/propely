// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Application.Common.Interfaces;
using Propely.AiApi.Infrastructure.Messaging;
using Microsoft.Extensions.Logging;

namespace Propely.AiApi.IntegrationTests.Infrastructure;

public sealed class InMemoryMessagePublisher : IMessagePublisher
{
    private readonly IEventProjector _eventProjector;
    private readonly ILogger<InMemoryMessagePublisher> _logger;

    public InMemoryMessagePublisher(
        IEventProjector eventProjector,
        ILogger<InMemoryMessagePublisher> logger)
    {
        _eventProjector = eventProjector;
        _logger = logger;
    }

    public async Task PublishAsync(
        string exchange,
        string routingKey,
        string payload,
        Guid? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("InMemoryMessagePublisher: Dispatching {EventType} directly to Projector", routingKey);

        // Direct dispatch to projector
        // routingKey in RabbitMQ config usually maps to EventType
        await _eventProjector.ProjectAsync(routingKey, payload, cancellationToken);
    }
}
