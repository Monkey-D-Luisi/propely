// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Domain.Common;
using Propely.OrgsApi.Domain.Organizations;
using FluentAssertions;

namespace Propely.OrgsApi.UnitTests.Domain.Organizations;

public sealed class MembershipSoftDeleteTests
{
    [Fact]
    public void SoftDelete_ShouldSetIsDeletedToTrue()
    {
        // Arrange
        var membership = Membership.Create(Guid.NewGuid(), Guid.NewGuid(), MembershipRole.Member);

        // Act
        membership.SoftDelete();

        // Assert
        membership.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void SoftDelete_ShouldSetDeletedAtUtc()
    {
        // Arrange
        var membership = Membership.Create(Guid.NewGuid(), Guid.NewGuid(), MembershipRole.Owner);

        // Act
        membership.SoftDelete();

        // Assert
        membership.DeletedAtUtc.Should().NotBeNull();
        membership.DeletedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_ShouldNotBeDeleted()
    {
        // Act
        var membership = Membership.Create(Guid.NewGuid(), Guid.NewGuid(), MembershipRole.Admin);

        // Assert
        membership.IsDeleted.Should().BeFalse();
        membership.DeletedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Membership_ShouldImplementISoftDeletable()
    {
        // Assert
        typeof(Membership).Should().Implement<ISoftDeletable>();
    }
}
