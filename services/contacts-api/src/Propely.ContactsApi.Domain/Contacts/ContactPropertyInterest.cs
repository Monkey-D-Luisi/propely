// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.ContactsApi.Domain.Common.Exceptions;

namespace Propely.ContactsApi.Domain.Contacts;

public sealed class ContactPropertyInterest
{
    public Guid Id { get; private set; }
    public Guid ContactId { get; private set; }
    public Guid PropertyId { get; private set; }
    public InterestType InterestType { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private ContactPropertyInterest() { }

    public static ContactPropertyInterest Create(
        Guid contactId,
        Guid propertyId,
        InterestType interestType,
        string? notes = null)
    {
        if (contactId == Guid.Empty)
            throw new DomainException("Contact ID is required.");

        if (propertyId == Guid.Empty)
            throw new DomainException("Property ID is required.");

        return new ContactPropertyInterest
        {
            Id = Guid.NewGuid(),
            ContactId = contactId,
            PropertyId = propertyId,
            InterestType = interestType,
            Notes = notes?.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
