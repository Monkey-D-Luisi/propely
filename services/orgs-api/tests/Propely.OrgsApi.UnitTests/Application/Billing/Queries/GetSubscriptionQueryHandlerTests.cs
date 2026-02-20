// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Billing.Interfaces;
using Propely.OrgsApi.Application.Billing.Queries.GetSubscription;
using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Domain.Billing;
using Propely.OrgsApi.Domain.Common.Exceptions;
using Propely.OrgsApi.Domain.Organizations;
using FluentAssertions;
using NSubstitute;

namespace Propely.OrgsApi.UnitTests.Application.Billing.Queries;

public sealed class GetSubscriptionQueryHandlerTests
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IPlanProvider _planProvider;
    private readonly GetSubscriptionQueryHandler _handler;

    public GetSubscriptionQueryHandlerTests()
    {
        _subscriptionRepository = Substitute.For<ISubscriptionRepository>();
        _membershipRepository = Substitute.For<IMembershipRepository>();
        _planProvider = Substitute.For<IPlanProvider>();
        _planProvider.GetFreePlan().Returns(new PlanInfo("free", "Free", 5, 1, ["5 members", "1 org"]));
        _handler = new GetSubscriptionQueryHandler(_subscriptionRepository, _membershipRepository, _planProvider);
    }

    [Fact]
    public async Task Handle_WhenNoSubscription_ShouldReturnFreePlan()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var membership = Membership.Create(userId, orgId, MembershipRole.Member);

        _membershipRepository.GetAsync(orgId, userId, Arg.Any<CancellationToken>()).Returns(membership);
        _subscriptionRepository.GetByOrgIdAsync(orgId, Arg.Any<CancellationToken>()).Returns((Subscription?)null);

        var query = new GetSubscriptionQuery(orgId, userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.PlanId.Should().Be("free");
        result.PlanName.Should().Be("Free");
        result.Status.Should().Be("free");
        result.CurrentPeriodEnd.Should().BeNull();
        result.Features.Should().Contain("5 members");
    }

    [Fact]
    public async Task Handle_WithActiveSubscription_ShouldReturnPlanDetails()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var membership = Membership.Create(userId, orgId, MembershipRole.Member);
        var periodEnd = DateTime.UtcNow.AddDays(30);
        var subscription = Subscription.Create(orgId, "cus_123", "sub_123", "pro", SubscriptionStatus.Active, periodEnd);
        var proPlan = new PlanInfo("pro", "Pro", 0, 5, ["Unlimited members", "5 orgs"]);

        _membershipRepository.GetAsync(orgId, userId, Arg.Any<CancellationToken>()).Returns(membership);
        _subscriptionRepository.GetByOrgIdAsync(orgId, Arg.Any<CancellationToken>()).Returns(subscription);
        _planProvider.GetPlan("pro").Returns(proPlan);

        var query = new GetSubscriptionQuery(orgId, userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.PlanId.Should().Be("pro");
        result.PlanName.Should().Be("Pro");
        result.Status.Should().Be("active");
        result.CurrentPeriodEnd.Should().Be(periodEnd);
        result.Features.Should().Contain("Unlimited members");
    }

    [Fact]
    public async Task Handle_WithTrialingSubscription_ShouldReturnPlanDetails()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var membership = Membership.Create(userId, orgId, MembershipRole.Member);
        var periodEnd = DateTime.UtcNow.AddDays(14);
        var subscription = Subscription.Create(orgId, "cus_123", "sub_123", "pro", SubscriptionStatus.Trialing, periodEnd);
        var proPlan = new PlanInfo("pro", "Pro", 0, 5, ["Unlimited members", "5 orgs"]);

        _membershipRepository.GetAsync(orgId, userId, Arg.Any<CancellationToken>()).Returns(membership);
        _subscriptionRepository.GetByOrgIdAsync(orgId, Arg.Any<CancellationToken>()).Returns(subscription);
        _planProvider.GetPlan("pro").Returns(proPlan);

        var query = new GetSubscriptionQuery(orgId, userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.PlanId.Should().Be("pro");
        result.Status.Should().Be("trialing");
    }

    [Fact]
    public async Task Handle_WithCancelledSubscription_ShouldReturnFreePlan()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var membership = Membership.Create(userId, orgId, MembershipRole.Member);
        var subscription = Subscription.Create(orgId, "cus_123", "sub_123", "pro", SubscriptionStatus.Cancelled, DateTime.UtcNow);

        _membershipRepository.GetAsync(orgId, userId, Arg.Any<CancellationToken>()).Returns(membership);
        _subscriptionRepository.GetByOrgIdAsync(orgId, Arg.Any<CancellationToken>()).Returns(subscription);

        var query = new GetSubscriptionQuery(orgId, userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.PlanId.Should().Be("free");
        result.Status.Should().Be("free");
    }

    [Fact]
    public async Task Handle_WhenUserNotMember_ShouldThrowForbiddenException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _membershipRepository.GetAsync(orgId, userId, Arg.Any<CancellationToken>()).Returns((Membership?)null);

        var query = new GetSubscriptionQuery(orgId, userId);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("User is not a member of this organization.");
    }

    [Fact]
    public async Task Handle_WithActiveSubscriptionButUnknownPlan_ShouldReturnFreePlan()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var membership = Membership.Create(userId, orgId, MembershipRole.Member);
        var subscription = Subscription.Create(orgId, "cus_123", "sub_123", "unknown_plan", SubscriptionStatus.Active, DateTime.UtcNow.AddDays(30));

        _membershipRepository.GetAsync(orgId, userId, Arg.Any<CancellationToken>()).Returns(membership);
        _subscriptionRepository.GetByOrgIdAsync(orgId, Arg.Any<CancellationToken>()).Returns(subscription);
        _planProvider.GetPlan("unknown_plan").Returns((PlanInfo?)null);

        var query = new GetSubscriptionQuery(orgId, userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.PlanId.Should().Be("free");
        result.Status.Should().Be("free");
    }
}
