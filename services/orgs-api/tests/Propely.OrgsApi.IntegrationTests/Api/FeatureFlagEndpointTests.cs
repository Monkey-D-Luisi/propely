// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Propely.OrgsApi.Infrastructure.Persistence;
using Propely.OrgsApi.IntegrationTests.Fixtures;
using FluentAssertions;

namespace Propely.OrgsApi.IntegrationTests.Api;

public sealed class FeatureFlagEndpointTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public FeatureFlagEndpointTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient() => _factory.CreateClient();

    private static string UniqueEmail() => $"test-{Guid.NewGuid():N}@example.com";
    private static int _ipSuffixCounter;
    private static string UniqueIpAddress()
    {
        var suffix = System.Threading.Interlocked.Increment(ref _ipSuffixCounter);
        suffix = ((suffix - 1) % 253) + 1;
        return $"198.51.100.{suffix}";
    }

    private static async Task<string> GetCsrfTokenAsync(HttpClient client)
    {
        var response = await client.GetAsync("/auth/csrf");
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("csrfToken").GetString()!;
    }

    private async Task<(string email, string password)> RegisterUserAsync(HttpClient client)
    {
        var email = UniqueEmail();
        var password = "TestPassword123!";
        var csrf = await GetCsrfTokenAsync(client);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/register")
        {
            Content = JsonContent.Create(new { email, password, name = "Test User" })
        };
        request.Headers.Add("x-csrf-token", csrf);
        request.Headers.Add("X-Real-IP", UniqueIpAddress());

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (email, password);
    }

    private async Task PromoteToAdminAndRelogin(HttpClient client, string email, string password)
    {
        // Promote user to system admin via direct DB update
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE users SET is_system_admin = true WHERE email = {email}");

        // Re-login to get a fresh JWT with the sys_admin claim
        var csrf = await GetCsrfTokenAsync(client);
        using var loginRequest = new HttpRequestMessage(HttpMethod.Post, "/auth/login")
        {
            Content = JsonContent.Create(new { email, password })
        };
        loginRequest.Headers.Add("x-csrf-token", csrf);
        loginRequest.Headers.Add("X-Real-IP", UniqueIpAddress());

        var loginResponse = await client.SendAsync(loginRequest);
        loginResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetAll_WhenAuthenticated_ShouldReturn200WithFlags()
    {
        // Arrange
        var client = CreateClient();
        await RegisterUserAsync(client);

        // Act
        var response = await client.GetAsync("/feature-flags");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var flags = body.GetProperty("flags");
        flags.GetArrayLength().Should().BeGreaterThanOrEqualTo(1);

        // Verify at least one known flag exists
        var flagNames = flags.EnumerateArray().Select(f => f.GetProperty("name").GetString()).ToList();
        flagNames.Should().Contain("Notifications");
    }

    [Fact]
    public async Task GetAll_WhenNotAuthenticated_ShouldReturn401()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var response = await client.GetAsync("/feature-flags");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Check_WhenAuthenticated_ShouldReturnFlagStatus()
    {
        // Arrange
        var client = CreateClient();
        await RegisterUserAsync(client);

        // Act
        var response = await client.GetAsync("/feature-flags/Notifications");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("name").GetString().Should().Be("Notifications");
        body.GetProperty("isEnabled").GetBoolean().Should().BeTrue();
        body.GetProperty("source").GetString().Should().Be("Configuration");
    }

    [Fact]
    public async Task Check_WhenFlagNotDefined_ShouldReturn404()
    {
        // Arrange
        var client = CreateClient();
        await RegisterUserAsync(client);

        // Act
        var response = await client.GetAsync("/feature-flags/NonExistentFlag");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Toggle_WhenAdmin_ShouldSucceed()
    {
        // Arrange — toggle requires AdminOnly policy (sys_admin claim)
        var client = CreateClient();
        var (email, password) = await RegisterUserAsync(client);
        await PromoteToAdminAndRelogin(client, email, password);

        // Act — toggle DarkMode to true
        var toggleResponse = await client.PutAsJsonAsync("/feature-flags/DarkMode", new { isEnabled = true });

        // Assert — should succeed for admin users
        toggleResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Toggle_WhenRegularUser_ShouldReturn403()
    {
        // Arrange — regular user without sys_admin claim
        var client = CreateClient();
        await RegisterUserAsync(client);

        // Act
        var toggleResponse = await client.PutAsJsonAsync("/feature-flags/DarkMode", new { isEnabled = true });

        // Assert — should be forbidden for non-admin users
        toggleResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Toggle_WhenNotAuthenticated_ShouldReturn401()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var response = await client.PutAsJsonAsync("/feature-flags/Notifications", new { isEnabled = false });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
