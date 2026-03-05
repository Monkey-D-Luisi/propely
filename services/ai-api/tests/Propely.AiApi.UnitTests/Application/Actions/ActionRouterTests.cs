// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Propely.AiApi.Application.Actions.Commands.PropertyActions;
using Propely.AiApi.Domain.Actions;
using Propely.AiApi.Infrastructure.AI;

namespace Propely.AiApi.UnitTests.Application.Actions;

public sealed class ActionRouterTests
{
    private readonly IMediator _mediator;
    private readonly ActionRouter _router;

    public ActionRouterTests()
    {
        _mediator = Substitute.For<IMediator>();
        var logger = Substitute.For<ILogger<ActionRouter>>();
        _router = new ActionRouter(_mediator, logger);
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

    [Fact]
    public async Task RouteAsync_WhenCreateProperty_ShouldDispatchToMediator()
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

        var expectedResult = ActionResult.Ok(
            data: new { propertyType = "apartment" },
            message: "I'll create an apartment",
            type: ActionType.CreateProperty);

        _mediator.Send(Arg.Any<CreatePropertyActionCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _router.RouteAsync(intent, tenantId, agentId);

        // Assert
        result.Success.Should().BeTrue();
        result.ActionType.Should().Be(ActionType.CreateProperty);
        await _mediator.Received(1).Send(
            Arg.Is<CreatePropertyActionCommand>(c =>
                c.Parameters == parameters && c.TenantId == tenantId && c.AgentId == agentId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RouteAsync_WhenQueryProperties_ShouldDispatchToMediator()
    {
        // Arrange
        var parameters = new Dictionary<string, object?> { ["city"] = "Madrid" };
        var intent = new ClassifiedIntent(ActionType.QueryProperties, parameters, 0.8);
        var tenantId = Guid.NewGuid();
        var agentId = Guid.NewGuid();

        var expectedResult = ActionResult.Ok(
            data: new { totalCount = 5 },
            message: "I found 5 properties in Madrid.",
            type: ActionType.QueryProperties);

        _mediator.Send(Arg.Any<QueryPropertiesActionCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _router.RouteAsync(intent, tenantId, agentId);

        // Assert
        result.Success.Should().BeTrue();
        result.ActionType.Should().Be(ActionType.QueryProperties);
        await _mediator.Received(1).Send(
            Arg.Is<QueryPropertiesActionCommand>(c =>
                c.Parameters == parameters && c.TenantId == tenantId && c.AgentId == agentId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RouteAsync_WhenUpdateProperty_ShouldDispatchToMediator()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            ["property_id"] = Guid.NewGuid().ToString(),
            ["field"] = "price",
            ["value"] = "300000"
        };
        var intent = new ClassifiedIntent(ActionType.UpdateProperty, parameters, 0.9);
        var tenantId = Guid.NewGuid();
        var agentId = Guid.NewGuid();

        var expectedResult = ActionResult.Ok(
            data: new { field = "price" },
            message: "I'll update the price",
            type: ActionType.UpdateProperty);

        _mediator.Send(Arg.Any<UpdatePropertyActionCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _router.RouteAsync(intent, tenantId, agentId);

        // Assert
        result.Success.Should().BeTrue();
        result.ActionType.Should().Be(ActionType.UpdateProperty);
        await _mediator.Received(1).Send(
            Arg.Is<UpdatePropertyActionCommand>(c =>
                c.Parameters == parameters && c.TenantId == tenantId && c.AgentId == agentId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RouteAsync_WhenChangePropertyStatus_ShouldDispatchToMediator()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            ["property_id"] = Guid.NewGuid().ToString(),
            ["status"] = "Active"
        };
        var intent = new ClassifiedIntent(ActionType.ChangePropertyStatus, parameters, 0.95);
        var tenantId = Guid.NewGuid();
        var agentId = Guid.NewGuid();

        var expectedResult = ActionResult.Ok(
            data: new { targetStatus = "Active" },
            message: "I'll change the status to Active",
            type: ActionType.ChangePropertyStatus);

        _mediator.Send(Arg.Any<ChangePropertyStatusActionCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _router.RouteAsync(intent, tenantId, agentId);

        // Assert
        result.Success.Should().BeTrue();
        result.ActionType.Should().Be(ActionType.ChangePropertyStatus);
        await _mediator.Received(1).Send(
            Arg.Is<ChangePropertyStatusActionCommand>(c =>
                c.Parameters == parameters && c.TenantId == tenantId && c.AgentId == agentId),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(ActionType.GenerateCopy)]
    [InlineData(ActionType.ExtractFromText)]
    [InlineData(ActionType.ReserveProperty)]
    [InlineData(ActionType.CloseOperation)]
    [InlineData(ActionType.ArchiveProperty)]
    [InlineData(ActionType.CreateLead)]
    [InlineData(ActionType.BookViewing)]
    public async Task RouteAsync_WhenNonPropertyActionType_ShouldReturnPlaceholder(ActionType actionType)
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
        result.Message.Should().Contain("available soon");
        result.Confidence.Should().Be(0.9);
    }
}
