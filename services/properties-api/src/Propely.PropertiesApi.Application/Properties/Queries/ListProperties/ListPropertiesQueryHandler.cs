// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.PropertiesApi.Application.Common.Models;
using Propely.PropertiesApi.Application.Properties.Dtos;
using Propely.PropertiesApi.Application.Properties.Interfaces;

namespace Propely.PropertiesApi.Application.Properties.Queries.ListProperties;

public sealed class ListPropertiesQueryHandler : IRequestHandler<ListPropertiesQuery, PagedResult<PropertyListItemDto>>
{
    private readonly IPropertyReadRepository _readRepository;

    public ListPropertiesQueryHandler(IPropertyReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<PagedResult<PropertyListItemDto>> Handle(ListPropertiesQuery request, CancellationToken cancellationToken)
    {
        var filter = new PropertyListFilter
        {
            TenantId = request.TenantId,
            Type = request.Type,
            Operation = request.Operation,
            Status = request.Status,
            MinPrice = request.MinPrice,
            MaxPrice = request.MaxPrice,
            City = request.City,
            AgentId = request.AgentId,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending,
            Page = request.Page,
            PageSize = request.PageSize
        };

        var pagedResult = await _readRepository.ListAsync(filter, cancellationToken);

        var dtos = pagedResult.Items.Select(PropertyMapper.ToListItemDto);

        return new PagedResult<PropertyListItemDto>(dtos, pagedResult.TotalCount, pagedResult.PageNumber, request.PageSize);
    }
}
