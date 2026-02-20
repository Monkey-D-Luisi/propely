// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.AuditLogs.DTOs;
using Propely.OrgsApi.Application.AuditLogs.Interfaces;
using MediatR;

namespace Propely.OrgsApi.Application.AuditLogs.Queries.ExportAuditLogs;

public sealed class ExportAuditLogsQueryHandler : IRequestHandler<ExportAuditLogsQuery, List<AuditLogDto>>
{
    /// <summary>
    /// Hard maximum number of rows returned in a single export to prevent
    /// unbounded memory allocation and denial-of-service via large exports.
    /// </summary>
    public const int MaxExportRows = 10_000;

    private readonly IAuditLogRepository _repository;

    public ExportAuditLogsQueryHandler(IAuditLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<AuditLogDto>> Handle(ExportAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var items = await _repository.GetAllFilteredAsync(
            request.DateFrom,
            request.DateTo,
            request.UserId,
            request.Action,
            request.EntityType,
            request.EntityId,
            MaxExportRows,
            cancellationToken);

        return items
            .Select(a => new AuditLogDto(
                a.Id,
                a.UserId,
                a.OrganizationId,
                a.Action,
                a.EntityType,
                a.EntityId,
                a.Changes,
                a.CorrelationId,
                a.CreatedAtUtc))
            .ToList();
    }
}
