// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Common.Email;
using FluentAssertions;

namespace Propely.OrgsApi.UnitTests.Application.Common.Email;

public sealed class InvitationEmailLocalizationTests
{
    [Theory]
    [InlineData(null, "en")]
    [InlineData("", "en")]
    [InlineData("   ", "en")]
    [InlineData("es", "es")]
    [InlineData("es-MX", "es")]
    [InlineData("EN", "en")]
    [InlineData("fr", "en")]
    public void NormalizeLocale_ShouldReturnExpectedValue(string? locale, string expected)
    {
        var normalized = InvitationEmailLocalization.NormalizeLocale(locale);

        normalized.Should().Be(expected);
    }

    [Fact]
    public void ResolveLocale_ShouldUseRequestedLocaleWhenPresent()
    {
        var resolved = InvitationEmailLocalization.ResolveLocale("es-AR", "en");

        resolved.Should().Be("es");
    }

    [Fact]
    public void ResolveLocale_ShouldUseFallbackWhenRequestedLocaleIsEmpty()
    {
        var resolved = InvitationEmailLocalization.ResolveLocale(" ", "es");

        resolved.Should().Be("es");
    }

    [Fact]
    public void BuildInvitationSubject_ShouldReturnSpanishSubjectForSpanishLocale()
    {
        var subject = InvitationEmailLocalization.BuildInvitationSubject("es", "Acme");

        subject.Should().Be("Invitaci\u00F3n para unirte a Acme");
    }

    [Fact]
    public void BuildInvitationSubject_ShouldFallbackToEnglishForUnknownLocale()
    {
        var subject = InvitationEmailLocalization.BuildInvitationSubject("fr", "Acme");

        subject.Should().Be("Invitation to join Acme");
    }

    [Fact]
    public void BuildFooterHelpPrefix_ShouldReturnLocalizedText()
    {
        InvitationEmailLocalization.BuildFooterHelpPrefix("es")
            .Should().Be("\u00BFNecesitas ayuda? Visita");
        InvitationEmailLocalization.BuildFooterHelpPrefix("fr")
            .Should().Be("Need help? Visit");
    }
}
