// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Propely.AiApi.Application.Actions.Commands.PropertyActions;
using Propely.AiApi.Application.Actions.Handlers;
using Propely.AiApi.Application.Actions.Parameters;
using Propely.AiApi.Domain.Actions;
using Propely.PropertiesApi.Client;
using Propely.PropertiesApi.Client.Dtos;

namespace Propely.AiApi.UnitTests.Application.Actions.Handlers;

public sealed class CreatePropertyActionHandlerTests
{
    private readonly IPropertiesApiClient _propertiesClient;
    private readonly CreatePropertyActionHandler _handler;
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AgentId = Guid.NewGuid();

    public CreatePropertyActionHandlerTests()
    {
        _propertiesClient = Substitute.For<IPropertiesApiClient>();
        var logger = Substitute.For<ILogger<CreatePropertyActionHandler>>();
        _handler = new CreatePropertyActionHandler(_propertiesClient, logger);
    }

    private void SetupCreateReturns(Action<CreatePropertyRequest>? verify = null)
    {
        _propertiesClient.CreateAsync(Arg.Any<CreatePropertyRequest>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var req = callInfo.Arg<CreatePropertyRequest>();
                verify?.Invoke(req);
                return new PropertyResponse
                {
                    Id = Guid.NewGuid(),
                    Title = req.Title,
                    PropertyType = req.PropertyType,
                    OperationType = req.OperationType,
                    Status = "Draft",
                    TenantId = TenantId,
                    AgentId = AgentId,
                    CreatedAtUtc = DateTime.UtcNow
                };
            });
    }

    [Fact]
    public async Task Handle_WhenValidParameters_ShouldCallPropertiesApiAndReturnSuccess()
    {
        // Arrange
        SetupCreateReturns();
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
        await _propertiesClient.Received(1).CreateAsync(Arg.Any<CreatePropertyRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenMissingPropertyType_ShouldReturnFailureWithoutCallingApi()
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
        await _propertiesClient.DidNotReceive().CreateAsync(Arg.Any<CreatePropertyRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenMinimalParameters_ShouldReturnSuccessWithDefaults()
    {
        // Arrange
        SetupCreateReturns();
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
        SetupCreateReturns();
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
    public async Task Handle_WhenValidParameters_ShouldIncludePropertyIdInResponse()
    {
        // Arrange
        SetupCreateReturns();
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
        data!["id"].Should().NotBeNull();
        data["propertyType"].Should().Be("Apartment");
        data["tenantId"].Should().Be(TenantId);
    }

    [Fact]
    public async Task Handle_WhenPropertiesApiThrows_ShouldReturnFailure()
    {
        // Arrange
        _propertiesClient.CreateAsync(Arg.Any<CreatePropertyRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        var parameters = new CreatePropertyParameters(
            Title: null,
            PropertyType: "apartment",
            OperationType: "Sale",
            Bedrooms: 2,
            Bathrooms: 1,
            Price: 131000m,
            City: "Sabadell",
            Description: null);
        var command = new CreatePropertyActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.CreateProperty);
        result.Message.Should().Contain("couldn't save");
    }

    [Fact]
    public async Task Handle_ShouldNormalizeLowercaseEnumsToPascalCase()
    {
        // Arrange
        CreatePropertyRequest? capturedRequest = null;
        _propertiesClient.CreateAsync(Arg.Any<CreatePropertyRequest>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                capturedRequest = callInfo.Arg<CreatePropertyRequest>();
                return new PropertyResponse
                {
                    Id = Guid.NewGuid(),
                    Title = capturedRequest.Title,
                    PropertyType = capturedRequest.PropertyType,
                    OperationType = capturedRequest.OperationType,
                    Status = "Draft",
                    TenantId = TenantId,
                    AgentId = AgentId,
                    CreatedAtUtc = DateTime.UtcNow
                };
            });

        var parameters = new CreatePropertyParameters(
            Title: null,
            PropertyType: "apartment",
            OperationType: "sale",
            Bedrooms: 2,
            Bathrooms: 1,
            Price: 131000m,
            City: "Sabadell Centro",
            Description: null);
        var command = new CreatePropertyActionCommand(parameters, TenantId, AgentId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.PropertyType.Should().Be("Apartment");
        capturedRequest.OperationType.Should().Be("Sale");
        capturedRequest.Address!.City.Should().Be("Sabadell Centro");
        capturedRequest.Features!.Bedrooms.Should().Be(2);
        capturedRequest.Features.Bathrooms.Should().Be(1);
        capturedRequest.Financials!.Price.Should().Be(131000m);
    }
}
