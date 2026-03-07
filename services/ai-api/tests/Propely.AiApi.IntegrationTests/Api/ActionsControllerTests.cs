// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Propely.AiApi.IntegrationTests.Fixtures;

namespace Propely.AiApi.IntegrationTests.Api;

/// <summary>
/// Integration tests for ActionsController endpoints.
/// Tests auth enforcement and request validation (not actual AI execution, which requires OpenAI keys).
/// </summary>
public sealed class ActionsControllerTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ActionsControllerTests(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Execute_ReturnsForbid_WhenNoOrgClaim()
    {
        // Arrange: Send request with X-Test-Org-Id set to "none" to simulate missing org claim.
        // DevScheme still authenticates (user is present), but org_id is missing,
        // so the controller returns 403 Forbidden.
        var request = new HttpRequestMessage(HttpMethod.Post, "/v1/actions/execute")
        {
            Content = JsonContent.Create(new { Text = "Create a property" })
        };
        request.Headers.Add("X-Test-Org-Id", "none");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Execute_ReturnsBadRequest_WhenTextIsEmpty()
    {
        // Arrange
        var request = new { Text = "" };

        // Act
        var response = await _client.PostAsJsonAsync("/v1/actions/execute", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Execute_ReturnsBadRequest_WhenTextIsNull()
    {
        // Arrange: Send request with null text (JSON body with no text field)
        var request = new { Text = (string?)null };

        // Act
        var response = await _client.PostAsJsonAsync("/v1/actions/execute", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task HealthLive_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/health/live");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
