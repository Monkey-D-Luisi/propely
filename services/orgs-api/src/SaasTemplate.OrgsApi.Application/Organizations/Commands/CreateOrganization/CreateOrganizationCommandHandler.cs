// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Application.Billing.Interfaces;
using SaasTemplate.OrgsApi.Application.Common.Interfaces;
using SaasTemplate.OrgsApi.Application.Organizations.Interfaces;
using SaasTemplate.OrgsApi.Domain.Common.Exceptions;
using SaasTemplate.OrgsApi.Domain.Organizations;
using MediatR;

namespace SaasTemplate.OrgsApi.Application.Organizations.Commands.CreateOrganization;

public sealed class CreateOrganizationCommandHandler : IRequestHandler<CreateOrganizationCommand, CreateOrganizationResult>
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEntitlementService _entitlementService;

    public CreateOrganizationCommandHandler(
        IOrganizationRepository organizationRepository,
        IMembershipRepository membershipRepository,
        IUnitOfWork unitOfWork,
        IEntitlementService entitlementService)
    {
        _organizationRepository = organizationRepository;
        _membershipRepository = membershipRepository;
        _unitOfWork = unitOfWork;
        _entitlementService = entitlementService;
    }

    public async Task<CreateOrganizationResult> Handle(CreateOrganizationCommand request, CancellationToken cancellationToken)
    {
        if (!await _entitlementService.CanCreateOrganizationAsync(request.OwnerUserId, cancellationToken))
            throw new ForbiddenException("Organization limit reached for your current plan.");

        var normalizedName = request.Name.Trim();

        if (await _organizationRepository.ExistsByNameAsync(normalizedName, cancellationToken: cancellationToken))
            throw new ConflictException("An organization with this name already exists.");

        var org = Organization.Create(normalizedName);
        var membership = Membership.Create(request.OwnerUserId, org.Id, MembershipRole.Owner);

        await _organizationRepository.AddAsync(org, cancellationToken);
        await _membershipRepository.AddAsync(membership, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateOrganizationResult(org.Id, org.Name);
    }
}
