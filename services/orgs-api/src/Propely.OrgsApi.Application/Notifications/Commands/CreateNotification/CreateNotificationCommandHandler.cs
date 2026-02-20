// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Application.Notifications.Interfaces;
using Propely.OrgsApi.Domain.Notifications;

namespace Propely.OrgsApi.Application.Notifications.Commands.CreateNotification;

public sealed class CreateNotificationCommandHandler : IRequestHandler<CreateNotificationCommand>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateNotificationCommandHandler(
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = Notification.Create(
            request.UserId,
            request.Type,
            request.Title,
            request.Body,
            request.Metadata);

        await _notificationRepository.AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
