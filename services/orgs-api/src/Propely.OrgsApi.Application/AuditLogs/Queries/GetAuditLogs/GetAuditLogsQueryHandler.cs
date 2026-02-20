// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.AuditLogs.DTOs;
using Propely.OrgsApi.Application.AuditLogs.Interfaces;
using Propely.OrgsApi.Application.Common.Models;
using MediatR;

namespace Propely.OrgsApi.Application.AuditLogs.Queries.GetAuditLogs;

public sealed class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, PagedResult<AuditLogDto>>
{
    private readonly IAuditLogRepository _repository;

    public GetAuditLogsQueryHandler(IAuditLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var (items, totalCount) = await _repository.GetPagedAsync(
            request.Page,
            pageSize,
            request.DateFrom,
            request.DateTo,
            request.UserId,
            request.Action,
            request.EntityType,
            request.EntityId,
            cancellationToken);

        var dtos = items
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

        return new PagedResult<AuditLogDto>(dtos, totalCount, request.Page, pageSize);
    }
}
