// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Billing;
using Propely.OrgsApi.Infrastructure.Billing;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace Propely.OrgsApi.UnitTests.Infrastructure.Billing;

public sealed class PlanProviderTests
{
    private static BillingConfiguration CreateConfig(string mode = "stripe") => new()
    {
        Mode = mode,
        Plans =
        [
            new PlanConfiguration { Id = "free", Name = "Free", MaxMembers = 5, MaxOrganizations = 1, Features = ["5 members", "1 org"] },
            new PlanConfiguration { Id = "pro", Name = "Pro", MaxMembers = 0, MaxOrganizations = 5, Features = ["Unlimited members", "5 orgs"] },
            new PlanConfiguration { Id = "enterprise", Name = "Enterprise", MaxMembers = 0, MaxOrganizations = 0, Features = ["Unlimited everything"] }
        ]
    };

    private static PlanProvider CreateProvider(BillingConfiguration config) =>
        new(Options.Create(config));

    [Fact]
    public void IsBillingEnabled_WhenStripe_ShouldReturnTrue()
    {
        var provider = CreateProvider(CreateConfig("stripe"));
        provider.IsBillingEnabled.Should().BeTrue();
    }

    [Fact]
    public void IsBillingEnabled_WhenFree_ShouldReturnFalse()
    {
        var provider = CreateProvider(CreateConfig("free"));
        provider.IsBillingEnabled.Should().BeFalse();
    }

    [Fact]
    public void IsBillingEnabled_ShouldBeCaseInsensitive()
    {
        var provider = CreateProvider(CreateConfig("Stripe"));
        provider.IsBillingEnabled.Should().BeTrue();
    }

    [Fact]
    public void GetPlan_WithValidId_ShouldReturnPlanInfo()
    {
        var provider = CreateProvider(CreateConfig());

        var plan = provider.GetPlan("pro");

        plan.Should().NotBeNull();
        plan!.Id.Should().Be("pro");
        plan.Name.Should().Be("Pro");
        plan.MaxMembers.Should().Be(0);
        plan.MaxOrganizations.Should().Be(5);
        plan.Features.Should().Contain("Unlimited members");
    }

    [Fact]
    public void GetPlan_ShouldBeCaseInsensitive()
    {
        var provider = CreateProvider(CreateConfig());

        var plan = provider.GetPlan("PRO");

        plan.Should().NotBeNull();
        plan!.Id.Should().Be("pro");
    }

    [Fact]
    public void GetPlan_WithInvalidId_ShouldReturnNull()
    {
        var provider = CreateProvider(CreateConfig());

        var plan = provider.GetPlan("nonexistent");

        plan.Should().BeNull();
    }

    [Fact]
    public void GetFreePlan_ShouldReturnFreePlanFromConfig()
    {
        var provider = CreateProvider(CreateConfig());

        var plan = provider.GetFreePlan();

        plan.Id.Should().Be("free");
        plan.Name.Should().Be("Free");
        plan.MaxMembers.Should().Be(5);
        plan.MaxOrganizations.Should().Be(1);
    }

    [Fact]
    public void GetFreePlan_WhenNoFreePlanInConfig_ShouldReturnDefaults()
    {
        var config = new BillingConfiguration { Mode = "stripe", Plans = [] };
        var provider = CreateProvider(config);

        var plan = provider.GetFreePlan();

        plan.Id.Should().Be("free");
        plan.Name.Should().Be("Free");
        plan.MaxMembers.Should().Be(5);
        plan.MaxOrganizations.Should().Be(1);
    }
}
