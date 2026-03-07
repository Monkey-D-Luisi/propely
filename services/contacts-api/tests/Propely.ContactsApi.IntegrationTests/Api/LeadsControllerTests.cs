// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Propely.ContactsApi.Application.Common.Models;
using Propely.ContactsApi.Application.Leads.Dtos;
using Propely.ContactsApi.IntegrationTests.Fixtures;

namespace Propely.ContactsApi.IntegrationTests.Api;

/// <summary>
/// Integration tests for LeadsController CRUD endpoints.
/// </summary>
public sealed class LeadsControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public LeadsControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ListLeads_ReturnsOk_WithPagedResult()
    {
        // Act
        var response = await _client.GetAsync("/api/leads");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<LeadListItemDto>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().NotBeNull();
        result.TotalCount.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task CreateLead_ReturnsCreated_WithValidRequest()
    {
        // Arrange
        var uniqueSuffix = Guid.NewGuid().ToString("N")[..8];
        var request = new
        {
            Name = "Test Lead " + uniqueSuffix,
            Email = $"lead.{uniqueSuffix}@example.com",
            PropertyId = Guid.NewGuid(),
            Source = "Website"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/leads", request, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var lead = await response.Content.ReadFromJsonAsync<LeadDto>(JsonOptions);
        lead.Should().NotBeNull();
        lead!.Id.Should().NotBeEmpty();
        lead.Name.Should().Be(request.Name);
        lead.Email.Should().Be(request.Email);
        lead.PropertyId.Should().Be(request.PropertyId);
    }

    [Fact]
    public async Task GetLeadById_ReturnsNotFound_WhenLeadDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/leads/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetLeadById_ReturnsLead_AfterCreation()
    {
        // Arrange: Create a lead first
        var uniqueSuffix = Guid.NewGuid().ToString("N")[..8];
        var createRequest = new
        {
            Name = "Retrievable Lead " + uniqueSuffix,
            Email = $"retrievable.{uniqueSuffix}@example.com",
            PropertyId = Guid.NewGuid()
        };

        var createResponse = await _client.PostAsJsonAsync("/api/leads", createRequest, JsonOptions);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<LeadDto>(JsonOptions);
        created.Should().NotBeNull();

        // Act
        var response = await _client.GetAsync($"/api/leads/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var lead = await response.Content.ReadFromJsonAsync<LeadDto>(JsonOptions);
        lead.Should().NotBeNull();
        lead!.Id.Should().Be(created.Id);
        lead.Name.Should().Be(createRequest.Name);
        lead.Email.Should().Be(createRequest.Email);
    }

    [Fact]
    public async Task DeleteLead_ReturnsNoContent()
    {
        // Arrange: Create a lead first
        var uniqueSuffix = Guid.NewGuid().ToString("N")[..8];
        var createRequest = new
        {
            Name = "Delete Lead " + uniqueSuffix,
            Email = $"delete.{uniqueSuffix}@example.com",
            PropertyId = Guid.NewGuid()
        };

        var createResponse = await _client.PostAsJsonAsync("/api/leads", createRequest, JsonOptions);
        var created = await createResponse.Content.ReadFromJsonAsync<LeadDto>(JsonOptions);
        created.Should().NotBeNull();

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/leads/{created!.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
