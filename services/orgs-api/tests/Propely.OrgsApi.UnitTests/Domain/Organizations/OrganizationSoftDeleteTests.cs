// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Domain.Common;
using Propely.OrgsApi.Domain.Organizations;
using FluentAssertions;

namespace Propely.OrgsApi.UnitTests.Domain.Organizations;

public sealed class OrganizationSoftDeleteTests
{
    [Fact]
    public void SoftDelete_ShouldSetIsDeletedToTrue()
    {
        // Arrange
        var org = Organization.Create("Test Org");

        // Act
        org.SoftDelete();

        // Assert
        org.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void SoftDelete_ShouldSetDeletedAtUtc()
    {
        // Arrange
        var org = Organization.Create("Test Org");

        // Act
        org.SoftDelete();

        // Assert
        org.DeletedAtUtc.Should().NotBeNull();
        org.DeletedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void SoftDelete_ShouldSetUpdatedAtUtc()
    {
        // Arrange
        var org = Organization.Create("Test Org");

        // Act
        org.SoftDelete();

        // Assert
        org.UpdatedAtUtc.Should().NotBeNull();
        org.UpdatedAtUtc.Should().Be(org.DeletedAtUtc);
    }

    [Fact]
    public void Create_ShouldNotBeDeleted()
    {
        // Act
        var org = Organization.Create("Test Org");

        // Assert
        org.IsDeleted.Should().BeFalse();
        org.DeletedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Organization_ShouldImplementISoftDeletable()
    {
        // Assert
        typeof(Organization).Should().Implement<ISoftDeletable>();
    }
}
