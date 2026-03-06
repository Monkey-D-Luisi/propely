// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.ContactsApi.Application.Common.Models;
using Propely.ContactsApi.Application.Contacts.Interfaces;
using Propely.ContactsApi.Domain.Contacts;
using Microsoft.EntityFrameworkCore;

namespace Propely.ContactsApi.Infrastructure.Persistence.Repositories;

public sealed class ContactReadRepository : IContactReadRepository
{
    private readonly AppDbContext _context;

    public ContactReadRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Contact?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Contacts
            .AsNoTracking()
            .Include(c => c.PropertyInterests)
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId, cancellationToken);
    }

    public async Task<Contact?> GetByEmailAsync(string email, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Contacts
            .AsNoTracking()
            .Include(c => c.PropertyInterests)
            .FirstOrDefaultAsync(c => c.Email == email.ToLowerInvariant() && c.TenantId == tenantId, cancellationToken);
    }

    public async Task<PagedResult<Contact>> ListAsync(ContactListFilter filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Contacts
            .AsNoTracking()
            .Include(c => c.PropertyInterests)
            .Where(c => c.TenantId == filter.TenantId);

        // Full-text search across name and email fields
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchPattern = $"%{filter.Search}%";
            query = query.Where(c =>
                EF.Functions.ILike(c.FirstName, searchPattern)
                || EF.Functions.ILike(c.LastName, searchPattern)
                || EF.Functions.ILike(c.Email, searchPattern)
                || (c.Company != null && EF.Functions.ILike(c.Company, searchPattern)));
        }

        // Note: Role filtering on JSON column requires application-side filtering
        // after initial query, or a raw SQL approach. For simplicity and correctness
        // with the JSON backing field, we filter in-memory after materialization
        // only when a role filter is specified.

        // Count before pagination (without role filter for accurate count with role)
        int totalCount;
        List<Contact> items;

        if (filter.Role.HasValue)
        {
            // Materialize filtered results, then apply role filter in-memory
            var allFiltered = await query.ToListAsync(cancellationToken);
            var roleFiltered = allFiltered.Where(c => c.Roles.Contains(filter.Role.Value)).ToList();
            totalCount = roleFiltered.Count;

            // Apply sorting
            var sorted = ApplySorting(roleFiltered.AsQueryable(), filter.SortBy, filter.SortDescending, filter.Search);

            // Apply pagination
            items = sorted
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();
        }
        else
        {
            totalCount = await query.CountAsync(cancellationToken);

            // Apply sorting
            query = ApplySorting(query, filter.SortBy, filter.SortDescending, filter.Search);

            // Apply pagination
            items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(cancellationToken);
        }

        return new PagedResult<Contact>(items, totalCount, filter.Page, filter.PageSize);
    }

    private static IQueryable<Contact> ApplySorting(IQueryable<Contact> query, string? sortBy, bool descending, string? search = null)
    {
        // When search is active and no explicit sort, use relevance: name matches first
        if (!string.IsNullOrWhiteSpace(search) && string.IsNullOrWhiteSpace(sortBy))
        {
            var searchPattern = $"%{search}%";
            return query
                .OrderByDescending(c => EF.Functions.ILike(c.FirstName, searchPattern)
                    || EF.Functions.ILike(c.LastName, searchPattern))
                .ThenByDescending(c => c.CreatedAtUtc);
        }

        return sortBy?.ToLowerInvariant() switch
        {
            "name" => descending
                ? query.OrderByDescending(c => c.LastName).ThenByDescending(c => c.FirstName)
                : query.OrderBy(c => c.LastName).ThenBy(c => c.FirstName),
            "email" => descending ? query.OrderByDescending(c => c.Email) : query.OrderBy(c => c.Email),
            "company" => descending ? query.OrderByDescending(c => c.Company) : query.OrderBy(c => c.Company),
            "created" => descending ? query.OrderByDescending(c => c.CreatedAtUtc) : query.OrderBy(c => c.CreatedAtUtc),
            _ => query.OrderByDescending(c => c.CreatedAtUtc) // default sort
        };
    }
}
