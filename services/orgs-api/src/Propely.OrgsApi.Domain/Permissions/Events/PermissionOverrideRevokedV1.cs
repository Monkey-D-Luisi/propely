// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Domain.Common;

namespace Propely.OrgsApi.Domain.Permissions.Events;

/// <summary>
/// Domain event raised when a permission override is revoked (removed entirely).
/// </summary>
public sealed record PermissionOverrideRevokedV1 : IDomainEvent
{
    public Guid EventId { get; }
    public string EventType => nameof(PermissionOverrideRevokedV1);
    public int SchemaVersion => 1;
    public DateTime OccurredAtUtc { get; }
    public Guid? CorrelationId { get; init; }
    public Guid? CausationId { get; init; }
    public string Producer => "OrgsApi";

    public PermissionOverrideRevokedV1Data Data { get; }

    public PermissionOverrideRevokedV1(
        Guid overrideId, Guid userId, Guid organizationId,
        Permission permission, DateTime revokedAtUtc)
    {
        EventId = Guid.NewGuid();
        OccurredAtUtc = revokedAtUtc;
        Data = new PermissionOverrideRevokedV1Data(
            overrideId, userId, organizationId, permission, revokedAtUtc);
    }
}

public sealed record PermissionOverrideRevokedV1Data(
    Guid OverrideId,
    Guid UserId,
    Guid OrganizationId,
    Permission Permission,
    DateTime RevokedAtUtc);
