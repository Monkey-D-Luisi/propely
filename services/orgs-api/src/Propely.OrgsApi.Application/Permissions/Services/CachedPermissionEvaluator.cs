// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Microsoft.Extensions.Logging;
using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Application.Permissions.DTOs;
using Propely.OrgsApi.Application.Permissions.Interfaces;
using Propely.OrgsApi.Domain.Permissions;

namespace Propely.OrgsApi.Application.Permissions.Services;

/// <summary>
/// Decorator that adds Redis caching to permission evaluation with a 30-second TTL.
/// </summary>
public sealed class CachedPermissionEvaluator : IPermissionEvaluator
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(30);
    private const string CacheKeyPrefix = "permissions";

    private readonly PermissionEvaluator _inner;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachedPermissionEvaluator> _logger;

    public CachedPermissionEvaluator(
        PermissionEvaluator inner,
        ICacheService cacheService,
        ILogger<CachedPermissionEvaluator> logger)
    {
        _inner = inner;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<bool> HasPermissionAsync(
        Guid userId, Guid organizationId, Permission permission, CancellationToken ct)
    {
        var cacheKey = BuildCacheKey(userId, organizationId);

        var cached = await _cacheService.GetAsync<CachedPermissions>(cacheKey, ct);
        if (cached is not null)
        {
            var permissionName = permission.ToString();
            if (cached.Permissions.TryGetValue(permissionName, out var granted))
                return granted;
        }

        // Cache miss: fetch all permissions, populate cache, return the requested one
        var allPermissions = await _inner.GetEffectivePermissionsAsync(userId, organizationId, ct);

        var toCache = new CachedPermissions
        {
            Permissions = allPermissions.ToDictionary(p => p.Permission.ToString(), p => p.Granted),
            EffectivePermissions = allPermissions.ToList()
        };

        await _cacheService.SetAsync(cacheKey, toCache, CacheTtl, ct);

        var match = allPermissions.FirstOrDefault(p => p.Permission == permission);
        return match?.Granted ?? false;
    }

    public async Task<IReadOnlyList<EffectivePermissionDto>> GetEffectivePermissionsAsync(
        Guid userId, Guid organizationId, CancellationToken ct)
    {
        var cacheKey = BuildCacheKey(userId, organizationId);

        var cached = await _cacheService.GetAsync<CachedPermissions>(cacheKey, ct);
        if (cached is not null)
        {
            return cached.EffectivePermissions;
        }

        var result = await _inner.GetEffectivePermissionsAsync(userId, organizationId, ct);

        var toCache = new CachedPermissions
        {
            Permissions = result.ToDictionary(p => p.Permission.ToString(), p => p.Granted),
            EffectivePermissions = result.ToList()
        };

        await _cacheService.SetAsync(cacheKey, toCache, CacheTtl, ct);

        return result;
    }

    /// <summary>
    /// Invalidates the cached permissions for a user in an organization.
    /// Called after permission overrides are changed.
    /// </summary>
    public async Task InvalidateAsync(Guid userId, Guid organizationId, CancellationToken ct)
    {
        var cacheKey = BuildCacheKey(userId, organizationId);
        await _cacheService.RemoveAsync(cacheKey, ct);
        _logger.LogInformation("Invalidated permission cache for user {UserId} in org {OrgId}", userId, organizationId);
    }

    private static string BuildCacheKey(Guid userId, Guid organizationId) =>
        $"{CacheKeyPrefix}:{userId}:{organizationId}";

    private sealed class CachedPermissions
    {
        public Dictionary<string, bool> Permissions { get; init; } = [];
        public List<EffectivePermissionDto> EffectivePermissions { get; init; } = [];
    }
}
