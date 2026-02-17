// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Application.Billing.Interfaces;
using SaasTemplate.OrgsApi.Application.Organizations.Interfaces;
using SaasTemplate.OrgsApi.Domain.Billing;
using SaasTemplate.OrgsApi.Domain.Organizations;
using SaasTemplate.OrgsApi.Infrastructure.Billing;
using FluentAssertions;
using NSubstitute;

namespace SaasTemplate.OrgsApi.UnitTests.Infrastructure.Billing;

public sealed class EntitlementServiceTests
{
    private readonly IPlanProvider _planProvider;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly EntitlementService _service;

    private static readonly PlanInfo FreePlan = new("free", "Free", 5, 1, ["5 members", "1 org"]);
    private static readonly PlanInfo ProPlan = new("pro", "Pro", 0, 5, ["Unlimited members", "5 orgs"]);
    private static readonly PlanInfo EnterprisePlan = new("enterprise", "Enterprise", 0, 0, ["Unlimited everything"]);

    public EntitlementServiceTests()
    {
        _planProvider = Substitute.For<IPlanProvider>();
        _subscriptionRepository = Substitute.For<ISubscriptionRepository>();
        _membershipRepository = Substitute.For<IMembershipRepository>();
        _planProvider.GetFreePlan().Returns(FreePlan);
        _service = new EntitlementService(_planProvider, _subscriptionRepository, _membershipRepository);
    }

    // --- CanCreateOrganizationAsync ---

    [Fact]
    public async Task CanCreateOrganization_WhenBillingDisabled_ShouldReturnTrue()
    {
        // Arrange
        _planProvider.IsBillingEnabled.Returns(false);

        // Act
        var result = await _service.CanCreateOrganizationAsync(Guid.NewGuid());

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanCreateOrganization_WhenUnderFreeLimit_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _planProvider.IsBillingEnabled.Returns(true);
        _membershipRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership>()); // 0 orgs, free limit is 1

        // Act
        var result = await _service.CanCreateOrganizationAsync(userId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanCreateOrganization_WhenAtFreeLimit_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        _planProvider.IsBillingEnabled.Returns(true);

        var memberships = new List<Membership> { Membership.Create(userId, orgId, MembershipRole.Owner) };
        _membershipRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(memberships);
        _subscriptionRepository.GetByOrgIdAsync(orgId, Arg.Any<CancellationToken>()).Returns((Subscription?)null);

        // Act
        var result = await _service.CanCreateOrganizationAsync(userId);

        // Assert
        result.Should().BeFalse(); // 1 org, free limit is 1
    }

    [Fact]
    public async Task CanCreateOrganization_WithProSubscription_ShouldUseHigherLimit()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        _planProvider.IsBillingEnabled.Returns(true);

        var memberships = new List<Membership> { Membership.Create(userId, orgId, MembershipRole.Owner) };
        _membershipRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(memberships);

        var subscription = Subscription.Create(orgId, "cus_1", "sub_1", "pro", SubscriptionStatus.Active, DateTime.UtcNow.AddDays(30));
        _subscriptionRepository.GetByOrgIdAsync(orgId, Arg.Any<CancellationToken>()).Returns(subscription);
        _planProvider.GetPlan("pro").Returns(ProPlan);

        // Act
        var result = await _service.CanCreateOrganizationAsync(userId);

        // Assert
        result.Should().BeTrue(); // 1 org, pro limit is 5
    }

    [Fact]
    public async Task CanCreateOrganization_WithUnlimitedPlan_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        _planProvider.IsBillingEnabled.Returns(true);

        var memberships = new List<Membership> { Membership.Create(userId, orgId, MembershipRole.Owner) };
        _membershipRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(memberships);

        var subscription = Subscription.Create(orgId, "cus_1", "sub_1", "enterprise", SubscriptionStatus.Active, DateTime.UtcNow.AddDays(30));
        _subscriptionRepository.GetByOrgIdAsync(orgId, Arg.Any<CancellationToken>()).Returns(subscription);
        _planProvider.GetPlan("enterprise").Returns(EnterprisePlan); // MaxOrganizations = 0 (unlimited)

        // Act
        var result = await _service.CanCreateOrganizationAsync(userId);

        // Assert
        result.Should().BeTrue();
    }

    // --- CanAddMemberAsync ---

    [Fact]
    public async Task CanAddMember_WhenBillingDisabled_ShouldReturnTrue()
    {
        // Arrange
        _planProvider.IsBillingEnabled.Returns(false);

        // Act
        var result = await _service.CanAddMemberAsync(Guid.NewGuid());

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanAddMember_WhenUnderFreeLimit_ShouldReturnTrue()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        _planProvider.IsBillingEnabled.Returns(true);
        _subscriptionRepository.GetByOrgIdAsync(orgId, Arg.Any<CancellationToken>()).Returns((Subscription?)null);
        _membershipRepository.GetByOrgIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership>()); // 0 members, free limit is 5

        // Act
        var result = await _service.CanAddMemberAsync(orgId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanAddMember_WhenAtFreeLimit_ShouldReturnFalse()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        _planProvider.IsBillingEnabled.Returns(true);
        _subscriptionRepository.GetByOrgIdAsync(orgId, Arg.Any<CancellationToken>()).Returns((Subscription?)null);

        var members = Enumerable.Range(0, 5)
            .Select(_ => Membership.Create(Guid.NewGuid(), orgId, MembershipRole.Member))
            .ToList();
        _membershipRepository.GetByOrgIdAsync(orgId, Arg.Any<CancellationToken>()).Returns(members);

        // Act
        var result = await _service.CanAddMemberAsync(orgId);

        // Assert
        result.Should().BeFalse(); // 5 members, free limit is 5
    }

    [Fact]
    public async Task CanAddMember_WithUnlimitedMemberPlan_ShouldReturnTrue()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        _planProvider.IsBillingEnabled.Returns(true);

        var subscription = Subscription.Create(orgId, "cus_1", "sub_1", "pro", SubscriptionStatus.Active, DateTime.UtcNow.AddDays(30));
        _subscriptionRepository.GetByOrgIdAsync(orgId, Arg.Any<CancellationToken>()).Returns(subscription);
        _planProvider.GetPlan("pro").Returns(ProPlan); // MaxMembers = 0 (unlimited)

        var members = Enumerable.Range(0, 100)
            .Select(_ => Membership.Create(Guid.NewGuid(), orgId, MembershipRole.Member))
            .ToList();
        _membershipRepository.GetByOrgIdAsync(orgId, Arg.Any<CancellationToken>()).Returns(members);

        // Act
        var result = await _service.CanAddMemberAsync(orgId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanAddMember_WithCancelledSubscription_ShouldFallBackToFreePlan()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        _planProvider.IsBillingEnabled.Returns(true);

        var subscription = Subscription.Create(orgId, "cus_1", "sub_1", "pro", SubscriptionStatus.Cancelled, DateTime.UtcNow);
        _subscriptionRepository.GetByOrgIdAsync(orgId, Arg.Any<CancellationToken>()).Returns(subscription);

        var members = Enumerable.Range(0, 5)
            .Select(_ => Membership.Create(Guid.NewGuid(), orgId, MembershipRole.Member))
            .ToList();
        _membershipRepository.GetByOrgIdAsync(orgId, Arg.Any<CancellationToken>()).Returns(members);

        // Act
        var result = await _service.CanAddMemberAsync(orgId);

        // Assert
        result.Should().BeFalse(); // Falls back to free plan (limit 5), has 5 members
    }
}
