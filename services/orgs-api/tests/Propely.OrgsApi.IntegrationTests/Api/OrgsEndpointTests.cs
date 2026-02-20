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
/// Integration tests for OrgsController endpoints.
/// IEmailService is mocked in ApiWebApplicationFactory.
/// </summary>
public sealed class OrgsEndpointTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public OrgsEndpointTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient() => _factory.CreateClient();

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

    [Fact]
    public async Task CreateOrg_WhenAuthenticated_ShouldReturn201()
    {
        // Arrange
        var client = CreateClient();
        await RegisterUserAsync(client);

        // Act
        var response = await client.PostAsJsonAsync("/orgs", new { name = "My Organization" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("id").GetString().Should().NotBeNullOrEmpty();
        body.GetProperty("name").GetString().Should().Be("My Organization");
    }

    [Fact]
    public async Task CreateOrg_WhenNotAuthenticated_ShouldReturn401()
    {
        // Arrange — fresh client with no cookies
        var client = CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/orgs", new { name = "My Organization" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyOrgs_ShouldReturnCreatedOrg()
    {
        // Arrange
        var client = CreateClient();
        await RegisterUserAsync(client);

        var createResponse = await client.PostAsJsonAsync("/orgs", new { name = "Test Org For Mine" });
        createResponse.EnsureSuccessStatusCode();

        // Act
        var response = await client.GetAsync("/orgs/mine");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("items").GetArrayLength().Should().BeGreaterThanOrEqualTo(1);

        var items = body.GetProperty("items").EnumerateArray().ToList();
        var match = items.FirstOrDefault(i => i.GetProperty("name").GetString() == "Test Org For Mine");
        match.ValueKind.Should().NotBe(JsonValueKind.Undefined, "the created org should appear in my orgs list");
        match.GetProperty("role").GetString().Should().Be("owner");
    }

    [Fact]
    public async Task GetMyOrgs_ShouldSupportPagination()
    {
        // Arrange
        var client = CreateClient();
        await RegisterUserAsync(client);
        await client.PostAsJsonAsync("/orgs", new { name = "Paged Org 1" });
        await client.PostAsJsonAsync("/orgs", new { name = "Paged Org 2" });

        // Act
        var response = await client.GetAsync("/orgs/mine?page=1&pageSize=1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("items").GetArrayLength().Should().Be(1);
        body.GetProperty("totalCount").GetInt32().Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetMembers_ShouldReturnOwnerAsMember()
    {
        // Arrange
        var client = CreateClient();
        var (_, email) = await RegisterUserAsync(client);
        var createResponse = await client.PostAsJsonAsync("/orgs", new { name = "Members Test Org" });
        var createBody = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var orgId = createBody.GetProperty("id").GetString();

        // Act
        var response = await client.GetAsync($"/orgs/{orgId}/members");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("items").GetArrayLength().Should().Be(1);
        var member = body.GetProperty("items")[0];
        member.GetProperty("email").GetString().Should().Be(email.ToLowerInvariant());
        member.GetProperty("role").GetString().Should().Be("owner");
    }

    [Fact]
    public async Task GetMembers_ShouldSupportSearch()
    {
        // Arrange
        var client = CreateClient();
        var (_, email) = await RegisterUserAsync(client);
        var createResponse = await client.PostAsJsonAsync("/orgs", new { name = "Search Test Org" });
        var createBody = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var orgId = createBody.GetProperty("id").GetString();

        // Act — search by the user's email
        var response = await client.GetAsync($"/orgs/{orgId}/members?search={email}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("items").GetArrayLength().Should().Be(1);
    }

    [Fact]
    public async Task CreateInvitation_ShouldReturnOk()
    {
        // Arrange
        var client = CreateClient();
        await RegisterUserAsync(client);
        var createResponse = await client.PostAsJsonAsync("/orgs", new { name = "Invite Test Org" });
        var createBody = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var orgId = createBody.GetProperty("id").GetString();

        // Act
        var response = await client.PostAsJsonAsync(
            $"/orgs/{orgId}/invitations",
            new { email = "invitee@example.com", role = "Member" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("ok").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task AcceptInvitation_ShouldAddMember()
    {
        // Arrange — user A creates org, invites user B's email
        var clientA = CreateClient();
        await RegisterUserAsync(clientA);
        var createResponse = await clientA.PostAsJsonAsync("/orgs", new { name = "Accept Invite Org" });
        var createBody = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var orgId = createBody.GetProperty("id").GetString();

        var inviteeEmail = UniqueEmail();
        var inviteResponse = await clientA.PostAsJsonAsync(
            $"/orgs/{orgId}/invitations",
            new { email = inviteeEmail, role = "Member" });
        inviteResponse.EnsureSuccessStatusCode();
        var token = await GetInvitationTokenAsync(inviteeEmail);

        // User B registers with the invited email
        var clientB = CreateClient();
        await RegisterUserAsync(clientB, inviteeEmail);

        // Act — user B accepts the invitation
        var acceptResponse = await clientB.PostAsJsonAsync("/orgs/accept-invite", new { token });

        // Assert
        acceptResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var acceptBody = await acceptResponse.Content.ReadFromJsonAsync<JsonElement>();
        acceptBody.GetProperty("ok").GetBoolean().Should().BeTrue();

        // Verify — user B can now see members of the org
        var membersResponse = await clientB.GetAsync($"/orgs/{orgId}/members");
        membersResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var membersBody = await membersResponse.Content.ReadFromJsonAsync<JsonElement>();
        membersBody.GetProperty("items").GetArrayLength().Should().Be(2);
    }

    [Fact]
    public async Task UpdateMemberRole_ShouldChangeRole()
    {
        // Arrange — create org, invite and accept user, then update their role
        var clientA = CreateClient();
        await RegisterUserAsync(clientA);
        var createResponse = await clientA.PostAsJsonAsync("/orgs", new { name = "Update Role Org" });
        var createBody = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var orgId = createBody.GetProperty("id").GetString();

        var inviteeEmail = UniqueEmail();
        var inviteResponse = await clientA.PostAsJsonAsync(
            $"/orgs/{orgId}/invitations",
            new { email = inviteeEmail, role = "Member" });
        inviteResponse.EnsureSuccessStatusCode();
        var token = await GetInvitationTokenAsync(inviteeEmail);

        // Register and accept as user B
        var clientB = CreateClient();
        var (memberUserId, _) = await RegisterUserAsync(clientB, inviteeEmail);
        await clientB.PostAsJsonAsync("/orgs/accept-invite", new { token });

        // Act — owner updates member's role to Admin
        var response = await clientA.PutAsJsonAsync(
            $"/orgs/{orgId}/members/{memberUserId}",
            new { role = "Admin" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify — member should now be Admin
        var membersResponse = await clientA.GetAsync($"/orgs/{orgId}/members");
        var membersBody = await membersResponse.Content.ReadFromJsonAsync<JsonElement>();
        var members = membersBody.GetProperty("items").EnumerateArray().ToList();
        var updatedMember = members.FirstOrDefault(m => m.GetProperty("userId").GetString() == memberUserId.ToString());
        updatedMember.ValueKind.Should().NotBe(JsonValueKind.Undefined, "the member should exist in the members list");
        updatedMember.GetProperty("role").GetString().Should().Be("admin");
    }

    [Fact]
    public async Task AcceptInvitation_WithInvalidToken_ShouldReturn400()
    {
        // Arrange
        var client = CreateClient();
        await RegisterUserAsync(client);

        // Act
        var response = await client.PostAsJsonAsync("/orgs/accept-invite", new { token = "invalid-token" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateMemberRole_WhenMemberNotFound_ShouldReturn404()
    {
        // Arrange
        var client = CreateClient();
        await RegisterUserAsync(client);
        var createResponse = await client.PostAsJsonAsync("/orgs", new { name = "NotFound Role Org" });
        var createBody = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var orgId = createBody.GetProperty("id").GetString();

        // Act
        var response = await client.PutAsJsonAsync(
            $"/orgs/{orgId}/members/{Guid.NewGuid()}",
            new { role = "Admin" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetOrg_ShouldReturnOrgDetails()
    {
        // Arrange
        var client = CreateClient();
        await RegisterUserAsync(client);
        var createResponse = await client.PostAsJsonAsync("/orgs", new { name = "GetOrg Test" });
        var createBody = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var orgId = createBody.GetProperty("id").GetString();

        // Act
        var response = await client.GetAsync($"/orgs/{orgId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("name").GetString().Should().Be("GetOrg Test");
        body.GetProperty("role").GetString().Should().Be("owner");
    }

    [Fact]
    public async Task GetOrg_WhenNotMember_ShouldReturn403()
    {
        // Arrange — user A creates org
        var clientA = CreateClient();
        await RegisterUserAsync(clientA);
        var createResponse = await clientA.PostAsJsonAsync("/orgs", new { name = "Forbidden Org" });
        var createBody = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var orgId = createBody.GetProperty("id").GetString();

        // User B registers but is NOT a member
        var clientB = CreateClient();
        await RegisterUserAsync(clientB);

        // Act
        var response = await clientB.GetAsync($"/orgs/{orgId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateOrg_AsOwner_ShouldUpdateNameAndDescription()
    {
        // Arrange
        var client = CreateClient();
        await RegisterUserAsync(client);
        var createResponse = await client.PostAsJsonAsync("/orgs", new { name = "Original Org" });
        var createBody = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var orgId = createBody.GetProperty("id").GetString();

        // Act
        using var patchRequest = new HttpRequestMessage(HttpMethod.Patch, $"/orgs/{orgId}")
        {
            Content = JsonContent.Create(new { name = "Updated Org", description = "A test description" })
        };
        var response = await client.SendAsync(patchRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("name").GetString().Should().Be("Updated Org");
        body.GetProperty("description").GetString().Should().Be("A test description");

        // Verify — GET should reflect the update
        var getResponse = await client.GetAsync($"/orgs/{orgId}");
        var getBody = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
        getBody.GetProperty("name").GetString().Should().Be("Updated Org");
        getBody.GetProperty("description").GetString().Should().Be("A test description");
    }

    [Fact]
    public async Task UpdateOrg_AsMember_ShouldReturn403()
    {
        // Arrange — user A creates org, invites user B as member
        var clientA = CreateClient();
        await RegisterUserAsync(clientA);
        var createResponse = await clientA.PostAsJsonAsync("/orgs", new { name = "Member Update Org" });
        var createBody = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var orgId = createBody.GetProperty("id").GetString();

        var inviteeEmail = UniqueEmail();
        var inviteResponse = await clientA.PostAsJsonAsync(
            $"/orgs/{orgId}/invitations",
            new { email = inviteeEmail, role = "Member" });
        inviteResponse.EnsureSuccessStatusCode();
        var token = await GetInvitationTokenAsync(inviteeEmail);

        var clientB = CreateClient();
        await RegisterUserAsync(clientB, inviteeEmail);
        await clientB.PostAsJsonAsync("/orgs/accept-invite", new { token });

        // Act — member tries to update org
        using var patchRequest = new HttpRequestMessage(HttpMethod.Patch, $"/orgs/{orgId}")
        {
            Content = JsonContent.Create(new { name = "Hacked Name", description = "Hacked" })
        };
        var response = await clientB.SendAsync(patchRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateOrg_WhenNotAuthenticated_ShouldReturn401()
    {
        // Arrange
        var client = CreateClient();

        // Act
        using var patchRequest = new HttpRequestMessage(HttpMethod.Patch, $"/orgs/{Guid.NewGuid()}")
        {
            Content = JsonContent.Create(new { name = "Test", description = "Test" })
        };
        var response = await client.SendAsync(patchRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateOrg_WithEmptyName_ShouldReturn400()
    {
        // Arrange
        var client = CreateClient();
        await RegisterUserAsync(client);
        var createResponse = await client.PostAsJsonAsync("/orgs", new { name = "Validation Org" });
        var createBody = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var orgId = createBody.GetProperty("id").GetString();

        // Act
        using var patchRequest = new HttpRequestMessage(HttpMethod.Patch, $"/orgs/{orgId}")
        {
            Content = JsonContent.Create(new { name = "", description = "Valid" })
        };
        var response = await client.SendAsync(patchRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateOrg_WithDuplicateName_ShouldReturn409()
    {
        // Arrange
        var client = CreateClient();
        await RegisterUserAsync(client);
        await client.PostAsJsonAsync("/orgs", new { name = "Duplicate Test Org" });

        // Act — create another org with the same name
        var response = await client.PostAsJsonAsync("/orgs", new { name = "Duplicate Test Org" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateOrg_WithDuplicateNameDifferentCase_ShouldReturn409()
    {
        // Arrange
        var client = CreateClient();
        await RegisterUserAsync(client);
        var uniqueName = $"CaseTest_{Guid.NewGuid():N}";
        await client.PostAsJsonAsync("/orgs", new { name = uniqueName });

        // Act — create another org with different casing
        var response = await client.PostAsJsonAsync("/orgs", new { name = uniqueName.ToUpperInvariant() });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateOrg_ToDuplicateName_ShouldReturn409()
    {
        // Arrange
        var client = CreateClient();
        await RegisterUserAsync(client);
        var nameA = $"OrgA_{Guid.NewGuid():N}";
        var nameB = $"OrgB_{Guid.NewGuid():N}";
        await client.PostAsJsonAsync("/orgs", new { name = nameA });
        var createB = await client.PostAsJsonAsync("/orgs", new { name = nameB });
        var bodyB = await createB.Content.ReadFromJsonAsync<JsonElement>();
        var orgBId = bodyB.GetProperty("id").GetString();

        // Act — rename Org B to Org A's name
        using var patchRequest = new HttpRequestMessage(HttpMethod.Patch, $"/orgs/{orgBId}")
        {
            Content = JsonContent.Create(new { name = nameA, description = (string?)null })
        };
        var response = await client.SendAsync(patchRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateOrg_KeepSameName_ShouldReturn200()
    {
        // Arrange
        var client = CreateClient();
        await RegisterUserAsync(client);
        var orgName = $"KeepName_{Guid.NewGuid():N}";
        var createResponse = await client.PostAsJsonAsync("/orgs", new { name = orgName });
        var createBody = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var orgId = createBody.GetProperty("id").GetString();

        // Act — update description but keep the same name
        using var patchRequest = new HttpRequestMessage(HttpMethod.Patch, $"/orgs/{orgId}")
        {
            Content = JsonContent.Create(new { name = orgName, description = "Updated description" })
        };
        var response = await client.SendAsync(patchRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
