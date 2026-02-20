// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Common.Models;
using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Application.Organizations.Queries.GetMembers;
using Propely.OrgsApi.Application.Organizations.Queries.GetMyOrgs;
using Propely.OrgsApi.Domain.Organizations;
using Microsoft.EntityFrameworkCore;

namespace Propely.OrgsApi.Infrastructure.Persistence.Repositories;

public sealed class MembershipRepository : IMembershipRepository
{
    private readonly AppDbContext _context;

    public MembershipRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Membership membership, CancellationToken cancellationToken = default)
    {
        await _context.Memberships.AddAsync(membership, cancellationToken);
    }

    public async Task<Membership?> GetAsync(Guid orgId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Memberships
            .FirstOrDefaultAsync(m => m.OrganizationId == orgId && m.UserId == userId, cancellationToken);
    }

    public async Task<List<Membership>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Memberships
            .Where(m => m.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Membership>> GetByOrgIdAsync(Guid orgId, CancellationToken cancellationToken = default)
    {
        return await _context.Memberships
            .Where(m => m.OrganizationId == orgId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Membership>> GetByOrgIdsAsync(IEnumerable<Guid> orgIds, CancellationToken cancellationToken = default)
    {
        var ids = orgIds.ToList();
        if (ids.Count == 0) return [];

        return await _context.Memberships
            .Where(m => ids.Contains(m.OrganizationId))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Membership>> GetByOrgIdForUpdateAsync(Guid orgId, CancellationToken cancellationToken = default)
    {
        // Pessimistic lock: FOR UPDATE prevents concurrent modifications to the same org's memberships.
        // IgnoreQueryFilters is required because EF Core wraps raw SQL in a subquery when a global
        // query filter exists, which is incompatible with PostgreSQL's FOR UPDATE clause.
        // The soft-delete filter is applied directly in the raw SQL instead.
        var entityType = _context.Model.FindEntityType(typeof(Membership))!;
        var tableName = entityType.GetTableName();
        var orgIdCol = entityType.FindProperty(nameof(Membership.OrganizationId))!.GetColumnName();
        var isDeletedCol = entityType.FindProperty(nameof(Membership.IsDeleted))!.GetColumnName();

        return await _context.Memberships
            .FromSqlRaw(
                $"SELECT * FROM {tableName} WHERE {orgIdCol} = {{0}} AND {isDeletedCol} = FALSE FOR UPDATE",
                orgId)
            .IgnoreQueryFilters()
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<OrgWithRole>> GetOrgsPagedAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Memberships
            .Where(m => m.UserId == userId)
            .Join(
                _context.Organizations,
                m => m.OrganizationId,
                o => o.Id,
                (m, o) => new { Membership = m, Organization = o });

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.Organization.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new OrgWithRole(
                x.Organization.Id,
                x.Organization.Name,
                x.Organization.Description,
                x.Membership.Role.ToString().ToLowerInvariant()))
            .ToListAsync(cancellationToken);

        return new PagedResult<OrgWithRole>(items, totalCount, page, pageSize);
    }

    public async Task<PagedResult<MemberDto>> GetMembersPagedAsync(
        Guid orgId, int page, int pageSize, string? search = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Memberships
            .Where(m => m.OrganizationId == orgId)
            .Join(
                _context.Users,
                m => m.UserId,
                u => u.Id,
                (m, u) => new { Membership = m, User = u });

        if (!string.IsNullOrWhiteSpace(search))
        {
            var escapedSearch = search
                .Replace("\\", "\\\\")
                .Replace("%", "\\%")
                .Replace("_", "\\_");
            var searchPattern = $"%{escapedSearch}%";
            query = query.Where(x =>
                EF.Functions.ILike(x.User.Email, searchPattern) ||
                (x.User.Name != null && EF.Functions.ILike(x.User.Name, searchPattern)));
        }

        var orderedQuery = query.OrderBy(x => x.User.Email);

        var totalCount = await orderedQuery.CountAsync(cancellationToken);
        var items = await orderedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new MemberDto(
                x.User.Id,
                x.User.Email,
                x.User.Name,
                x.Membership.Role.ToString().ToLowerInvariant()))
            .ToListAsync(cancellationToken);

        return new PagedResult<MemberDto>(items, totalCount, page, pageSize);
    }
}
