// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.PropertiesApi.Application.Common.Models;
using Propely.PropertiesApi.Application.Properties.Interfaces;
using Propely.PropertiesApi.Domain.Properties;
using Microsoft.EntityFrameworkCore;

namespace Propely.PropertiesApi.Infrastructure.Persistence.Repositories;

public sealed class PropertyReadRepository : IPropertyReadRepository
{
    private readonly AppDbContext _context;

    public PropertyReadRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Property?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Properties
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId, cancellationToken);
    }

    public async Task<PagedResult<Property>> ListAsync(PropertyListFilter filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Properties
            .AsNoTracking()
            .Where(p => p.TenantId == filter.TenantId);

        // Apply filters
        if (filter.Type.HasValue)
            query = query.Where(p => p.PropertyType == filter.Type.Value);

        if (filter.Operation.HasValue)
            query = query.Where(p => p.OperationType == filter.Operation.Value);

        if (filter.Status.HasValue)
            query = query.Where(p => p.Status == filter.Status.Value);

        if (filter.AgentId.HasValue)
            query = query.Where(p => p.AgentId == filter.AgentId.Value);

        if (filter.MinPrice.HasValue)
            query = query.Where(p => p.Financials != null && p.Financials.Price >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(p => p.Financials != null && p.Financials.Price <= filter.MaxPrice.Value);

        if (!string.IsNullOrWhiteSpace(filter.City))
            query = query.Where(p => p.Address != null && p.Address.City != null
                && EF.Functions.ILike(p.Address.City, $"%{filter.City}%"));

        // Count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = ApplySorting(query, filter.SortBy, filter.SortDescending);

        // Apply pagination
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Property>(items, totalCount, filter.Page, filter.PageSize);
    }

    public async Task<Dictionary<PropertyStatus, int>> CountByStatusAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Properties
            .AsNoTracking()
            .Where(p => p.TenantId == tenantId)
            .GroupBy(p => p.Status)
            .ToDictionaryAsync(
                g => g.Key,
                g => g.Count(),
                cancellationToken);
    }

    private static IQueryable<Property> ApplySorting(IQueryable<Property> query, string? sortBy, bool descending)
    {
        return sortBy?.ToLowerInvariant() switch
        {
            "title" => descending ? query.OrderByDescending(p => p.Title) : query.OrderBy(p => p.Title),
            "price" => descending
                ? query.OrderByDescending(p => p.Financials != null ? p.Financials.Price : null)
                : query.OrderBy(p => p.Financials != null ? p.Financials.Price : null),
            "status" => descending ? query.OrderByDescending(p => p.Status) : query.OrderBy(p => p.Status),
            "type" => descending ? query.OrderByDescending(p => p.PropertyType) : query.OrderBy(p => p.PropertyType),
            "created" => descending ? query.OrderByDescending(p => p.CreatedAtUtc) : query.OrderBy(p => p.CreatedAtUtc),
            _ => query.OrderByDescending(p => p.CreatedAtUtc) // default sort
        };
    }
}
