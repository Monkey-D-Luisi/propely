// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Propely.OrgsApi.IntegrationTests.Fixtures;
using FluentAssertions;

namespace Propely.OrgsApi.IntegrationTests.Api.Permissions;

/// <summary>
/// E2E tests verifying that owner users always have full permissions
/// and that overrides cannot be set on owners.
/// </summary>
public sealed class OwnerImmunityTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public OwnerImmunityTests(ApiWebApplicationFactory factory)
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
        return $"10.97.0.{suffix}";
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

    private static async Task<Guid> CreateOrgAsync(HttpClient client)
    {
        var name = $"OwnerImmunity Org {Guid.NewGuid():N}";
        var response = await client.PostAsJsonAsync("/orgs", new { name });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return Guid.Parse(body.GetProperty("id").GetString()!);
    }

    [Fact]
    public async Task OwnerPermissions_ShouldAllBeGrantedAsRoleDefault()
    {
        var client = CreateClient();
        var (userId, _) = await RegisterUserAsync(client);
        var orgId = await CreateOrgAsync(client);

        var response = await client.GetAsync($"/api/organizations/{orgId}/permissions/{userId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var permissions = (await response.Content.ReadFromJsonAsync<JsonElement>()).EnumerateArray().ToList();
        permissions.Should().HaveCount(8);

        permissions.Should().AllSatisfy(p =>
        {
            p.GetProperty("granted").GetBoolean().Should().BeTrue();
            p.GetProperty("source").GetString().Should().Be("Role Default");
        });
    }

    [Fact]
    public async Task SetGrantOverrideOnOwner_ShouldReturn400()
    {
        var client = CreateClient();
        var (userId, _) = await RegisterUserAsync(client);
        var orgId = await CreateOrgAsync(client);

        var response = await client.PutAsJsonAsync(
            $"/api/organizations/{orgId}/permissions/{userId}/PropertiesViewAll",
            new { granted = true });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SetDenyOverrideOnOwner_ShouldReturn400()
    {
        var client = CreateClient();
        var (userId, _) = await RegisterUserAsync(client);
        var orgId = await CreateOrgAsync(client);

        var response = await client.PutAsJsonAsync(
            $"/api/organizations/{orgId}/permissions/{userId}/PropertiesViewAll",
            new { granted = false });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RemoveOverrideOnOwner_ShouldReturn400()
    {
        var client = CreateClient();
        var (userId, _) = await RegisterUserAsync(client);
        var orgId = await CreateOrgAsync(client);

        var response = await client.DeleteAsync(
            $"/api/organizations/{orgId}/permissions/{userId}/PropertiesViewAll");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
