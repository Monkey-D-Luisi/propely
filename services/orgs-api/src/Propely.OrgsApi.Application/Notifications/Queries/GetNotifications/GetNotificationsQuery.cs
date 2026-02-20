// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.OrgsApi.Application.Common.Models;

namespace Propely.OrgsApi.Application.Notifications.Queries.GetNotifications;

public sealed record GetNotificationsQuery(Guid UserId, int Page = 1, int PageSize = 20) : IRequest<GetNotificationsResult>;

public sealed record GetNotificationsResult(PagedResult<NotificationDto> Notifications, int UnreadCount);

public sealed record NotificationDto(Guid Id, string Type, string Title, string Body, bool IsRead, DateTime CreatedAtUtc, string? Metadata);
