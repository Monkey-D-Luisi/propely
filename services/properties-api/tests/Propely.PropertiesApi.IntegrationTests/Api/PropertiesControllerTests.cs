// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Propely.PropertiesApi.Application.Common.Models;
using Propely.PropertiesApi.Application.Properties.Dtos;
using Propely.PropertiesApi.Domain.Properties;
using Propely.PropertiesApi.IntegrationTests.Fixtures;

namespace Propely.PropertiesApi.IntegrationTests.Api;

/// <summary>
/// Integration tests for PropertiesController CRUD endpoints.
/// Uses DevScheme authentication which auto-authenticates with a default user/org.
/// </summary>
public sealed class PropertiesControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public PropertiesControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ListProperties_ReturnsOk_WithPagedResult()
    {
        // Act
        var response = await _client.GetAsync("/api/properties");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<PropertyListItemDto>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().NotBeNull();
        result.TotalCount.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task CreateProperty_ReturnsCreated_WithValidRequest()
    {
        // Arrange
        var request = new
        {
            Title = "Test Property " + Guid.NewGuid().ToString("N")[..8],
            PropertyType = "Apartment",
            OperationType = "Sale"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/properties", request, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var property = await response.Content.ReadFromJsonAsync<PropertyDto>(JsonOptions);
        property.Should().NotBeNull();
        property!.Id.Should().NotBeEmpty();
        property.Title.Should().Be(request.Title);
        property.PropertyType.Should().Be(PropertyType.Apartment);
        property.OperationType.Should().Be(OperationType.Sale);
    }

    [Fact]
    public async Task GetPropertyById_ReturnsNotFound_WhenPropertyDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/properties/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPropertyById_ReturnsProperty_AfterCreation()
    {
        // Arrange: Create a property first
        var title = "Retrievable Property " + Guid.NewGuid().ToString("N")[..8];
        var createRequest = new
        {
            Title = title,
            PropertyType = "House",
            OperationType = "Rent"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/properties", createRequest, JsonOptions);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<PropertyDto>(JsonOptions);
        created.Should().NotBeNull();

        // Act
        var response = await _client.GetAsync($"/api/properties/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var property = await response.Content.ReadFromJsonAsync<PropertyDto>(JsonOptions);
        property.Should().NotBeNull();
        property!.Id.Should().Be(created.Id);
        property.Title.Should().Be(title);
        property.PropertyType.Should().Be(PropertyType.House);
        property.OperationType.Should().Be(OperationType.Rent);
    }

    [Fact]
    public async Task DeleteProperty_ReturnsNoContent_AndPropertyIsGone()
    {
        // Arrange: Create a property first
        var createRequest = new
        {
            Title = "Property To Delete " + Guid.NewGuid().ToString("N")[..8],
            PropertyType = "Apartment",
            OperationType = "Sale"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/properties", createRequest, JsonOptions);
        var created = await createResponse.Content.ReadFromJsonAsync<PropertyDto>(JsonOptions);
        created.Should().NotBeNull();

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/properties/{created!.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify property is no longer retrievable (soft-deleted)
        var getResponse = await _client.GetAsync($"/api/properties/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ListProperties_ReturnsCreatedProperties()
    {
        // Arrange: Create a property with a unique title
        var uniqueTag = Guid.NewGuid().ToString("N")[..8];
        var createRequest = new
        {
            Title = "Listable Property " + uniqueTag,
            PropertyType = "Apartment",
            OperationType = "Sale"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/properties", createRequest, JsonOptions);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act
        var response = await _client.GetAsync($"/api/properties?search={uniqueTag}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<PropertyListItemDto>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().ContainSingle(p => p.Title.Contains(uniqueTag));
    }

    [Fact]
    public async Task GetPropertyById_ReturnsUnauthorized_WhenNoOrgClaim()
    {
        // Arrange: Send request with X-Test-Org-Id set to "none" to simulate missing org claim
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"/api/properties/{Guid.NewGuid()}");
        requestMessage.Headers.Add("X-Test-Org-Id", "none");

        // Act
        var response = await _client.SendAsync(requestMessage);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
