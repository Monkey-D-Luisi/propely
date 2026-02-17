// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Domain.Notifications;
using FluentAssertions;

namespace SaasTemplate.OrgsApi.UnitTests.Domain.Notifications;

public sealed class NotificationTests
{
    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var notification = Notification.Create(
            userId,
            NotificationType.InvitationAccepted,
            "Test Title",
            "Test body message",
            "{\"key\":\"value\"}");

        // Assert
        notification.Id.Should().NotBeEmpty();
        notification.UserId.Should().Be(userId);
        notification.Type.Should().Be(NotificationType.InvitationAccepted);
        notification.Title.Should().Be("Test Title");
        notification.Body.Should().Be("Test body message");
        notification.IsRead.Should().BeFalse();
        notification.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        notification.Metadata.Should().Be("{\"key\":\"value\"}");
    }

    [Fact]
    public void Create_WithNullMetadata_ShouldSetMetadataToNull()
    {
        // Act
        var notification = Notification.Create(
            Guid.NewGuid(),
            NotificationType.RoleChanged,
            "Title",
            "Body");

        // Assert
        notification.Metadata.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldTrimTitleAndBody()
    {
        // Act
        var notification = Notification.Create(
            Guid.NewGuid(),
            NotificationType.InvitationAccepted,
            "  Trimmed Title  ",
            "  Trimmed Body  ");

        // Assert
        notification.Title.Should().Be("Trimmed Title");
        notification.Body.Should().Be("Trimmed Body");
    }

    [Fact]
    public void Create_ShouldDefaultToUnread()
    {
        // Act
        var notification = Notification.Create(
            Guid.NewGuid(),
            NotificationType.InvitationAccepted,
            "Title",
            "Body");

        // Assert
        notification.IsRead.Should().BeFalse();
    }

    [Fact]
    public void MarkAsRead_ShouldSetIsReadToTrue()
    {
        // Arrange
        var notification = Notification.Create(
            Guid.NewGuid(),
            NotificationType.InvitationAccepted,
            "Title",
            "Body");

        // Act
        notification.MarkAsRead();

        // Assert
        notification.IsRead.Should().BeTrue();
    }

    [Fact]
    public void MarkAsRead_WhenAlreadyRead_ShouldRemainRead()
    {
        // Arrange
        var notification = Notification.Create(
            Guid.NewGuid(),
            NotificationType.InvitationAccepted,
            "Title",
            "Body");
        notification.MarkAsRead();

        // Act
        notification.MarkAsRead();

        // Assert
        notification.IsRead.Should().BeTrue();
    }
}
