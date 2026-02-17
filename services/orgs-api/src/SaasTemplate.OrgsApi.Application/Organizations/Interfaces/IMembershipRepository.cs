// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Application.Common.Models;
using SaasTemplate.OrgsApi.Application.Organizations.Queries.GetMembers;
using SaasTemplate.OrgsApi.Application.Organizations.Queries.GetMyOrgs;
using SaasTemplate.OrgsApi.Domain.Organizations;

namespace SaasTemplate.OrgsApi.Application.Organizations.Interfaces;

public interface IMembershipRepository
{
    Task AddAsync(Membership membership, CancellationToken cancellationToken = default);
    Task<Membership?> GetAsync(Guid orgId, Guid userId, CancellationToken cancellationToken = default);
    Task<List<Membership>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<Membership>> GetByOrgIdAsync(Guid orgId, CancellationToken cancellationToken = default);
    Task<List<Membership>> GetByOrgIdsAsync(IEnumerable<Guid> orgIds, CancellationToken cancellationToken = default);
    Task<List<Membership>> GetByOrgIdForUpdateAsync(Guid orgId, CancellationToken cancellationToken = default);
    Task<PagedResult<OrgWithRole>> GetOrgsPagedAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<MemberDto>> GetMembersPagedAsync(Guid orgId, int page, int pageSize, string? search = null, CancellationToken cancellationToken = default);
}
