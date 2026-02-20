// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Application.Common.Models;
using Propely.AiApi.Application.WorkItems.Dtos;
using Propely.AiApi.Application.WorkItems.Interfaces;
using MediatR;

namespace Propely.AiApi.Application.WorkItems.Queries.ListWorkItems;

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
