// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Application.Common.Interfaces;
using SaasTemplate.OrgsApi.Application.Notifications.Commands.MarkAllNotificationsRead;
using SaasTemplate.OrgsApi.Application.Notifications.Interfaces;
using NSubstitute;

namespace SaasTemplate.OrgsApi.UnitTests.Application.Notifications.Commands;

public sealed class MarkAllNotificationsReadCommandHandlerTests
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly MarkAllNotificationsReadCommandHandler _handler;

    public MarkAllNotificationsReadCommandHandlerTests()
    {
        _notificationRepository = Substitute.For<INotificationRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new MarkAllNotificationsReadCommandHandler(_notificationRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldCallMarkAllAsReadAndSave()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new MarkAllNotificationsReadCommand(userId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _notificationRepository.Received(1).MarkAllAsReadAsync(userId, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
