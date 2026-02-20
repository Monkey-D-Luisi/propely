// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Common.Models;
using Propely.OrgsApi.Application.Notifications.Interfaces;
using Propely.OrgsApi.Application.Notifications.Queries.GetNotifications;
using Propely.OrgsApi.Domain.Notifications;
using FluentAssertions;
using NSubstitute;

namespace Propely.OrgsApi.UnitTests.Application.Notifications.Queries;

public sealed class GetNotificationsQueryHandlerTests
{
    private readonly INotificationRepository _notificationRepository;
    private readonly GetNotificationsQueryHandler _handler;

    public GetNotificationsQueryHandlerTests()
    {
        _notificationRepository = Substitute.For<INotificationRepository>();
        _handler = new GetNotificationsQueryHandler(_notificationRepository);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotificationsAndUnreadCount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notifications = new List<Notification>
        {
            Notification.Create(userId, NotificationType.RoleChanged, "Role changed", "Your role was changed."),
            Notification.Create(userId, NotificationType.InvitationAccepted, "Invitation accepted", "User joined."),
        };
        var pagedResult = new PagedResult<Notification>(notifications, 2, 1, 20);

        _notificationRepository.GetByUserIdPagedAsync(userId, 1, 20, Arg.Any<CancellationToken>())
            .Returns(pagedResult);
        _notificationRepository.GetUnreadCountAsync(userId, Arg.Any<CancellationToken>())
            .Returns(2);

        var query = new GetNotificationsQuery(userId, 1, 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.UnreadCount.Should().Be(2);
        result.Notifications.Items.Should().HaveCount(2);
        result.Notifications.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldClampPageAndPageSize()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var emptyResult = new PagedResult<Notification>([], 0, 1, 100);

        _notificationRepository.GetByUserIdPagedAsync(userId, 1, 100, Arg.Any<CancellationToken>())
            .Returns(emptyResult);
        _notificationRepository.GetUnreadCountAsync(userId, Arg.Any<CancellationToken>())
            .Returns(0);

        var query = new GetNotificationsQuery(userId, -5, 500);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _notificationRepository.Received(1).GetByUserIdPagedAsync(userId, 1, 100, Arg.Any<CancellationToken>());
        result.Notifications.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldMapNotificationDtosCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notification = Notification.Create(userId, NotificationType.RoleChanged, "Role changed", "Body text", "{\"orgId\":\"abc\"}");
        var pagedResult = new PagedResult<Notification>([notification], 1, 1, 20);

        _notificationRepository.GetByUserIdPagedAsync(userId, 1, 20, Arg.Any<CancellationToken>())
            .Returns(pagedResult);
        _notificationRepository.GetUnreadCountAsync(userId, Arg.Any<CancellationToken>())
            .Returns(1);

        var query = new GetNotificationsQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var dto = result.Notifications.Items.First();
        dto.Id.Should().Be(notification.Id);
        dto.Type.Should().Be("RoleChanged");
        dto.Title.Should().Be("Role changed");
        dto.Body.Should().Be("Body text");
        dto.IsRead.Should().BeFalse();
        dto.Metadata.Should().Be("{\"orgId\":\"abc\"}");
    }
}
