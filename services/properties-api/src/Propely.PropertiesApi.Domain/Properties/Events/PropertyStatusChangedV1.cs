// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.PropertiesApi.Domain.Common;

namespace Propely.PropertiesApi.Domain.Properties.Events;

public sealed record PropertyStatusChangedV1 : IDomainEvent
{
    public Guid EventId { get; }
    public string EventType => nameof(PropertyStatusChangedV1);
    public int SchemaVersion => 1;
    public DateTime OccurredAtUtc { get; }
    public Guid? CorrelationId { get; init; }
    public Guid? CausationId { get; init; }
    public string Producer => "PropertiesApi";

    public PropertyStatusChangedV1Data Data { get; }

    public PropertyStatusChangedV1(Guid propertyId, Guid tenantId, Guid agentId, PropertyStatus previousStatus, PropertyStatus newStatus, DateTime occurredAtUtc)
    {
        EventId = Guid.NewGuid();
        OccurredAtUtc = occurredAtUtc;
        Data = new PropertyStatusChangedV1Data(propertyId, tenantId, agentId, previousStatus, newStatus);
    }
}

public sealed record PropertyStatusChangedV1Data(
    Guid PropertyId,
    Guid TenantId,
    Guid AgentId,
    PropertyStatus PreviousStatus,
    PropertyStatus NewStatus);
