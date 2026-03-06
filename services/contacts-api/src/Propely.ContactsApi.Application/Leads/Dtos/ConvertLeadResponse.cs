// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.ContactsApi.Application.Contacts.Dtos;

namespace Propely.ContactsApi.Application.Leads.Dtos;

public sealed record ConvertLeadResponse
{
    public LeadDto Lead { get; init; } = null!;
    public ContactDto Contact { get; init; } = null!;
    public bool WasNewContact { get; init; }
}
