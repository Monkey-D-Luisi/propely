// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.ContactsApi.Domain.Contacts;

namespace Propely.ContactsApi.Application.Leads.Dtos;

public sealed record ConvertLeadRequest
{
    public ContactRole Role { get; init; } = ContactRole.Buyer;
    public string? Notes { get; init; }
}
