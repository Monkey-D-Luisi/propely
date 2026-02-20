// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Common.Email;
using Propely.OrgsApi.Infrastructure.Email;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Propely.OrgsApi.UnitTests.Infrastructure.Email;

public sealed class RazorEmailTemplateRendererTests
{
    private readonly RazorEmailTemplateRenderer _renderer;

    public RazorEmailTemplateRendererTests()
    {
        var logger = Substitute.For<ILogger<RazorEmailTemplateRenderer>>();
        _renderer = new RazorEmailTemplateRenderer(logger);
    }

    [Fact]
    public async Task RenderAsync_WithEnglishLocale_ShouldRenderInvitationWithLayout()
    {
        // Arrange
        var model = new InvitationEmailModel
        {
            AppName = "SaaS Starter Kit",
            SupportUrl = "https://example.com/support",
            Year = 2026,
            OrganizationName = "Acme Org",
            InviteUrl = "https://app.test/invite/token-123"
        };

        // Act
        var html = await _renderer.RenderAsync("Invitation", model, "en");

        // Assert
        html.Should().Contain("<html");
        html.Should().Contain("SaaS Starter Kit");
        html.Should().Contain("You have been invited");
        html.Should().Contain("Acme Org");
        html.Should().Contain("https://app.test/invite/token-123");
        html.Should().Contain("lang=\"en\"");
        html.Should().Contain("Need help? Visit");
    }

    [Fact]
    public async Task RenderAsync_WithSpanishLocale_ShouldRenderSpanishTemplate()
    {
        // Arrange
        var model = new InvitationEmailModel
        {
            AppName = "SaaS Starter Kit",
            SupportUrl = "https://example.com/support",
            Year = 2026,
            OrganizationName = "Acme Org",
            InviteUrl = "https://app.test/invite/token-123"
        };

        // Act
        var html = await _renderer.RenderAsync("Invitation", model, "es");

        // Assert
        html.Should().Contain("Has recibido una invitaci&oacute;n");
        html.Should().Contain("Aceptar invitaci&oacute;n");
        html.Should().Contain("Acme Org");
        html.Should().Contain("lang=\"es\"");
        html.Should().Contain("&#xBF;Necesitas ayuda? Visita");
    }

    [Fact]
    public async Task RenderAsync_WithUnknownLocale_ShouldFallbackToEnglish()
    {
        // Arrange
        var model = new InvitationEmailModel
        {
            AppName = "SaaS Starter Kit",
            SupportUrl = "https://example.com/support",
            Year = 2026,
            OrganizationName = "Acme Org",
            InviteUrl = "https://app.test/invite/token-123"
        };

        // Act
        var html = await _renderer.RenderAsync("Invitation", model, "fr");

        // Assert
        html.Should().Contain("You have been invited");
        html.Should().NotContain("Has recibido una invitaci&oacute;n");
        html.Should().Contain("lang=\"en\"");
    }

    [Fact]
    public async Task RenderAsync_WithUnknownTemplate_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var model = new InvitationEmailModel
        {
            AppName = "SaaS Starter Kit",
            SupportUrl = "https://example.com/support",
            Year = 2026,
            OrganizationName = "Acme Org",
            InviteUrl = "https://app.test/invite/token-123"
        };

        // Act
        var act = () => _renderer.RenderAsync("UnknownTemplate", model, "en");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task RenderAsync_WithEmailVerificationTemplate_ShouldRenderVerificationUrl()
    {
        // Arrange
        var model = new EmailVerificationEmailModel
        {
            AppName = "SaaS Starter Kit",
            SupportUrl = "https://example.com/support",
            Year = 2026,
            VerificationUrl = "https://app.test/en/verify-email?token=abc"
        };

        // Act
        var html = await _renderer.RenderAsync("EmailVerification", model, "en");

        // Assert
        html.Should().Contain("Verify your email address");
        html.Should().Contain("https://app.test/en/verify-email?token=abc");
        html.Should().Contain("lang=\"en\"");
    }

    [Fact]
    public async Task RenderAsync_WithPasswordResetTemplate_ShouldRenderContent()
    {
        // Arrange
        var model = new PasswordResetEmailModel
        {
            AppName = "SaaS Starter Kit",
            SupportUrl = "https://example.com/support",
            Year = 2026,
            ResetUrl = "https://app.test/en/reset-password?token=abc123"
        };

        // Act
        var html = await _renderer.RenderAsync("PasswordReset", model, "en");

        // Assert
        html.Should().Contain("Reset your password");
        html.Should().Contain("https://app.test/en/reset-password?token=abc123");
        html.Should().Contain("lang=\"en\"");
    }
}
