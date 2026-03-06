// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.ContactsApi.Domain.Leads;

namespace Propely.ContactsApi.Application.Leads.Dtos;

public sealed record LeadDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string? Phone { get; init; }
    public string? Message { get; init; }
    public string? Source { get; init; }
    public Guid PropertyId { get; init; }
    public Guid TenantId { get; init; }
    public LeadStatus Status { get; init; }
    public Guid? AssignedAgentId { get; init; }
    public Guid? ContactId { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
}

public sealed record LeadListItemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string? Source { get; init; }
    public Guid PropertyId { get; init; }
    public LeadStatus Status { get; init; }
    public Guid? AssignedAgentId { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}
