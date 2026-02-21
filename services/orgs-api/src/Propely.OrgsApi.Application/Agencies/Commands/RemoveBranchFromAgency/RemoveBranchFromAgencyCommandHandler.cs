// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Agencies.Interfaces;
using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Domain.Common.Exceptions;
using MediatR;

namespace Propely.OrgsApi.Application.Agencies.Commands.RemoveBranchFromAgency;

public sealed class RemoveBranchFromAgencyCommandHandler : IRequestHandler<RemoveBranchFromAgencyCommand>
{
    private readonly IAgencyRepository _agencyRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveBranchFromAgencyCommandHandler(
        IAgencyRepository agencyRepository,
        IOrganizationRepository organizationRepository,
        IUnitOfWork unitOfWork)
    {
        _agencyRepository = agencyRepository;
        _organizationRepository = organizationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(RemoveBranchFromAgencyCommand request, CancellationToken cancellationToken)
    {
        var agency = await _agencyRepository.GetByIdAsync(request.AgencyId, cancellationToken)
            ?? throw new NotFoundException("Agency not found.");

        if (agency.CreatedByUserId != request.RequestingUserId)
            throw new ForbiddenException("Only the agency owner can remove branches.");

        var organization = await _organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken)
            ?? throw new NotFoundException("Organization not found.");

        agency.RemoveBranch(request.OrganizationId);
        organization.RemoveFromAgency();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
