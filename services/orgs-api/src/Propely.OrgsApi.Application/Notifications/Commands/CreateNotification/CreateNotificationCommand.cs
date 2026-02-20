// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.OrgsApi.Domain.Notifications;

namespace Propely.OrgsApi.Application.Notifications.Commands.CreateNotification;

public sealed record CreateNotificationCommand(
    Guid UserId,
    NotificationType Type,
    string Title,
    string Body,
    string? Metadata = null) : IRequest;
