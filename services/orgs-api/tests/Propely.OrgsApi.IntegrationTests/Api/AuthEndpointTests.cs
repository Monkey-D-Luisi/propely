// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Net;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Propely.OrgsApi.Application.Auth.Security;
using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Application.Users.Interfaces;
using Propely.OrgsApi.IntegrationTests.Fixtures;
using Propely.OrgsApi.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using NSubstitute.Core;

namespace Propely.OrgsApi.IntegrationTests.Api;

/// <summary>
/// Integration tests for AuthController endpoints.
/// </summary>
public sealed class AuthEndpointTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public AuthEndpointTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient() => _factory.CreateClient();
    private HttpClient CreateNoRedirectClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false
    });

    private static string UniqueEmail() => $"test-{Guid.NewGuid():N}@example.com";
    private static int _ipSuffixCounter;
    private static string UniqueIpAddress()
    {
        var suffix = System.Threading.Interlocked.Increment(ref _ipSuffixCounter);
        suffix = ((suffix - 1) % 253) + 1;
        return $"203.0.113.{suffix}";
    }

    private static async Task<string> GetCsrfTokenAsync(HttpClient client, string? simulatedIp = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/auth/csrf");
        request.Headers.Add("X-Real-IP", simulatedIp ?? UniqueIpAddress());
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("csrfToken").GetString()!;
    }

    private static async Task<HttpResponseMessage> SendPostWithCsrfAsync(
        HttpClient client,
        string path,
        object payload,
        string simulatedIp)
    {
        var csrf = await GetCsrfTokenAsync(client, simulatedIp);
        using var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Add("x-csrf-token", csrf);
        request.Headers.Add("X-Real-IP", simulatedIp);

        return await client.SendAsync(request);
    }

    private IEmailService GetEmailServiceMock()
    {
        using var scope = _factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<IEmailService>();
    }

    private string BuildExpiredVerificationToken(Guid userId, string email)
    {
        using var scope = _factory.Services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var secret = configuration["Jwt:Secret"]!;
        var issuer = configuration["Jwt:Issuer"] ?? "orgs-api";
        var audience = configuration["Jwt:Audience"] ?? "propely";

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new("purpose", "email-verification"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow.AddHours(-2),
            expires: DateTime.UtcNow.AddHours(-1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static async Task<(Guid UserId, string Email)> RegisterUserAsync(HttpClient client, string? email = null)
    {
        var simulatedIp = UniqueIpAddress();
        var csrf = await GetCsrfTokenAsync(client, simulatedIp);
        email ??= UniqueEmail();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/register")
        {
            Content = JsonContent.Create(new { email, password = "TestPassword123!", name = "Test User" })
        };
        request.Headers.Add("x-csrf-token", csrf);
        request.Headers.Add("X-Real-IP", simulatedIp);

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var userId = Guid.Parse(body.GetProperty("userId").GetString()!);
        return (userId, email);
    }

    private async Task<string> GenerateResetTokenAsync(Guid userId)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var jwtTokenService = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();

        var user = await dbContext.Users.SingleAsync(u => u.Id == userId);
        var fingerprint = PasswordResetTokenFingerprint.FromPasswordHash(user.PasswordHash);

        return jwtTokenService.GeneratePasswordResetToken(user.Id, user.Email, fingerprint);
    }

    [Fact]
    public async Task GetCsrf_ShouldReturnCsrfToken()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var response = await client.GetAsync("/auth/csrf");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("csrfToken").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_WithValidRequest_ShouldReturn201()
    {
        // Arrange
        var client = CreateClient();
        var csrf = await GetCsrfTokenAsync(client);
        var email = UniqueEmail();

        using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/register")
        {
            Content = JsonContent.Create(new { email, password = "TestPassword123!", name = "John Doe" })
        };
        request.Headers.Add("x-csrf-token", csrf);
        request.Headers.Add("X-Real-IP", UniqueIpAddress());

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("userId").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_WithValidRequest_ShouldSendVerificationEmail()
    {
        // Arrange
        var client = CreateClient();
        var emailService = GetEmailServiceMock();
        emailService.ClearReceivedCalls();
        var email = UniqueEmail();

        // Act
        await RegisterUserAsync(client, email);

        // Assert
        await emailService.Received(1).SendEmailVerificationEmailAsync(
            email.ToLowerInvariant(),
            Arg.Is<string>(url => url.Contains("/en/verify-email?token=", StringComparison.Ordinal)),
            Arg.Any<CancellationToken>(),
            "en");
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturn409()
    {
        // Arrange
        var client = CreateClient();
        var email = UniqueEmail();
        await RegisterUserAsync(client, email);

        // Re-fetch CSRF for second request
        var csrf = await GetCsrfTokenAsync(client);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/register")
        {
            Content = JsonContent.Create(new { email, password = "TestPassword123!", name = "Another" })
        };
        request.Headers.Add("x-csrf-token", csrf);
        request.Headers.Add("X-Real-IP", UniqueIpAddress());

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_WithoutCsrfToken_ShouldReturn403()
    {
        // Arrange
        var client = CreateClient();

        // Act — no CSRF header
        using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/register")
        {
            Content = JsonContent.Create(new { email = UniqueEmail(), password = "TestPassword123!", name = "John" })
        };
        request.Headers.Add("X-Real-IP", UniqueIpAddress());
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ShouldReturn400()
    {
        // Arrange
        var client = CreateClient();
        var csrf = await GetCsrfTokenAsync(client);

        using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/register")
        {
            Content = JsonContent.Create(new { email = "not-an-email", password = "TestPassword123!", name = "John" })
        };
        request.Headers.Add("x-csrf-token", csrf);
        request.Headers.Add("X-Real-IP", UniqueIpAddress());

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task VerifyEmail_WithValidToken_ShouldMarkUserAsVerified()
    {
        // Arrange
        var client = CreateClient();
        var emailService = GetEmailServiceMock();
        emailService.ClearReceivedCalls();
        var (_, email) = await RegisterUserAsync(client);

        var verificationCall = emailService.ReceivedCalls()
            .Last(call => call.GetMethodInfo().Name == nameof(IEmailService.SendEmailVerificationEmailAsync));
        var verificationUrl = verificationCall.GetArguments()[1].ToString()!;
        var token = verificationUrl.Split("token=", StringSplitOptions.RemoveEmptyEntries)[1];

        var csrf = await GetCsrfTokenAsync(client);
        using var verifyRequest = new HttpRequestMessage(HttpMethod.Post, "/auth/verify-email")
        {
            Content = JsonContent.Create(new { token })
        };
        verifyRequest.Headers.Add("x-csrf-token", csrf);

        // Act
        var verifyResponse = await client.SendAsync(verifyRequest);
        var meResponse = await client.GetAsync("/auth/me");

        // Assert
        verifyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        meResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var meBody = await meResponse.Content.ReadFromJsonAsync<JsonElement>();
        meBody.GetProperty("user").GetProperty("email").GetString().Should().Be(email.ToLowerInvariant());
        meBody.GetProperty("user").GetProperty("emailVerified").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task VerifyEmail_WithExpiredToken_ShouldReturn400()
    {
        // Arrange
        var client = CreateClient();
        var (userId, email) = await RegisterUserAsync(client);
        var expiredToken = BuildExpiredVerificationToken(userId, email);
        var csrf = await GetCsrfTokenAsync(client);
        using var verifyRequest = new HttpRequestMessage(HttpMethod.Post, "/auth/verify-email")
        {
            Content = JsonContent.Create(new { token = expiredToken })
        };
        verifyRequest.Headers.Add("x-csrf-token", csrf);

        // Act
        var response = await client.SendAsync(verifyRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("detail").GetString().Should().Be("INVALID_OR_EXPIRED_VERIFICATION_TOKEN");
    }

    [Fact]
    public async Task ResendVerification_WhenAuthenticated_ShouldSendVerificationEmail()
    {
        // Arrange
        var client = CreateClient();
        var emailService = GetEmailServiceMock();
        emailService.ClearReceivedCalls();
        var (_, email) = await RegisterUserAsync(client, "resend-user@example.com");
        emailService.ClearReceivedCalls();

        var csrf = await GetCsrfTokenAsync(client);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/resend-verification")
        {
            Content = JsonContent.Create(new { })
        };
        request.Headers.Add("x-csrf-token", csrf);
        request.Headers.Add("X-Real-IP", "203.0.113.51");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await emailService.Received(1).SendEmailVerificationEmailAsync(
            email.ToLowerInvariant(),
            Arg.Is<string>(url => url.Contains("/en/verify-email?token=", StringComparison.Ordinal)),
            Arg.Any<CancellationToken>(),
            "en");
    }

    [Fact]
    public async Task ResendVerification_WithRequestedLocale_ShouldSendLocalizedVerificationEmail()
    {
        // Arrange
        var client = CreateClient();
        var emailService = GetEmailServiceMock();
        emailService.ClearReceivedCalls();
        var (_, email) = await RegisterUserAsync(client, "resend-locale-user@example.com");
        emailService.ClearReceivedCalls();

        var csrf = await GetCsrfTokenAsync(client);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/resend-verification")
        {
            Content = JsonContent.Create(new { locale = "es" })
        };
        request.Headers.Add("x-csrf-token", csrf);
        request.Headers.Add("X-Real-IP", "203.0.113.71");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await emailService.Received(1).SendEmailVerificationEmailAsync(
            email.ToLowerInvariant(),
            Arg.Is<string>(url => url.Contains("/es/verify-email?token=", StringComparison.Ordinal)),
            Arg.Any<CancellationToken>(),
            "es");
    }

    [Fact]
    public async Task ResendVerification_ShouldBeRateLimited()
    {
        // Arrange
        var client = CreateClient();
        await RegisterUserAsync(client, "rate-limited-user@example.com");

        async Task<HttpResponseMessage> SendResendAsync()
        {
            var csrf = await GetCsrfTokenAsync(client);
            using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/resend-verification")
            {
                Content = JsonContent.Create(new { })
            };
            request.Headers.Add("x-csrf-token", csrf);
            request.Headers.Add("X-Real-IP", "203.0.113.61");
            return await client.SendAsync(request);
        }

        // Act
        var first = await SendResendAsync();
        var second = await SendResendAsync();

        // Assert
        first.StatusCode.Should().Be(HttpStatusCode.OK);
        second.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturn200()
    {
        // Arrange
        var client = CreateClient();
        var email = UniqueEmail();
        await RegisterUserAsync(client, email);

        var csrf = await GetCsrfTokenAsync(client);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/login")
        {
            Content = JsonContent.Create(new { email, password = "TestPassword123!" })
        };
        request.Headers.Add("x-csrf-token", csrf);
        request.Headers.Add("X-Real-IP", UniqueIpAddress());

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("userId").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturn401()
    {
        // Arrange
        var client = CreateClient();
        var email = UniqueEmail();
        await RegisterUserAsync(client, email);

        var csrf = await GetCsrfTokenAsync(client);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/login")
        {
            Content = JsonContent.Create(new { email, password = "WrongPassword!" })
        };
        request.Headers.Add("x-csrf-token", csrf);
        request.Headers.Add("X-Real-IP", UniqueIpAddress());

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ForgotPassword_WithUnknownEmail_ShouldReturn200WithoutEnumeration()
    {
        // Arrange
        var client = CreateClient();
        var csrf = await GetCsrfTokenAsync(client);

        using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/forgot-password")
        {
            Content = JsonContent.Create(new { email = UniqueEmail() })
        };
        request.Headers.Add("x-csrf-token", csrf);
        request.Headers.Add("X-Real-IP", UniqueIpAddress());

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("ok").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task ForgotPassword_WithExistingEmail_ShouldSendPasswordResetEmail()
    {
        // Arrange
        var client = CreateClient();
        var (_, email) = await RegisterUserAsync(client);
        var emailService = _factory.Services.GetRequiredService<IEmailService>();
        emailService.ClearReceivedCalls();

        var csrf = await GetCsrfTokenAsync(client);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/forgot-password")
        {
            Content = JsonContent.Create(new { email, locale = "es" })
        };
        request.Headers.Add("x-csrf-token", csrf);
        request.Headers.Add("X-Real-IP", UniqueIpAddress());

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await emailService.Received(1).SendPasswordResetEmailAsync(
            email.ToLowerInvariant(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>(),
            "es");
    }

    [Fact]
    public async Task ForgotPassword_ShouldApplyEndpointRateLimit()
    {
        // Arrange
        var client = CreateClient();
        var simulatedIp = UniqueIpAddress();

        // Act
        for (var i = 0; i < 3; i++)
        {
            var response = await SendPostWithCsrfAsync(
                client,
                "/auth/forgot-password",
                new { email = UniqueEmail() },
                simulatedIp);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        var blockedResponse = await SendPostWithCsrfAsync(
            client,
            "/auth/forgot-password",
            new { email = UniqueEmail() },
            simulatedIp);

        // Assert
        blockedResponse.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        blockedResponse.Headers.Contains("Retry-After").Should().BeTrue();
    }

    [Fact]
    public async Task Login_ShouldApplyEndpointRateLimit()
    {
        // Arrange
        var client = CreateClient();
        var email = UniqueEmail();
        await RegisterUserAsync(client, email);
        var simulatedIp = UniqueIpAddress();
        const int limit = 5;
        HttpResponseMessage? response = null;

        // Act
        for (var i = 0; i <= limit; i++)
        {
            response = await SendPostWithCsrfAsync(
                client,
                "/auth/login",
                new { email, password = "WrongPassword!" },
                simulatedIp);

            if (i < limit)
            {
                response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        // Assert
        response.Should().NotBeNull();
        response!.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        response.Headers.Contains("Retry-After").Should().BeTrue();
    }

    [Fact]
    public async Task Register_ShouldApplyEndpointRateLimit()
    {
        // Arrange
        var client = CreateClient();
        var simulatedIp = UniqueIpAddress();
        const int limit = 10;
        HttpResponseMessage? response = null;

        // Act
        for (var i = 0; i <= limit; i++)
        {
            response = await SendPostWithCsrfAsync(
                client,
                "/auth/register",
                new
                {
                    email = UniqueEmail(),
                    password = "TestPassword123!",
                    name = "Rate Limit User"
                },
                simulatedIp);

            if (i < limit)
            {
                response.StatusCode.Should().Be(HttpStatusCode.Created);
            }
        }

        // Assert
        response.Should().NotBeNull();
        response!.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        response.Headers.Contains("Retry-After").Should().BeTrue();
    }

    [Fact]
    public async Task CsrfEndpoint_ShouldKeepGlobalRateLimitFallback()
    {
        // Arrange
        var client = CreateClient();
        var simulatedIp = UniqueIpAddress();

        // Act & Assert
        for (var i = 0; i < 20; i++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "/auth/csrf");
            request.Headers.Add("X-Real-IP", simulatedIp);

            var response = await client.SendAsync(request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task GlobalRateLimit_ShouldReturn429After100RequestsFromSameIp()
    {
        // Arrange
        var client = CreateClient();
        var simulatedIp = UniqueIpAddress();
        HttpResponseMessage? lastResponse = null;

        // Act — send 101 requests from the same IP to a non-endpoint-specific route
        for (var i = 0; i < 101; i++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "/auth/csrf");
            request.Headers.Add("X-Real-IP", simulatedIp);

            lastResponse = await client.SendAsync(request);
        }

        // Assert — the 101st request should be rate limited
        lastResponse.Should().NotBeNull();
        lastResponse!.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        lastResponse.Headers.Contains("Retry-After").Should().BeTrue();
    }

    [Fact]
    public async Task ResetPassword_WithValidToken_ShouldAllowLoginWithNewPassword()
    {
        // Arrange
        var client = CreateClient();
        var (userId, email) = await RegisterUserAsync(client);
        var resetToken = await GenerateResetTokenAsync(userId);
        var csrf = await GetCsrfTokenAsync(client);

        using var resetRequest = new HttpRequestMessage(HttpMethod.Post, "/auth/reset-password")
        {
            Content = JsonContent.Create(new
            {
                token = resetToken,
                newPassword = "ResetPassword456!"
            })
        };
        resetRequest.Headers.Add("x-csrf-token", csrf);

        // Act
        var resetResponse = await client.SendAsync(resetRequest);

        // Assert
        resetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginClient = CreateClient();
        var loginCsrf = await GetCsrfTokenAsync(loginClient);

        using var loginWithOldPasswordRequest = new HttpRequestMessage(HttpMethod.Post, "/auth/login")
        {
            Content = JsonContent.Create(new { email, password = "TestPassword123!" })
        };
        loginWithOldPasswordRequest.Headers.Add("x-csrf-token", loginCsrf);
        loginWithOldPasswordRequest.Headers.Add("X-Real-IP", UniqueIpAddress());
        var oldPasswordLoginResponse = await loginClient.SendAsync(loginWithOldPasswordRequest);
        oldPasswordLoginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var newLoginCsrf = await GetCsrfTokenAsync(loginClient);
        using var loginWithNewPasswordRequest = new HttpRequestMessage(HttpMethod.Post, "/auth/login")
        {
            Content = JsonContent.Create(new { email, password = "ResetPassword456!" })
        };
        loginWithNewPasswordRequest.Headers.Add("x-csrf-token", newLoginCsrf);
        loginWithNewPasswordRequest.Headers.Add("X-Real-IP", UniqueIpAddress());
        var newPasswordLoginResponse = await loginClient.SendAsync(loginWithNewPasswordRequest);
        newPasswordLoginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ResetPassword_WithAlreadyUsedToken_ShouldReturn400()
    {
        // Arrange
        var client = CreateClient();
        var (userId, _) = await RegisterUserAsync(client);
        var resetToken = await GenerateResetTokenAsync(userId);
        var csrf = await GetCsrfTokenAsync(client);

        using var firstResetRequest = new HttpRequestMessage(HttpMethod.Post, "/auth/reset-password")
        {
            Content = JsonContent.Create(new
            {
                token = resetToken,
                newPassword = "ResetPassword456!"
            })
        };
        firstResetRequest.Headers.Add("x-csrf-token", csrf);
        var firstResetResponse = await client.SendAsync(firstResetRequest);
        firstResetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var secondCsrf = await GetCsrfTokenAsync(client);
        using var secondResetRequest = new HttpRequestMessage(HttpMethod.Post, "/auth/reset-password")
        {
            Content = JsonContent.Create(new
            {
                token = resetToken,
                newPassword = "ResetPassword789!"
            })
        };
        secondResetRequest.Headers.Add("x-csrf-token", secondCsrf);

        // Act
        var secondResetResponse = await client.SendAsync(secondResetRequest);

        // Assert
        secondResetResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await secondResetResponse.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("detail").GetString().Should().Be("RESET_TOKEN_ALREADY_USED");
    }

    [Fact]
    public async Task Me_WhenAuthenticated_ShouldReturnUser()
    {
        // Arrange — register sets access_token cookie automatically
        var client = CreateClient();
        var (_, email) = await RegisterUserAsync(client);

        // Act
        var response = await client.GetAsync("/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var user = body.GetProperty("user");
        user.GetProperty("email").GetString().Should().Be(email.ToLowerInvariant());
        user.GetProperty("name").GetString().Should().Be("Test User");
        user.GetProperty("emailVerified").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task Me_WhenNotAuthenticated_ShouldReturn401()
    {
        // Arrange — fresh client with no cookies
        var client = CreateClient();

        // Act
        var response = await client.GetAsync("/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_ShouldReturn200()
    {
        // Arrange
        var client = CreateClient();
        await RegisterUserAsync(client);

        // Act
        var response = await client.PostAsync("/auth/logout", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("ok").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task UpdateProfile_WhenAuthenticated_ShouldReturn200()
    {
        // Arrange
        var client = CreateClient();
        await RegisterUserAsync(client);
        var csrf = await GetCsrfTokenAsync(client);

        using var request = new HttpRequestMessage(HttpMethod.Patch, "/auth/me")
        {
            Content = JsonContent.Create(new { name = "Updated Name" })
        };
        request.Headers.Add("x-csrf-token", csrf);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("name").GetString().Should().Be("Updated Name");
    }

    [Fact]
    public async Task UpdateProfile_WhenNotAuthenticated_ShouldReturn401()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var response = await client.PatchAsJsonAsync("/auth/me", new { name = "Updated Name" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateProfile_WithoutCsrfToken_ShouldReturn403()
    {
        // Arrange
        var client = CreateClient();
        await RegisterUserAsync(client);

        // Act — no CSRF header
        var response = await client.PatchAsJsonAsync("/auth/me", new { name = "Updated Name" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateProfile_WithNullName_ShouldClearName()
    {
        // Arrange
        var client = CreateClient();
        await RegisterUserAsync(client);
        var csrf = await GetCsrfTokenAsync(client);

        using var request = new HttpRequestMessage(HttpMethod.Patch, "/auth/me")
        {
            Content = JsonContent.Create(new { name = (string?)null })
        };
        request.Headers.Add("x-csrf-token", csrf);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("name").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public async Task UpdateProfile_ShouldPersistChanges()
    {
        // Arrange
        var client = CreateClient();
        await RegisterUserAsync(client);
        var csrf = await GetCsrfTokenAsync(client);

        // Act — update the name
        using var request = new HttpRequestMessage(HttpMethod.Patch, "/auth/me")
        {
            Content = JsonContent.Create(new { name = "Persisted Name" })
        };
        request.Headers.Add("x-csrf-token", csrf);
        var updateResponse = await client.SendAsync(request);
        updateResponse.EnsureSuccessStatusCode();

        // Assert — verify via GET /auth/me
        var meResponse = await client.GetAsync("/auth/me");
        meResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await meResponse.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("user").GetProperty("name").GetString().Should().Be("Persisted Name");
    }

    [Fact]
    public async Task ChangePassword_WithValidCredentials_ShouldReturn200()
    {
        // Arrange
        var client = CreateClient();
        await RegisterUserAsync(client);
        var csrf = await GetCsrfTokenAsync(client);

        using var request = new HttpRequestMessage(HttpMethod.Put, "/auth/password")
        {
            Content = JsonContent.Create(new
            {
                currentPassword = "TestPassword123!",
                newPassword = "NewPassword456!"
            })
        };
        request.Headers.Add("x-csrf-token", csrf);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("ok").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task ChangePassword_ShouldAllowLoginWithNewPassword()
    {
        // Arrange
        var client = CreateClient();
        var (_, email) = await RegisterUserAsync(client);

        // Change password
        var csrf = await GetCsrfTokenAsync(client);
        using var changeRequest = new HttpRequestMessage(HttpMethod.Put, "/auth/password")
        {
            Content = JsonContent.Create(new
            {
                currentPassword = "TestPassword123!",
                newPassword = "NewPassword456!"
            })
        };
        changeRequest.Headers.Add("x-csrf-token", csrf);
        var changeResponse = await client.SendAsync(changeRequest);
        changeResponse.EnsureSuccessStatusCode();

        // Act — login with new password
        var loginClient = CreateClient();
        var loginCsrf = await GetCsrfTokenAsync(loginClient);
        using var loginRequest = new HttpRequestMessage(HttpMethod.Post, "/auth/login")
        {
            Content = JsonContent.Create(new { email, password = "NewPassword456!" })
        };
        loginRequest.Headers.Add("x-csrf-token", loginCsrf);
        loginRequest.Headers.Add("X-Real-IP", UniqueIpAddress());

        var loginResponse = await loginClient.SendAsync(loginRequest);

        // Assert
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ChangePassword_WithWrongCurrentPassword_ShouldReturn401()
    {
        // Arrange
        var client = CreateClient();
        await RegisterUserAsync(client);
        var csrf = await GetCsrfTokenAsync(client);

        using var request = new HttpRequestMessage(HttpMethod.Put, "/auth/password")
        {
            Content = JsonContent.Create(new
            {
                currentPassword = "WrongPassword!",
                newPassword = "NewPassword456!"
            })
        };
        request.Headers.Add("x-csrf-token", csrf);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangePassword_WithoutCsrfToken_ShouldReturn403()
    {
        // Arrange
        var client = CreateClient();
        await RegisterUserAsync(client);

        // Act — no CSRF header
        var response = await client.PutAsJsonAsync("/auth/password",
            new { currentPassword = "TestPassword123!", newPassword = "NewPassword456!" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ChangePassword_WhenNotAuthenticated_ShouldReturn401()
    {
        // Arrange
        var client = CreateClient();
        var csrf = await GetCsrfTokenAsync(client);

        using var request = new HttpRequestMessage(HttpMethod.Put, "/auth/password")
        {
            Content = JsonContent.Create(new
            {
                currentPassword = "TestPassword123!",
                newPassword = "NewPassword456!"
            })
        };
        request.Headers.Add("x-csrf-token", csrf);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangePassword_WithShortNewPassword_ShouldReturn400()
    {
        // Arrange
        var client = CreateClient();
        await RegisterUserAsync(client);
        var csrf = await GetCsrfTokenAsync(client);

        using var request = new HttpRequestMessage(HttpMethod.Put, "/auth/password")
        {
            Content = JsonContent.Create(new
            {
                currentPassword = "TestPassword123!",
                newPassword = "short"
            })
        };
        request.Headers.Add("x-csrf-token", csrf);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Response_ShouldContainCorrelationIdHeader()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var response = await client.GetAsync("/auth/csrf");

        // Assert
        response.Headers.Should().ContainKey("X-Correlation-ID");
        var correlationId = response.Headers.GetValues("X-Correlation-ID").First();
        correlationId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Response_ShouldContainSecurityHeaders()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var response = await client.GetAsync("/auth/csrf");

        // Assert
        response.Headers.Should().ContainKey("X-Content-Type-Options");
        response.Headers.GetValues("X-Content-Type-Options").Should().Contain("nosniff");

        response.Headers.Should().ContainKey("X-Frame-Options");
        response.Headers.GetValues("X-Frame-Options").Should().Contain("DENY");

        response.Headers.Should().ContainKey("Referrer-Policy");
        response.Headers.GetValues("Referrer-Policy").Should().Contain("strict-origin-when-cross-origin");
    }

    [Fact]
    public async Task OAuthGoogle_WhenProviderConfigured_ShouldRedirectToGoogle()
    {
        // Arrange
        var client = CreateNoRedirectClient();

        // Act
        var response = await client.GetAsync("/auth/oauth/google?next=%2Fen%2Forgs%2Fmine");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.AbsoluteUri.Should().Contain("accounts.google.com");
    }

    [Fact]
    public async Task OAuthGitHub_WhenProviderConfigured_ShouldRedirectToGitHub()
    {
        // Arrange
        var client = CreateNoRedirectClient();

        // Act
        var response = await client.GetAsync("/auth/oauth/github?next=%2Fen%2Forgs%2Fmine");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.AbsoluteUri.Should().Contain("github.com/login/oauth/authorize");
    }

    [Fact]
    public async Task OAuthCallback_WithoutExternalPrincipal_ShouldRedirectToLoginWithOauthError()
    {
        // Arrange
        var client = CreateNoRedirectClient();

        // Act
        var response = await client.GetAsync("/auth/oauth/callback?provider=google&next=%2Fes%2Forgs%2Fmine");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain("http://localhost:3000/es/login");
        response.Headers.Location!.ToString().Should().Contain("oauthError=external_auth_failed");
    }

    [Fact]
    public async Task Login_WithoutCsrfToken_ShouldReturn403()
    {
        // Arrange
        var client = CreateClient();
        var email = UniqueEmail();
        await RegisterUserAsync(client, email);

        // Act — no CSRF header
        using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/login")
        {
            Content = JsonContent.Create(new { email, password = "TestPassword123!" })
        };
        request.Headers.Add("X-Real-IP", UniqueIpAddress());
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ForgotPassword_WithoutCsrfToken_ShouldReturn403()
    {
        // Arrange
        var client = CreateClient();

        // Act — no CSRF header
        using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/forgot-password")
        {
            Content = JsonContent.Create(new { email = UniqueEmail() })
        };
        request.Headers.Add("X-Real-IP", UniqueIpAddress());
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ResetPassword_WithoutCsrfToken_ShouldReturn403()
    {
        // Arrange
        var client = CreateClient();

        // Act — no CSRF header
        using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/reset-password")
        {
            Content = JsonContent.Create(new { token = "some-token", newPassword = "NewPassword123!" })
        };
        request.Headers.Add("X-Real-IP", UniqueIpAddress());
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task VerifyEmail_WithoutCsrfToken_ShouldReturn403()
    {
        // Arrange
        var client = CreateClient();

        // Act — no CSRF header
        using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/verify-email")
        {
            Content = JsonContent.Create(new { token = "some-token" })
        };
        request.Headers.Add("X-Real-IP", UniqueIpAddress());
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task OAuthCallback_WithUnknownProvider_ShouldRedirectToLoginWithError()
    {
        // Arrange
        var client = CreateNoRedirectClient();

        // Act
        var response = await client.GetAsync("/auth/oauth/callback?provider=unknown&next=%2Fen%2Forgs%2Fmine");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain("http://localhost:3000/en/login");
        response.Headers.Location!.ToString().Should().Contain("oauthError=provider_not_configured");
    }

    [Fact]
    public async Task ChangePassword_ShouldInvalidateOldToken()
    {
        // Arrange — register and capture the pre-change token cookie
        var client = CreateClient();
        await RegisterUserAsync(client);

        // Save the current access_token cookie by making a /me request
        var meBeforeChange = await client.GetAsync("/auth/me");
        meBeforeChange.StatusCode.Should().Be(HttpStatusCode.OK);

        // Change password — this increments PasswordVersion
        var csrf = await GetCsrfTokenAsync(client);
        using var changeRequest = new HttpRequestMessage(HttpMethod.Put, "/auth/password")
        {
            Content = JsonContent.Create(new
            {
                currentPassword = "TestPassword123!",
                newPassword = "NewPassword456!"
            })
        };
        changeRequest.Headers.Add("x-csrf-token", csrf);
        var changeResponse = await client.SendAsync(changeRequest);
        changeResponse.EnsureSuccessStatusCode();

        // Act — use the same client (still has old token with pwd_ver=0) to access /me
        var meAfterChange = await client.GetAsync("/auth/me");

        // Assert — old token should be rejected because pwd_ver no longer matches
        meAfterChange.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task OAuthCallback_WithUnverifiedEmail_ShouldRedirectWithEmailNotVerifiedError()
    {
        // Arrange — forge a valid ExternalOAuth cookie with unverified email claims
        using var scope = _factory.Services.CreateScope();
        var optionsMonitor = scope.ServiceProvider
            .GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>();
        var cookieOptions = optionsMonitor.Get("ExternalOAuth");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "ext-google-123456"),
            new(ClaimTypes.Email, "unverified@example.com"),
            new(ClaimTypes.Name, "Test User"),
            new("email_verified", "false"),
            new("oauth_provider", "google")
        };
        var identity = new ClaimsIdentity(claims, "Google");
        var principal = new ClaimsPrincipal(identity);
        var properties = new AuthenticationProperties();
        properties.Items["oauth_provider"] = "google";
        var ticket = new AuthenticationTicket(principal, properties, "ExternalOAuth");
        var cookieValue = cookieOptions.TicketDataFormat.Protect(ticket);

        var client = CreateNoRedirectClient();
        using var request = new HttpRequestMessage(HttpMethod.Get,
            "/auth/oauth/callback?provider=google&next=%2Fen%2Forgs%2Fmine");
        request.Headers.Add("Cookie", $"orgsapi_external_oauth={cookieValue}");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain("oauthError=email_not_verified");
    }
}
