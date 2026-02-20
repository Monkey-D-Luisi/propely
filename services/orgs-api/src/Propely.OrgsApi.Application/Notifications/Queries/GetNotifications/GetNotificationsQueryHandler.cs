// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.OrgsApi.Application.Common.Models;
using Propely.OrgsApi.Application.Notifications.Interfaces;

namespace Propely.OrgsApi.Application.Notifications.Queries.GetNotifications;

public sealed class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, GetNotificationsResult>
{
    private readonly INotificationRepository _notificationRepository;

    public GetNotificationsQueryHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<GetNotificationsResult> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var notifications = await _notificationRepository.GetByUserIdPagedAsync(
            request.UserId, page, pageSize, cancellationToken);

        var unreadCount = await _notificationRepository.GetUnreadCountAsync(
            request.UserId, cancellationToken);

        var dtos = new PagedResult<NotificationDto>(
            notifications.Items.Select(n => new NotificationDto(
                n.Id,
                n.Type.ToString(),
                n.Title,
                n.Body,
                n.IsRead,
                n.CreatedAtUtc,
                n.Metadata)),
            notifications.TotalCount,
            notifications.PageNumber,
            pageSize);

        return new GetNotificationsResult(dtos, unreadCount);
    }
}
