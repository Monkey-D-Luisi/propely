// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.PublishingApi.Application.Common.Interfaces;

public interface IMessagePublisher
{
    Task PublishAsync(
        string exchange,
        string routingKey,
        string payload,
        Guid? correlationId = null,
        CancellationToken cancellationToken = default);
}
