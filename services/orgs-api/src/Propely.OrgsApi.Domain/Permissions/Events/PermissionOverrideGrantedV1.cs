// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Domain.Common;

namespace Propely.OrgsApi.Domain.Permissions.Events;

/// <summary>
/// Domain event raised when a permission override is granted (Granted=true).
/// </summary>
public sealed record PermissionOverrideGrantedV1 : IDomainEvent
{
    public Guid EventId { get; }
    public string EventType => nameof(PermissionOverrideGrantedV1);
    public int SchemaVersion => 1;
    public DateTime OccurredAtUtc { get; }
    public Guid? CorrelationId { get; init; }
    public Guid? CausationId { get; init; }
    public string Producer => "OrgsApi";

    public PermissionOverrideGrantedV1Data Data { get; }

    public PermissionOverrideGrantedV1(
        Guid overrideId, Guid userId, Guid organizationId,
        Permission permission, Guid grantedBy, DateTime grantedAtUtc)
    {
        EventId = Guid.NewGuid();
        OccurredAtUtc = grantedAtUtc;
        Data = new PermissionOverrideGrantedV1Data(
            overrideId, userId, organizationId, permission, grantedBy, grantedAtUtc);
    }
}

public sealed record PermissionOverrideGrantedV1Data(
    Guid OverrideId,
    Guid UserId,
    Guid OrganizationId,
    Permission Permission,
    Guid GrantedBy,
    DateTime GrantedAtUtc);
