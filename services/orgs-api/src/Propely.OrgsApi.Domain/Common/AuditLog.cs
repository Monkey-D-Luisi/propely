// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Domain.Common;

/// <summary>
/// Represents an audit log entry capturing who performed what action on which entity.
/// </summary>
public sealed class AuditLog
{
    public Guid Id { get; private set; }
    public Guid? UserId { get; private set; }
    public Guid? OrganizationId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public string EntityId { get; private set; } = string.Empty;
    public string? Changes { get; private set; }
    public string? CorrelationId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private AuditLog() { }

    public static AuditLog Create(
        Guid? userId,
        Guid? organizationId,
        string action,
        string entityType,
        string entityId,
        string? changes,
        string? correlationId)
    {
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            OrganizationId = organizationId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Changes = changes,
            CorrelationId = correlationId,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
