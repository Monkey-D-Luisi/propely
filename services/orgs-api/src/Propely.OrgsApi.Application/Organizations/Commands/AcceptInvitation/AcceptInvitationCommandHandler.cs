// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Application.Notifications.Interfaces;
using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Domain.Common.Exceptions;
using Propely.OrgsApi.Domain.Notifications;
using Propely.OrgsApi.Domain.Organizations;
using MediatR;

namespace Propely.OrgsApi.Application.Organizations.Commands.AcceptInvitation;

public sealed class AcceptInvitationCommandHandler : IRequestHandler<AcceptInvitationCommand>
{
    private readonly IInvitationRepository _invitationRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AcceptInvitationCommandHandler(
        IInvitationRepository invitationRepository,
        IMembershipRepository membershipRepository,
        IOrganizationRepository organizationRepository,
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork)
    {
        _invitationRepository = invitationRepository;
        _membershipRepository = membershipRepository;
        _organizationRepository = organizationRepository;
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AcceptInvitationCommand request, CancellationToken cancellationToken)
    {
        var invitation = await _invitationRepository.GetByTokenAsync(request.Token, cancellationToken)
            ?? throw new DomainException("INVALID_TOKEN");

        if (invitation.Status == InvitationStatus.Accepted)
        {
            throw new DomainException("ALREADY_USED");
        }

        if (invitation.ExpiresAtUtc < DateTime.UtcNow)
        {
            throw new DomainException("EXPIRED");
        }

        if (!string.Equals(invitation.Email, request.UserEmail, StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException("USER_EMAIL_MISMATCH");
        }

        // Check if already a member
        var existing = await _membershipRepository.GetAsync(invitation.OrganizationId, request.UserId, cancellationToken);
        if (existing is null)
        {
            var membership = Membership.Create(request.UserId, invitation.OrganizationId, invitation.Role);
            await _membershipRepository.AddAsync(membership, cancellationToken);
        }

        invitation.Accept();

        // Notify org admins/owners about the accepted invitation
        var org = await _organizationRepository.GetByIdAsync(invitation.OrganizationId, cancellationToken);
        var orgName = org?.Name ?? "the organization";
        var allMembers = await _membershipRepository.GetByOrgIdAsync(invitation.OrganizationId, cancellationToken);
        var adminsAndOwners = allMembers
            .Where(m => m.Role is MembershipRole.Owner or MembershipRole.Admin && m.UserId != request.UserId);

        var notifications = adminsAndOwners.Select(m => Notification.Create(
            m.UserId,
            NotificationType.InvitationAccepted,
            "Invitation accepted",
            $"{request.UserEmail} has joined {orgName}.",
            System.Text.Json.JsonSerializer.Serialize(new { orgId = invitation.OrganizationId, userEmail = request.UserEmail })));

        await _notificationRepository.AddRangeAsync(notifications, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
