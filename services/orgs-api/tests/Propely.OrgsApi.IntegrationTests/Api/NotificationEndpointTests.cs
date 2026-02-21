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

namespace Propely.OrgsApi.IntegrationTests.Api;

public sealed class NotificationEndpointTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public NotificationEndpointTests(ApiWebApplicationFactory factory)
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
    public async Task GetNotifications_WhenAuthenticated_ShouldReturn200()
    {
        // Arrange
        var client = CreateClient();
        await RegisterUserAsync(client);

        // Act
        var response = await client.GetAsync("/notifications");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("items").GetArrayLength().Should().Be(0);
        body.GetProperty("unreadCount").GetInt32().Should().Be(0);
        body.GetProperty("totalCount").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task GetNotifications_WhenNotAuthenticated_ShouldReturn401()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var response = await client.GetAsync("/notifications");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetNotifications_ShouldReturnNotificationAfterRoleChange()
    {
        // Arrange — user A creates org, invites user B, accepts, then changes role
        var clientA = CreateClient();
        await RegisterUserAsync(clientA);
        var createResponse = await clientA.PostAsJsonAsync("/orgs", new { name = "Notif Role Org" });
        var createBody = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var orgId = createBody.GetProperty("id").GetString();

        var inviteeEmail = UniqueEmail();
        var inviteResponse = await clientA.PostAsJsonAsync(
            $"/orgs/{orgId}/invitations",
            new { email = inviteeEmail, role = "Agent" });
        var inviteBody = await inviteResponse.Content.ReadFromJsonAsync<JsonElement>();
        var token = await GetInvitationTokenAsync(inviteeEmail);

        var clientB = CreateClient();
        var (memberUserId, _) = await RegisterUserAsync(clientB, inviteeEmail);
        await clientB.PostAsJsonAsync("/orgs/accept-invite", new { token });

        // Change role to Admin — should trigger notification for user B
        await clientA.PutAsJsonAsync($"/orgs/{orgId}/members/{memberUserId}", new { role = "Admin" });

        // Act — user B checks notifications
        var response = await clientB.GetAsync("/notifications");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("items").GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
        body.GetProperty("unreadCount").GetInt32().Should().BeGreaterThanOrEqualTo(1);

        var notification = body.GetProperty("items")[0];
        notification.GetProperty("type").GetString().Should().Be("RoleChanged");
        notification.GetProperty("isRead").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task MarkAsRead_ShouldMarkNotificationAsRead()
    {
        // Arrange — create a notification via role change
        var clientA = CreateClient();
        await RegisterUserAsync(clientA);
        var createResponse = await clientA.PostAsJsonAsync("/orgs", new { name = "MarkRead Org" });
        var createBody = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var orgId = createBody.GetProperty("id").GetString();

        var inviteeEmail = UniqueEmail();
        var inviteResponse = await clientA.PostAsJsonAsync(
            $"/orgs/{orgId}/invitations",
            new { email = inviteeEmail, role = "Agent" });
        var inviteBody = await inviteResponse.Content.ReadFromJsonAsync<JsonElement>();
        var token = await GetInvitationTokenAsync(inviteeEmail);

        var clientB = CreateClient();
        var (memberUserId, _) = await RegisterUserAsync(clientB, inviteeEmail);
        await clientB.PostAsJsonAsync("/orgs/accept-invite", new { token });
        await clientA.PutAsJsonAsync($"/orgs/{orgId}/members/{memberUserId}", new { role = "Admin" });

        // Get the notification ID
        var notifResponse = await clientB.GetAsync("/notifications");
        var notifBody = await notifResponse.Content.ReadFromJsonAsync<JsonElement>();
        var notificationId = notifBody.GetProperty("items")[0].GetProperty("id").GetString();

        // Act — mark as read
        using var patchRequest = new HttpRequestMessage(HttpMethod.Patch, $"/notifications/{notificationId}/read");
        var response = await clientB.SendAsync(patchRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify
        var verifyResponse = await clientB.GetAsync("/notifications");
        var verifyBody = await verifyResponse.Content.ReadFromJsonAsync<JsonElement>();
        var item = verifyBody.GetProperty("items").EnumerateArray()
            .FirstOrDefault(i => i.GetProperty("id").GetString() == notificationId);
        item.GetProperty("isRead").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task MarkAllAsRead_ShouldMarkAllNotificationsAsRead()
    {
        // Arrange — create notifications via role change
        var clientA = CreateClient();
        await RegisterUserAsync(clientA);
        var createResponse = await clientA.PostAsJsonAsync("/orgs", new { name = "MarkAllRead Org" });
        var createBody = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var orgId = createBody.GetProperty("id").GetString();

        var inviteeEmail = UniqueEmail();
        var inviteResponse = await clientA.PostAsJsonAsync(
            $"/orgs/{orgId}/invitations",
            new { email = inviteeEmail, role = "Agent" });
        var inviteBody = await inviteResponse.Content.ReadFromJsonAsync<JsonElement>();
        var token = await GetInvitationTokenAsync(inviteeEmail);

        var clientB = CreateClient();
        var (memberUserId, _) = await RegisterUserAsync(clientB, inviteeEmail);
        await clientB.PostAsJsonAsync("/orgs/accept-invite", new { token });

        // Change role twice to generate multiple notifications
        await clientA.PutAsJsonAsync($"/orgs/{orgId}/members/{memberUserId}", new { role = "Admin" });
        await clientA.PutAsJsonAsync($"/orgs/{orgId}/members/{memberUserId}", new { role = "Agent" });

        // Verify there are unread notifications
        var beforeResponse = await clientB.GetAsync("/notifications");
        var beforeBody = await beforeResponse.Content.ReadFromJsonAsync<JsonElement>();
        beforeBody.GetProperty("unreadCount").GetInt32().Should().BeGreaterThanOrEqualTo(2);

        // Act
        using var patchRequest = new HttpRequestMessage(HttpMethod.Patch, "/notifications/read-all");
        var response = await clientB.SendAsync(patchRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify — all should be read now
        var afterResponse = await clientB.GetAsync("/notifications");
        var afterBody = await afterResponse.Content.ReadFromJsonAsync<JsonElement>();
        afterBody.GetProperty("unreadCount").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task MarkAsRead_WhenNotAuthenticated_ShouldReturn401()
    {
        // Arrange
        var client = CreateClient();

        // Act
        using var patchRequest = new HttpRequestMessage(HttpMethod.Patch, $"/notifications/{Guid.NewGuid()}/read");
        var response = await client.SendAsync(patchRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task MarkAsRead_WhenNotificationNotFound_ShouldReturn404()
    {
        // Arrange
        var client = CreateClient();
        await RegisterUserAsync(client);

        // Act
        using var patchRequest = new HttpRequestMessage(HttpMethod.Patch, $"/notifications/{Guid.NewGuid()}/read");
        var response = await client.SendAsync(patchRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetNotifications_ShouldSupportPagination()
    {
        // Arrange
        var client = CreateClient();
        await RegisterUserAsync(client);

        // Act
        var response = await client.GetAsync("/notifications?page=1&pageSize=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("pageNumber").GetInt32().Should().Be(1);
    }
}
