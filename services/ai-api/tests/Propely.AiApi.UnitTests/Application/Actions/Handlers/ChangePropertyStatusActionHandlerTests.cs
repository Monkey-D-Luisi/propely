// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Propely.AiApi.Application.Actions.Commands.PropertyActions;
using Propely.AiApi.Application.Actions.Handlers;
using Propely.AiApi.Application.Actions.Parameters;
using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.UnitTests.Application.Actions.Handlers;

public sealed class ChangePropertyStatusActionHandlerTests
{
    private readonly ChangePropertyStatusActionHandler _handler;
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AgentId = Guid.NewGuid();

    public ChangePropertyStatusActionHandlerTests()
    {
        var logger = Substitute.For<ILogger<ChangePropertyStatusActionHandler>>();
        _handler = new ChangePropertyStatusActionHandler(logger);
    }

    [Fact]
    public async Task Handle_WhenValidParameters_ShouldReturnSuccess()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var parameters = new ChangePropertyStatusParameters(
            PropertyId: propertyId,
            Reference: null,
            Status: "Active");
        var command = new ChangePropertyStatusActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.ActionType.Should().Be(ActionType.ChangePropertyStatus);
        result.Message.Should().Contain("Active");
    }

    [Fact]
    public async Task Handle_WhenMissingPropertyIdentifier_ShouldReturnFailure()
    {
        // Arrange
        var parameters = new ChangePropertyStatusParameters(
            PropertyId: null,
            Reference: null,
            Status: "Active");
        var command = new ChangePropertyStatusActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.ChangePropertyStatus);
        result.Errors.Should().NotBeEmpty();
        result.Message.Should().Contain("property ID");
    }

    [Fact]
    public async Task Handle_WhenMissingTargetStatus_ShouldReturnFailure()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var parameters = new ChangePropertyStatusParameters(
            PropertyId: propertyId,
            Reference: null,
            Status: null);
        var command = new ChangePropertyStatusActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.ChangePropertyStatus);
        result.Errors.Should().NotBeEmpty();
        result.Message.Should().Contain("target status");
    }

    [Fact]
    public async Task Handle_WhenInvalidStatus_ShouldReturnFailure()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var parameters = new ChangePropertyStatusParameters(
            PropertyId: propertyId,
            Reference: null,
            Status: "InvalidStatus");
        var command = new ChangePropertyStatusActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.ChangePropertyStatus);
        result.Errors.Should().NotBeEmpty();
        result.Message.Should().Contain("InvalidStatus");
        result.Message.Should().Contain("not a valid status");
    }

    [Theory]
    [InlineData("Draft")]
    [InlineData("Active")]
    [InlineData("Reserved")]
    [InlineData("Sold")]
    [InlineData("Rented")]
    [InlineData("Archived")]
    [InlineData("Withdrawn")]
    public async Task Handle_WhenValidStatus_ShouldReturnSuccess(string validStatus)
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var parameters = new ChangePropertyStatusParameters(
            PropertyId: propertyId,
            Reference: null,
            Status: validStatus);
        var command = new ChangePropertyStatusActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.ActionType.Should().Be(ActionType.ChangePropertyStatus);
        result.Message.Should().Contain(validStatus);
    }

    [Fact]
    public async Task Handle_WhenUsingReference_ShouldReturnSuccess()
    {
        // Arrange
        var parameters = new ChangePropertyStatusParameters(
            PropertyId: null,
            Reference: "PROP-042",
            Status: "Sold");
        var command = new ChangePropertyStatusActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.ActionType.Should().Be(ActionType.ChangePropertyStatus);
        result.Message.Should().Contain("PROP-042");
        result.Message.Should().Contain("Sold");
    }

    [Fact]
    public async Task Handle_WhenValidParameters_ShouldIncludeStructuredData()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var parameters = new ChangePropertyStatusParameters(
            PropertyId: propertyId,
            Reference: null,
            Status: "Reserved");
        var command = new ChangePropertyStatusActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        var data = result.Data as Dictionary<string, object?>;
        data.Should().NotBeNull();
        data!["propertyId"].Should().Be(propertyId);
        data["targetStatus"].Should().Be("Reserved");
        data["agentId"].Should().Be(AgentId);
        data["tenantId"].Should().Be(TenantId);
    }
}
