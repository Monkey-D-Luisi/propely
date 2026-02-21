// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Domain.Agencies;
using Propely.OrgsApi.Domain.Agencies.Events;
using Propely.OrgsApi.Domain.Common;
using Propely.OrgsApi.Domain.Common.Exceptions;
using FluentAssertions;

namespace Propely.OrgsApi.UnitTests.Domain.Agencies;

public sealed class AgencyTests
{
    [Fact]
    public void Create_WithValidInputs_ShouldCreateAgency()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var agency = Agency.Create("Test Agency", "test-agency", userId);

        // Assert
        agency.Id.Should().NotBeEmpty();
        agency.Name.Should().Be("Test Agency");
        agency.Slug.Value.Should().Be("test-agency");
        agency.CreatedByUserId.Should().Be(userId);
        agency.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        agency.UpdatedAtUtc.Should().BeNull();
        agency.IsDeleted.Should().BeFalse();
        agency.DeletedAtUtc.Should().BeNull();
        agency.BranchIds.Should().BeEmpty();
    }

    [Fact]
    public void Create_ShouldRaiseAgencyCreatedV1Event()
    {
        // Act
        var agency = Agency.Create("Test Agency", "test-agency", Guid.NewGuid());

        // Assert
        agency.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<AgencyCreatedV1>();
        var evt = (AgencyCreatedV1)agency.DomainEvents.First();
        evt.Data.AgencyId.Should().Be(agency.Id);
        evt.Data.Name.Should().Be("Test Agency");
        evt.Data.Slug.Should().Be("test-agency");
        evt.Data.CreatedByUserId.Should().Be(agency.CreatedByUserId);
    }

    [Fact]
    public void Create_ShouldTrimName()
    {
        // Act
        var agency = Agency.Create("  Test Agency  ", "test-agency", Guid.NewGuid());

        // Assert
        agency.Name.Should().Be("Test Agency");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyName_ShouldThrow(string? name)
    {
        // Act
        var act = () => Agency.Create(name!, "test-agency", Guid.NewGuid());

        // Assert
        act.Should().Throw<DomainException>().WithMessage("Agency name is required.");
    }

    [Fact]
    public void Create_WithNameExceedingMaxLength_ShouldThrow()
    {
        // Arrange
        var longName = new string('a', Agency.NameMaxLength + 1);

        // Act
        var act = () => Agency.Create(longName, "test-agency", Guid.NewGuid());

        // Assert
        act.Should().Throw<DomainException>().WithMessage($"Agency name must not exceed {Agency.NameMaxLength} characters.");
    }

    [Fact]
    public void AddBranch_ShouldAddBranchAndRaiseEvent()
    {
        // Arrange
        var agency = Agency.Create("Test Agency", "test-agency", Guid.NewGuid());
        agency.ClearDomainEvents();
        var orgId = Guid.NewGuid();

        // Act
        agency.AddBranch(orgId);

        // Assert
        agency.BranchIds.Should().ContainSingle().Which.Should().Be(orgId);
        agency.UpdatedAtUtc.Should().NotBeNull();
        agency.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<BranchAddedToAgencyV1>();
        var evt = (BranchAddedToAgencyV1)agency.DomainEvents.First();
        evt.Data.AgencyId.Should().Be(agency.Id);
        evt.Data.OrganizationId.Should().Be(orgId);
    }

    [Fact]
    public void AddBranch_DuplicateBranch_ShouldThrowDomainException()
    {
        // Arrange
        var agency = Agency.Create("Test Agency", "test-agency", Guid.NewGuid());
        var orgId = Guid.NewGuid();
        agency.AddBranch(orgId);

        // Act
        var act = () => agency.AddBranch(orgId);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("This branch is already part of the agency.");
    }

    [Fact]
    public void RemoveBranch_ShouldRemoveBranchAndRaiseEvent()
    {
        // Arrange
        var agency = Agency.Create("Test Agency", "test-agency", Guid.NewGuid());
        var orgId = Guid.NewGuid();
        agency.AddBranch(orgId);
        agency.ClearDomainEvents();

        // Act
        agency.RemoveBranch(orgId);

        // Assert
        agency.BranchIds.Should().BeEmpty();
        agency.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<BranchRemovedFromAgencyV1>();
    }

    [Fact]
    public void RemoveBranch_NonExistentBranch_ShouldThrowNotFoundException()
    {
        // Arrange
        var agency = Agency.Create("Test Agency", "test-agency", Guid.NewGuid());

        // Act
        var act = () => agency.RemoveBranch(Guid.NewGuid());

        // Assert
        act.Should().Throw<NotFoundException>().WithMessage("This branch is not part of the agency.");
    }

    [Fact]
    public void Update_WithValidName_ShouldUpdateNameAndTimestamp()
    {
        // Arrange
        var agency = Agency.Create("Test Agency", "test-agency", Guid.NewGuid());

        // Act
        agency.Update("Updated Agency");

        // Assert
        agency.Name.Should().Be("Updated Agency");
        agency.UpdatedAtUtc.Should().NotBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Update_WithEmptyName_ShouldThrow(string? name)
    {
        // Arrange
        var agency = Agency.Create("Test Agency", "test-agency", Guid.NewGuid());

        // Act
        var act = () => agency.Update(name!);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("Agency name is required.");
    }

    [Fact]
    public void SoftDelete_ShouldSetIsDeletedAndTimestamps()
    {
        // Arrange
        var agency = Agency.Create("Test Agency", "test-agency", Guid.NewGuid());

        // Act
        agency.SoftDelete();

        // Assert
        agency.IsDeleted.Should().BeTrue();
        agency.DeletedAtUtc.Should().NotBeNull();
        agency.UpdatedAtUtc.Should().Be(agency.DeletedAtUtc);
    }

    [Fact]
    public void Agency_ShouldImplementISoftDeletable()
    {
        typeof(Agency).Should().Implement<ISoftDeletable>();
    }
}
