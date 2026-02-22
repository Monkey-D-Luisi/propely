// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Permissions.Interfaces;
using Propely.OrgsApi.Domain.Permissions;
using Microsoft.EntityFrameworkCore;

namespace Propely.OrgsApi.Infrastructure.Persistence.Repositories;

public sealed class PermissionOverrideRepository : IPermissionOverrideRepository
{
    private readonly AppDbContext _context;

    public PermissionOverrideRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<PermissionOverride>> GetOverridesAsync(
        Guid userId, Guid organizationId, CancellationToken ct)
    {
        return await _context.PermissionOverrides
            .Where(p => p.UserId == userId && p.OrganizationId == organizationId)
            .ToListAsync(ct);
    }

    public async Task<PermissionOverride?> GetOverrideAsync(
        Guid userId, Guid organizationId, Permission permission, CancellationToken ct)
    {
        return await _context.PermissionOverrides
            .FirstOrDefaultAsync(p =>
                p.UserId == userId &&
                p.OrganizationId == organizationId &&
                p.Permission == permission, ct);
    }

    public async Task AddAsync(PermissionOverride permissionOverride, CancellationToken ct)
    {
        await _context.PermissionOverrides.AddAsync(permissionOverride, ct);
    }

    public Task RemoveAsync(PermissionOverride permissionOverride, CancellationToken ct)
    {
        _context.PermissionOverrides.Remove(permissionOverride);
        return Task.CompletedTask;
    }
}
