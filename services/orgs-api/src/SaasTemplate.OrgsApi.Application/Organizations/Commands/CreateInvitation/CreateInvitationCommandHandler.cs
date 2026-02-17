// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Application.Billing.Interfaces;
using SaasTemplate.OrgsApi.Application.Common.Interfaces;
using SaasTemplate.OrgsApi.Application.Organizations.Interfaces;
using SaasTemplate.OrgsApi.Domain.Common.Exceptions;
using SaasTemplate.OrgsApi.Domain.Organizations;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace SaasTemplate.OrgsApi.Application.Organizations.Commands.CreateInvitation;

public sealed class CreateInvitationCommandHandler : IRequestHandler<CreateInvitationCommand, CreateInvitationResult>
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IInvitationRepository _invitationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IEntitlementService _entitlementService;
    private readonly string _frontendBaseUrl;

    public CreateInvitationCommandHandler(
        IOrganizationRepository organizationRepository,
        IMembershipRepository membershipRepository,
        IInvitationRepository invitationRepository,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IEntitlementService entitlementService,
        IConfiguration configuration)
    {
        _organizationRepository = organizationRepository;
        _membershipRepository = membershipRepository;
        _invitationRepository = invitationRepository;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _entitlementService = entitlementService;
        _frontendBaseUrl = configuration["Auth:FrontendBaseUrl"]?.TrimEnd('/') ?? "http://localhost:3000";
    }

    public async Task<CreateInvitationResult> Handle(CreateInvitationCommand request, CancellationToken cancellationToken)
    {
        // Verify requesting user is an Owner or Admin of the org
        var requestingMembership = await _membershipRepository.GetAsync(request.OrgId, request.RequestingUserId, cancellationToken)
            ?? throw new ForbiddenException("Not authorized.");

        if (requestingMembership.Role != MembershipRole.Owner && requestingMembership.Role != MembershipRole.Admin)
        {
            throw new ForbiddenException("Only owners and admins can invite members.");
        }

        var org = await _organizationRepository.GetByIdAsync(request.OrgId, cancellationToken)
            ?? throw new NotFoundException("Organization not found.");

        if (!await _entitlementService.CanAddMemberAsync(request.OrgId, cancellationToken))
            throw new ForbiddenException("Member limit reached for your current plan.");

        // Admins cannot invite as Owner or Admin
        if (requestingMembership.Role == MembershipRole.Admin &&
            (request.Role == MembershipRole.Owner || request.Role == MembershipRole.Admin))
        {
            throw new ForbiddenException("Admins can only invite members and viewers.");
        }

        var invitation = Invitation.Create(request.OrgId, request.Email, request.Role);

        await _invitationRepository.AddAsync(invitation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var inviteUrl = $"{_frontendBaseUrl}/orgs/accept-invite?token={invitation.Token}";

        // Send invitation email
        await _emailService.SendOrgInvitationEmailAsync(request.Email, org.Name, inviteUrl, cancellationToken);

        return new CreateInvitationResult(true, inviteUrl);
    }
}
