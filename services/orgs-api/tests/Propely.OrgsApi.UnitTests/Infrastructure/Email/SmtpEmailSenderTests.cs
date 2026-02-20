// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Propely.OrgsApi.Infrastructure.Email;

namespace Propely.OrgsApi.UnitTests.Infrastructure.Email;

public sealed class SmtpEmailSenderTests
{
    [Fact]
    public async Task SendAsync_WhenSmtpHostUnreachable_ShouldThrow()
    {
        // Arrange — use an unreachable host
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Smtp:Host"] = "127.0.0.1", // localhost with unused port, immediate connection refused
                ["Smtp:Port"] = "19999",
                ["Smtp:From"] = "no-reply@propely.test",
                ["Smtp:EnableSsl"] = "false"
            })
            .Build();
        var sender = new SmtpEmailSender(configuration, Substitute.For<ILogger<SmtpEmailSender>>());

        // Act
        var act = () => sender.SendAsync("user@example.com", "Test Subject", "<p>Hello</p>");

        // Assert — SmtpClient throws when it cannot connect
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task CheckHealthAsync_WhenHostUnreachable_ShouldReturnFalse()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Smtp:Host"] = "127.0.0.1", // localhost with unused port, immediate connection refused
                ["Smtp:Port"] = "19999"
            })
            .Build();
        var sender = new SmtpEmailSender(configuration, Substitute.For<ILogger<SmtpEmailSender>>());

        // Act
        var result = await sender.CheckHealthAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CheckHealthAsync_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Smtp:Host"] = "localhost",
                ["Smtp:Port"] = "10025"
            })
            .Build();
        var sender = new SmtpEmailSender(configuration, Substitute.For<ILogger<SmtpEmailSender>>());
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var act = () => sender.CheckHealthAsync(cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task CheckHealthAsync_UsesConfiguredHostAndPort()
    {
        // Arrange — use a port that definitely won't have a listener
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Smtp:Host"] = "127.0.0.1",
                ["Smtp:Port"] = "19998"
            })
            .Build();
        var sender = new SmtpEmailSender(configuration, Substitute.For<ILogger<SmtpEmailSender>>());

        // Act
        var result = await sender.CheckHealthAsync();

        // Assert — should fail gracefully since nothing listens on port 19998
        result.Should().BeFalse();
    }
}
