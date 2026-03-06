// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AppointmentsApi.Application.Appointments.Interfaces;
using Propely.AppointmentsApi.Application.Common.Models;
using Propely.AppointmentsApi.Domain.Appointments;
using Microsoft.EntityFrameworkCore;

namespace Propely.AppointmentsApi.Infrastructure.Persistence.Repositories;

public sealed class AppointmentReadRepository : IAppointmentReadRepository
{
    private readonly AppDbContext _context;

    public AppointmentReadRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Appointment?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .AsNoTracking()
            .Include(a => a.CalendarSyncInfos)
            .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == tenantId, cancellationToken);
    }

    public async Task<PagedResult<Appointment>> ListAsync(AppointmentListFilter filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Appointments
            .AsNoTracking()
            .Include(a => a.CalendarSyncInfos)
            .Where(a => a.TenantId == filter.TenantId);

        // Full-text search across title and location fields
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchPattern = $"%{filter.Search}%";
            query = query.Where(a =>
                EF.Functions.ILike(a.Title, searchPattern)
                || (a.Description != null && EF.Functions.ILike(a.Description, searchPattern))
                || (a.Location != null && EF.Functions.ILike(a.Location, searchPattern)));
        }

        // Status filter
        if (filter.Status.HasValue)
        {
            query = query.Where(a => a.Status == filter.Status.Value);
        }

        // Type filter
        if (filter.Type.HasValue)
        {
            query = query.Where(a => a.Type == filter.Type.Value);
        }

        // Agent filter
        if (filter.AgentId.HasValue)
        {
            query = query.Where(a => a.AgentId == filter.AgentId.Value);
        }

        // Property filter
        if (filter.PropertyId.HasValue)
        {
            query = query.Where(a => a.PropertyId == filter.PropertyId.Value);
        }

        // Contact filter
        if (filter.ContactId.HasValue)
        {
            query = query.Where(a => a.ContactId == filter.ContactId.Value);
        }

        // Date range filter
        if (filter.FromUtc.HasValue)
        {
            query = query.Where(a => a.StartTimeUtc >= filter.FromUtc.Value);
        }

        if (filter.ToUtc.HasValue)
        {
            query = query.Where(a => a.EndTimeUtc <= filter.ToUtc.Value);
        }

        // Count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = ApplySorting(query, filter.SortBy, filter.SortDescending, filter.Search);

        // Apply pagination
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Appointment>(items, totalCount, filter.Page, filter.PageSize);
    }

    private static IQueryable<Appointment> ApplySorting(IQueryable<Appointment> query, string? sortBy, bool descending, string? search = null)
    {
        // When search is active and no explicit sort, use relevance: title matches first
        if (!string.IsNullOrWhiteSpace(search) && string.IsNullOrWhiteSpace(sortBy))
        {
            var searchPattern = $"%{search}%";
            return query
                .OrderByDescending(a => EF.Functions.ILike(a.Title, searchPattern))
                .ThenByDescending(a => a.StartTimeUtc);
        }

        return sortBy?.ToLowerInvariant() switch
        {
            "title" => descending ? query.OrderByDescending(a => a.Title) : query.OrderBy(a => a.Title),
            "start" => descending ? query.OrderByDescending(a => a.StartTimeUtc) : query.OrderBy(a => a.StartTimeUtc),
            "end" => descending ? query.OrderByDescending(a => a.EndTimeUtc) : query.OrderBy(a => a.EndTimeUtc),
            "status" => descending ? query.OrderByDescending(a => a.Status) : query.OrderBy(a => a.Status),
            "type" => descending ? query.OrderByDescending(a => a.Type) : query.OrderBy(a => a.Type),
            "created" => descending ? query.OrderByDescending(a => a.CreatedAtUtc) : query.OrderBy(a => a.CreatedAtUtc),
            _ => query.OrderByDescending(a => a.StartTimeUtc) // default sort: most recent appointments first
        };
    }
}
