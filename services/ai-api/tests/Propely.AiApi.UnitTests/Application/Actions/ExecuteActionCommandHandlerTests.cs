// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Propely.AiApi.Application.Actions.Commands.ExecuteAction;
using Propely.AiApi.Application.Actions.Interfaces;
using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.UnitTests.Application.Actions;

public sealed class ExecuteActionCommandHandlerTests
{
    private readonly IIntentClassifier _intentClassifier;
    private readonly IActionRouter _actionRouter;
    private readonly ExecuteActionCommandHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AgentId = Guid.NewGuid();

    public ExecuteActionCommandHandlerTests()
    {
        _intentClassifier = Substitute.For<IIntentClassifier>();
        _actionRouter = Substitute.For<IActionRouter>();
        var logger = Substitute.For<ILogger<ExecuteActionCommandHandler>>();
        _handler = new ExecuteActionCommandHandler(_intentClassifier, _actionRouter, logger);
    }

    [Fact]
    public async Task Handle_WhenIntentIsUnknown_ShouldReturnFailureWithMessage()
    {
        // Arrange
        var command = new ExecuteActionCommand("hello world", TenantId, AgentId);
        _intentClassifier.ClassifyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ClassifiedIntent(ActionType.Unknown, new Dictionary<string, object?>(), 0.0));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.Unknown);
        result.Message.Should().Contain("didn't understand");
        await _actionRouter.DidNotReceive()
            .RouteAsync(Arg.Any<ClassifiedIntent>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenConfidenceIsBelowThreshold_ShouldReturnFailureWithSpecificMessage()
    {
        // Arrange
        var command = new ExecuteActionCommand("maybe something", TenantId, AgentId);
        _intentClassifier.ClassifyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ClassifiedIntent(ActionType.CreateProperty, new Dictionary<string, object?>(), 0.3));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.CreateProperty);
        result.Message.Should().Contain("more specific");
        await _actionRouter.DidNotReceive()
            .RouteAsync(Arg.Any<ClassifiedIntent>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenConfidenceIsExactlyAtThreshold_ShouldRouteToActionRouter()
    {
        // Arrange
        var command = new ExecuteActionCommand("create a property", TenantId, AgentId);
        var intent = new ClassifiedIntent(ActionType.CreateProperty, new Dictionary<string, object?>(), 0.5);
        _intentClassifier.ClassifyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(intent);
        _actionRouter.RouteAsync(Arg.Any<ClassifiedIntent>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ActionResult.Ok(null, "Property created", ActionType.CreateProperty));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        await _actionRouter.Received(1)
            .RouteAsync(intent, TenantId, AgentId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenValidIntentClassified_ShouldRouteToActionRouter()
    {
        // Arrange
        var command = new ExecuteActionCommand("Create a 3 bedroom apartment in Malaga for 250k", TenantId, AgentId);
        var parameters = new Dictionary<string, object?>
        {
            ["property_type"] = "apartment",
            ["bedrooms"] = 3L,
            ["city"] = "Malaga",
            ["price"] = 250000.0
        };
        var intent = new ClassifiedIntent(ActionType.CreateProperty, parameters, 1.0, "create_property");
        _intentClassifier.ClassifyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(intent);

        var expectedResult = ActionResult.Ok(
            new { parameters },
            "Property creation initiated",
            ActionType.CreateProperty,
            1.0);
        _actionRouter.RouteAsync(intent, TenantId, AgentId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.ActionType.Should().Be(ActionType.CreateProperty);
        result.Confidence.Should().Be(1.0);
        await _actionRouter.Received(1)
            .RouteAsync(intent, TenantId, AgentId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenClassifierThrows_ShouldReturnGracefulFailure()
    {
        // Arrange
        var command = new ExecuteActionCommand("create something", TenantId, AgentId);
        _intentClassifier.ClassifyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("OpenAI API key not configured"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.Unknown);
        result.Message.Should().Contain("error");
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenRouterThrows_ShouldReturnGracefulFailure()
    {
        // Arrange
        var command = new ExecuteActionCommand("create a villa", TenantId, AgentId);
        var intent = new ClassifiedIntent(ActionType.CreateProperty, new Dictionary<string, object?>(), 0.9);
        _intentClassifier.ClassifyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(intent);
        _actionRouter.RouteAsync(Arg.Any<ClassifiedIntent>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.CreateProperty);
        result.Message.Should().Contain("error");
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenRouterReturnsFailure_ShouldPropagateResult()
    {
        // Arrange
        var command = new ExecuteActionCommand("create a property", TenantId, AgentId);
        var intent = new ClassifiedIntent(ActionType.CreateProperty, new Dictionary<string, object?>(), 0.8);
        _intentClassifier.ClassifyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(intent);

        var failResult = ActionResult.Fail(
            ["Property creation failed due to missing required fields."],
            ActionType.CreateProperty,
            "Could not create property. Missing required fields.");
        _actionRouter.RouteAsync(Arg.Any<ClassifiedIntent>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(failResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.CreateProperty);
        result.Errors.Should().Contain("Property creation failed due to missing required fields.");
    }
}
