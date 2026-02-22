// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Domain.Permissions;

namespace Propely.OrgsApi.Application.Permissions.Interfaces;

/// <summary>
/// Repository interface for permission overrides.
/// </summary>
public interface IPermissionOverrideRepository
{
    Task<List<PermissionOverride>> GetOverridesAsync(Guid userId, Guid organizationId, CancellationToken ct);
    Task<PermissionOverride?> GetOverrideAsync(Guid userId, Guid organizationId, Permission permission, CancellationToken ct);
    Task AddAsync(PermissionOverride permissionOverride, CancellationToken ct);
    Task RemoveAsync(PermissionOverride permissionOverride, CancellationToken ct);
}
