// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Text.Json;
using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Application.Notifications.Interfaces;
using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Domain.Common.Exceptions;
using Propely.OrgsApi.Domain.Notifications;
using Propely.OrgsApi.Domain.Organizations;
using MediatR;

namespace Propely.OrgsApi.Application.Organizations.Commands.RemoveMember;

public sealed class RemoveMemberCommandHandler : IRequestHandler<RemoveMemberCommand>
{
    private readonly IMembershipRepository _membershipRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveMemberCommandHandler(
        IMembershipRepository membershipRepository,
        IOrganizationRepository organizationRepository,
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork)
    {
        _membershipRepository = membershipRepository;
        _organizationRepository = organizationRepository;
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(RemoveMemberCommand request, CancellationToken cancellationToken)
    {
        // Cannot remove yourself — use "leave" instead
        if (request.TargetUserId == request.RequestingUserId)
        {
            throw new DomainException("CANNOT_REMOVE_SELF");
        }

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var allMembers = await _membershipRepository.GetByOrgIdForUpdateAsync(request.OrgId, cancellationToken);

            var requestingMembership = allMembers.FirstOrDefault(m => m.UserId == request.RequestingUserId)
                ?? throw new ForbiddenException("Not authorized.");

            var targetMembership = allMembers.FirstOrDefault(m => m.UserId == request.TargetUserId)
                ?? throw new NotFoundException("Target member not found.");

            // Authorization hierarchy: Owner can remove anyone, Admin can remove Members/Viewers only
            if (requestingMembership.Role == MembershipRole.Admin)
            {
                if (targetMembership.Role == MembershipRole.Owner || targetMembership.Role == MembershipRole.Admin)
                {
                    throw new ForbiddenException("Admins can only remove members and viewers.");
                }
            }
            else if (requestingMembership.Role != MembershipRole.Owner)
            {
                throw new ForbiddenException("Only owners and admins can remove members.");
            }

            // Prevent removing the last owner
            if (targetMembership.Role == MembershipRole.Owner)
            {
                var ownerCount = allMembers.Count(m => m.Role == MembershipRole.Owner);
                if (ownerCount <= 1)
                {
                    throw new DomainException("CANNOT_REMOVE_LAST_OWNER");
                }
            }

            targetMembership.SoftDelete();

            // Notify the removed member
            var org = await _organizationRepository.GetByIdAsync(request.OrgId, cancellationToken);
            var orgName = org?.Name ?? "the organization";

            var notification = Notification.Create(
                request.TargetUserId,
                NotificationType.MemberRemoved,
                "Removed from organization",
                $"You have been removed from {orgName}.",
                JsonSerializer.Serialize(new { orgId = request.OrgId }));

            await _notificationRepository.AddAsync(notification, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }, cancellationToken);
    }
}
