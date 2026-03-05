// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.PropertiesApi.Application.Common.Models;
using Propely.PropertiesApi.Application.Properties.Dtos;
using Propely.PropertiesApi.Application.Properties.Interfaces;
using Propely.PropertiesApi.Domain.Properties;

namespace Propely.PropertiesApi.Application.Properties.Queries.ListProperties;

public sealed record ListPropertiesQuery : IRequest<PagedResult<PropertyListItemDto>>
{
    public Guid TenantId { get; init; }
    public string? Search { get; init; }
    public PropertyType? Type { get; init; }
    public OperationType? Operation { get; init; }
    public PropertyStatus? Status { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public string? City { get; init; }
    public Guid? AgentId { get; init; }
    public int? MinBedrooms { get; init; }
    public int? MinBathrooms { get; init; }
    public decimal? MinArea { get; init; }
    public decimal? MaxArea { get; init; }
    public bool? HasPool { get; init; }
    public bool? HasGarden { get; init; }
    public bool? HasGarage { get; init; }
    public bool? HasElevator { get; init; }
    public bool? HasTerrace { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
