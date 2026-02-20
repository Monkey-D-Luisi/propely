// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Common.Models;
using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Domain.Common.Exceptions;
using MediatR;

namespace Propely.OrgsApi.Application.Organizations.Queries.GetMembers;

public sealed class GetMembersQueryHandler : IRequestHandler<GetMembersQuery, PagedResult<MemberDto>>
{
    private readonly IMembershipRepository _membershipRepository;

    public GetMembersQueryHandler(IMembershipRepository membershipRepository)
    {
        _membershipRepository = membershipRepository;
    }

    public async Task<PagedResult<MemberDto>> Handle(GetMembersQuery request, CancellationToken cancellationToken)
    {
        var membership = await _membershipRepository.GetAsync(
            request.OrgId, request.RequestingUserId, cancellationToken);

        if (membership is null)
            throw new ForbiddenException("User is not a member of this organization.");

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        return await _membershipRepository.GetMembersPagedAsync(
            request.OrgId, page, pageSize, request.Search, cancellationToken);
    }
}
