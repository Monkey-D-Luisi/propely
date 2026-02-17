// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace SaasTemplate.OrgsApi.Domain.Common;

/// <summary>
/// Marker interface for domain events.
/// </summary>
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
