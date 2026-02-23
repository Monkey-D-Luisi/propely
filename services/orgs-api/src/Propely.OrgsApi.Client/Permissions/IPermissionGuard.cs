// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Client.Permissions;

/// <summary>
/// Simplified permission checking interface for cross-service authorization.
/// Wraps <see cref="IPermissionsApi"/> with convenience methods and fail-closed semantics.
/// </summary>
public interface IPermissionGuard
{
    /// <summary>
    /// Check if a user has a specific permission. Returns false if the permission is denied
    /// or if the permission service is unreachable (fail-closed).
    /// </summary>
    Task<bool> HasPermissionAsync(Guid userId, Guid organizationId, string permission, CancellationToken ct = default);

    /// <summary>
    /// Require a specific permission. Throws <see cref="ForbiddenException"/> if the permission
    /// is denied or if the permission service is unreachable (fail-closed).
    /// </summary>
    Task RequirePermissionAsync(Guid userId, Guid organizationId, string permission, CancellationToken ct = default);
}
