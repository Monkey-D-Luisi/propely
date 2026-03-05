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

        // Full-text search across title and address fields
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchPattern = $"%{filter.Search}%";
            query = query.Where(p =>
                EF.Functions.ILike(p.Title, searchPattern)
                || (p.Address != null && p.Address.City != null && EF.Functions.ILike(p.Address.City, searchPattern))
                || (p.Address != null && p.Address.Province != null && EF.Functions.ILike(p.Address.Province, searchPattern))
                || (p.Address != null && p.Address.Street != null && EF.Functions.ILike(p.Address.Street, searchPattern)));
        }

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

        // Advanced filters: bedrooms, bathrooms, area
        if (filter.MinBedrooms.HasValue)
            query = query.Where(p => p.Features != null && p.Features.Bedrooms >= filter.MinBedrooms.Value);

        if (filter.MinBathrooms.HasValue)
            query = query.Where(p => p.Features != null && p.Features.Bathrooms >= filter.MinBathrooms.Value);

        if (filter.MinArea.HasValue)
            query = query.Where(p => p.Features != null && p.Features.BuiltArea >= filter.MinArea.Value);

        if (filter.MaxArea.HasValue)
            query = query.Where(p => p.Features != null && p.Features.BuiltArea <= filter.MaxArea.Value);

        // Amenity filters (only filter when true)
        if (filter.HasPool == true)
            query = query.Where(p => p.Features != null && p.Features.HasPool);

        if (filter.HasGarden == true)
            query = query.Where(p => p.Features != null && p.Features.HasGarden);

        if (filter.HasGarage == true)
            query = query.Where(p => p.Features != null && p.Features.HasGarage);

        if (filter.HasElevator == true)
            query = query.Where(p => p.Features != null && p.Features.HasElevator);

        if (filter.HasTerrace == true)
            query = query.Where(p => p.Features != null && p.Features.HasTerrace);

        // Count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting (with relevance sort when search is active)
        query = ApplySorting(query, filter.SortBy, filter.SortDescending, filter.Search);

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

    private static IQueryable<Property> ApplySorting(IQueryable<Property> query, string? sortBy, bool descending, string? search = null)
    {
        // When search is active and no explicit sort, use relevance: title matches first, then address matches
        if (!string.IsNullOrWhiteSpace(search) && string.IsNullOrWhiteSpace(sortBy))
        {
            var searchPattern = $"%{search}%";
            return query.OrderByDescending(p => EF.Functions.ILike(p.Title, searchPattern))
                .ThenByDescending(p => p.CreatedAtUtc);
        }

        return sortBy?.ToLowerInvariant() switch
        {
            "title" => descending ? query.OrderByDescending(p => p.Title) : query.OrderBy(p => p.Title),
            "price" => descending
                ? query.OrderByDescending(p => p.Financials != null ? p.Financials.Price : null)
                : query.OrderBy(p => p.Financials != null ? p.Financials.Price : null),
            "status" => descending ? query.OrderByDescending(p => p.Status) : query.OrderBy(p => p.Status),
            "type" => descending ? query.OrderByDescending(p => p.PropertyType) : query.OrderBy(p => p.PropertyType),
            "created" => descending ? query.OrderByDescending(p => p.CreatedAtUtc) : query.OrderBy(p => p.CreatedAtUtc),
            "relevance" when !string.IsNullOrWhiteSpace(search) =>
                query.OrderByDescending(p => EF.Functions.ILike(p.Title, $"%{search}%"))
                    .ThenByDescending(p => p.CreatedAtUtc),
            _ => query.OrderByDescending(p => p.CreatedAtUtc) // default sort
        };
    }
}
