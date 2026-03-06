// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.ContactsApi.Application.Leads.Dtos;

namespace Propely.ContactsApi.Application.Leads.Commands.CreateLead;

public sealed record CreateLeadCommand : IRequest<LeadDto>
{
    public string Name { get; init; } = null!;
    public string Email { get; init; } = null!;
    public Guid PropertyId { get; init; }
    public Guid TenantId { get; init; }
    public string? Phone { get; init; }
    public string? Message { get; init; }
    public string? Source { get; init; }
    public Guid? AssignedAgentId { get; init; }
}
