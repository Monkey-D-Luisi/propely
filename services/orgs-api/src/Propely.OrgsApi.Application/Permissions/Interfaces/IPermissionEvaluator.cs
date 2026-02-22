// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Permissions.DTOs;
using Propely.OrgsApi.Domain.Permissions;

namespace Propely.OrgsApi.Application.Permissions.Interfaces;

/// <summary>
/// Evaluates a user's effective permissions by combining role defaults with overrides.
/// </summary>
public interface IPermissionEvaluator
{
    /// <summary>
    /// Checks if the user has the specified permission in the given organization.
    /// </summary>
    Task<bool> HasPermissionAsync(Guid userId, Guid organizationId, Permission permission, CancellationToken ct);

    /// <summary>
    /// Returns all effective permissions for the user in the given organization.
    /// </summary>
    Task<IReadOnlyList<EffectivePermissionDto>> GetEffectivePermissionsAsync(Guid userId, Guid organizationId, CancellationToken ct);
}
