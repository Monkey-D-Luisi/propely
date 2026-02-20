// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NSubstitute;
using Propely.OrgsApi.Api.Configuration;
using Propely.OrgsApi.Application.Common.Interfaces;

namespace Propely.OrgsApi.UnitTests.Api.Configuration;

public sealed class EmailHealthCheckTests
{
    private readonly IEmailSender _emailSender;
    private readonly EmailHealthCheck _healthCheck;

    public EmailHealthCheckTests()
    {
        _emailSender = Substitute.For<IEmailSender>();
        _healthCheck = new EmailHealthCheck(_emailSender);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenSenderIsHealthy_ShouldReturnHealthy()
    {
        // Arrange
        _emailSender.CheckHealthAsync(Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var result = await _healthCheck.CheckHealthAsync(
            new HealthCheckContext(),
            CancellationToken.None);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenSenderIsUnhealthy_ShouldReturnUnhealthy()
    {
        // Arrange
        _emailSender.CheckHealthAsync(Arg.Any<CancellationToken>()).Returns(false);

        // Act
        var result = await _healthCheck.CheckHealthAsync(
            new HealthCheckContext(),
            CancellationToken.None);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Contain("unreachable");
    }
}
