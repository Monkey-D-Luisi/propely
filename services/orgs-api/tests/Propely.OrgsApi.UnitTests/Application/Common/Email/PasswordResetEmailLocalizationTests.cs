// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Common.Email;
using FluentAssertions;

namespace Propely.OrgsApi.UnitTests.Application.Common.Email;

public sealed class PasswordResetEmailLocalizationTests
{
    [Fact]
    public void BuildPasswordResetSubject_WithSpanishLocale_ShouldReturnSpanishSubject()
    {
        var subject = PasswordResetEmailLocalization.BuildPasswordResetSubject("es");

        subject.Should().Be("Restablece tu contrase\u00F1a");
    }

    [Fact]
    public void BuildPasswordResetSubject_WithUnknownLocale_ShouldReturnEnglishSubject()
    {
        var subject = PasswordResetEmailLocalization.BuildPasswordResetSubject("fr");

        subject.Should().Be("Reset your password");
    }

    [Fact]
    public void BuildPasswordResetSubject_WithEnglishLocale_ShouldReturnEnglishSubject()
    {
        var subject = PasswordResetEmailLocalization.BuildPasswordResetSubject("en");

        subject.Should().Be("Reset your password");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void BuildPasswordResetSubject_WithNullOrEmptyLocale_ShouldReturnEnglishSubject(string? locale)
    {
        var subject = PasswordResetEmailLocalization.BuildPasswordResetSubject(locale!);

        subject.Should().Be("Reset your password");
    }

    [Fact]
    public void BuildPasswordResetSubject_WithSpanishRegionalVariant_ShouldReturnSpanishSubject()
    {
        var subject = PasswordResetEmailLocalization.BuildPasswordResetSubject("es-MX");

        subject.Should().Be("Restablece tu contrase\u00F1a");
    }

    [Fact]
    public void BuildPasswordResetSubject_WithUppercaseLocale_ShouldNormalizeCorrectly()
    {
        var subject = PasswordResetEmailLocalization.BuildPasswordResetSubject("ES");

        subject.Should().Be("Restablece tu contrase\u00F1a");
    }
}
