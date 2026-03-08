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

public sealed class CreatePropertyActionHandlerTests
{
    private readonly CreatePropertyActionHandler _handler;
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AgentId = Guid.NewGuid();

    public CreatePropertyActionHandlerTests()
    {
        var logger = Substitute.For<ILogger<CreatePropertyActionHandler>>();
        _handler = new CreatePropertyActionHandler(logger);
    }

    [Fact]
    public async Task Handle_WhenValidParameters_ShouldReturnSuccess()
    {
        // Arrange
        var parameters = new CreatePropertyParameters(
            Title: null,
            PropertyType: "apartment",
            OperationType: "Sale",
            Bedrooms: 3,
            Bathrooms: 2,
            Price: 250000m,
            City: "Malaga",
            Description: "Beautiful apartment in the center");
        var command = new CreatePropertyActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.ActionType.Should().Be(ActionType.CreateProperty);
        result.Data.Should().NotBeNull();
        result.Message.Should().Contain("apartment");
        result.Message.Should().Contain("Malaga");
    }

    [Fact]
    public async Task Handle_WhenMissingPropertyType_ShouldReturnFailure()
    {
        // Arrange
        var parameters = new CreatePropertyParameters(
            Title: null,
            PropertyType: null,
            OperationType: null,
            Bedrooms: 3,
            Bathrooms: null,
            Price: null,
            City: "Madrid",
            Description: null);
        var command = new CreatePropertyActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.CreateProperty);
        result.Errors.Should().NotBeEmpty();
        result.Message.Should().Contain("property type");
    }

    [Fact]
    public async Task Handle_WhenMinimalParameters_ShouldReturnSuccessWithDefaults()
    {
        // Arrange
        var parameters = new CreatePropertyParameters(
            Title: null,
            PropertyType: "villa",
            OperationType: null,
            Bedrooms: null,
            Bathrooms: null,
            Price: null,
            City: null,
            Description: null);
        var command = new CreatePropertyActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.ActionType.Should().Be(ActionType.CreateProperty);
        result.Message.Should().Contain("villa");
    }

    [Fact]
    public async Task Handle_WhenValidParameters_ShouldBuildCorrectMessage()
    {
        // Arrange
        var parameters = new CreatePropertyParameters(
            Title: null,
            PropertyType: "house",
            OperationType: "Rent",
            Bedrooms: 4,
            Bathrooms: 3,
            Price: 1500m,
            City: "Barcelona",
            Description: null);
        var command = new CreatePropertyActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("house");
        result.Message.Should().Contain("rent");
        result.Message.Should().Contain("Barcelona");
        result.Message.Should().Contain("4 bedroom");
        result.Message.Should().Contain("3 bathroom");
        result.Message.Should().Contain("1,500 EUR");
    }

    [Fact]
    public async Task Handle_WhenValidParameters_ShouldIncludeStructuredData()
    {
        // Arrange
        var parameters = new CreatePropertyParameters(
            Title: null,
            PropertyType: "apartment",
            OperationType: null,
            Bedrooms: null,
            Bathrooms: null,
            Price: 200000m,
            City: "Valencia",
            Description: null);
        var command = new CreatePropertyActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        var data = result.Data as Dictionary<string, object?>;
        data.Should().NotBeNull();
        data!["propertyType"].Should().Be("apartment");
        data["city"].Should().Be("Valencia");
        data["price"].Should().Be(200000m);
        data["agentId"].Should().Be(AgentId);
        data["tenantId"].Should().Be(TenantId);
    }
}
