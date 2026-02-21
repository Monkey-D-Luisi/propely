// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Agencies.Interfaces;
using Propely.OrgsApi.Domain.Agencies;
using Microsoft.EntityFrameworkCore;

namespace Propely.OrgsApi.Infrastructure.Persistence.Repositories;

public sealed class AgencyRepository : IAgencyRepository
{
    private readonly AppDbContext _context;

    public AgencyRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Agency agency, CancellationToken cancellationToken = default)
    {
        await _context.Agencies.AddAsync(agency, cancellationToken);
    }

    public async Task<Agency?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var agency = await _context.Agencies.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (agency is not null)
        {
            var branchIds = await GetBranchIdsForAgencyAsync(agency.Id, cancellationToken);
            agency.HydrateBranches(branchIds);
        }
        return agency;
    }

    public async Task<Agency?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var agency = await _context.Agencies.FirstOrDefaultAsync(a => EF.Property<string>(a, "Slug") == slug, cancellationToken);
        if (agency is not null)
        {
            var branchIds = await GetBranchIdsForAgencyAsync(agency.Id, cancellationToken);
            agency.HydrateBranches(branchIds);
        }
        return agency;
    }

    public async Task<bool> ExistsBySlugAsync(string slug, Guid? excludeAgencyId = null, CancellationToken cancellationToken = default)
    {
        return await _context.Agencies
            .Where(a => EF.Property<string>(a, "Slug") == slug)
            .Where(a => excludeAgencyId == null || a.Id != excludeAgencyId)
            .AnyAsync(cancellationToken);
    }

    public async Task<List<Agency>> ListForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Find agencies where the user is the creator (owner)
        var ownedAgencies = await _context.Agencies
            .Where(a => a.CreatedByUserId == userId)
            .ToListAsync(cancellationToken);

        // Find agencies where the user is a member of one of the branches
        var userOrgIds = await _context.Memberships
            .Where(m => m.UserId == userId)
            .Select(m => m.OrganizationId)
            .ToListAsync(cancellationToken);

        var memberAgencyIds = await _context.Organizations
            .Where(o => userOrgIds.Contains(o.Id) && o.AgencyId != null)
            .Select(o => o.AgencyId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        var ownedAgencyIds = ownedAgencies.Select(a => a.Id).ToHashSet();
        var memberAgencies = await _context.Agencies
            .Where(a => memberAgencyIds.Contains(a.Id) && !ownedAgencyIds.Contains(a.Id))
            .ToListAsync(cancellationToken);

        var allAgencies = ownedAgencies.Concat(memberAgencies).ToList();

        // Batch-load all branch IDs in a single query
        var allAgencyIds = allAgencies.Select(a => a.Id).ToList();
        var branchIdsByAgency = await _context.Organizations
            .Where(o => o.AgencyId != null && allAgencyIds.Contains(o.AgencyId.Value))
            .GroupBy(o => o.AgencyId!.Value)
            .ToDictionaryAsync(
                g => g.Key,
                g => g.Select(o => o.Id).ToList(),
                cancellationToken);

        foreach (var agency in allAgencies)
        {
            if (branchIdsByAgency.TryGetValue(agency.Id, out var branchIds))
            {
                agency.HydrateBranches(branchIds);
            }
        }

        return allAgencies;
    }

    private async Task<List<Guid>> GetBranchIdsForAgencyAsync(Guid agencyId, CancellationToken cancellationToken)
    {
        return await _context.Organizations
            .Where(o => o.AgencyId == agencyId)
            .Select(o => o.Id)
            .ToListAsync(cancellationToken);
    }
}
