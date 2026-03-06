// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.ContactsApi.Domain.Leads;

namespace Propely.ContactsApi.Api.Dtos;

public sealed record CreateLeadRequest
{
    public string Name { get; init; } = null!;
    public string Email { get; init; } = null!;
    public Guid PropertyId { get; init; }
    public string? Phone { get; init; }
    public string? Message { get; init; }
    public string? Source { get; init; }
    public Guid? AssignedAgentId { get; init; }
}

public sealed record AssignLeadRequest
{
    public Guid AgentId { get; init; }
}

public sealed record ChangeLeadStatusRequest
{
    public LeadStatus Status { get; init; }
}
