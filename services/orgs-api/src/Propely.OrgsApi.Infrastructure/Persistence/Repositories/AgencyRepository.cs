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
            await LoadBranchIdsAsync(agency, cancellationToken);
        }
        return agency;
    }

    public async Task<Agency?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var agency = await _context.Agencies.FirstOrDefaultAsync(a => EF.Property<string>(a, "Slug") == slug, cancellationToken);
        if (agency is not null)
        {
            await LoadBranchIdsAsync(agency, cancellationToken);
        }
        return agency;
    }

    public async Task<bool> ExistsBySlugAsync(string slug, Guid? excludeAgencyId = null, CancellationToken cancellationToken = default)
    {
        var normalizedSlug = slug.Trim().ToLower();

        return await _context.Agencies
            .Where(a => EF.Property<string>(a, "Slug") == normalizedSlug)
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

        var memberAgencies = await _context.Agencies
            .Where(a => memberAgencyIds.Contains(a.Id) && !ownedAgencies.Select(oa => oa.Id).Contains(a.Id))
            .ToListAsync(cancellationToken);

        var allAgencies = ownedAgencies.Concat(memberAgencies).ToList();

        // Load branch IDs for each agency
        foreach (var agency in allAgencies)
        {
            await LoadBranchIdsAsync(agency, cancellationToken);
        }

        return allAgencies;
    }

    private async Task LoadBranchIdsAsync(Agency agency, CancellationToken cancellationToken)
    {
        var branchIds = await _context.Organizations
            .Where(o => o.AgencyId == agency.Id)
            .Select(o => o.Id)
            .ToListAsync(cancellationToken);

        foreach (var branchId in branchIds)
        {
            // Only add if not already tracked (to avoid duplicates if called multiple times)
            if (!agency.BranchIds.Contains(branchId))
            {
                agency.AddBranch(branchId);
                // Clear the domain event since this is just loading, not a real add
            }
        }

        // Clear domain events raised during loading
        agency.ClearDomainEvents();
    }
}
