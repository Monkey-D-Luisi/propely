// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.ContactsApi.Domain.Contacts;

namespace Propely.ContactsApi.Application.Contacts.Dtos;

public static class ContactMapper
{
    public static ContactDto ToDto(Contact contact) => new()
    {
        Id = contact.Id,
        FirstName = contact.FirstName,
        LastName = contact.LastName,
        Email = contact.Email,
        Phone = contact.Phone,
        SecondaryPhone = contact.SecondaryPhone,
        Company = contact.Company,
        Notes = contact.Notes,
        PreferredLanguage = contact.PreferredLanguage,
        Source = contact.Source,
        AssignedAgentId = contact.AssignedAgentId,
        TenantId = contact.TenantId,
        Roles = contact.Roles.ToList(),
        PropertyInterests = contact.PropertyInterests.Select(pi => new PropertyInterestDto
        {
            Id = pi.Id,
            PropertyId = pi.PropertyId,
            InterestType = pi.InterestType,
            Notes = pi.Notes,
            CreatedAtUtc = pi.CreatedAtUtc
        }).ToList(),
        CreatedAtUtc = contact.CreatedAtUtc,
        UpdatedAtUtc = contact.UpdatedAtUtc
    };

    public static ContactListItemDto ToListItemDto(Contact contact) => new()
    {
        Id = contact.Id,
        FirstName = contact.FirstName,
        LastName = contact.LastName,
        Email = contact.Email,
        Phone = contact.Phone,
        Company = contact.Company,
        Roles = contact.Roles.ToList(),
        CreatedAtUtc = contact.CreatedAtUtc
    };
}
