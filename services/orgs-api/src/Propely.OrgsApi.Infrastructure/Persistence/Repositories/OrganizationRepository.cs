// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Domain.Organizations;
using Microsoft.EntityFrameworkCore;

namespace Propely.OrgsApi.Infrastructure.Persistence.Repositories;

public sealed class OrganizationRepository : IOrganizationRepository
{
    private readonly AppDbContext _context;

    public OrganizationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Organization organization, CancellationToken cancellationToken = default)
    {
        await _context.Organizations.AddAsync(organization, cancellationToken);
    }

    public async Task<Organization?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Organizations.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<List<Organization>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        if (idList.Count == 0) return [];

        return await _context.Organizations
            .Where(o => idList.Contains(o.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeOrgId = null, CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim();

        return await _context.Organizations
            .Where(o => o.Name.ToLower() == normalizedName.ToLower())
            .Where(o => excludeOrgId == null || o.Id != excludeOrgId)
            .AnyAsync(cancellationToken);
    }
}
