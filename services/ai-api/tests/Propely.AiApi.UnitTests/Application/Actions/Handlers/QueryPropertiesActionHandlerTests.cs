// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Propely.AiApi.Application.Actions.Commands.PropertyActions;
using Propely.AiApi.Application.Actions.Handlers;
using Propely.AiApi.Domain.Actions;
using Propely.PropertiesApi.Client;
using Propely.PropertiesApi.Client.Dtos;

namespace Propely.AiApi.UnitTests.Application.Actions.Handlers;

public sealed class QueryPropertiesActionHandlerTests
{
    private readonly IPropertiesApiClient _propertiesClient;
    private readonly QueryPropertiesActionHandler _handler;
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AgentId = Guid.NewGuid();

    public QueryPropertiesActionHandlerTests()
    {
        _propertiesClient = Substitute.For<IPropertiesApiClient>();
        var logger = Substitute.For<ILogger<QueryPropertiesActionHandler>>();
        _handler = new QueryPropertiesActionHandler(_propertiesClient, logger);
    }

    [Fact]
    public async Task Handle_WhenPropertiesFound_ShouldReturnSuccessWithSummary()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            ["property_type"] = "apartment",
            ["city"] = "Malaga"
        };
        var command = new QueryPropertiesActionCommand(parameters, TenantId, AgentId);

        var properties = new List<PropertyListItemResponse>
        {
            new() { Id = Guid.NewGuid(), Title = "Luxury Apartment", PropertyType = "Apartment", OperationType = "Sale", Status = "Active", Price = 300000, City = "Malaga", Bedrooms = 3, Bathrooms = 2 },
            new() { Id = Guid.NewGuid(), Title = "Cozy Studio", PropertyType = "Apartment", OperationType = "Rent", Status = "Active", Price = 800, City = "Malaga", Bedrooms = 1, Bathrooms = 1 }
        };

        _propertiesClient.ListAsync(
            type: "apartment", operation: null, status: null,
            minPrice: null, maxPrice: null, city: "Malaga",
            agentId: null,
            search: Arg.Any<string?>(), minBedrooms: Arg.Any<int?>(),
            minBathrooms: Arg.Any<int?>(), minArea: Arg.Any<decimal?>(),
            maxArea: Arg.Any<decimal?>(), hasPool: Arg.Any<bool?>(),
            hasGarden: Arg.Any<bool?>(), hasGarage: Arg.Any<bool?>(),
            hasElevator: Arg.Any<bool?>(), hasTerrace: Arg.Any<bool?>(),
            sortBy: null, sortDesc: false,
            page: 1, pageSize: 20,
            ct: Arg.Any<CancellationToken>())
            .Returns(new PagedResult<PropertyListItemResponse>
            {
                Items = properties,
                PageNumber = 1,
                TotalPages = 1,
                TotalCount = 2,
                HasPreviousPage = false,
                HasNextPage = false
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.ActionType.Should().Be(ActionType.QueryProperties);
        result.Message.Should().Contain("2 properties");
        result.Message.Should().Contain("Malaga");
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenNoPropertiesFound_ShouldReturnSuccessWithEmptyMessage()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            ["city"] = "Timbuktu"
        };
        var command = new QueryPropertiesActionCommand(parameters, TenantId, AgentId);

        _propertiesClient.ListAsync(
            type: Arg.Any<string?>(), operation: Arg.Any<string?>(), status: Arg.Any<string?>(),
            minPrice: Arg.Any<decimal?>(), maxPrice: Arg.Any<decimal?>(), city: Arg.Any<string?>(),
            agentId: Arg.Any<Guid?>(),
            search: Arg.Any<string?>(), minBedrooms: Arg.Any<int?>(),
            minBathrooms: Arg.Any<int?>(), minArea: Arg.Any<decimal?>(),
            maxArea: Arg.Any<decimal?>(), hasPool: Arg.Any<bool?>(),
            hasGarden: Arg.Any<bool?>(), hasGarage: Arg.Any<bool?>(),
            hasElevator: Arg.Any<bool?>(), hasTerrace: Arg.Any<bool?>(),
            sortBy: Arg.Any<string?>(), sortDesc: Arg.Any<bool>(),
            page: Arg.Any<int>(), pageSize: Arg.Any<int>(),
            ct: Arg.Any<CancellationToken>())
            .Returns(new PagedResult<PropertyListItemResponse>
            {
                Items = [],
                PageNumber = 1,
                TotalPages = 0,
                TotalCount = 0,
                HasPreviousPage = false,
                HasNextPage = false
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.ActionType.Should().Be(ActionType.QueryProperties);
        result.Message.Should().Contain("No properties found");
        result.Message.Should().Contain("Timbuktu");
    }

    [Fact]
    public async Task Handle_WhenSdkCallsWithCorrectFilters_ShouldPassParametersToClient()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            ["property_type"] = "villa",
            ["operation_type"] = "Sale",
            ["status"] = "Active",
            ["city"] = "Marbella",
            ["min_price"] = 500000m,
            ["max_price"] = 1000000m
        };
        var command = new QueryPropertiesActionCommand(parameters, TenantId, AgentId);

        _propertiesClient.ListAsync(
            type: Arg.Any<string?>(), operation: Arg.Any<string?>(), status: Arg.Any<string?>(),
            minPrice: Arg.Any<decimal?>(), maxPrice: Arg.Any<decimal?>(), city: Arg.Any<string?>(),
            agentId: Arg.Any<Guid?>(),
            search: Arg.Any<string?>(), minBedrooms: Arg.Any<int?>(),
            minBathrooms: Arg.Any<int?>(), minArea: Arg.Any<decimal?>(),
            maxArea: Arg.Any<decimal?>(), hasPool: Arg.Any<bool?>(),
            hasGarden: Arg.Any<bool?>(), hasGarage: Arg.Any<bool?>(),
            hasElevator: Arg.Any<bool?>(), hasTerrace: Arg.Any<bool?>(),
            sortBy: Arg.Any<string?>(), sortDesc: Arg.Any<bool>(),
            page: Arg.Any<int>(), pageSize: Arg.Any<int>(),
            ct: Arg.Any<CancellationToken>())
            .Returns(new PagedResult<PropertyListItemResponse>
            {
                Items = [],
                TotalCount = 0,
                PageNumber = 1,
                TotalPages = 0
            });

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _propertiesClient.Received(1).ListAsync(
            type: "villa",
            operation: "Sale",
            status: "Active",
            minPrice: 500000m,
            maxPrice: 1000000m,
            city: "Marbella",
            agentId: Arg.Any<Guid?>(),
            search: Arg.Any<string?>(), minBedrooms: Arg.Any<int?>(),
            minBathrooms: Arg.Any<int?>(), minArea: Arg.Any<decimal?>(),
            maxArea: Arg.Any<decimal?>(), hasPool: Arg.Any<bool?>(),
            hasGarden: Arg.Any<bool?>(), hasGarage: Arg.Any<bool?>(),
            hasElevator: Arg.Any<bool?>(), hasTerrace: Arg.Any<bool?>(),
            sortBy: Arg.Any<string?>(),
            sortDesc: false,
            page: 1,
            pageSize: 20,
            ct: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSdkThrows_ShouldReturnFailureGracefully()
    {
        // Arrange
        var parameters = new Dictionary<string, object?> { ["city"] = "Madrid" };
        var command = new QueryPropertiesActionCommand(parameters, TenantId, AgentId);

        _propertiesClient.ListAsync(
            type: Arg.Any<string?>(), operation: Arg.Any<string?>(), status: Arg.Any<string?>(),
            minPrice: Arg.Any<decimal?>(), maxPrice: Arg.Any<decimal?>(), city: Arg.Any<string?>(),
            agentId: Arg.Any<Guid?>(),
            search: Arg.Any<string?>(), minBedrooms: Arg.Any<int?>(),
            minBathrooms: Arg.Any<int?>(), minArea: Arg.Any<decimal?>(),
            maxArea: Arg.Any<decimal?>(), hasPool: Arg.Any<bool?>(),
            hasGarden: Arg.Any<bool?>(), hasGarage: Arg.Any<bool?>(),
            hasElevator: Arg.Any<bool?>(), hasTerrace: Arg.Any<bool?>(),
            sortBy: Arg.Any<string?>(), sortDesc: Arg.Any<bool>(),
            page: Arg.Any<int>(), pageSize: Arg.Any<int>(),
            ct: Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.QueryProperties);
        result.Errors.Should().NotBeEmpty();
        result.Message.Should().Contain("error");
    }

    [Fact]
    public async Task Handle_WhenResultsContainSummary_ShouldBuildHumanReadableMessage()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            ["property_type"] = "house"
        };
        var command = new QueryPropertiesActionCommand(parameters, TenantId, AgentId);

        var properties = new List<PropertyListItemResponse>
        {
            new() { Id = Guid.NewGuid(), Title = "Villa Rosa", PropertyType = "House", OperationType = "Sale", Status = "Active", Price = 450000, City = "Malaga", Bedrooms = 4 },
            new() { Id = Guid.NewGuid(), Title = "Casa Blanca", PropertyType = "House", OperationType = "Sale", Status = "Active", Price = 350000, City = "Malaga", Bedrooms = 3 },
            new() { Id = Guid.NewGuid(), Title = "El Cortijo", PropertyType = "House", OperationType = "Sale", Status = "Active", Price = 550000, City = "Ronda", Bedrooms = 5 }
        };

        _propertiesClient.ListAsync(
            type: Arg.Any<string?>(), operation: Arg.Any<string?>(), status: Arg.Any<string?>(),
            minPrice: Arg.Any<decimal?>(), maxPrice: Arg.Any<decimal?>(), city: Arg.Any<string?>(),
            agentId: Arg.Any<Guid?>(),
            search: Arg.Any<string?>(), minBedrooms: Arg.Any<int?>(),
            minBathrooms: Arg.Any<int?>(), minArea: Arg.Any<decimal?>(),
            maxArea: Arg.Any<decimal?>(), hasPool: Arg.Any<bool?>(),
            hasGarden: Arg.Any<bool?>(), hasGarage: Arg.Any<bool?>(),
            hasElevator: Arg.Any<bool?>(), hasTerrace: Arg.Any<bool?>(),
            sortBy: Arg.Any<string?>(), sortDesc: Arg.Any<bool>(),
            page: Arg.Any<int>(), pageSize: Arg.Any<int>(),
            ct: Arg.Any<CancellationToken>())
            .Returns(new PagedResult<PropertyListItemResponse>
            {
                Items = properties,
                PageNumber = 1,
                TotalPages = 1,
                TotalCount = 3,
                HasPreviousPage = false,
                HasNextPage = false
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("3 properties");
        result.Message.Should().Contain("house");
        result.Message.Should().Contain("Top results");
        result.Message.Should().Contain("Villa Rosa");
    }
}
