// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Propely.OrgsApi.Infrastructure.Persistence;
using Propely.OrgsApi.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Propely.OrgsApi.IntegrationTests.Api.Permissions;

/// <summary>
/// E2E tests for the full permission lifecycle: set override, verify, remove, verify revocation.
/// </summary>
public sealed class PermissionLifecycleTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public PermissionLifecycleTests(ApiWebApplicationFactory factory)
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
        return $"10.98.0.{suffix}";
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
        var name = $"PermLifecycle Org {Guid.NewGuid():N}";
        var response = await client.PostAsJsonAsync("/orgs", new { name });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return Guid.Parse(body.GetProperty("id").GetString()!);
    }

    private async Task<string> GetInvitationTokenAsync(string email)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var invitation = await db.Invitations
            .AsNoTracking()
            .Where(i => i.Email == email.Trim().ToLowerInvariant())
            .OrderByDescending(i => i.CreatedAtUtc)
            .FirstAsync();
        return invitation.Token;
    }

    private async Task<(HttpClient OwnerClient, Guid OwnerId, Guid OrgId, HttpClient AgentClient, Guid AgentId)> SetupOrgWithAgentAsync()
    {
        // Owner registers and creates org
        var ownerClient = CreateClient();
        var (ownerId, _) = await RegisterUserAsync(ownerClient);
        var orgId = await CreateOrgAsync(ownerClient);

        // Owner invites agent
        var agentEmail = UniqueEmail();
        var inviteResponse = await ownerClient.PostAsJsonAsync(
            $"/orgs/{orgId}/invitations",
            new { email = agentEmail, role = "Agent" });
        inviteResponse.EnsureSuccessStatusCode();

        var token = await GetInvitationTokenAsync(agentEmail);

        // Agent registers and accepts invite
        var agentClient = CreateClient();
        var (agentId, _) = await RegisterUserAsync(agentClient, agentEmail);
        var acceptResponse = await agentClient.PostAsJsonAsync("/orgs/accept-invite", new { token });
        acceptResponse.EnsureSuccessStatusCode();

        return (ownerClient, ownerId, orgId, agentClient, agentId);
    }

    // =========================================================================
    // Agent Default Permissions
    // =========================================================================

    [Fact]
    public async Task AgentDefaultPermissions_ShouldOnlyHaveLeadsManageGranted()
    {
        var (ownerClient, _, orgId, _, agentId) = await SetupOrgWithAgentAsync();

        var response = await ownerClient.GetAsync($"/api/organizations/{orgId}/permissions/{agentId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var permissions = (await response.Content.ReadFromJsonAsync<JsonElement>()).EnumerateArray().ToList();
        permissions.Should().HaveCount(8);

        var leadsManage = permissions.First(p => p.GetProperty("permission").GetString() == "LeadsManage");
        leadsManage.GetProperty("granted").GetBoolean().Should().BeTrue();
        leadsManage.GetProperty("source").GetString().Should().Be("Role Default");

        // All others should be denied by role default
        var otherPermissions = permissions.Where(p => p.GetProperty("permission").GetString() != "LeadsManage");
        otherPermissions.Should().AllSatisfy(p =>
        {
            p.GetProperty("granted").GetBoolean().Should().BeFalse();
            p.GetProperty("source").GetString().Should().Be("Role Default");
        });
    }

    // =========================================================================
    // Grant Override Lifecycle
    // =========================================================================

    [Fact]
    public async Task GrantOverride_AgentGetsNewPermission_ShouldShowOverrideSource()
    {
        var (ownerClient, _, orgId, _, agentId) = await SetupOrgWithAgentAsync();

        // Grant PropertiesViewAll override to agent
        var setResponse = await ownerClient.PutAsJsonAsync(
            $"/api/organizations/{orgId}/permissions/{agentId}/PropertiesViewAll",
            new { granted = true });
        setResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify the permission is now granted with Override source
        var getResponse = await ownerClient.GetAsync($"/api/organizations/{orgId}/permissions/{agentId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var permissions = (await getResponse.Content.ReadFromJsonAsync<JsonElement>()).EnumerateArray().ToList();
        var propertiesView = permissions.First(p => p.GetProperty("permission").GetString() == "PropertiesViewAll");
        propertiesView.GetProperty("granted").GetBoolean().Should().BeTrue();
        propertiesView.GetProperty("source").GetString().Should().Be("Override");
    }

    [Fact]
    public async Task RemoveOverride_AgentLosesGrantedPermission_ShouldReturnToRoleDefault()
    {
        var (ownerClient, _, orgId, _, agentId) = await SetupOrgWithAgentAsync();

        // Grant override
        var grantResponse = await ownerClient.PutAsJsonAsync(
            $"/api/organizations/{orgId}/permissions/{agentId}/PropertiesViewAll",
            new { granted = true });
        grantResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Remove override
        var deleteResponse = await ownerClient.DeleteAsync(
            $"/api/organizations/{orgId}/permissions/{agentId}/PropertiesViewAll");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify the permission reverted to role default (denied for agent)
        var getResponse = await ownerClient.GetAsync($"/api/organizations/{orgId}/permissions/{agentId}");
        var permissions = (await getResponse.Content.ReadFromJsonAsync<JsonElement>()).EnumerateArray().ToList();
        var propertiesView = permissions.First(p => p.GetProperty("permission").GetString() == "PropertiesViewAll");
        propertiesView.GetProperty("granted").GetBoolean().Should().BeFalse();
        propertiesView.GetProperty("source").GetString().Should().Be("Role Default");
    }

    // =========================================================================
    // Deny Override Lifecycle
    // =========================================================================

    [Fact]
    public async Task DenyOverride_OnAgentDefaultPermission_ShouldRevokeAccess()
    {
        var (ownerClient, _, orgId, _, agentId) = await SetupOrgWithAgentAsync();

        // Agent has LeadsManage by default. Deny it via override.
        var setResponse = await ownerClient.PutAsJsonAsync(
            $"/api/organizations/{orgId}/permissions/{agentId}/LeadsManage",
            new { granted = false });
        setResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify LeadsManage is now denied with Override source
        var getResponse = await ownerClient.GetAsync($"/api/organizations/{orgId}/permissions/{agentId}");
        var permissions = (await getResponse.Content.ReadFromJsonAsync<JsonElement>()).EnumerateArray().ToList();
        var leadsManage = permissions.First(p => p.GetProperty("permission").GetString() == "LeadsManage");
        leadsManage.GetProperty("granted").GetBoolean().Should().BeFalse();
        leadsManage.GetProperty("source").GetString().Should().Be("Override");
    }

    [Fact]
    public async Task RemoveDenyOverride_AgentRegainsDefaultPermission()
    {
        var (ownerClient, _, orgId, _, agentId) = await SetupOrgWithAgentAsync();

        // Deny LeadsManage
        await ownerClient.PutAsJsonAsync(
            $"/api/organizations/{orgId}/permissions/{agentId}/LeadsManage",
            new { granted = false });

        // Remove deny override
        var deleteResponse = await ownerClient.DeleteAsync(
            $"/api/organizations/{orgId}/permissions/{agentId}/LeadsManage");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify LeadsManage is back to role default (granted)
        var getResponse = await ownerClient.GetAsync($"/api/organizations/{orgId}/permissions/{agentId}");
        var permissions = (await getResponse.Content.ReadFromJsonAsync<JsonElement>()).EnumerateArray().ToList();
        var leadsManage = permissions.First(p => p.GetProperty("permission").GetString() == "LeadsManage");
        leadsManage.GetProperty("granted").GetBoolean().Should().BeTrue();
        leadsManage.GetProperty("source").GetString().Should().Be("Role Default");
    }
}
