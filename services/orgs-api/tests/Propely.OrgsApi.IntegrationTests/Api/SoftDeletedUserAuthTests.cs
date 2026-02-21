// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Net;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Propely.OrgsApi.Domain.Organizations;
using Propely.OrgsApi.Infrastructure.Persistence;
using Propely.OrgsApi.IntegrationTests.Fixtures;

namespace Propely.OrgsApi.IntegrationTests.Api;

public sealed class SoftDeletedUserAuthTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;
    private const string Password = "TestPassword123!";
    private static int _ipSuffixCounter;

    public SoftDeletedUserAuthTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient() => _factory.CreateClient();

    private static string UniqueEmail() => $"test-{Guid.NewGuid():N}@example.com";

    private static string UniqueIpAddress()
    {
        var suffix = System.Threading.Interlocked.Increment(ref _ipSuffixCounter);
        suffix = ((suffix - 1) % 253) + 1;
        return $"203.0.113.{suffix}";
    }

    private static async Task<string> GetCsrfTokenAsync(HttpClient client, string simulatedIp)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/auth/csrf");
        request.Headers.Add("X-Real-IP", simulatedIp);
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("csrfToken").GetString()!;
    }

    private static async Task<(Guid UserId, string Email)> RegisterUserAsync(HttpClient client, string? email = null)
    {
        var simulatedIp = UniqueIpAddress();
        var csrf = await GetCsrfTokenAsync(client, simulatedIp);
        email ??= UniqueEmail();

        using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/register")
        {
            Content = JsonContent.Create(new
            {
                email,
                password = Password,
                name = "Soft Delete User"
            })
        };
        request.Headers.Add("x-csrf-token", csrf);
        request.Headers.Add("X-Real-IP", simulatedIp);

        var response = await client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return (Guid.Parse(body.GetProperty("userId").GetString()!), email);
    }

    private async Task SoftDeleteUserAsync(Guid userId)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await dbContext.Users
            .IgnoreQueryFilters()
            .SingleAsync(u => u.Id == userId);
        user.SoftDelete();
        await dbContext.SaveChangesAsync();
    }

    private async Task AddMembershipAsync(Guid userId, Guid orgId, MembershipRole role)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Memberships.Add(Membership.Create(userId, orgId, role));
        await dbContext.SaveChangesAsync();
    }

    private string GenerateAccessToken(string subject, string email = "soft-delete@example.com")
    {
        using var scope = _factory.Services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var secret = configuration["Jwt:Secret"]!;
        var issuer = configuration["Jwt:Issuer"] ?? "orgs-api";
        var audience = configuration["Jwt:Audience"] ?? "propely";

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, subject),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow.AddMinutes(-1),
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Fact]
    public async Task Me_WithSoftDeletedUserJwt_ShouldReturn401()
    {
        // Arrange
        var client = CreateClient();
        var (userId, _) = await RegisterUserAsync(client);
        await SoftDeleteUserAsync(userId);

        // Act
        var response = await client.GetAsync("/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithSoftDeletedUserCredentials_ShouldReturn401()
    {
        // Arrange
        var registrationClient = CreateClient();
        var (userId, email) = await RegisterUserAsync(registrationClient);
        await SoftDeleteUserAsync(userId);

        var loginClient = CreateClient();
        var simulatedIp = UniqueIpAddress();
        var csrf = await GetCsrfTokenAsync(loginClient, simulatedIp);

        using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/login")
        {
            Content = JsonContent.Create(new { email, password = Password })
        };
        request.Headers.Add("x-csrf-token", csrf);
        request.Headers.Add("X-Real-IP", simulatedIp);

        // Act
        var response = await loginClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_WithMalformedSubjectClaim_ShouldReturn401()
    {
        // Arrange
        var client = CreateClient();
        var malformedToken = GenerateAccessToken("not-a-guid");
        client.DefaultRequestHeaders.Add("Cookie", $"access_token={malformedToken}");

        // Act
        var response = await client.GetAsync("/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateOrg_WithSoftDeletedUserJwt_ShouldReturn401()
    {
        // Arrange
        var client = CreateClient();
        var (userId, _) = await RegisterUserAsync(client);
        await SoftDeleteUserAsync(userId);

        // Act
        var response = await client.PostAsJsonAsync("/orgs", new { name = "Should Not Be Created" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMembers_ShouldExcludeSoftDeletedUsers()
    {
        // Arrange: owner creates org
        var ownerClient = CreateClient();
        var (_, _) = await RegisterUserAsync(ownerClient);
        var createOrgResponse = await ownerClient.PostAsJsonAsync("/orgs", new { name = "Members Soft Delete Org" });
        createOrgResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createOrgBody = await createOrgResponse.Content.ReadFromJsonAsync<JsonElement>();
        var orgId = Guid.Parse(createOrgBody.GetProperty("id").GetString()!);

        // Arrange: create second user and add membership directly
        var memberClient = CreateClient();
        var (memberUserId, memberEmail) = await RegisterUserAsync(memberClient);
        await AddMembershipAsync(memberUserId, orgId, MembershipRole.Agent);

        // Pre-condition: member appears in list
        var beforeResponse = await ownerClient.GetAsync($"/orgs/{orgId}/members");
        beforeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var beforeBody = await beforeResponse.Content.ReadFromJsonAsync<JsonElement>();
        beforeBody.GetProperty("items")
            .EnumerateArray()
            .Any(item => string.Equals(
                item.GetProperty("email").GetString(),
                memberEmail,
                StringComparison.OrdinalIgnoreCase))
            .Should().BeTrue();

        // Act: soft-delete member user
        await SoftDeleteUserAsync(memberUserId);
        var afterResponse = await ownerClient.GetAsync($"/orgs/{orgId}/members");

        // Assert: deleted user is not returned
        afterResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var afterBody = await afterResponse.Content.ReadFromJsonAsync<JsonElement>();
        afterBody.GetProperty("items")
            .EnumerateArray()
            .Any(item => string.Equals(
                item.GetProperty("email").GetString(),
                memberEmail,
                StringComparison.OrdinalIgnoreCase))
            .Should().BeFalse();
    }
}
