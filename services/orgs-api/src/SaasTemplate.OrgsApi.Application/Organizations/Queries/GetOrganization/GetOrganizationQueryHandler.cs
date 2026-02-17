// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Application.Organizations.Interfaces;
using SaasTemplate.OrgsApi.Application.Organizations.Queries.GetMyOrgs;
using SaasTemplate.OrgsApi.Domain.Common.Exceptions;
using MediatR;

namespace SaasTemplate.OrgsApi.Application.Organizations.Queries.GetOrganization;

public sealed class GetOrganizationQueryHandler : IRequestHandler<GetOrganizationQuery, OrgWithRole>
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IMembershipRepository _membershipRepository;

    public GetOrganizationQueryHandler(
        IOrganizationRepository organizationRepository,
        IMembershipRepository membershipRepository)
    {
        _organizationRepository = organizationRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<OrgWithRole> Handle(GetOrganizationQuery request, CancellationToken cancellationToken)
    {
        var membership = await _membershipRepository.GetAsync(
            request.OrgId, request.RequestingUserId, cancellationToken);

        if (membership is null)
            throw new ForbiddenException("User is not a member of this organization.");

        var org = await _organizationRepository.GetByIdAsync(request.OrgId, cancellationToken)
            ?? throw new NotFoundException($"Organization {request.OrgId} not found.");

        return new OrgWithRole(
            org.Id,
            org.Name,
            org.Description,
            membership.Role.ToString().ToLowerInvariant());
    }
}
