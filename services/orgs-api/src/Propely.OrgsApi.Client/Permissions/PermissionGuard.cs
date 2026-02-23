// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Microsoft.Extensions.Logging;

namespace Propely.OrgsApi.Client.Permissions;

/// <summary>
/// Default implementation of <see cref="IPermissionGuard"/> that wraps <see cref="IPermissionsApi"/>
/// with fail-closed semantics: if the permission service is unreachable, all checks deny access.
/// </summary>
public sealed class PermissionGuard : IPermissionGuard
{
    private readonly IPermissionsApi _permissionsApi;
    private readonly ILogger<PermissionGuard> _logger;

    public PermissionGuard(IPermissionsApi permissionsApi, ILogger<PermissionGuard> logger)
    {
        _permissionsApi = permissionsApi ?? throw new ArgumentNullException(nameof(permissionsApi));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> HasPermissionAsync(
        Guid userId, Guid organizationId, string permission, CancellationToken ct = default)
    {
        try
        {
            var permissions = await _permissionsApi.GetEffectivePermissionsAsync(
                organizationId, userId, ct);

            var match = permissions.FirstOrDefault(p =>
                string.Equals(p.Permission, permission, StringComparison.OrdinalIgnoreCase));

            return match?.Granted ?? false;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Permission check failed for user {UserId} in org {OrgId} for permission {Permission}. Denying access (fail-closed).",
                userId, organizationId, permission);
            return false;
        }
    }

    public async Task RequirePermissionAsync(
        Guid userId, Guid organizationId, string permission, CancellationToken ct = default)
    {
        var hasPermission = await HasPermissionAsync(userId, organizationId, permission, ct);

        if (!hasPermission)
        {
            throw new ForbiddenException(userId, organizationId, permission);
        }
    }
}
