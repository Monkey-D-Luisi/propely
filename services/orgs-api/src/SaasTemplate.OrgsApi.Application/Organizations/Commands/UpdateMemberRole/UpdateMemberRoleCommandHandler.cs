// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Application.Common.Interfaces;
using SaasTemplate.OrgsApi.Application.Notifications.Interfaces;
using SaasTemplate.OrgsApi.Application.Organizations.Interfaces;
using SaasTemplate.OrgsApi.Domain.Common.Exceptions;
using SaasTemplate.OrgsApi.Domain.Notifications;
using SaasTemplate.OrgsApi.Domain.Organizations;
using MediatR;

namespace SaasTemplate.OrgsApi.Application.Organizations.Commands.UpdateMemberRole;

public sealed class UpdateMemberRoleCommandHandler : IRequestHandler<UpdateMemberRoleCommand>
{
    private readonly IMembershipRepository _membershipRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateMemberRoleCommandHandler(
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

    public async Task Handle(UpdateMemberRoleCommand request, CancellationToken cancellationToken)
    {
        // Cannot change your own role
        if (request.TargetUserId == request.RequestingUserId)
        {
            throw new DomainException("CANNOT_CHANGE_OWN_ROLE");
        }

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var allMembers = await _membershipRepository.GetByOrgIdForUpdateAsync(request.OrgId, cancellationToken);

            var requestingMembership = allMembers.FirstOrDefault(m => m.UserId == request.RequestingUserId)
                ?? throw new ForbiddenException("Not authorized.");

            var targetMembership = allMembers.FirstOrDefault(m => m.UserId == request.TargetUserId)
                ?? throw new NotFoundException("Member not found.");

            // Authorization hierarchy: Owner can change any role, Admin can change Members/Viewers only
            if (requestingMembership.Role == MembershipRole.Admin)
            {
                if (targetMembership.Role == MembershipRole.Owner || targetMembership.Role == MembershipRole.Admin)
                {
                    throw new ForbiddenException("Admins can only change roles of members and viewers.");
                }
                // Admins cannot promote to Owner or Admin
                if (request.NewRole == MembershipRole.Owner || request.NewRole == MembershipRole.Admin)
                {
                    throw new ForbiddenException("Admins cannot promote to owner or admin.");
                }
            }
            else if (requestingMembership.Role != MembershipRole.Owner)
            {
                throw new ForbiddenException("Only owners and admins can change member roles.");
            }

            var oldRole = targetMembership.Role;

            // Prevent removing the last owner
            if (targetMembership.Role == MembershipRole.Owner && request.NewRole != MembershipRole.Owner)
            {
                var ownerCount = allMembers.Count(m => m.Role == MembershipRole.Owner);
                if (ownerCount <= 1)
                {
                    throw new DomainException("CANNOT_REMOVE_LAST_OWNER");
                }
            }

            targetMembership.UpdateRole(request.NewRole);

            // Notify the affected user about the role change
            if (oldRole != request.NewRole)
            {
                var org = await _organizationRepository.GetByIdAsync(request.OrgId, cancellationToken);
                var orgName = org?.Name ?? "the organization";
                var notification = Notification.Create(
                    request.TargetUserId,
                    NotificationType.RoleChanged,
                    "Role changed",
                    $"Your role in {orgName} was changed from {oldRole.ToString().ToLowerInvariant()} to {request.NewRole.ToString().ToLowerInvariant()}.",
                    System.Text.Json.JsonSerializer.Serialize(new { orgId = request.OrgId, oldRole = oldRole.ToString().ToLowerInvariant(), newRole = request.NewRole.ToString().ToLowerInvariant() }));

                await _notificationRepository.AddAsync(notification, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }, cancellationToken);
    }
}
