// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Client.Permissions;

/// <summary>
/// Exception thrown when a permission check fails (user is not authorized).
/// </summary>
public sealed class ForbiddenException : Exception
{
    public Guid UserId { get; }
    public Guid OrganizationId { get; }
    public string Permission { get; }

    public ForbiddenException(Guid userId, Guid organizationId, string permission)
        : base($"User {userId} does not have permission '{permission}' in organization {organizationId}.")
    {
        UserId = userId;
        OrganizationId = organizationId;
        Permission = permission;
    }
}
