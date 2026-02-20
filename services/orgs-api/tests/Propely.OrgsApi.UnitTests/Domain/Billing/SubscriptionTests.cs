// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Domain.Billing;
using FluentAssertions;

namespace Propely.OrgsApi.UnitTests.Domain.Billing;

public sealed class SubscriptionTests
{
    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var periodEnd = DateTime.UtcNow.AddDays(30);

        // Act
        var subscription = Subscription.Create(
            orgId,
            "cus_test123",
            "sub_test456",
            "pro",
            SubscriptionStatus.Active,
            periodEnd);

        // Assert
        subscription.Id.Should().NotBeEmpty();
        subscription.OrganizationId.Should().Be(orgId);
        subscription.StripeCustomerId.Should().Be("cus_test123");
        subscription.StripeSubscriptionId.Should().Be("sub_test456");
        subscription.PlanId.Should().Be("pro");
        subscription.Status.Should().Be(SubscriptionStatus.Active);
        subscription.CurrentPeriodEnd.Should().Be(periodEnd);
        subscription.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        subscription.CancelledAtUtc.Should().BeNull();
        subscription.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void UpdateStatus_ShouldUpdateStatusAndPeriodEnd()
    {
        // Arrange
        var subscription = Subscription.Create(
            Guid.NewGuid(),
            "cus_test",
            "sub_test",
            "pro",
            SubscriptionStatus.Active,
            DateTime.UtcNow.AddDays(30));

        var newPeriodEnd = DateTime.UtcNow.AddDays(60);

        // Act
        subscription.UpdateStatus(SubscriptionStatus.PastDue, newPeriodEnd);

        // Assert
        subscription.Status.Should().Be(SubscriptionStatus.PastDue);
        subscription.CurrentPeriodEnd.Should().Be(newPeriodEnd);
        subscription.UpdatedAtUtc.Should().NotBeNull();
        subscription.UpdatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Cancel_ShouldSetCancelledStatus()
    {
        // Arrange
        var subscription = Subscription.Create(
            Guid.NewGuid(),
            "cus_test",
            "sub_test",
            "pro",
            SubscriptionStatus.Active,
            DateTime.UtcNow.AddDays(30));

        // Act
        subscription.Cancel();

        // Assert
        subscription.Status.Should().Be(SubscriptionStatus.Cancelled);
        subscription.CancelledAtUtc.Should().NotBeNull();
        subscription.CancelledAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        subscription.UpdatedAtUtc.Should().NotBeNull();
        subscription.CancelledAtUtc.Should().Be(subscription.UpdatedAtUtc);
    }
}
