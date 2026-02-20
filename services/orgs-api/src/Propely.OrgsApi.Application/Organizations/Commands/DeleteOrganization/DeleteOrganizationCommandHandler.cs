// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Text.Json;
using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Application.Notifications.Interfaces;
using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Domain.Common.Exceptions;
using Propely.OrgsApi.Domain.Notifications;
using Propely.OrgsApi.Domain.Organizations;
using MediatR;

namespace Propely.OrgsApi.Application.Organizations.Commands.DeleteOrganization;

public sealed class DeleteOrganizationCommandHandler : IRequestHandler<DeleteOrganizationCommand>
{
    private readonly IMembershipRepository _membershipRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IInvitationRepository _invitationRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteOrganizationCommandHandler(
        IMembershipRepository membershipRepository,
        IOrganizationRepository organizationRepository,
        IInvitationRepository invitationRepository,
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork)
    {
        _membershipRepository = membershipRepository;
        _organizationRepository = organizationRepository;
        _invitationRepository = invitationRepository;
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteOrganizationCommand request, CancellationToken cancellationToken)
    {
        var allMembers = await _membershipRepository.GetByOrgIdAsync(request.OrgId, cancellationToken);
        var membership = allMembers.FirstOrDefault(m => m.UserId == request.UserId)
            ?? throw new ForbiddenException("Not authorized.");

        if (membership.Role != MembershipRole.Owner)
        {
            throw new ForbiddenException("Only owners can delete the organization.");
        }

        var org = await _organizationRepository.GetByIdAsync(request.OrgId, cancellationToken)
            ?? throw new NotFoundException("Organization not found.");

        // Soft-delete the organization
        org.SoftDelete();

        // Cascade soft-delete all memberships
        foreach (var member in allMembers)
        {
            member.SoftDelete();
        }

        // Cancel all pending invitations
        await _invitationRepository.CancelByOrgAsync(request.OrgId, cancellationToken);

        // Notify all other members about the deletion
        var orgName = org.Name;
        var otherMembers = allMembers.Where(m => m.UserId != request.UserId).ToList();

        var notifications = otherMembers.Select(m => Notification.Create(
            m.UserId,
            NotificationType.OrgDeleted,
            "Organization deleted",
            $"The organization {orgName} has been deleted.",
            JsonSerializer.Serialize(new { orgId = request.OrgId })));

        await _notificationRepository.AddRangeAsync(notifications, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
