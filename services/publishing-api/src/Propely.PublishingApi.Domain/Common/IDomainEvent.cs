// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.PublishingApi.Domain.Common;

public interface IDomainEvent
{
    Guid EventId { get; }
    string EventType { get; }
    int SchemaVersion { get; }
    DateTime OccurredAtUtc { get; }
    Guid? CorrelationId { get; }
    Guid? CausationId { get; }
    string Producer { get; }
}
