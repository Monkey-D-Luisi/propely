// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.AiApi.Application.Common.Models;
using SaasTemplate.AiApi.Application.WorkItems.Dtos;
using SaasTemplate.AiApi.Application.WorkItems.Interfaces;
using MediatR;

namespace SaasTemplate.AiApi.Application.WorkItems.Queries.ListWorkItems;

public class ListWorkItemsQueryHandler : IRequestHandler<ListWorkItemsQuery, PagedResult<WorkItemDto>>
{
    private readonly IWorkItemReadRepository _repository;

    public ListWorkItemsQueryHandler(IWorkItemReadRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<WorkItemDto>> Handle(ListWorkItemsQuery request, CancellationToken cancellationToken)
    {
        return await _repository.ListAsync(
            request.Page,
            request.PageSize,
            request.Status,
            request.Search,
            cancellationToken);
    }
}
