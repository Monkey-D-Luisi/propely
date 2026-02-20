// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Propely.AiApi.Api.Dtos;
using Propely.AiApi.IntegrationTests.Fixtures;
using FluentAssertions;
using Propely.AiApi.Domain.WorkItems;
using Propely.AiApi.Application.Common.Models;
using Propely.AiApi.Application.WorkItems.Dtos;
using System.Linq;

namespace Propely.AiApi.IntegrationTests.Api;

/// <summary>
/// Integration tests for WorkItemsController.
/// </summary>
public sealed class WorkItemsControllerTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public WorkItemsControllerTests(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task POST_ReturnsCreated_WithValidRequest()
    {
        // Arrange
        var request = new CreateWorkItemRequest("Test Work Item", "A description");

        // Act
        var response = await _client.PostAsJsonAsync("/v1/work-items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var content = await response.Content.ReadFromJsonAsync<WorkItemResponse>();
        content.Should().NotBeNull();
        content!.Id.Should().NotBeEmpty();
        content.Title.Should().Be("Test Work Item");
        content.Description.Should().Be("A description");
        content.Status.Should().Be("Pending");
        content.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        content.UpdatedAtUtc.Should().Be(content.CreatedAtUtc);
    }

    [Fact]
    public async Task POST_ReturnsBadRequest_WhenTitleEmpty()
    {
        // Arrange
        var request = new CreateWorkItemRequest("", null);

        // Act
        var response = await _client.PostAsJsonAsync("/v1/work-items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_ReturnsBadRequest_WhenTitleTooLong()
    {
        // Arrange
        var longTitle = new string('A', 201);
        var request = new CreateWorkItemRequest(longTitle, null);

        // Act
        var response = await _client.PostAsJsonAsync("/v1/work-items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_ReturnsBadRequest_WhenDescriptionTooLong()
    {
        // Arrange
        var longDescription = new string('A', 2001);
        var request = new CreateWorkItemRequest("Valid Title", longDescription);

        // Act
        var response = await _client.PostAsJsonAsync("/v1/work-items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GET_ReturnsOk_WhenWorkItemExists()
    {
        // Arrange: Create a work item first
        var createRequest = new CreateWorkItemRequest("Retrievable Item", "Description");
        var createResponse = await _client.PostAsJsonAsync("/v1/work-items", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<WorkItemResponse>();

        // Act
        var response = await _client.GetAsync($"/v1/work-items/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<WorkItemResponse>();
        content.Should().NotBeNull();
        content!.Id.Should().Be(created.Id);
        content.Title.Should().Be("Retrievable Item");
        content.Description.Should().Be("Description");
    }

    [Fact]
    public async Task GET_ReturnsNotFound_WhenWorkItemDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/v1/work-items/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Response_ContainsSecurityHeaders()
    {
        // Arrange
        var request = new CreateWorkItemRequest("Security Test", null);

        // Act
        var response = await _client.PostAsJsonAsync("/v1/work-items", request);

        // Assert
        response.Headers.Should().ContainKey("X-Content-Type-Options");
        response.Headers.GetValues("X-Content-Type-Options").Should().Contain("nosniff");

        response.Headers.Should().ContainKey("X-Frame-Options");
        response.Headers.GetValues("X-Frame-Options").Should().Contain("DENY");

        // Note: X-XSS-Protection removed (deprecated, CSP provides protection)

        response.Headers.Should().ContainKey("Referrer-Policy");
        response.Headers.GetValues("Referrer-Policy").Should().Contain("strict-origin-when-cross-origin");

        response.Headers.Should().ContainKey("Content-Security-Policy");
        response.Headers.GetValues("Content-Security-Policy").Should().Contain("default-src 'self'");
    }

    [Fact]
    public async Task Response_ContainsCorrelationIdHeader()
    {
        // Arrange
        var request = new CreateWorkItemRequest("Correlation Test", null);

        // Act
        var response = await _client.PostAsJsonAsync("/v1/work-items", request);

        // Assert - response should contain X-Correlation-ID header
        response.Headers.Should().ContainKey("X-Correlation-ID");
        var correlationId = response.Headers.GetValues("X-Correlation-ID").First();
        correlationId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Response_PreservesProvidedCorrelationId()
    {
        // Arrange
        var providedCorrelationId = "my-test-correlation-123";
        var request = new CreateWorkItemRequest("Correlation Preserve Test", null);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/v1/work-items");
        httpRequest.Content = JsonContent.Create(request);
        httpRequest.Headers.Add("X-Correlation-ID", providedCorrelationId);

        // Act
        var response = await _client.SendAsync(httpRequest);

        // Assert - response should echo back the provided correlation ID
        response.Headers.Should().ContainKey("X-Correlation-ID");
        var returnedCorrelationId = response.Headers.GetValues("X-Correlation-ID").First();
        returnedCorrelationId.Should().Be(providedCorrelationId);
    }

    [Fact]
    public async Task POST_ReturnsCreated_WithNullDescription()
    {
        // Arrange
        var request = new CreateWorkItemRequest("Work Item Without Description", null);

        // Act
        var response = await _client.PostAsJsonAsync("/v1/work-items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content.ReadFromJsonAsync<WorkItemResponse>();
        content.Should().NotBeNull();
        content!.Title.Should().Be("Work Item Without Description");
        content.Description.Should().BeNull();
    }
    [Fact]
    public async Task PUT_ReturnsNoContent_WhenUpdateIsSuccessful()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/v1/work-items", new CreateWorkItemRequest("Original", "Desc"));
        var created = await createResponse.Content.ReadFromJsonAsync<WorkItemResponse>();
        var updateRequest = new UpdateWorkItemRequest("Updated", "New Desc", WorkItemStatus.Active);

        // Act
        var response = await _client.PutAsJsonAsync($"/v1/work-items/{created!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify update
        var getResponse = await _client.GetAsync($"/v1/work-items/{created.Id}");
        var content = await getResponse.Content.ReadFromJsonAsync<WorkItemResponse>();
        content!.Title.Should().Be("Updated");
        content.Description.Should().Be("New Desc");
        content.Status.Should().Be("Active");
    }

    [Fact]
    public async Task PUT_ReturnsNotFound_WhenWorkItemDoesNotExist()
    {
        // Arrange
        var updateRequest = new UpdateWorkItemRequest("Updated", "New Desc", WorkItemStatus.Active);

        // Act
        var response = await _client.PutAsJsonAsync($"/v1/work-items/{Guid.NewGuid()}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DELETE_ReturnsNoContent_WhenDeleteIsSuccessful()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/v1/work-items", new CreateWorkItemRequest("To Delete", "Desc"));
        var created = await createResponse.Content.ReadFromJsonAsync<WorkItemResponse>();

        // Act
        var response = await _client.DeleteAsync($"/v1/work-items/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion (Soft Delete -> Status should be Deleted, or 404 if API hides deleted items?)
        // The Requirements said "Verify deletion: GET ... Expect 404".
        // Let's verify that expectation.
        var getResponse = await _client.GetAsync($"/v1/work-items/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DELETE_ReturnsNotFound_WhenWorkItemDoesNotExist()
    {
        // Act
        var response = await _client.DeleteAsync($"/v1/work-items/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    [Fact]
    public async Task GET_ReturnsPagedList_WhenItemsExist()
    {
        // Arrange
        var titlePrefix = Guid.NewGuid().ToString();
        for (int i = 0; i < 5; i++)
        {
            await _client.PostAsJsonAsync("/v1/work-items", new CreateWorkItemRequest($"{titlePrefix}_{i}", "Desc"));
        }

        // Poll until projection completes (outbox dispatcher polls every 1s)
        PagedResult<WorkItemDto>? result = null;
        for (int attempt = 0; attempt < 10; attempt++)
        {
            await Task.Delay(500);
            var poll = await _client.GetAsync($"/v1/work-items?pageSize=50&search={titlePrefix}");
            poll.StatusCode.Should().Be(HttpStatusCode.OK);
            result = await poll.Content.ReadFromJsonAsync<PagedResult<WorkItemDto>>(JsonOptions);
            if (result?.Items?.Count() >= 5) break;
        }

        // Assert
        result.Should().NotBeNull();
        result!.Items.Should().HaveCountGreaterThanOrEqualTo(5);
        result.Items.Should().OnlyContain(x => x.Title.StartsWith(titlePrefix));
        result.TotalCount.Should().BeGreaterThanOrEqualTo(5);
    }

    [Fact]
    public async Task GET_ReturnsFilteredList_WhenStatusFilterIsProvided()
    {
        // Arrange
        var prefix = Guid.NewGuid().ToString();
        // Create 1 Active
        var r1 = await _client.PostAsJsonAsync("/v1/work-items", new CreateWorkItemRequest($"{prefix}_Active", "Desc"));
        var item1 = await r1.Content.ReadFromJsonAsync<WorkItemResponse>();
        var putResponse = await _client.PutAsJsonAsync($"/v1/work-items/{item1!.Id}", new UpdateWorkItemRequest("Upd", "Desc", WorkItemStatus.Active));
        putResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Create 1 Pending
        await _client.PostAsJsonAsync("/v1/work-items", new CreateWorkItemRequest($"{prefix}_Pending", "Desc"));

        // Wait for projection
        await Task.Delay(1500);

        // Act
        var response = await _client.GetAsync("/v1/work-items?status=Active");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<WorkItemDto>>(JsonOptions);

        result.Should().NotBeNull();
        result!.Items.Should().Contain(x => x.Title == "Upd"); // The renamed active item
        result.Items.Any(x => x.Title.Contains($"{prefix}_Pending")).Should().BeFalse();
    }
}
