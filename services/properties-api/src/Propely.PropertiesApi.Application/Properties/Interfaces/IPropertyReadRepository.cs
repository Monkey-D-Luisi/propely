// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.PropertiesApi.Application.Common.Models;
using Propely.PropertiesApi.Domain.Properties;

namespace Propely.PropertiesApi.Application.Properties.Interfaces;

public interface IPropertyReadRepository
{
    Task<Property?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);
    Task<PagedResult<Property>> ListAsync(PropertyListFilter filter, CancellationToken cancellationToken = default);
    Task<Dictionary<PropertyStatus, int>> CountByStatusAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

public sealed class PropertyListFilter
{
    public Guid TenantId { get; init; }
    public PropertyType? Type { get; init; }
    public OperationType? Operation { get; init; }
    public PropertyStatus? Status { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public string? City { get; init; }
    public Guid? AgentId { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
