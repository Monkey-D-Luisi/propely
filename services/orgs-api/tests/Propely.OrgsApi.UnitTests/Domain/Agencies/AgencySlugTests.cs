// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Domain.Agencies;
using Propely.OrgsApi.Domain.Common.Exceptions;
using FluentAssertions;

namespace Propely.OrgsApi.UnitTests.Domain.Agencies;

public sealed class AgencySlugTests
{
    [Theory]
    [InlineData("test-agency")]
    [InlineData("my-agency-123")]
    [InlineData("abc")]
    [InlineData("a1b")]
    [InlineData("agency-name-with-many-hyphens-and-numbers-123456")]
    public void Create_WithValidSlug_ShouldSucceed(string slug)
    {
        // Act
        var result = AgencySlug.Create(slug);

        // Assert
        result.Value.Should().Be(slug);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptySlug_ShouldThrow(string? slug)
    {
        // Act
        var act = () => AgencySlug.Create(slug!);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("Agency slug is required.");
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("a")]
    public void Create_WithTooShortSlug_ShouldThrow(string slug)
    {
        // Act
        var act = () => AgencySlug.Create(slug);

        // Assert
        act.Should().Throw<DomainException>().WithMessage($"Agency slug must be at least {AgencySlug.MinLength} characters.");
    }

    [Fact]
    public void Create_WithTooLongSlug_ShouldThrow()
    {
        // Arrange
        var slug = new string('a', AgencySlug.MaxLength + 1);

        // Act
        var act = () => AgencySlug.Create(slug);

        // Assert
        act.Should().Throw<DomainException>().WithMessage($"Agency slug must not exceed {AgencySlug.MaxLength} characters.");
    }

    [Theory]
    [InlineData("Test-Agency")]
    [InlineData("UPPERCASE")]
    [InlineData("-start-with-hyphen")]
    [InlineData("end-with-hyphen-")]
    [InlineData("has spaces")]
    [InlineData("has_underscore")]
    [InlineData("has.dot")]
    [InlineData("has@special")]
    [InlineData("test--agency")]
    [InlineData("a--b")]
    public void Create_WithInvalidFormat_ShouldThrow(string slug)
    {
        // Act
        var act = () => AgencySlug.Create(slug);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Equals_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var slug1 = AgencySlug.Create("test-agency");
        var slug2 = AgencySlug.Create("test-agency");

        // Assert
        slug1.Should().Be(slug2);
        slug1.GetHashCode().Should().Be(slug2.GetHashCode());
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldNotBeEqual()
    {
        // Arrange
        var slug1 = AgencySlug.Create("agency-one");
        var slug2 = AgencySlug.Create("agency-two");

        // Assert
        slug1.Should().NotBe(slug2);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var slug = AgencySlug.Create("test-agency");

        // Assert
        slug.ToString().Should().Be("test-agency");
    }
}
