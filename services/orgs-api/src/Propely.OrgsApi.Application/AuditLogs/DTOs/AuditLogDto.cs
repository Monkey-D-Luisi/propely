// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Application.AuditLogs.DTOs;

public sealed record AuditLogDto(
    Guid Id,
    Guid? UserId,
    Guid? OrganizationId,
    string Action,
    string EntityType,
    string EntityId,
    string? Changes,
    string? CorrelationId,
    DateTime CreatedAtUtc);
