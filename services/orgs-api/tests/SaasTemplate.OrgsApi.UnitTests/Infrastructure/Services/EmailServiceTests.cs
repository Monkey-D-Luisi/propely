// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SaasTemplate.OrgsApi.Application.Common.Email;
using SaasTemplate.OrgsApi.Application.Common.Interfaces;
using SaasTemplate.OrgsApi.Infrastructure.Services;

namespace SaasTemplate.OrgsApi.UnitTests.Infrastructure.Services;

public sealed class EmailServiceTests
{
    private readonly IEmailSender _emailSender;
    private readonly IEmailTemplateRenderer _templateRenderer;
    private readonly EmailService _service;

    public EmailServiceTests()
    {
        _emailSender = Substitute.For<IEmailSender>();
        _templateRenderer = Substitute.For<IEmailTemplateRenderer>();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["App:Name"] = "Test App",
                ["App:SupportUrl"] = "https://example.com/support"
            })
            .Build();
        var logger = Substitute.For<ILogger<EmailService>>();
        _service = new EmailService(configuration, _templateRenderer, _emailSender, logger);
    }

    [Fact]
    public async Task SendOrgInvitationEmailAsync_ShouldDelegateToEmailSender()
    {
        // Arrange
        _templateRenderer
            .RenderAsync(Arg.Any<string>(), Arg.Any<InvitationEmailModel>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("<html>invite</html>");

        // Act
        await _service.SendOrgInvitationEmailAsync("user@example.com", "Test Org", "https://invite.url");

        // Assert
        await _emailSender.Received(1).SendAsync(
            "user@example.com",
            Arg.Any<string>(),
            "<html>invite</html>",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendEmailVerificationEmailAsync_ShouldDelegateToEmailSender()
    {
        // Arrange
        _templateRenderer
            .RenderAsync(Arg.Any<string>(), Arg.Any<EmailVerificationEmailModel>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("<html>verify</html>");

        // Act
        await _service.SendEmailVerificationEmailAsync("user@example.com", "https://verify.url");

        // Assert
        await _emailSender.Received(1).SendAsync(
            "user@example.com",
            Arg.Any<string>(),
            "<html>verify</html>",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendPasswordResetEmailAsync_ShouldDelegateToEmailSender()
    {
        // Arrange
        _templateRenderer
            .RenderAsync(Arg.Any<string>(), Arg.Any<PasswordResetEmailModel>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("<html>reset</html>");

        // Act
        await _service.SendPasswordResetEmailAsync("user@example.com", "https://reset.url");

        // Assert
        await _emailSender.Received(1).SendAsync(
            "user@example.com",
            Arg.Any<string>(),
            "<html>reset</html>",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendOrgInvitationEmailAsync_WhenSenderThrows_ShouldRethrow()
    {
        // Arrange
        _templateRenderer
            .RenderAsync(Arg.Any<string>(), Arg.Any<InvitationEmailModel>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("<html>invite</html>");
        _emailSender
            .SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Send failed"));

        // Act & Assert — critical email should throw
        var act = () => _service.SendOrgInvitationEmailAsync("user@example.com", "Org", "https://invite.url");
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Send failed");
    }

    [Fact]
    public async Task SendOrgInvitationEmailAsync_WhenCancelled_ShouldRethrow()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        _templateRenderer
            .RenderAsync(Arg.Any<string>(), Arg.Any<InvitationEmailModel>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException(cts.Token));

        // Act & Assert
        var act = () => _service.SendOrgInvitationEmailAsync("user@example.com", "Org", "https://invite.url", cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task SendWelcomeEmailAsync_ShouldDelegateToEmailSender()
    {
        // Arrange
        _templateRenderer
            .RenderAsync(Arg.Any<string>(), Arg.Any<WelcomeEmailModel>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("<html>welcome</html>");

        // Act
        await _service.SendWelcomeEmailAsync("user@example.com", "John", "https://login.url");

        // Assert
        await _emailSender.Received(1).SendAsync(
            "user@example.com",
            Arg.Any<string>(),
            "<html>welcome</html>",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendRoleChangeEmailAsync_ShouldDelegateToEmailSender()
    {
        // Arrange
        _templateRenderer
            .RenderAsync(Arg.Any<string>(), Arg.Any<RoleChangeEmailModel>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("<html>role-change</html>");

        // Act
        await _service.SendRoleChangeEmailAsync("user@example.com", "John", "Test Org", "Member", "Admin");

        // Assert
        await _emailSender.Received(1).SendAsync(
            "user@example.com",
            Arg.Any<string>(),
            "<html>role-change</html>",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendMemberRemovedEmailAsync_ShouldDelegateToEmailSender()
    {
        // Arrange
        _templateRenderer
            .RenderAsync(Arg.Any<string>(), Arg.Any<MemberRemovedEmailModel>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("<html>removed</html>");

        // Act
        await _service.SendMemberRemovedEmailAsync("user@example.com", "John", "Test Org");

        // Assert
        await _emailSender.Received(1).SendAsync(
            "user@example.com",
            Arg.Any<string>(),
            "<html>removed</html>",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendEmailVerificationEmailAsync_WhenSenderThrows_ShouldRethrow()
    {
        // Arrange
        _templateRenderer
            .RenderAsync(Arg.Any<string>(), Arg.Any<EmailVerificationEmailModel>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("<html>verify</html>");
        _emailSender
            .SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Send failed"));

        // Act & Assert — critical email should throw
        var act = () => _service.SendEmailVerificationEmailAsync("user@example.com", "https://verify.url");
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Send failed");
    }

    [Fact]
    public async Task SendPasswordResetEmailAsync_WhenSenderThrows_ShouldRethrow()
    {
        // Arrange
        _templateRenderer
            .RenderAsync(Arg.Any<string>(), Arg.Any<PasswordResetEmailModel>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("<html>reset</html>");
        _emailSender
            .SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Send failed"));

        // Act & Assert — critical email should throw
        var act = () => _service.SendPasswordResetEmailAsync("user@example.com", "https://reset.url");
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Send failed");
    }

    [Fact]
    public async Task SendWelcomeEmailAsync_WhenSenderThrows_ShouldNotRethrow()
    {
        // Arrange
        _templateRenderer
            .RenderAsync(Arg.Any<string>(), Arg.Any<WelcomeEmailModel>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("<html>welcome</html>");
        _emailSender
            .SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Send failed"));

        // Act & Assert — non-critical email should not throw
        var act = () => _service.SendWelcomeEmailAsync("user@example.com", "John", "https://login.url");
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SendRoleChangeEmailAsync_WhenSenderThrows_ShouldNotRethrow()
    {
        // Arrange
        _templateRenderer
            .RenderAsync(Arg.Any<string>(), Arg.Any<RoleChangeEmailModel>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("<html>role-change</html>");
        _emailSender
            .SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Send failed"));

        // Act & Assert — non-critical email should not throw
        var act = () => _service.SendRoleChangeEmailAsync("user@example.com", "John", "Test Org", "Member", "Admin");
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SendMemberRemovedEmailAsync_WhenSenderThrows_ShouldNotRethrow()
    {
        // Arrange
        _templateRenderer
            .RenderAsync(Arg.Any<string>(), Arg.Any<MemberRemovedEmailModel>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("<html>removed</html>");
        _emailSender
            .SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Send failed"));

        // Act & Assert — non-critical email should not throw
        var act = () => _service.SendMemberRemovedEmailAsync("user@example.com", "John", "Test Org");
        await act.Should().NotThrowAsync();
    }
}
