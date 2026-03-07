// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Propely.ContactsApi.Application.Common.Models;
using Propely.ContactsApi.Application.Contacts.Dtos;
using Propely.ContactsApi.IntegrationTests.Fixtures;

namespace Propely.ContactsApi.IntegrationTests.Api;

/// <summary>
/// Integration tests for ContactsController CRUD endpoints.
/// </summary>
public sealed class ContactsControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public ContactsControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ListContacts_ReturnsOk_WithPagedResult()
    {
        // Act
        var response = await _client.GetAsync("/api/contacts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<ContactListItemDto>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().NotBeNull();
        result.TotalCount.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task CreateContact_ReturnsCreated_WithValidRequest()
    {
        // Arrange
        var uniqueSuffix = Guid.NewGuid().ToString("N")[..8];
        var request = new
        {
            FirstName = "John",
            LastName = "Doe",
            Email = $"john.doe.{uniqueSuffix}@example.com",
            Roles = new[] { "Buyer" }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/contacts", request, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var contact = await response.Content.ReadFromJsonAsync<ContactDto>(JsonOptions);
        contact.Should().NotBeNull();
        contact!.Id.Should().NotBeEmpty();
        contact.FirstName.Should().Be("John");
        contact.LastName.Should().Be("Doe");
        contact.Email.Should().Be(request.Email);
    }

    [Fact]
    public async Task GetContactById_ReturnsNotFound_WhenContactDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/contacts/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetContactById_ReturnsContact_AfterCreation()
    {
        // Arrange: Create a contact first
        var uniqueSuffix = Guid.NewGuid().ToString("N")[..8];
        var createRequest = new
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = $"jane.smith.{uniqueSuffix}@example.com",
            Roles = new[] { "Seller" }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/contacts", createRequest, JsonOptions);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<ContactDto>(JsonOptions);
        created.Should().NotBeNull();

        // Act
        var response = await _client.GetAsync($"/api/contacts/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var contact = await response.Content.ReadFromJsonAsync<ContactDto>(JsonOptions);
        contact.Should().NotBeNull();
        contact!.Id.Should().Be(created.Id);
        contact.FirstName.Should().Be("Jane");
        contact.LastName.Should().Be("Smith");
    }

    [Fact]
    public async Task DeleteContact_ReturnsNoContent()
    {
        // Arrange: Create a contact first
        var uniqueSuffix = Guid.NewGuid().ToString("N")[..8];
        var createRequest = new
        {
            FirstName = "Delete",
            LastName = "Me",
            Email = $"delete.me.{uniqueSuffix}@example.com",
            Roles = new[] { "Buyer" }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/contacts", createRequest, JsonOptions);
        var created = await createResponse.Content.ReadFromJsonAsync<ContactDto>(JsonOptions);
        created.Should().NotBeNull();

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/contacts/{created!.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
