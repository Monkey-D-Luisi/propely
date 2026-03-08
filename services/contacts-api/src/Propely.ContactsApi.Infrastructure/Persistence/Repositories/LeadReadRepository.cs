// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.ContactsApi.Application.Common.Models;
using Propely.ContactsApi.Application.Leads.Interfaces;
using Propely.ContactsApi.Domain.Leads;
using Microsoft.EntityFrameworkCore;

namespace Propely.ContactsApi.Infrastructure.Persistence.Repositories;

public sealed class LeadReadRepository : ILeadReadRepository
{
    private readonly AppDbContext _context;

    public LeadReadRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Lead?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Leads
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id && l.TenantId == tenantId, cancellationToken);
    }

    public async Task<PagedResult<Lead>> ListAsync(LeadListFilter filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Leads
            .AsNoTracking()
            .Where(l => l.TenantId == filter.TenantId);

        // Full-text search across name and email fields
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var escaped = filter.Search.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
            var searchPattern = $"%{escaped}%";
            query = query.Where(l =>
                EF.Functions.ILike(l.Name, searchPattern)
                || EF.Functions.ILike(l.Email, searchPattern));
        }

        // Apply filters
        if (filter.Status.HasValue)
            query = query.Where(l => l.Status == filter.Status.Value);

        if (filter.PropertyId.HasValue)
            query = query.Where(l => l.PropertyId == filter.PropertyId.Value);

        if (filter.AssignedAgentId.HasValue)
            query = query.Where(l => l.AssignedAgentId == filter.AssignedAgentId.Value);

        // Count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = ApplySorting(query, filter.SortBy, filter.SortDescending, filter.Search);

        // Apply pagination
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Lead>(items, totalCount, filter.Page, filter.PageSize);
    }

    public async Task<bool> ExistsByEmailAndPropertyAsync(string email, Guid propertyId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Leads
            .AsNoTracking()
            .AnyAsync(l => l.Email == email.ToLowerInvariant()
                && l.PropertyId == propertyId
                && l.TenantId == tenantId, cancellationToken);
    }

    public async Task<Dictionary<LeadStatus, int>> CountByStatusAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Leads
            .AsNoTracking()
            .Where(l => l.TenantId == tenantId)
            .GroupBy(l => l.Status)
            .ToDictionaryAsync(
                g => g.Key,
                g => g.Count(),
                cancellationToken);
    }

    private static IQueryable<Lead> ApplySorting(IQueryable<Lead> query, string? sortBy, bool descending, string? search = null)
    {
        // When search is active and no explicit sort, use relevance: name matches first
        if (!string.IsNullOrWhiteSpace(search) && string.IsNullOrWhiteSpace(sortBy))
        {
            var escaped = search.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
            var searchPattern = $"%{escaped}%";
            return query.OrderByDescending(l => EF.Functions.ILike(l.Name, searchPattern))
                .ThenByDescending(l => l.CreatedAtUtc);
        }

        return sortBy?.ToLowerInvariant() switch
        {
            "name" => descending ? query.OrderByDescending(l => l.Name) : query.OrderBy(l => l.Name),
            "email" => descending ? query.OrderByDescending(l => l.Email) : query.OrderBy(l => l.Email),
            "status" => descending ? query.OrderByDescending(l => l.Status) : query.OrderBy(l => l.Status),
            "created" => descending ? query.OrderByDescending(l => l.CreatedAtUtc) : query.OrderBy(l => l.CreatedAtUtc),
            "relevance" when !string.IsNullOrWhiteSpace(search) =>
                query.OrderByDescending(l => EF.Functions.ILike(l.Name,
                    $"%{search.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_")}%"))
                    .ThenByDescending(l => l.CreatedAtUtc),
            _ => query.OrderByDescending(l => l.CreatedAtUtc) // default sort
        };
    }
}
