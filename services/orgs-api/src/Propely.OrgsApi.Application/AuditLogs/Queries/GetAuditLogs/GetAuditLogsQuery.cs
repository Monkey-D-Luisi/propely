// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.AuditLogs.DTOs;
using Propely.OrgsApi.Application.Common.Models;
using MediatR;

namespace Propely.OrgsApi.Application.AuditLogs.Queries.GetAuditLogs;

public sealed record GetAuditLogsQuery(
    int Page,
    int PageSize,
    DateTime? DateFrom,
    DateTime? DateTo,
    Guid? UserId,
    string? Action,
    string? EntityType,
    string? EntityId) : IRequest<PagedResult<AuditLogDto>>;
