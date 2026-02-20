// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Application.Notifications.Commands.MarkNotificationRead;
using Propely.OrgsApi.Application.Notifications.Interfaces;
using Propely.OrgsApi.Domain.Common.Exceptions;
using Propely.OrgsApi.Domain.Notifications;
using FluentAssertions;
using NSubstitute;

namespace Propely.OrgsApi.UnitTests.Application.Notifications.Commands;

public sealed class MarkNotificationReadCommandHandlerTests
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly MarkNotificationReadCommandHandler _handler;

    public MarkNotificationReadCommandHandlerTests()
    {
        _notificationRepository = Substitute.For<INotificationRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new MarkNotificationReadCommandHandler(_notificationRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithValidNotification_ShouldMarkAsRead()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notification = Notification.Create(userId, NotificationType.RoleChanged, "Test", "Test body");
        var command = new MarkNotificationReadCommand(notification.Id, userId);

        _notificationRepository.GetByIdAsync(notification.Id, Arg.Any<CancellationToken>())
            .Returns(notification);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        notification.IsRead.Should().BeTrue();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentNotification_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new MarkNotificationReadCommand(Guid.NewGuid(), Guid.NewGuid());

        _notificationRepository.GetByIdAsync(command.NotificationId, Arg.Any<CancellationToken>())
            .Returns((Notification?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithDifferentUserId_ShouldThrowForbiddenException()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var notification = Notification.Create(ownerId, NotificationType.RoleChanged, "Test", "Test body");
        var command = new MarkNotificationReadCommand(notification.Id, otherUserId);

        _notificationRepository.GetByIdAsync(notification.Id, Arg.Any<CancellationToken>())
            .Returns(notification);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WithAlreadyReadNotification_ShouldStillCallSave()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notification = Notification.Create(userId, NotificationType.RoleChanged, "Test", "Test body");
        notification.MarkAsRead();
        var command = new MarkNotificationReadCommand(notification.Id, userId);

        _notificationRepository.GetByIdAsync(notification.Id, Arg.Any<CancellationToken>())
            .Returns(notification);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        notification.IsRead.Should().BeTrue();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
