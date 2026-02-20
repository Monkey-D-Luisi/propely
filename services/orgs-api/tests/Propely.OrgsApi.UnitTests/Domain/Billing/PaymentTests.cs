// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Domain.Billing;
using FluentAssertions;

namespace Propely.OrgsApi.UnitTests.Domain.Billing;

public sealed class PaymentTests
{
    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        // Arrange
        var orgId = Guid.NewGuid();

        // Act
        var payment = Payment.Create(
            orgId,
            "pi_test123",
            5000,
            "USD",
            "Test payment",
            PaymentStatus.Pending);

        // Assert
        payment.Id.Should().NotBeEmpty();
        payment.OrganizationId.Should().Be(orgId);
        payment.StripePaymentIntentId.Should().Be("pi_test123");
        payment.Amount.Should().Be(5000);
        payment.Currency.Should().Be("usd");
        payment.Description.Should().Be("Test payment");
        payment.Status.Should().Be(PaymentStatus.Pending);
        payment.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_ShouldNormalizeCurrencyToLowercase()
    {
        // Act
        var payment = Payment.Create(
            Guid.NewGuid(),
            "pi_test",
            1000,
            "  EUR  ",
            "Payment",
            PaymentStatus.Pending);

        // Assert
        payment.Currency.Should().Be("eur");
    }

    [Fact]
    public void Create_ShouldTrimStripePaymentIntentId()
    {
        // Act
        var payment = Payment.Create(
            Guid.NewGuid(),
            "  pi_test123  ",
            1000,
            "usd",
            "Payment",
            PaymentStatus.Pending);

        // Assert
        payment.StripePaymentIntentId.Should().Be("pi_test123");
    }

    [Fact]
    public void Create_ShouldTrimDescription()
    {
        // Act
        var payment = Payment.Create(
            Guid.NewGuid(),
            "pi_test",
            1000,
            "usd",
            "  Trimmed description  ",
            PaymentStatus.Pending);

        // Assert
        payment.Description.Should().Be("Trimmed description");
    }

    [Fact]
    public void MarkSucceeded_ShouldUpdateStatus()
    {
        // Arrange
        var payment = Payment.Create(
            Guid.NewGuid(),
            "pi_test",
            1000,
            "usd",
            "Payment",
            PaymentStatus.Pending);

        // Act
        payment.MarkSucceeded();

        // Assert
        payment.Status.Should().Be(PaymentStatus.Succeeded);
    }

    [Fact]
    public void MarkFailed_ShouldUpdateStatus()
    {
        // Arrange
        var payment = Payment.Create(
            Guid.NewGuid(),
            "pi_test",
            1000,
            "usd",
            "Payment",
            PaymentStatus.Pending);

        // Act
        payment.MarkFailed();

        // Assert
        payment.Status.Should().Be(PaymentStatus.Failed);
    }
}
