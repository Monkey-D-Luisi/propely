// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.PropertiesApi.Domain.Common;

namespace Propely.PropertiesApi.Domain.Properties.Events;

public sealed record PropertyCreatedV1 : IDomainEvent
{
    public Guid EventId { get; }
    public string EventType => nameof(PropertyCreatedV1);
    public int SchemaVersion => 1;
    public DateTime OccurredAtUtc { get; }
    public Guid? CorrelationId { get; init; }
    public Guid? CausationId { get; init; }
    public string Producer => "PropertiesApi";

    public PropertyCreatedV1Data Data { get; }

    public PropertyCreatedV1(Guid propertyId, Guid tenantId, Guid agentId, string title, PropertyType propertyType, OperationType operationType, DateTime occurredAtUtc)
    {
        EventId = Guid.NewGuid();
        OccurredAtUtc = occurredAtUtc;
        Data = new PropertyCreatedV1Data(propertyId, tenantId, agentId, title, propertyType, operationType);
    }
}

public sealed record PropertyCreatedV1Data(
    Guid PropertyId,
    Guid TenantId,
    Guid AgentId,
    string Title,
    PropertyType PropertyType,
    OperationType OperationType);
