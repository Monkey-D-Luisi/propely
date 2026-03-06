// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.ContactsApi.Domain.Contacts;

namespace Propely.ContactsApi.Application.Contacts.Dtos;

public sealed record ContactDto
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string? Phone { get; init; }
    public string? SecondaryPhone { get; init; }
    public string? Company { get; init; }
    public string? Notes { get; init; }
    public string? PreferredLanguage { get; init; }
    public ContactSource? Source { get; init; }
    public Guid? AssignedAgentId { get; init; }
    public Guid TenantId { get; init; }
    public IReadOnlyCollection<ContactRole> Roles { get; init; } = [];
    public IReadOnlyCollection<PropertyInterestDto> PropertyInterests { get; init; } = [];
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
}

public sealed record PropertyInterestDto
{
    public Guid Id { get; init; }
    public Guid PropertyId { get; init; }
    public InterestType InterestType { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}

public sealed record ContactListItemDto
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string? Phone { get; init; }
    public string? Company { get; init; }
    public IReadOnlyCollection<ContactRole> Roles { get; init; } = [];
    public DateTime CreatedAtUtc { get; init; }
}
