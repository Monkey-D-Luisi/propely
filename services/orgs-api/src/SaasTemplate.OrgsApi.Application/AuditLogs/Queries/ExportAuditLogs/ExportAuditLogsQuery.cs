// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Application.AuditLogs.DTOs;
using MediatR;

namespace SaasTemplate.OrgsApi.Application.AuditLogs.Queries.ExportAuditLogs;

public sealed record ExportAuditLogsQuery(
    DateTime? DateFrom,
    DateTime? DateTo,
    Guid? UserId,
    string? Action,
    string? EntityType,
    string? EntityId) : IRequest<List<AuditLogDto>>;
