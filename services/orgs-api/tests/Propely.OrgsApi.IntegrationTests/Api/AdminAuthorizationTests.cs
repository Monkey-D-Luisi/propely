// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Net;
using System.Net.Http.Json;
using Propely.OrgsApi.IntegrationTests.Fixtures;
using FluentAssertions;

namespace Propely.OrgsApi.IntegrationTests.Api;

/// <summary>
/// Integration tests verifying authentication and authorization requirements for admin endpoints.
/// Admin endpoints require the sys_admin claim in the JWT token.
/// </summary>
public sealed class AdminAuthorizationTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public AdminAuthorizationTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient() => _factory.CreateClient();

    [Fact]
    public async Task GetAuditLogs_WhenUnauthenticated_ShouldReturn401()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var response = await client.GetAsync("/admin/audit-logs");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ToggleFeatureFlag_WhenUnauthenticated_ShouldReturn401()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var response = await client.PutAsJsonAsync("/feature-flags/Notifications", new { isEnabled = false });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetFeatureFlags_WhenAuthenticated_ShouldReturn200()
    {
        // Arrange — any authenticated user can read feature flags
        var client = CreateClient();
        await RegisterUserAsync(client);

        // Act
        var response = await client.GetAsync("/feature-flags");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static int _ipSuffixCounter;
    private static string UniqueIpAddress()
    {
        var suffix = System.Threading.Interlocked.Increment(ref _ipSuffixCounter);
        suffix = ((suffix - 1) % 253) + 1;
        return $"192.0.2.{suffix}";
    }

    private static async Task RegisterUserAsync(HttpClient client)
    {
        var csrfResponse = await client.GetAsync("/auth/csrf");
        csrfResponse.EnsureSuccessStatusCode();
        var body = await csrfResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var csrf = body.GetProperty("csrfToken").GetString()!;

        var email = $"test-{Guid.NewGuid():N}@example.com";
        using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/register")
        {
            Content = JsonContent.Create(new { email, password = "TestPassword123!", name = "Test User" })
        };
        request.Headers.Add("x-csrf-token", csrf);
        request.Headers.Add("X-Real-IP", UniqueIpAddress());

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
}
