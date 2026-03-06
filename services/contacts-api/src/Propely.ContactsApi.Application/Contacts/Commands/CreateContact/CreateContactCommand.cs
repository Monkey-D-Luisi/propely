// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.ContactsApi.Application.Contacts.Dtos;
using Propely.ContactsApi.Domain.Contacts;

namespace Propely.ContactsApi.Application.Contacts.Commands.CreateContact;

public sealed record CreateContactCommand : IRequest<ContactDto>
{
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public Guid TenantId { get; init; }
    public IReadOnlyCollection<ContactRole> Roles { get; init; } = [];
    public string? Phone { get; init; }
    public string? SecondaryPhone { get; init; }
    public string? Company { get; init; }
    public string? Notes { get; init; }
    public string? PreferredLanguage { get; init; }
    public ContactSource? Source { get; init; }
    public Guid? AssignedAgentId { get; init; }
}
