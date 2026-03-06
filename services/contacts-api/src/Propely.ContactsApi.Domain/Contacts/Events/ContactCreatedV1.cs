// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.ContactsApi.Domain.Common;

namespace Propely.ContactsApi.Domain.Contacts.Events;

public sealed record ContactCreatedV1 : IDomainEvent
{
    public Guid EventId { get; }
    public string EventType => nameof(ContactCreatedV1);
    public int SchemaVersion => 1;
    public DateTime OccurredAtUtc { get; }
    public Guid? CorrelationId { get; init; }
    public Guid? CausationId { get; init; }
    public string Producer => "ContactsApi";

    public ContactCreatedV1Data Data { get; }

    public ContactCreatedV1(Guid contactId, Guid tenantId, string email, DateTime occurredAtUtc)
    {
        EventId = Guid.NewGuid();
        OccurredAtUtc = occurredAtUtc;
        Data = new ContactCreatedV1Data(contactId, tenantId, email);
    }
}

public sealed record ContactCreatedV1Data(Guid ContactId, Guid TenantId, string Email);
