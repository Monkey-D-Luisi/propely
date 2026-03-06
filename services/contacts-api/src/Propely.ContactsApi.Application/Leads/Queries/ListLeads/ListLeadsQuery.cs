// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.ContactsApi.Application.Common.Models;
using Propely.ContactsApi.Application.Leads.Dtos;
using Propely.ContactsApi.Domain.Leads;

namespace Propely.ContactsApi.Application.Leads.Queries.ListLeads;

public sealed record ListLeadsQuery : IRequest<PagedResult<LeadListItemDto>>
{
    public Guid TenantId { get; init; }
    public LeadStatus? Status { get; init; }
    public Guid? PropertyId { get; init; }
    public Guid? AssignedAgentId { get; init; }
    public string? Search { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
