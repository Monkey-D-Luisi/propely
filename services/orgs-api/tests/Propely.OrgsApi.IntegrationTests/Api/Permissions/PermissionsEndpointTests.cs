// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Propely.OrgsApi.IntegrationTests.Fixtures;
using FluentAssertions;

namespace Propely.OrgsApi.IntegrationTests.Api.Permissions;

/// <summary>
/// Integration tests for PermissionsController endpoints.
/// </summary>
public sealed class PermissionsEndpointTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public PermissionsEndpointTests(ApiWebApplicationFactory factory)
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
        return $"10.99.0.{suffix}";
    }

    private static async Task<string> GetCsrfTokenAsync(HttpClient client)
    {
        var response = await client.GetAsync("/auth/csrf");
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("csrfToken").GetString()!;
    }

    private static async Task<(Guid UserId, string Email)> RegisterUserAsync(HttpClient client, string? email = null)
    {
        var csrf = await GetCsrfTokenAsync(client);
        email ??= UniqueEmail();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/register")
        {
            Content = JsonContent.Create(new { email, password = "TestPassword123!", name = "Test User" })
        };
        request.Headers.Add("x-csrf-token", csrf);
        request.Headers.Add("X-Real-IP", UniqueIpAddress());

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var userId = Guid.Parse(body.GetProperty("userId").GetString()!);
        return (userId, email);
    }

    private static async Task<(Guid OrgId, string OrgName)> CreateOrgAsync(HttpClient client, string? name = null)
    {
        name ??= $"Perm Test Org {Guid.NewGuid():N}";
        var response = await client.PostAsJsonAsync("/orgs", new { name });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var orgId = Guid.Parse(body.GetProperty("id").GetString()!);
        return (orgId, name);
    }

    // =========================================================================
    // GET /api/organizations/{orgId}/permissions/{userId} - Get effective permissions
    // =========================================================================

    [Fact]
    public async Task GetPermissions_OwnerViewingOwnPermissions_ShouldReturn200()
    {
        var client = CreateClient();
        var (userId, _) = await RegisterUserAsync(client);
        var (orgId, _) = await CreateOrgAsync(client);

        var response = await client.GetAsync($"/api/organizations/{orgId}/permissions/{userId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetArrayLength().Should().Be(8); // 8 permissions
    }

    [Fact]
    public async Task GetPermissions_OwnerPermissions_ShouldAllBeGranted()
    {
        var client = CreateClient();
        var (userId, _) = await RegisterUserAsync(client);
        var (orgId, _) = await CreateOrgAsync(client);

        var response = await client.GetAsync($"/api/organizations/{orgId}/permissions/{userId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var permissions = body.EnumerateArray().ToList();

        permissions.Should().AllSatisfy(p =>
        {
            p.GetProperty("granted").GetBoolean().Should().BeTrue();
            p.GetProperty("source").GetString().Should().Be("Role Default");
        });
    }

    [Fact]
    public async Task GetPermissions_Unauthenticated_ShouldReturn401()
    {
        var client = CreateClient();
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var response = await client.GetAsync($"/api/organizations/{orgId}/permissions/{userId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // =========================================================================
    // PUT /api/organizations/{orgId}/permissions/{userId}/{permission} - Set override
    // =========================================================================

    [Fact]
    public async Task SetOverride_InvalidPermission_ShouldReturn400()
    {
        var client = CreateClient();
        var (userId, _) = await RegisterUserAsync(client);
        var (orgId, _) = await CreateOrgAsync(client);

        var response = await client.PutAsJsonAsync(
            $"/api/organizations/{orgId}/permissions/{userId}/NotARealPermission",
            new { granted = true });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SetOverride_OnOwner_ShouldReturn400()
    {
        var client = CreateClient();
        var (userId, _) = await RegisterUserAsync(client);
        var (orgId, _) = await CreateOrgAsync(client);

        // Trying to set override on the owner (ourselves)
        var response = await client.PutAsJsonAsync(
            $"/api/organizations/{orgId}/permissions/{userId}/PropertiesViewAll",
            new { granted = false });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SetOverride_Unauthenticated_ShouldReturn401()
    {
        var client = CreateClient();
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var response = await client.PutAsJsonAsync(
            $"/api/organizations/{orgId}/permissions/{userId}/PropertiesViewAll",
            new { granted = true });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // =========================================================================
    // DELETE /api/organizations/{orgId}/permissions/{userId}/{permission} - Remove override
    // =========================================================================

    [Fact]
    public async Task RemoveOverride_InvalidPermission_ShouldReturn400()
    {
        var client = CreateClient();
        var (userId, _) = await RegisterUserAsync(client);
        var (orgId, _) = await CreateOrgAsync(client);

        var response = await client.DeleteAsync(
            $"/api/organizations/{orgId}/permissions/{userId}/NotARealPermission");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RemoveOverride_Unauthenticated_ShouldReturn401()
    {
        var client = CreateClient();
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var response = await client.DeleteAsync(
            $"/api/organizations/{orgId}/permissions/{userId}/PropertiesViewAll");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RemoveOverride_NoExisting_ShouldReturn200()
    {
        var client = CreateClient();
        var (userId, _) = await RegisterUserAsync(client);
        var (orgId, _) = await CreateOrgAsync(client);

        // Removing a non-existent override for a non-member target should return
        // 403 because the requesting user is trying to manage permissions for
        // someone not in the org, but the handler returns success for "nothing to remove"
        // on an existing member.
        // Since we are owner, removing our own override isn't blocked.
        // Actually the handler only checks requestor role, not target.
        // Let's just verify the endpoint is reachable.
        var response = await client.DeleteAsync(
            $"/api/organizations/{orgId}/permissions/{userId}/PropertiesViewAll");

        // The handler succeeds silently when there's no override to remove
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
