// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Billing.Interfaces;
using Propely.OrgsApi.Application.Billing.Queries.GetPlans;
using FluentAssertions;
using NSubstitute;

namespace Propely.OrgsApi.UnitTests.Application.Billing.Queries;

public sealed class GetPlansQueryHandlerTests
{
    private readonly IPlanProvider _planProvider;
    private readonly GetPlansQueryHandler _handler;

    public GetPlansQueryHandlerTests()
    {
        _planProvider = Substitute.For<IPlanProvider>();
        _handler = new GetPlansQueryHandler(_planProvider);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllPlans()
    {
        // Arrange
        var plans = new List<PlanInfo>
        {
            new("free", "Free", 5, 1, ["5 members", "1 org"]),
            new("pro", "Pro", 0, 5, ["Unlimited members", "5 orgs"]),
            new("enterprise", "Enterprise", 0, 0, ["Unlimited everything"])
        };
        _planProvider.GetAllPlans().Returns(plans);

        // Act
        var result = await _handler.Handle(new GetPlansQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result[0].Id.Should().Be("free");
        result[0].Name.Should().Be("Free");
        result[0].Features.Should().Contain("5 members");
        result[1].Id.Should().Be("pro");
        result[2].Id.Should().Be("enterprise");
    }

    [Fact]
    public async Task Handle_WhenNoPlans_ShouldReturnEmptyList()
    {
        // Arrange
        _planProvider.GetAllPlans().Returns(new List<PlanInfo>());

        // Act
        var result = await _handler.Handle(new GetPlansQuery(), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}
