// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Domain.Common;
using Propely.OrgsApi.Domain.Permissions.Events;

namespace Propely.OrgsApi.Domain.Permissions;

/// <summary>
/// Represents a per-user permission override that extends or restricts their base role capabilities.
/// </summary>
public sealed class PermissionOverride : Entity
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid OrganizationId { get; private set; }
    public Permission Permission { get; private set; }
    public bool Granted { get; private set; }
    public Guid GrantedBy { get; private set; }
    public DateTime GrantedAtUtc { get; private set; }

    private PermissionOverride() { }

    /// <summary>
    /// Creates a new permission override. Raises PermissionOverrideGrantedV1 if granted=true,
    /// or PermissionOverrideDeniedV1 if granted=false.
    /// </summary>
    public static PermissionOverride Create(
        Guid userId, Guid organizationId, Permission permission, bool granted, Guid grantedBy)
    {
        var now = DateTime.UtcNow;
        var permissionOverride = new PermissionOverride
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            OrganizationId = organizationId,
            Permission = permission,
            Granted = granted,
            GrantedBy = grantedBy,
            GrantedAtUtc = now
        };

        if (granted)
        {
            permissionOverride.RaiseDomainEvent(new PermissionOverrideGrantedV1(
                permissionOverride.Id, userId, organizationId, permission, grantedBy, now));
        }
        else
        {
            permissionOverride.RaiseDomainEvent(new PermissionOverrideDeniedV1(
                permissionOverride.Id, userId, organizationId, permission, grantedBy, now));
        }

        return permissionOverride;
    }

    /// <summary>
    /// Revokes this override, raising PermissionOverrideRevokedV1.
    /// </summary>
    public void Revoke()
    {
        RaiseDomainEvent(new PermissionOverrideRevokedV1(
            Id, UserId, OrganizationId, Permission, DateTime.UtcNow));
    }
}
