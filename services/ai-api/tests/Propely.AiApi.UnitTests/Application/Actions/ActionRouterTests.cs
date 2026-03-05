// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Propely.AiApi.Domain.Actions;
using Propely.AiApi.Infrastructure.AI;

namespace Propely.AiApi.UnitTests.Application.Actions;

public sealed class ActionRouterTests
{
    private readonly ActionRouter _router;

    public ActionRouterTests()
    {
        var logger = Substitute.For<ILogger<ActionRouter>>();
        _router = new ActionRouter(logger);
    }

    [Fact]
    public async Task RouteAsync_WhenUnknownActionType_ShouldReturnFailure()
    {
        // Arrange
        var intent = new ClassifiedIntent(ActionType.Unknown, new Dictionary<string, object?>(), 0.0);
        var tenantId = Guid.NewGuid();
        var agentId = Guid.NewGuid();

        // Act
        var result = await _router.RouteAsync(intent, tenantId, agentId);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.Unknown);
        result.Errors.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(ActionType.CreateProperty)]
    [InlineData(ActionType.UpdateProperty)]
    [InlineData(ActionType.QueryProperties)]
    [InlineData(ActionType.ChangePropertyStatus)]
    [InlineData(ActionType.GenerateCopy)]
    [InlineData(ActionType.ExtractFromText)]
    [InlineData(ActionType.ReserveProperty)]
    [InlineData(ActionType.CloseOperation)]
    [InlineData(ActionType.ArchiveProperty)]
    public async Task RouteAsync_WhenKnownActionType_ShouldReturnSuccessWithPlaceholderMessage(ActionType actionType)
    {
        // Arrange
        var parameters = new Dictionary<string, object?> { ["test_param"] = "value" };
        var intent = new ClassifiedIntent(actionType, parameters, 0.9);
        var tenantId = Guid.NewGuid();
        var agentId = Guid.NewGuid();

        // Act
        var result = await _router.RouteAsync(intent, tenantId, agentId);

        // Assert
        result.Success.Should().BeTrue();
        result.ActionType.Should().Be(actionType);
        result.Message.Should().NotBeNullOrEmpty();
        result.Confidence.Should().Be(0.9);
    }

    [Fact]
    public async Task RouteAsync_WhenCreateProperty_ShouldIncludeParametersInData()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            ["property_type"] = "apartment",
            ["bedrooms"] = 3L,
            ["city"] = "Malaga"
        };
        var intent = new ClassifiedIntent(ActionType.CreateProperty, parameters, 1.0, "create_property");
        var tenantId = Guid.NewGuid();
        var agentId = Guid.NewGuid();

        // Act
        var result = await _router.RouteAsync(intent, tenantId, agentId);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.ActionType.Should().Be(ActionType.CreateProperty);
        result.Message.Should().Contain("create a property");
    }

    [Fact]
    public async Task RouteAsync_WhenQueryProperties_ShouldReturnPlaceholder()
    {
        // Arrange
        var parameters = new Dictionary<string, object?> { ["city"] = "Madrid" };
        var intent = new ClassifiedIntent(ActionType.QueryProperties, parameters, 0.8);
        var tenantId = Guid.NewGuid();
        var agentId = Guid.NewGuid();

        // Act
        var result = await _router.RouteAsync(intent, tenantId, agentId);

        // Assert
        result.Success.Should().BeTrue();
        result.ActionType.Should().Be(ActionType.QueryProperties);
        result.Message.Should().Contain("search properties");
    }
}
