// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Domain.Common;

namespace Propely.OrgsApi.Domain.Agencies.Events;

/// <summary>
/// Domain event raised when an agency is created.
/// </summary>
public sealed record AgencyCreatedV1 : IDomainEvent
{
    public Guid EventId { get; }
    public string EventType => nameof(AgencyCreatedV1);
    public int SchemaVersion => 1;
    public DateTime OccurredAtUtc { get; }
    public Guid? CorrelationId { get; init; }
    public Guid? CausationId { get; init; }
    public string Producer => "OrgsApi";

    public AgencyCreatedV1Data Data { get; }

    public AgencyCreatedV1(Guid agencyId, string name, string slug, Guid createdByUserId, DateTime createdAtUtc)
    {
        EventId = Guid.NewGuid();
        OccurredAtUtc = createdAtUtc;
        Data = new AgencyCreatedV1Data(agencyId, name, slug, createdByUserId, createdAtUtc);
    }
}

public sealed record AgencyCreatedV1Data(
    Guid AgencyId,
    string Name,
    string Slug,
    Guid CreatedByUserId,
    DateTime CreatedAtUtc);
