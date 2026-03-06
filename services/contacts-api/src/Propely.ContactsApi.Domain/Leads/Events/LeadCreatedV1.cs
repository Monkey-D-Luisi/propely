// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.ContactsApi.Domain.Common;

namespace Propely.ContactsApi.Domain.Leads.Events;

public sealed record LeadCreatedV1 : IDomainEvent
{
    public Guid EventId { get; }
    public string EventType => nameof(LeadCreatedV1);
    public int SchemaVersion => 1;
    public DateTime OccurredAtUtc { get; }
    public Guid? CorrelationId { get; init; }
    public Guid? CausationId { get; init; }
    public string Producer => "ContactsApi";

    public LeadCreatedV1Data Data { get; }

    public LeadCreatedV1(Guid leadId, Guid propertyId, Guid tenantId, string email, DateTime occurredAtUtc)
    {
        EventId = Guid.NewGuid();
        OccurredAtUtc = occurredAtUtc;
        Data = new LeadCreatedV1Data(leadId, propertyId, tenantId, email);
    }
}

public sealed record LeadCreatedV1Data(Guid LeadId, Guid PropertyId, Guid TenantId, string Email);
