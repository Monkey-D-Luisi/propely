// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Text.Json;
using SaasTemplate.OrgsApi.Application.Common.Interfaces;
using SaasTemplate.OrgsApi.Application.Notifications.Interfaces;
using SaasTemplate.OrgsApi.Application.Organizations.Interfaces;
using SaasTemplate.OrgsApi.Domain.Common.Exceptions;
using SaasTemplate.OrgsApi.Domain.Notifications;
using SaasTemplate.OrgsApi.Domain.Organizations;
using MediatR;

namespace SaasTemplate.OrgsApi.Application.Organizations.Commands.LeaveOrganization;

public sealed class LeaveOrganizationCommandHandler : IRequestHandler<LeaveOrganizationCommand>
{
    private readonly IMembershipRepository _membershipRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LeaveOrganizationCommandHandler(
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

    public async Task Handle(LeaveOrganizationCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var allMembers = await _membershipRepository.GetByOrgIdForUpdateAsync(request.OrgId, cancellationToken);
            var membership = allMembers.FirstOrDefault(m => m.UserId == request.UserId)
                ?? throw new ForbiddenException("Not authorized.");

            // Prevent the last owner from leaving
            if (membership.Role == MembershipRole.Owner)
            {
                var ownerCount = allMembers.Count(m => m.Role == MembershipRole.Owner);
                if (ownerCount <= 1)
                {
                    throw new DomainException("CANNOT_REMOVE_LAST_OWNER");
                }
            }

            membership.SoftDelete();

            // Notify org admins and owners about the departure
            var org = await _organizationRepository.GetByIdAsync(request.OrgId, cancellationToken);
            var orgName = org?.Name ?? "the organization";

            var adminsAndOwners = allMembers
                .Where(m => m.UserId != request.UserId && (m.Role == MembershipRole.Owner || m.Role == MembershipRole.Admin))
                .ToList();

            var notifications = adminsAndOwners.Select(m => Notification.Create(
                m.UserId,
                NotificationType.MemberLeft,
                "Member left",
                $"A member has left {orgName}.",
                JsonSerializer.Serialize(new { orgId = request.OrgId, leftUserId = request.UserId })));

            await _notificationRepository.AddRangeAsync(notifications, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }, cancellationToken);
    }
}
