// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Api.Validators;
using FluentAssertions;

namespace Propely.OrgsApi.UnitTests.Api.Validators;

public sealed class UrlValidationTests
{
    [Theory]
    [InlineData("/en/pricing", true)]
    [InlineData("/", true)]
    [InlineData("/orgs/123/billing", true)]
    [InlineData("/path?query=1", true)]
    public void IsAllowedRelativePath_WithValidPaths_ShouldReturnTrue(string path, bool expected)
    {
        UrlValidation.IsAllowedRelativePath(path).Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsAllowedRelativePath_WithEmptyOrWhitespace_ShouldReturnFalse(string? path)
    {
        UrlValidation.IsAllowedRelativePath(path).Should().BeFalse();
    }

    [Fact]
    public void IsAllowedRelativePath_WithoutLeadingSlash_ShouldReturnFalse()
    {
        UrlValidation.IsAllowedRelativePath("en/pricing").Should().BeFalse();
    }

    [Fact]
    public void IsAllowedRelativePath_WithProtocolRelativeUrl_ShouldReturnFalse()
    {
        UrlValidation.IsAllowedRelativePath("//attacker.com").Should().BeFalse();
    }

    [Fact]
    public void IsAllowedRelativePath_WithProtocolRelativeUrlAndPath_ShouldReturnFalse()
    {
        UrlValidation.IsAllowedRelativePath("//attacker.com/evil").Should().BeFalse();
    }

    [Fact]
    public void IsAllowedRelativePath_WithAbsoluteUrl_ShouldReturnFalse()
    {
        UrlValidation.IsAllowedRelativePath("https://attacker.com/evil").Should().BeFalse();
    }

    [Fact]
    public void IsAllowedRelativePath_WithSchemeInPath_ShouldReturnFalse()
    {
        UrlValidation.IsAllowedRelativePath("/redirect?url=http://evil.com").Should().BeFalse();
    }
}
