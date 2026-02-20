// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Microsoft.Extensions.Logging;
using NSubstitute;
using Propely.OrgsApi.Infrastructure.Billing;
using FluentAssertions;

namespace Propely.OrgsApi.UnitTests.Infrastructure.Billing;

public sealed class NoOpPaymentServiceTests
{
    private readonly NoOpPaymentService _service;

    public NoOpPaymentServiceTests()
    {
        var logger = Substitute.For<ILogger<NoOpPaymentService>>();
        _service = new NoOpPaymentService(logger);
    }

    [Fact]
    public void IsEnabled_ShouldReturnFalse()
    {
        _service.IsEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task CreateCheckoutSessionAsync_ShouldReturnEmptyString()
    {
        // Act
        var result = await _service.CreateCheckoutSessionAsync(
            Guid.NewGuid(),
            "pro",
            "https://example.com/success",
            "https://example.com/cancel");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateCustomerPortalSessionAsync_ShouldReturnEmptyString()
    {
        // Act
        var result = await _service.CreateCustomerPortalSessionAsync(
            Guid.NewGuid(),
            "https://example.com/return");

        // Assert
        result.Should().BeEmpty();
    }
}
