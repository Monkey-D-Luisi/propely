// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.ContactsApi.Domain.Common;

namespace Propely.ContactsApi.Domain.Leads.Events;

public sealed record LeadDeletedV1 : IDomainEvent
{
    public Guid EventId { get; }
    public string EventType => nameof(LeadDeletedV1);
    public int SchemaVersion => 1;
    public DateTime OccurredAtUtc { get; }
    public Guid? CorrelationId { get; init; }
    public Guid? CausationId { get; init; }
    public string Producer => "ContactsApi";

    public LeadDeletedV1Data Data { get; }

    public LeadDeletedV1(Guid leadId, Guid propertyId, Guid tenantId, DateTime occurredAtUtc)
    {
        EventId = Guid.NewGuid();
        OccurredAtUtc = occurredAtUtc;
        Data = new LeadDeletedV1Data(leadId, propertyId, tenantId);
    }
}

public sealed record LeadDeletedV1Data(Guid LeadId, Guid PropertyId, Guid TenantId);
