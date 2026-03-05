// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Propely.AiApi.Application.Actions.Commands.PropertyActions;
using Propely.AiApi.Application.Actions.Handlers;
using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.UnitTests.Application.Actions.Handlers;

public sealed class UpdatePropertyActionHandlerTests
{
    private readonly UpdatePropertyActionHandler _handler;
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AgentId = Guid.NewGuid();

    public UpdatePropertyActionHandlerTests()
    {
        var logger = Substitute.For<ILogger<UpdatePropertyActionHandler>>();
        _handler = new UpdatePropertyActionHandler(logger);
    }

    [Fact]
    public async Task Handle_WhenSingleFieldUpdate_ShouldReturnSuccess()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var parameters = new Dictionary<string, object?>
        {
            ["property_id"] = propertyId.ToString(),
            ["field"] = "price",
            ["value"] = "300000"
        };
        var command = new UpdatePropertyActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.ActionType.Should().Be(ActionType.UpdateProperty);
        result.Message.Should().Contain("price");
        result.Message.Should().Contain("300000");
    }

    [Fact]
    public async Task Handle_WhenMissingPropertyIdentifier_ShouldReturnFailure()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            ["field"] = "price",
            ["value"] = "300000"
        };
        var command = new UpdatePropertyActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.UpdateProperty);
        result.Errors.Should().NotBeEmpty();
        result.Message.Should().Contain("property ID");
    }

    [Fact]
    public async Task Handle_WhenUsingReference_ShouldReturnSuccess()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            ["reference"] = "PROP-001",
            ["field"] = "city",
            ["value"] = "Barcelona"
        };
        var command = new UpdatePropertyActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.ActionType.Should().Be(ActionType.UpdateProperty);
        result.Message.Should().Contain("city");
        result.Message.Should().Contain("PROP-001");
    }

    [Fact]
    public async Task Handle_WhenMultipleFieldUpdates_ShouldReturnSuccess()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var parameters = new Dictionary<string, object?>
        {
            ["property_id"] = propertyId.ToString(),
            ["price"] = 350000m,
            ["bedrooms"] = 4
        };
        var command = new UpdatePropertyActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.ActionType.Should().Be(ActionType.UpdateProperty);
        result.Message.Should().Contain("price");
        result.Message.Should().Contain("bedrooms");
    }

    [Fact]
    public async Task Handle_WhenNoUpdateFieldsSpecified_ShouldReturnFailure()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var parameters = new Dictionary<string, object?>
        {
            ["property_id"] = propertyId.ToString()
        };
        var command = new UpdatePropertyActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.UpdateProperty);
        result.Errors.Should().NotBeEmpty();
        result.Message.Should().Contain("what to update");
    }
}
