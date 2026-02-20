// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Domain.Users;
using FluentAssertions;

namespace Propely.OrgsApi.UnitTests.Domain.Users;

public sealed class UserTests
{
    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        // Act
        var user = User.Create("Test@Example.com", "hashed-password", "Test User");

        // Assert
        user.Id.Should().NotBeEmpty();
        user.Email.Should().Be("test@example.com");
        user.PasswordHash.Should().Be("hashed-password");
        user.Name.Should().Be("Test User");
        user.EmailVerified.Should().BeFalse();
        user.EmailVerifiedAtUtc.Should().BeNull();
        user.IsDeleted.Should().BeFalse();
        user.DeletedAtUtc.Should().BeNull();
        user.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.PasswordVersion.Should().Be(0);
    }

    [Fact]
    public void Create_WithEmailVerified_ShouldSetEmailVerifiedAtUtc()
    {
        // Act
        var user = User.Create("test@example.com", "hash", emailVerified: true);

        // Assert
        user.EmailVerified.Should().BeTrue();
        user.EmailVerifiedAtUtc.Should().NotBeNull();
        user.EmailVerifiedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithWhitespaceName_ShouldSetNameToNull()
    {
        // Act
        var user = User.Create("test@example.com", "hash", "  ");

        // Assert
        user.Name.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldNormalizeEmail()
    {
        // Act
        var user = User.Create("  USER@Example.COM  ", "hash");

        // Assert
        user.Email.Should().Be("user@example.com");
    }

    [Fact]
    public void MarkEmailAsVerified_ShouldSetEmailVerifiedAndTimestamp()
    {
        // Arrange
        var user = User.Create("test@example.com", "hash");

        // Act
        user.MarkEmailAsVerified();

        // Assert
        user.EmailVerified.Should().BeTrue();
        user.EmailVerifiedAtUtc.Should().NotBeNull();
        user.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void MarkEmailAsVerified_WhenAlreadyVerified_ShouldBeNoOp()
    {
        // Arrange
        var user = User.Create("test@example.com", "hash", emailVerified: true);
        var originalUpdatedAt = user.UpdatedAtUtc;

        // Act
        user.MarkEmailAsVerified();

        // Assert
        user.UpdatedAtUtc.Should().Be(originalUpdatedAt);
    }

    [Fact]
    public void UpdateProfile_ShouldSetNameAndTimestamp()
    {
        // Arrange
        var user = User.Create("test@example.com", "hash");

        // Act
        user.UpdateProfile("New Name");

        // Assert
        user.Name.Should().Be("New Name");
        user.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void ChangePassword_ShouldIncrementPasswordVersion()
    {
        // Arrange
        var user = User.Create("test@example.com", "old-hash");

        // Act
        user.ChangePassword("new-hash");

        // Assert
        user.PasswordHash.Should().Be("new-hash");
        user.PasswordVersion.Should().Be(1);
        user.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void SoftDelete_ShouldSetIsDeletedAndTimestamps()
    {
        // Arrange
        var user = User.Create("test@example.com", "hash");

        // Act
        user.SoftDelete();

        // Assert
        user.IsDeleted.Should().BeTrue();
        user.DeletedAtUtc.Should().NotBeNull();
        user.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void SoftDelete_WhenAlreadyDeleted_ShouldBeNoOp()
    {
        // Arrange
        var user = User.Create("test@example.com", "hash");
        user.SoftDelete();
        var originalDeletedAt = user.DeletedAtUtc;

        // Act
        user.SoftDelete();

        // Assert
        user.DeletedAtUtc.Should().Be(originalDeletedAt);
    }
}
