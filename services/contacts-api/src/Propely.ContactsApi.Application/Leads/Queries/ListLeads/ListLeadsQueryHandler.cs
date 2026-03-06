// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.ContactsApi.Application.Common.Models;
using Propely.ContactsApi.Application.Leads.Dtos;
using Propely.ContactsApi.Application.Leads.Interfaces;

namespace Propely.ContactsApi.Application.Leads.Queries.ListLeads;

public sealed class ListLeadsQueryHandler : IRequestHandler<ListLeadsQuery, PagedResult<LeadListItemDto>>
{
    private readonly ILeadReadRepository _leadReadRepository;

    public ListLeadsQueryHandler(ILeadReadRepository leadReadRepository)
    {
        _leadReadRepository = leadReadRepository;
    }

    public async Task<PagedResult<LeadListItemDto>> Handle(ListLeadsQuery request, CancellationToken cancellationToken)
    {
        var filter = new LeadListFilter
        {
            TenantId = request.TenantId,
            Status = request.Status,
            PropertyId = request.PropertyId,
            AssignedAgentId = request.AssignedAgentId,
            Search = request.Search,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending,
            Page = request.Page,
            PageSize = request.PageSize
        };

        var result = await _leadReadRepository.ListAsync(filter, cancellationToken);

        return new PagedResult<LeadListItemDto>(
            result.Items.Select(LeadMapper.ToListItemDto).ToList(),
            result.PageNumber,
            result.TotalPages,
            result.TotalCount);
    }
}
