// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Application.Permissions.DTOs;
using Propely.OrgsApi.Application.Permissions.Interfaces;
using Propely.OrgsApi.Domain.Organizations;
using Propely.OrgsApi.Domain.Permissions;

namespace Propely.OrgsApi.Application.Permissions.Services;

/// <summary>
/// Evaluates a user's effective permissions by combining role defaults with overrides.
/// Owner role always has all permissions regardless of overrides.
/// </summary>
public sealed class PermissionEvaluator : IPermissionEvaluator
{
    private readonly IMembershipRepository _membershipRepository;
    private readonly IPermissionOverrideRepository _overrideRepository;

    public PermissionEvaluator(
        IMembershipRepository membershipRepository,
        IPermissionOverrideRepository overrideRepository)
    {
        _membershipRepository = membershipRepository;
        _overrideRepository = overrideRepository;
    }

    public async Task<bool> HasPermissionAsync(
        Guid userId, Guid organizationId, Permission permission, CancellationToken ct)
    {
        var membership = await _membershipRepository.GetAsync(organizationId, userId, ct);
        if (membership is null || membership.IsDeleted)
            return false;

        // Owner always has all permissions
        if (membership.Role == MembershipRole.Owner)
            return true;

        var roleDefaults = DefaultPermissionMatrix.GetDefaults(membership.Role);
        var hasDefault = roleDefaults.Contains(permission);

        var permissionOverride = await _overrideRepository.GetOverrideAsync(userId, organizationId, permission, ct);

        if (permissionOverride is not null)
            return permissionOverride.Granted;

        return hasDefault;
    }

    public async Task<IReadOnlyList<EffectivePermissionDto>> GetEffectivePermissionsAsync(
        Guid userId, Guid organizationId, CancellationToken ct)
    {
        var membership = await _membershipRepository.GetAsync(organizationId, userId, ct);
        if (membership is null || membership.IsDeleted)
            return [];

        var roleDefaults = DefaultPermissionMatrix.GetDefaults(membership.Role);
        var overrides = await _overrideRepository.GetOverridesAsync(userId, organizationId, ct);
        var overrideLookup = overrides.ToDictionary(o => o.Permission);

        var allPermissions = Enum.GetValues<Permission>();
        var result = new List<EffectivePermissionDto>(allPermissions.Length);

        foreach (var permission in allPermissions)
        {
            // Owner always has all permissions
            if (membership.Role == MembershipRole.Owner)
            {
                result.Add(new EffectivePermissionDto(permission, true, "Role Default"));
                continue;
            }

            if (overrideLookup.TryGetValue(permission, out var ov))
            {
                result.Add(new EffectivePermissionDto(permission, ov.Granted, "Override"));
            }
            else
            {
                var granted = roleDefaults.Contains(permission);
                result.Add(new EffectivePermissionDto(permission, granted, "Role Default"));
            }
        }

        return result;
    }
}
