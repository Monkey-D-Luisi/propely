// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Domain.Common;
using Propely.OrgsApi.Domain.Organizations;
using FluentAssertions;

namespace Propely.OrgsApi.UnitTests.Domain.Organizations;

public sealed class InvitationSoftDeleteTests
{
    [Fact]
    public void SoftDelete_ShouldSetIsDeletedToTrue()
    {
        // Arrange
        var invitation = Invitation.Create(Guid.NewGuid(), "test@example.com", MembershipRole.Member);

        // Act
        invitation.SoftDelete();

        // Assert
        invitation.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void SoftDelete_ShouldSetDeletedAtUtc()
    {
        // Arrange
        var invitation = Invitation.Create(Guid.NewGuid(), "test@example.com", MembershipRole.Admin);

        // Act
        invitation.SoftDelete();

        // Assert
        invitation.DeletedAtUtc.Should().NotBeNull();
        invitation.DeletedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_ShouldNotBeDeleted()
    {
        // Act
        var invitation = Invitation.Create(Guid.NewGuid(), "test@example.com", MembershipRole.Member);

        // Assert
        invitation.IsDeleted.Should().BeFalse();
        invitation.DeletedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Invitation_ShouldImplementISoftDeletable()
    {
        // Assert
        typeof(Invitation).Should().Implement<ISoftDeletable>();
    }
}
