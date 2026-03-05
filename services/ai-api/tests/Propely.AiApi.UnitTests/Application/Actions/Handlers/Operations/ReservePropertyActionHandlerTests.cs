// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Propely.AiApi.Application.Actions.Commands.Operations;
using Propely.AiApi.Application.Actions.Handlers.Operations;
using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.UnitTests.Application.Actions.Handlers.Operations;

public sealed class ReservePropertyActionHandlerTests
{
    private readonly ReservePropertyActionHandler _handler;
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AgentId = Guid.NewGuid();

    public ReservePropertyActionHandlerTests()
    {
        var logger = Substitute.For<ILogger<ReservePropertyActionHandler>>();
        _handler = new ReservePropertyActionHandler(logger);
    }

    [Fact]
    public async Task Handle_WhenPropertyIdAndContactName_ShouldReturnSuccess()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var parameters = new Dictionary<string, object?>
        {
            ["property_id"] = propertyId.ToString(),
            ["contact_name"] = "Maria Garcia"
        };
        var command = new ReservePropertyActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.ActionType.Should().Be(ActionType.ReserveProperty);
        result.Message.Should().Contain(propertyId.ToString());
        result.Message.Should().Contain("Maria Garcia");
    }

    [Fact]
    public async Task Handle_WhenReferenceAndContactName_ShouldReturnSuccess()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            ["reference"] = "AP-2024-001",
            ["contact_name"] = "Carlos Lopez"
        };
        var command = new ReservePropertyActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.ActionType.Should().Be(ActionType.ReserveProperty);
        result.Message.Should().Contain("AP-2024-001");
        result.Message.Should().Contain("Carlos Lopez");
    }

    [Fact]
    public async Task Handle_WhenPropertyIdWithoutContactName_ShouldReturnSuccess()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var parameters = new Dictionary<string, object?>
        {
            ["property_id"] = propertyId.ToString()
        };
        var command = new ReservePropertyActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.ActionType.Should().Be(ActionType.ReserveProperty);
        result.Message.Should().Contain(propertyId.ToString());
        result.Message.Should().Contain("Reserved");
    }

    [Fact]
    public async Task Handle_WhenMissingPropertyIdentifier_ShouldReturnFailure()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            ["contact_name"] = "Maria Garcia"
        };
        var command = new ReservePropertyActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.ReserveProperty);
        result.Errors.Should().NotBeEmpty();
        result.Message.Should().Contain("property");
    }

    [Fact]
    public async Task Handle_WhenValidParameters_ShouldIncludeStructuredData()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var parameters = new Dictionary<string, object?>
        {
            ["property_id"] = propertyId.ToString(),
            ["contact_name"] = "Maria Garcia"
        };
        var command = new ReservePropertyActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        var data = result.Data as Dictionary<string, object?>;
        data.Should().NotBeNull();
        data!["propertyId"].Should().Be(propertyId);
        data["contactName"].Should().Be("Maria Garcia");
        data["targetStatus"].Should().Be("Reserved");
        data["agentId"].Should().Be(AgentId);
        data["tenantId"].Should().Be(TenantId);
    }

    [Fact]
    public async Task Handle_WhenEmptyParameters_ShouldReturnFailure()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>();
        var command = new ReservePropertyActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.ReserveProperty);
    }
}
