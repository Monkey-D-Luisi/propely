// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Client.Dtos;
using Refit;

namespace Propely.OrgsApi.Client.Permissions;

/// <summary>
/// Refit client interface for the orgs-api permission management endpoints.
/// </summary>
public interface IPermissionsApi
{
    /// <summary>
    /// Get effective permissions for a user in an organization.
    /// </summary>
    [Get("/api/organizations/{orgId}/permissions/{userId}")]
    Task<List<EffectivePermissionResponse>> GetEffectivePermissionsAsync(
        Guid orgId, Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Set a permission override for a user. Only admins and owners can do this.
    /// </summary>
    [Put("/api/organizations/{orgId}/permissions/{userId}/{permission}")]
    Task SetPermissionOverrideAsync(
        Guid orgId, Guid userId, string permission,
        [Body] Dtos.SetPermissionOverrideRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Remove a permission override, reverting to role default. Only admins and owners can do this.
    /// </summary>
    [Delete("/api/organizations/{orgId}/permissions/{userId}/{permission}")]
    Task RemovePermissionOverrideAsync(
        Guid orgId, Guid userId, string permission,
        CancellationToken ct = default);
}
