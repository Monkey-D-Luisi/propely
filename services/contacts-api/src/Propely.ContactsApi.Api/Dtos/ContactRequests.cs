// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.ContactsApi.Domain.Contacts;

namespace Propely.ContactsApi.Api.Dtos;

public sealed record CreateContactRequest
{
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public IReadOnlyCollection<ContactRole> Roles { get; init; } = [];
    public string? Phone { get; init; }
    public string? SecondaryPhone { get; init; }
    public string? Company { get; init; }
    public string? Notes { get; init; }
    public string? PreferredLanguage { get; init; }
    public ContactSource? Source { get; init; }
    public Guid? AssignedAgentId { get; init; }
}

public sealed record UpdateContactRequest
{
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Email { get; init; }
    public IReadOnlyCollection<ContactRole>? Roles { get; init; }
    public string? Phone { get; init; }
    public string? SecondaryPhone { get; init; }
    public string? Company { get; init; }
    public string? Notes { get; init; }
    public string? PreferredLanguage { get; init; }
    public ContactSource? Source { get; init; }
    public Guid? AssignedAgentId { get; init; }
}
