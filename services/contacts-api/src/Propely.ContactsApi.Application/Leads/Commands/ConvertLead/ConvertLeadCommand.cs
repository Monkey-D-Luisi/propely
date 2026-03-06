// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.ContactsApi.Application.Leads.Dtos;
using Propely.ContactsApi.Domain.Contacts;

namespace Propely.ContactsApi.Application.Leads.Commands.ConvertLead;

public sealed record ConvertLeadCommand : IRequest<ConvertLeadResponse>
{
    public Guid LeadId { get; init; }
    public Guid TenantId { get; init; }
    public ContactRole Role { get; init; } = ContactRole.Buyer;
    public string? Notes { get; init; }
}
