// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Domain.Common;

namespace Propely.OrgsApi.Domain.Permissions.Events;

/// <summary>
/// Domain event raised when a permission override is denied (Granted=false).
/// </summary>
public sealed record PermissionOverrideDeniedV1 : IDomainEvent
{
    public Guid EventId { get; }
    public string EventType => nameof(PermissionOverrideDeniedV1);
    public int SchemaVersion => 1;
    public DateTime OccurredAtUtc { get; }
    public Guid? CorrelationId { get; init; }
    public Guid? CausationId { get; init; }
    public string Producer => "OrgsApi";

    public PermissionOverrideDeniedV1Data Data { get; }

    public PermissionOverrideDeniedV1(
        Guid overrideId, Guid userId, Guid organizationId,
        Permission permission, Guid grantedBy, DateTime deniedAtUtc)
    {
        EventId = Guid.NewGuid();
        OccurredAtUtc = deniedAtUtc;
        Data = new PermissionOverrideDeniedV1Data(
            overrideId, userId, organizationId, permission, grantedBy, deniedAtUtc);
    }
}

public sealed record PermissionOverrideDeniedV1Data(
    Guid OverrideId,
    Guid UserId,
    Guid OrganizationId,
    Permission Permission,
    Guid GrantedBy,
    DateTime DeniedAtUtc);
