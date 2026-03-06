// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.ContactsApi.Domain.Common;

namespace Propely.ContactsApi.Domain.Leads.Events;

public sealed record LeadConvertedV1 : IDomainEvent
{
    public Guid EventId { get; }
    public string EventType => nameof(LeadConvertedV1);
    public int SchemaVersion => 1;
    public DateTime OccurredAtUtc { get; }
    public Guid? CorrelationId { get; init; }
    public Guid? CausationId { get; init; }
    public string Producer => "ContactsApi";

    public LeadConvertedV1Data Data { get; }

    public LeadConvertedV1(Guid leadId, Guid contactId, Guid propertyId, Guid tenantId, bool wasNewContact, DateTime occurredAtUtc)
    {
        EventId = Guid.NewGuid();
        OccurredAtUtc = occurredAtUtc;
        Data = new LeadConvertedV1Data(leadId, contactId, propertyId, tenantId, wasNewContact);
    }
}

public sealed record LeadConvertedV1Data(Guid LeadId, Guid ContactId, Guid PropertyId, Guid TenantId, bool WasNewContact);
