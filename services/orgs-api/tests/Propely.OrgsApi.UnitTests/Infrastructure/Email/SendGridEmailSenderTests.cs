// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Propely.OrgsApi.Infrastructure.Email;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net;

namespace Propely.OrgsApi.UnitTests.Infrastructure.Email;

public sealed class SendGridEmailSenderTests
{
    private readonly ISendGridClient _sendGridClient;
    private readonly SendGridEmailSender _sender;

    public SendGridEmailSenderTests()
    {
        _sendGridClient = Substitute.For<ISendGridClient>();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SendGrid:From"] = "noreply@example.com",
                ["SendGrid:FromName"] = "Test App"
            })
            .Build();
        var logger = Substitute.For<ILogger<SendGridEmailSender>>();
        _sender = new SendGridEmailSender(_sendGridClient, configuration, logger);
    }

    [Fact]
    public async Task SendAsync_WhenSuccessful_ShouldCallSendGridClient()
    {
        // Arrange
        var response = new Response(HttpStatusCode.Accepted, null, null);
        _sendGridClient
            .SendEmailAsync(Arg.Any<SendGridMessage>(), Arg.Any<CancellationToken>())
            .Returns(response);

        // Act
        await _sender.SendAsync("user@example.com", "Subject", "<p>Hello</p>");

        // Assert
        await _sendGridClient.Received(1)
            .SendEmailAsync(Arg.Is<SendGridMessage>(m =>
                m.From.Email == "noreply@example.com" &&
                m.From.Name == "Test App" &&
                m.Subject == "Subject" &&
                m.HtmlContent == "<p>Hello</p>"),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAsync_WhenApiFails_ShouldThrowInvalidOperationException()
    {
        // Arrange
        using var content = new StringContent("Unauthorized");
        var response = new Response(HttpStatusCode.Unauthorized, content, null);
        _sendGridClient
            .SendEmailAsync(Arg.Any<SendGridMessage>(), Arg.Any<CancellationToken>())
            .Returns(response);

        // Act
        var act = () => _sender.SendAsync("user@example.com", "Subject", "<p>Hello</p>");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*401*");
    }

    [Fact]
    public async Task SendAsync_UsesSmtpFromFallback_WhenSendGridFromNotConfigured()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Smtp:From"] = "fallback@example.com"
            })
            .Build();
        var sender = new SendGridEmailSender(
            _sendGridClient,
            configuration,
            Substitute.For<ILogger<SendGridEmailSender>>());

        var response = new Response(HttpStatusCode.Accepted, null, null);
        _sendGridClient
            .SendEmailAsync(Arg.Any<SendGridMessage>(), Arg.Any<CancellationToken>())
            .Returns(response);

        // Act
        await sender.SendAsync("user@example.com", "Subject", "<p>Hello</p>");

        // Assert
        await _sendGridClient.Received(1)
            .SendEmailAsync(Arg.Is<SendGridMessage>(m =>
                m.From.Email == "fallback@example.com"),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CheckHealthAsync_WhenApiReturnsSuccess_ShouldReturnTrue()
    {
        // Arrange
        var response = new Response(HttpStatusCode.OK, null, null);
        _sendGridClient
            .RequestAsync(
                Arg.Any<SendGridClient.Method>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        // Act
        var result = await _sender.CheckHealthAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckHealthAsync_WhenApiThrows_ShouldReturnFalse()
    {
        // Arrange
        _sendGridClient
            .RequestAsync(
                Arg.Any<SendGridClient.Method>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        // Act
        var result = await _sender.CheckHealthAsync();

        // Assert
        result.Should().BeFalse();
    }
}
