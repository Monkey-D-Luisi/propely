// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Propely.OrgsApi.IntegrationTests.Infrastructure;

public sealed class InMemoryMessagePublisher : IMessagePublisher
{
    private readonly ILogger<InMemoryMessagePublisher> _logger;

    public InMemoryMessagePublisher(
        ILogger<InMemoryMessagePublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync(
        string exchange,
        string routingKey,
        string payload,
        Guid? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("InMemoryMessagePublisher: Received {EventType} on exchange {Exchange}", routingKey, exchange);
        return Task.CompletedTask;
    }
}
