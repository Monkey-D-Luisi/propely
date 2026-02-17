// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Domain.FeatureFlags;
using FluentAssertions;

namespace SaasTemplate.OrgsApi.UnitTests.Domain.FeatureFlags;

public sealed class FeatureFlagTests
{
    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        // Act
        var flag = FeatureFlag.Create("TestFlag", true, "A test flag");

        // Assert
        flag.Id.Should().NotBeEmpty();
        flag.Name.Should().Be("TestFlag");
        flag.IsEnabled.Should().BeTrue();
        flag.Description.Should().Be("A test flag");
        flag.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        flag.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Create_WithDisabledFlag_ShouldSetIsEnabledToFalse()
    {
        // Act
        var flag = FeatureFlag.Create("DisabledFlag", false);

        // Assert
        flag.IsEnabled.Should().BeFalse();
    }

    [Fact]
    public void Create_WithNullDescription_ShouldSetDescriptionToNull()
    {
        // Act
        var flag = FeatureFlag.Create("NoDesc", true);

        // Assert
        flag.Description.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldTrimName()
    {
        // Act
        var flag = FeatureFlag.Create("  TrimmedName  ", true);

        // Assert
        flag.Name.Should().Be("TrimmedName");
    }

    [Fact]
    public void Create_WithNullOrWhitespaceName_ShouldThrowArgumentException()
    {
        // Act
        var act = () => FeatureFlag.Create("", true);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNameExceedingMaxLength_ShouldThrowArgumentException()
    {
        // Arrange
        var longName = new string('a', FeatureFlag.NameMaxLength + 1);

        // Act
        var act = () => FeatureFlag.Create(longName, true);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage($"*{FeatureFlag.NameMaxLength}*");
    }

    [Fact]
    public void SetEnabled_ShouldChangeValueAndSetUpdatedAt()
    {
        // Arrange
        var flag = FeatureFlag.Create("TestFlag", false);

        // Act
        flag.SetEnabled(true);

        // Assert
        flag.IsEnabled.Should().BeTrue();
        flag.UpdatedAtUtc.Should().NotBeNull();
        flag.UpdatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void SetEnabled_ToSameValue_ShouldStillUpdateTimestamp()
    {
        // Arrange
        var flag = FeatureFlag.Create("TestFlag", true);

        // Act
        flag.SetEnabled(true);

        // Assert
        flag.UpdatedAtUtc.Should().NotBeNull();
    }
}
