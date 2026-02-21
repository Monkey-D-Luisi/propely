// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Agencies.Interfaces;
using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Domain.Common.Exceptions;
using MediatR;

namespace Propely.OrgsApi.Application.Agencies.Commands.AddBranchToAgency;

public sealed class AddBranchToAgencyCommandHandler : IRequestHandler<AddBranchToAgencyCommand>
{
    private readonly IAgencyRepository _agencyRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddBranchToAgencyCommandHandler(
        IAgencyRepository agencyRepository,
        IOrganizationRepository organizationRepository,
        IUnitOfWork unitOfWork)
    {
        _agencyRepository = agencyRepository;
        _organizationRepository = organizationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AddBranchToAgencyCommand request, CancellationToken cancellationToken)
    {
        var agency = await _agencyRepository.GetByIdAsync(request.AgencyId, cancellationToken)
            ?? throw new NotFoundException("Agency not found.");

        if (agency.CreatedByUserId != request.RequestingUserId)
            throw new ForbiddenException("Only the agency owner can add branches.");

        var organization = await _organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken)
            ?? throw new NotFoundException("Organization not found.");

        if (organization.AgencyId is not null)
            throw new DomainException("This organization is already assigned to an agency.");

        agency.AddBranch(request.OrganizationId);
        organization.AssignToAgency(request.AgencyId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
