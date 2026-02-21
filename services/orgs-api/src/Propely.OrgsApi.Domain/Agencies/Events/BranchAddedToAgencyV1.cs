// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Domain.Common;

namespace Propely.OrgsApi.Domain.Agencies.Events;

/// <summary>
/// Domain event raised when a branch (organization) is added to an agency.
/// </summary>
public sealed record BranchAddedToAgencyV1 : IDomainEvent
{
    public Guid EventId { get; }
    public string EventType => nameof(BranchAddedToAgencyV1);
    public int SchemaVersion => 1;
    public DateTime OccurredAtUtc { get; }
    public Guid? CorrelationId { get; init; }
    public Guid? CausationId { get; init; }
    public string Producer => "OrgsApi";

    public BranchAddedToAgencyV1Data Data { get; }

    public BranchAddedToAgencyV1(Guid agencyId, Guid organizationId, DateTime occurredAtUtc)
    {
        EventId = Guid.NewGuid();
        OccurredAtUtc = occurredAtUtc;
        Data = new BranchAddedToAgencyV1Data(agencyId, organizationId, occurredAtUtc);
    }
}

public sealed record BranchAddedToAgencyV1Data(
    Guid AgencyId,
    Guid OrganizationId,
    DateTime AddedAtUtc);
