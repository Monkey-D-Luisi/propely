// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Domain.Common;

namespace Propely.OrgsApi.Domain.Agencies.Events;

/// <summary>
/// Domain event raised when a branch (organization) is removed from an agency.
/// </summary>
public sealed record BranchRemovedFromAgencyV1 : IDomainEvent
{
    public Guid EventId { get; }
    public string EventType => nameof(BranchRemovedFromAgencyV1);
    public int SchemaVersion => 1;
    public DateTime OccurredAtUtc { get; }
    public Guid? CorrelationId { get; init; }
    public Guid? CausationId { get; init; }
    public string Producer => "OrgsApi";

    public BranchRemovedFromAgencyV1Data Data { get; }

    public BranchRemovedFromAgencyV1(Guid agencyId, Guid organizationId, DateTime occurredAtUtc)
    {
        EventId = Guid.NewGuid();
        OccurredAtUtc = occurredAtUtc;
        Data = new BranchRemovedFromAgencyV1Data(agencyId, organizationId, occurredAtUtc);
    }
}

public sealed record BranchRemovedFromAgencyV1Data(
    Guid AgencyId,
    Guid OrganizationId,
    DateTime RemovedAtUtc);
