// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.PropertiesApi.Domain.Common;

namespace Propely.PropertiesApi.Domain.Properties.Events;

public sealed record PropertyDeletedV1 : IDomainEvent
{
    public Guid EventId { get; }
    public string EventType => nameof(PropertyDeletedV1);
    public int SchemaVersion => 1;
    public DateTime OccurredAtUtc { get; }
    public Guid? CorrelationId { get; init; }
    public Guid? CausationId { get; init; }
    public string Producer => "PropertiesApi";

    public PropertyDeletedV1Data Data { get; }

    public PropertyDeletedV1(Guid propertyId, Guid tenantId, Guid agentId, DateTime occurredAtUtc)
    {
        EventId = Guid.NewGuid();
        OccurredAtUtc = occurredAtUtc;
        Data = new PropertyDeletedV1Data(propertyId, tenantId, agentId);
    }
}

public sealed record PropertyDeletedV1Data(
    Guid PropertyId,
    Guid TenantId,
    Guid AgentId);
