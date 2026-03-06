// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.ContactsApi.Domain.Common;

namespace Propely.ContactsApi.Domain.Leads.Events;

public sealed record LeadStatusChangedV1 : IDomainEvent
{
    public Guid EventId { get; }
    public string EventType => nameof(LeadStatusChangedV1);
    public int SchemaVersion => 1;
    public DateTime OccurredAtUtc { get; }
    public Guid? CorrelationId { get; init; }
    public Guid? CausationId { get; init; }
    public string Producer => "ContactsApi";

    public LeadStatusChangedV1Data Data { get; }

    public LeadStatusChangedV1(Guid leadId, Guid propertyId, Guid tenantId, LeadStatus previousStatus, LeadStatus newStatus, DateTime occurredAtUtc)
    {
        EventId = Guid.NewGuid();
        OccurredAtUtc = occurredAtUtc;
        Data = new LeadStatusChangedV1Data(leadId, propertyId, tenantId, previousStatus, newStatus);
    }
}

public sealed record LeadStatusChangedV1Data(Guid LeadId, Guid PropertyId, Guid TenantId, LeadStatus PreviousStatus, LeadStatus NewStatus);
