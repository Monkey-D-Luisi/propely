// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.AuditLogs.Interfaces;
using Propely.OrgsApi.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Propely.OrgsApi.Infrastructure.Persistence.Repositories;

public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _context;

    public AuditLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(List<AuditLog> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        DateTime? dateFrom,
        DateTime? dateTo,
        Guid? userId,
        string? action,
        string? entityType,
        string? entityId,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilters(_context.AuditLogs.AsNoTracking(), dateFrom, dateTo, userId, action, entityType, entityId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<List<AuditLog>> GetAllFilteredAsync(
        DateTime? dateFrom,
        DateTime? dateTo,
        Guid? userId,
        string? action,
        string? entityType,
        string? entityId,
        int maxRows = 10_000,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilters(_context.AuditLogs.AsNoTracking(), dateFrom, dateTo, userId, action, entityType, entityId);

        return await query
            .OrderByDescending(a => a.CreatedAtUtc)
            .Take(maxRows)
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<AuditLog> ApplyFilters(
        IQueryable<AuditLog> query,
        DateTime? dateFrom,
        DateTime? dateTo,
        Guid? userId,
        string? action,
        string? entityType,
        string? entityId)
    {
        if (dateFrom.HasValue)
            query = query.Where(a => a.CreatedAtUtc >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(a => a.CreatedAtUtc <= dateTo.Value);

        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId.Value);

        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(a => a.Action == action);

        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(a => a.EntityType == entityType);

        if (!string.IsNullOrWhiteSpace(entityId))
            query = query.Where(a => a.EntityId == entityId);

        return query;
    }
}
