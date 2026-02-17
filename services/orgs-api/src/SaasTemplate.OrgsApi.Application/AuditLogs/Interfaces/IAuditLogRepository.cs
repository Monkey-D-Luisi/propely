// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Domain.Common;

namespace SaasTemplate.OrgsApi.Application.AuditLogs.Interfaces;

public interface IAuditLogRepository
{
    Task<(List<AuditLog> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        DateTime? dateFrom,
        DateTime? dateTo,
        Guid? userId,
        string? action,
        string? entityType,
        string? entityId,
        CancellationToken cancellationToken = default);

    Task<List<AuditLog>> GetAllFilteredAsync(
        DateTime? dateFrom,
        DateTime? dateTo,
        Guid? userId,
        string? action,
        string? entityType,
        string? entityId,
        int maxRows = 10_000,
        CancellationToken cancellationToken = default);
}
