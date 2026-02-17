// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Application.Common.Models;
using SaasTemplate.OrgsApi.Application.Organizations.Interfaces;
using MediatR;

namespace SaasTemplate.OrgsApi.Application.Organizations.Queries.GetMyOrgs;

public sealed class GetMyOrgsQueryHandler : IRequestHandler<GetMyOrgsQuery, PagedResult<OrgWithRole>>
{
    private readonly IMembershipRepository _membershipRepository;

    public GetMyOrgsQueryHandler(IMembershipRepository membershipRepository)
    {
        _membershipRepository = membershipRepository;
    }

    public async Task<PagedResult<OrgWithRole>> Handle(GetMyOrgsQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        return await _membershipRepository.GetOrgsPagedAsync(
            request.UserId, page, pageSize, cancellationToken);
    }
}
