// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Propely.AiApi.Application.Actions.Commands.Operations;
using Propely.AiApi.Application.Actions.Handlers.Operations;
using Propely.AiApi.Application.Actions.Parameters;
using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.UnitTests.Application.Actions.Handlers.Operations;

public sealed class CloseOperationActionHandlerTests
{
    private readonly CloseOperationActionHandler _handler;
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AgentId = Guid.NewGuid();

    public CloseOperationActionHandlerTests()
    {
        var logger = Substitute.For<ILogger<CloseOperationActionHandler>>();
        _handler = new CloseOperationActionHandler(logger);
    }

    [Fact]
    public async Task Handle_WhenSaleOperationType_ShouldReturnSoldStatus()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var parameters = new CloseOperationParameters(
            PropertyId: propertyId,
            Reference: null,
            OperationType: "sale");
        var command = new CloseOperationActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.ActionType.Should().Be(ActionType.CloseOperation);
        var data = result.Data as Dictionary<string, object?>;
        data.Should().NotBeNull();
        data!["targetStatus"].Should().Be("Sold");
        result.Message.Should().Contain("Sold");
    }

    [Fact]
    public async Task Handle_WhenRentOperationType_ShouldReturnRentedStatus()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var parameters = new CloseOperationParameters(
            PropertyId: propertyId,
            Reference: null,
            OperationType: "rent");
        var command = new CloseOperationActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.ActionType.Should().Be(ActionType.CloseOperation);
        var data = result.Data as Dictionary<string, object?>;
        data.Should().NotBeNull();
        data!["targetStatus"].Should().Be("Rented");
        result.Message.Should().Contain("Rented");
    }

    [Fact]
    public async Task Handle_WhenUsingReference_ShouldReturnSuccess()
    {
        // Arrange
        var parameters = new CloseOperationParameters(
            PropertyId: null,
            Reference: "AP-2024-001",
            OperationType: "sale");
        var command = new CloseOperationActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.ActionType.Should().Be(ActionType.CloseOperation);
        result.Message.Should().Contain("AP-2024-001");
    }

    [Fact]
    public async Task Handle_WhenNoOperationType_ShouldDefaultToSold()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var parameters = new CloseOperationParameters(
            PropertyId: propertyId,
            Reference: null,
            OperationType: null);
        var command = new CloseOperationActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.ActionType.Should().Be(ActionType.CloseOperation);
        var data = result.Data as Dictionary<string, object?>;
        data.Should().NotBeNull();
        data!["targetStatus"].Should().Be("Sold");
    }

    [Fact]
    public async Task Handle_WhenMissingPropertyIdentifier_ShouldReturnFailure()
    {
        // Arrange
        var parameters = new CloseOperationParameters(
            PropertyId: null,
            Reference: null,
            OperationType: "sale");
        var command = new CloseOperationActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.CloseOperation);
        result.Errors.Should().NotBeEmpty();
        result.Message.Should().Contain("property");
    }

    [Fact]
    public async Task Handle_WhenInvalidOperationType_ShouldReturnFailure()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var parameters = new CloseOperationParameters(
            PropertyId: propertyId,
            Reference: null,
            OperationType: "transfer");
        var command = new CloseOperationActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.CloseOperation);
        result.Errors.Should().NotBeEmpty();
        (result.Message.Contains("sale", StringComparison.OrdinalIgnoreCase)
            || result.Message.Contains("rent", StringComparison.OrdinalIgnoreCase))
            .Should().BeTrue("the error message should mention valid operation types");
    }

    [Fact]
    public async Task Handle_WhenValidParameters_ShouldIncludeStructuredData()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var parameters = new CloseOperationParameters(
            PropertyId: propertyId,
            Reference: null,
            OperationType: "rent");
        var command = new CloseOperationActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        var data = result.Data as Dictionary<string, object?>;
        data.Should().NotBeNull();
        data!["propertyId"].Should().Be(propertyId);
        data["operationType"].Should().Be("rent");
        data["targetStatus"].Should().Be("Rented");
        data["agentId"].Should().Be(AgentId);
        data["tenantId"].Should().Be(TenantId);
    }

    [Theory]
    [InlineData("Sale", "Sold")]
    [InlineData("SALE", "Sold")]
    [InlineData("sale", "Sold")]
    [InlineData("Rent", "Rented")]
    [InlineData("RENT", "Rented")]
    [InlineData("rent", "Rented")]
    public async Task Handle_WhenOperationTypeCaseInsensitive_ShouldInferCorrectStatus(
        string operationType, string expectedStatus)
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var parameters = new CloseOperationParameters(
            PropertyId: propertyId,
            Reference: null,
            OperationType: operationType);
        var command = new CloseOperationActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        var data = result.Data as Dictionary<string, object?>;
        data!["targetStatus"].Should().Be(expectedStatus);
    }

    [Fact]
    public async Task Handle_WhenEmptyParameters_ShouldReturnFailure()
    {
        // Arrange
        var parameters = new CloseOperationParameters(
            PropertyId: null,
            Reference: null,
            OperationType: null);
        var command = new CloseOperationActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.CloseOperation);
    }
}
