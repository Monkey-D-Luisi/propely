// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Propely.OrgsApi.IntegrationTests.Fixtures;
using FluentAssertions;

namespace Propely.OrgsApi.IntegrationTests.Api.Permissions;

/// <summary>
/// E2E tests verifying cross-tenant isolation: a user in Org A
/// cannot view or modify permissions in Org B.
/// </summary>
public sealed class CrossTenantPermissionTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public CrossTenantPermissionTests(ApiWebApplicationFactory factory)
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
        return $"10.96.0.{suffix}";
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
        var name = $"CrossTenant Org {Guid.NewGuid():N}";
        var response = await client.PostAsJsonAsync("/orgs", new { name });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return Guid.Parse(body.GetProperty("id").GetString()!);
    }

    [Fact]
    public async Task GetPermissions_ForOtherOrgMember_ShouldReturnForbiddenOrNotFound()
    {
        // User A creates Org A
        var clientA = CreateClient();
        await RegisterUserAsync(clientA);
        _ = await CreateOrgAsync(clientA);

        // User B creates Org B
        var clientB = CreateClient();
        var (userBId, _) = await RegisterUserAsync(clientB);
        var orgBId = await CreateOrgAsync(clientB);

        // User A tries to view User B's permissions in Org B
        var response = await clientA.GetAsync($"/api/organizations/{orgBId}/permissions/{userBId}");

        // Should be denied — either 403 (Forbidden) or 404 (Not Found) depending on implementation
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SetOverride_OnOtherOrgMember_ShouldReturnForbiddenOrNotFound()
    {
        // User A creates Org A
        var clientA = CreateClient();
        await RegisterUserAsync(clientA);
        _ = await CreateOrgAsync(clientA);

        // User B creates Org B
        var clientB = CreateClient();
        var (userBId, _) = await RegisterUserAsync(clientB);
        var orgBId = await CreateOrgAsync(clientB);

        // User A tries to set override on User B in Org B
        var response = await clientA.PutAsJsonAsync(
            $"/api/organizations/{orgBId}/permissions/{userBId}/PropertiesViewAll",
            new { granted = true });

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPermissions_ForNonMemberInOwnOrg_ShouldReturnBadRequest()
    {
        var client = CreateClient();
        await RegisterUserAsync(client);
        var orgId = await CreateOrgAsync(client);

        // Try to view permissions for a user ID that isn't a member
        var randomUserId = Guid.NewGuid();
        var response = await client.GetAsync($"/api/organizations/{orgId}/permissions/{randomUserId}");

        // The API returns 400 for non-members (target user validation)
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
