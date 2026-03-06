// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.ContactsApi.Domain.Common;

namespace Propely.ContactsApi.Domain.Contacts.Events;

public sealed record ContactDeletedV1 : IDomainEvent
{
    public Guid EventId { get; }
    public string EventType => nameof(ContactDeletedV1);
    public int SchemaVersion => 1;
    public DateTime OccurredAtUtc { get; }
    public Guid? CorrelationId { get; init; }
    public Guid? CausationId { get; init; }
    public string Producer => "ContactsApi";

    public ContactDeletedV1Data Data { get; }

    public ContactDeletedV1(Guid contactId, Guid tenantId, DateTime occurredAtUtc)
    {
        EventId = Guid.NewGuid();
        OccurredAtUtc = occurredAtUtc;
        Data = new ContactDeletedV1Data(contactId, tenantId);
    }
}

public sealed record ContactDeletedV1Data(Guid ContactId, Guid TenantId);
