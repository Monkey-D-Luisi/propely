// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Domain.Common.Exceptions;
using Propely.OrgsApi.Domain.Organizations;
using MediatR;

namespace Propely.OrgsApi.Application.Organizations.Commands.UpdateOrganization;

public sealed class UpdateOrganizationCommandHandler : IRequestHandler<UpdateOrganizationCommand, UpdateOrganizationResult>
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateOrganizationCommandHandler(
        IOrganizationRepository organizationRepository,
        IMembershipRepository membershipRepository,
        IUnitOfWork unitOfWork)
    {
        _organizationRepository = organizationRepository;
        _membershipRepository = membershipRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdateOrganizationResult> Handle(UpdateOrganizationCommand request, CancellationToken cancellationToken)
    {
        var membership = await _membershipRepository.GetAsync(
            request.OrgId, request.RequestingUserId, cancellationToken);

        if (membership is null)
            throw new ForbiddenException("User is not a member of this organization.");

        if (membership.Role is not (MembershipRole.Owner or MembershipRole.Admin))
            throw new ForbiddenException("Only owners and admins can update organization settings.");

        var org = await _organizationRepository.GetByIdAsync(request.OrgId, cancellationToken)
            ?? throw new NotFoundException($"Organization {request.OrgId} not found.");

        var normalizedName = request.Name.Trim();

        if (!string.Equals(org.Name, normalizedName, StringComparison.OrdinalIgnoreCase)
            && await _organizationRepository.ExistsByNameAsync(normalizedName, request.OrgId, cancellationToken))
            throw new ConflictException("An organization with this name already exists.");

        org.Update(normalizedName, request.Description);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateOrganizationResult(org.Id, org.Name, org.Description);
    }
}
