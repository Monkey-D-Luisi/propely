// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.FeatureFlags.Interfaces;
using Propely.OrgsApi.Domain.FeatureFlags;
using Microsoft.EntityFrameworkCore;

namespace Propely.OrgsApi.Infrastructure.Persistence.Repositories;

public sealed class FeatureFlagRepository : IFeatureFlagRepository
{
    private readonly AppDbContext _context;

    public FeatureFlagRepository(AppDbContext context) => _context = context;

    public async Task<FeatureFlag?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.FeatureFlags
            .FirstOrDefaultAsync(f => f.Name == name, cancellationToken);
    }

    public async Task<List<FeatureFlag>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.FeatureFlags.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(FeatureFlag flag, CancellationToken cancellationToken = default)
    {
        await _context.FeatureFlags.AddAsync(flag, cancellationToken);
    }
}
