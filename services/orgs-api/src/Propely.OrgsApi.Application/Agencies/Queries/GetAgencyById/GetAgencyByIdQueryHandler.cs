// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Agencies.DTOs;
using Propely.OrgsApi.Application.Agencies.Interfaces;
using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Domain.Common.Exceptions;
using MediatR;

namespace Propely.OrgsApi.Application.Agencies.Queries.GetAgencyById;

public sealed class GetAgencyByIdQueryHandler : IRequestHandler<GetAgencyByIdQuery, AgencyDetailDto>
{
    private readonly IAgencyRepository _agencyRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IMembershipRepository _membershipRepository;

    public GetAgencyByIdQueryHandler(
        IAgencyRepository agencyRepository,
        IOrganizationRepository organizationRepository,
        IMembershipRepository membershipRepository)
    {
        _agencyRepository = agencyRepository;
        _organizationRepository = organizationRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<AgencyDetailDto> Handle(GetAgencyByIdQuery request, CancellationToken cancellationToken)
    {
        var agency = await _agencyRepository.GetByIdAsync(request.AgencyId, cancellationToken)
            ?? throw new NotFoundException("Agency not found.");

        // Verify the requesting user has access (owner or member of any branch)
        var isOwner = agency.CreatedByUserId == request.RequestingUserId;
        if (!isOwner)
        {
            var userMemberships = await _membershipRepository.GetByUserIdAsync(request.RequestingUserId, cancellationToken);
            var userOrgIds = userMemberships.Select(m => m.OrganizationId).ToHashSet();
            var hasAccess = agency.BranchIds.Any(branchId => userOrgIds.Contains(branchId));
            if (!hasAccess)
                throw new ForbiddenException("You do not have access to this agency.");
        }

        var branches = new List<AgencyBranchDto>();
        if (agency.BranchIds.Count > 0)
        {
            var orgs = await _organizationRepository.GetByIdsAsync(agency.BranchIds, cancellationToken);
            branches = orgs.Select(o => new AgencyBranchDto(o.Id, o.Name, o.CreatedAtUtc)).ToList();
        }

        return new AgencyDetailDto(
            agency.Id,
            agency.Name,
            agency.Slug.Value,
            agency.CreatedByUserId,
            agency.CreatedAtUtc,
            agency.UpdatedAtUtc,
            branches);
    }
}
