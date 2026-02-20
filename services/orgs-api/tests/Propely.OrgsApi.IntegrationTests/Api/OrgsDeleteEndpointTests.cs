// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Propely.OrgsApi.Infrastructure.Persistence;
using Propely.OrgsApi.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Propely.OrgsApi.IntegrationTests.Api;

/// <summary>
/// Integration tests for DELETE endpoints on OrgsController:
/// Leave org, Remove member, Delete org.
/// </summary>
public sealed class OrgsDeleteEndpointTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public OrgsDeleteEndpointTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient()
    {
        var client = _factory.CreateClient();
        // Each client gets a unique IP to avoid cross-test rate limit collisions
        // on the delete:/orgs/* rule (10 req/min per IP).
        client.DefaultRequestHeaders.Add("X-Real-IP", UniqueIpAddress());
        return client;
    }

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

    private static async Task<string> CreateOrgAsync(HttpClient client, string name)
    {
        var response = await client.PostAsJsonAsync("/orgs", new { name });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetString()!;
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

    private async Task<Guid> InviteAndAcceptAsync(
        HttpClient ownerClient, string orgId,
        HttpClient inviteeClient, string inviteeEmail,
        string role = "Member")
    {
        var inviteResponse = await ownerClient.PostAsJsonAsync(
            $"/orgs/{orgId}/invitations",
            new { email = inviteeEmail, role });
        inviteResponse.EnsureSuccessStatusCode();
        var token = await GetInvitationTokenAsync(inviteeEmail);

        var (userId, _) = await RegisterUserAsync(inviteeClient, inviteeEmail);
        var acceptResponse = await inviteeClient.PostAsJsonAsync("/orgs/accept-invite", new { token });
        acceptResponse.EnsureSuccessStatusCode();
        return userId;
    }

    // ── Leave Organization ───────────────────────────────────────────────

    [Fact]
    public async Task LeaveOrg_AsMember_ShouldReturn200()
    {
        // Arrange
        var ownerClient = CreateClient();
        await RegisterUserAsync(ownerClient);
        var orgId = await CreateOrgAsync(ownerClient, $"Leave-{Guid.NewGuid():N}");

        var memberClient = CreateClient();
        await InviteAndAcceptAsync(ownerClient, orgId, memberClient, UniqueEmail());

        // Act
        var response = await memberClient.DeleteAsync($"/orgs/{orgId}/members/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task LeaveOrg_AsLastOwner_ShouldReturn400()
    {
        // Arrange — single owner
        var ownerClient = CreateClient();
        await RegisterUserAsync(ownerClient);
        var orgId = await CreateOrgAsync(ownerClient, $"LeaveLastOwner-{Guid.NewGuid():N}");

        // Act
        var response = await ownerClient.DeleteAsync($"/orgs/{orgId}/members/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task LeaveOrg_WhenNotAuthenticated_ShouldReturn401()
    {
        var client = CreateClient();
        var response = await client.DeleteAsync($"/orgs/{Guid.NewGuid()}/members/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LeaveOrg_WhenNotMember_ShouldReturn403()
    {
        // Arrange — user A creates org, user B is not a member
        var ownerClient = CreateClient();
        await RegisterUserAsync(ownerClient);
        var orgId = await CreateOrgAsync(ownerClient, $"LeaveNotMember-{Guid.NewGuid():N}");

        var outsiderClient = CreateClient();
        await RegisterUserAsync(outsiderClient);

        // Act
        var response = await outsiderClient.DeleteAsync($"/orgs/{orgId}/members/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── Remove Member ────────────────────────────────────────────────────

    [Fact]
    public async Task RemoveMember_OwnerRemovesMember_ShouldReturn200()
    {
        // Arrange
        var ownerClient = CreateClient();
        await RegisterUserAsync(ownerClient);
        var orgId = await CreateOrgAsync(ownerClient, $"Remove-{Guid.NewGuid():N}");

        var memberClient = CreateClient();
        var memberId = await InviteAndAcceptAsync(ownerClient, orgId, memberClient, UniqueEmail());

        // Act
        var response = await ownerClient.DeleteAsync($"/orgs/{orgId}/members/{memberId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RemoveMember_MemberRemovesMember_ShouldReturn403()
    {
        // Arrange
        var ownerClient = CreateClient();
        await RegisterUserAsync(ownerClient);
        var orgId = await CreateOrgAsync(ownerClient, $"RemoveForbid-{Guid.NewGuid():N}");

        var memberClientA = CreateClient();
        await InviteAndAcceptAsync(ownerClient, orgId, memberClientA, UniqueEmail());

        var memberClientB = CreateClient();
        var memberBId = await InviteAndAcceptAsync(ownerClient, orgId, memberClientB, UniqueEmail());

        // Act — member A tries to remove member B
        var response = await memberClientA.DeleteAsync($"/orgs/{orgId}/members/{memberBId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RemoveMember_RemoveSelf_ShouldReturn400()
    {
        // Arrange
        var ownerClient = CreateClient();
        var (ownerId, _) = await RegisterUserAsync(ownerClient);
        var orgId = await CreateOrgAsync(ownerClient, $"RemoveSelf-{Guid.NewGuid():N}");

        // Act
        var response = await ownerClient.DeleteAsync($"/orgs/{orgId}/members/{ownerId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RemoveMember_TargetNotFound_ShouldReturn404()
    {
        // Arrange
        var ownerClient = CreateClient();
        await RegisterUserAsync(ownerClient);
        var orgId = await CreateOrgAsync(ownerClient, $"RemoveNotFound-{Guid.NewGuid():N}");

        // Act
        var response = await ownerClient.DeleteAsync($"/orgs/{orgId}/members/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveMember_WhenNotAuthenticated_ShouldReturn401()
    {
        var client = CreateClient();
        var response = await client.DeleteAsync($"/orgs/{Guid.NewGuid()}/members/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RemoveMember_WhenNotMember_ShouldReturn403()
    {
        // Arrange
        var ownerClient = CreateClient();
        await RegisterUserAsync(ownerClient);
        var orgId = await CreateOrgAsync(ownerClient, $"RemoveNotMember-{Guid.NewGuid():N}");

        var memberClient = CreateClient();
        var memberId = await InviteAndAcceptAsync(ownerClient, orgId, memberClient, UniqueEmail());

        var outsiderClient = CreateClient();
        await RegisterUserAsync(outsiderClient);

        // Act — outsider tries to remove a member
        var response = await outsiderClient.DeleteAsync($"/orgs/{orgId}/members/{memberId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── Delete Organization ──────────────────────────────────────────────

    [Fact]
    public async Task DeleteOrg_AsOwner_ShouldReturn200()
    {
        // Arrange
        var ownerClient = CreateClient();
        await RegisterUserAsync(ownerClient);
        var orgId = await CreateOrgAsync(ownerClient, $"DeleteOrg-{Guid.NewGuid():N}");

        // Act
        var response = await ownerClient.DeleteAsync($"/orgs/{orgId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify — org should no longer appear in my orgs
        var myOrgsResponse = await ownerClient.GetAsync("/orgs/mine");
        var body = await myOrgsResponse.Content.ReadFromJsonAsync<JsonElement>();
        var items = body.GetProperty("items").EnumerateArray().ToList();
        items.Any(i => i.GetProperty("id").GetString() == orgId).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteOrg_AsMember_ShouldReturn403()
    {
        // Arrange
        var ownerClient = CreateClient();
        await RegisterUserAsync(ownerClient);
        var orgId = await CreateOrgAsync(ownerClient, $"DeleteForbid-{Guid.NewGuid():N}");

        var memberClient = CreateClient();
        await InviteAndAcceptAsync(ownerClient, orgId, memberClient, UniqueEmail());

        // Act
        var response = await memberClient.DeleteAsync($"/orgs/{orgId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteOrg_WhenNotAuthenticated_ShouldReturn401()
    {
        var client = CreateClient();
        var response = await client.DeleteAsync($"/orgs/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteOrg_WhenNotMember_ShouldReturn403()
    {
        // Arrange
        var ownerClient = CreateClient();
        await RegisterUserAsync(ownerClient);
        var orgId = await CreateOrgAsync(ownerClient, $"DeleteNotMember-{Guid.NewGuid():N}");

        var outsiderClient = CreateClient();
        await RegisterUserAsync(outsiderClient);

        // Act
        var response = await outsiderClient.DeleteAsync($"/orgs/{orgId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
