// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SaasTemplate.OrgsApi.Domain.Users;
using SaasTemplate.OrgsApi.Infrastructure.Services;

namespace SaasTemplate.OrgsApi.UnitTests.Infrastructure.Services;

public sealed class JwtTokenServiceTests
{
    private const string JwtSecret = "unit-test-secret-key-that-is-at-least-32-bytes";
    private readonly JwtTokenService _service;

    public JwtTokenServiceTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = JwtSecret,
                ["Jwt:Issuer"] = "orgs-api",
                ["Jwt:Audience"] = "saas-template"
            })
            .Build();

        _service = new JwtTokenService(configuration);
    }

    [Fact]
    public void GenerateAndValidateEmailVerificationToken_ShouldReturnPayload()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string email = "user@example.com";

        // Act
        var token = _service.GenerateEmailVerificationToken(userId, email);
        var payload = _service.ValidateEmailVerificationToken(token);

        // Assert
        payload.Should().NotBeNull();
        payload!.UserId.Should().Be(userId);
        payload.Email.Should().Be(email);
    }

    [Fact]
    public void ValidateEmailVerificationToken_WithRegularAccessToken_ShouldReturnNull()
    {
        // Arrange
        var user = User.Create("user@example.com", "hash", "User");
        var accessToken = _service.GenerateToken(user, Array.Empty<Guid>());

        // Act
        var payload = _service.ValidateEmailVerificationToken(accessToken);

        // Assert
        payload.Should().BeNull();
    }

    [Fact]
    public void ValidateEmailVerificationToken_WithExpiredToken_ShouldReturnNull()
    {
        // Arrange
        var expiredToken = CreateExpiredVerificationToken();

        // Act
        var payload = _service.ValidateEmailVerificationToken(expiredToken);

        // Assert
        payload.Should().BeNull();
    }

    [Fact]
    public void ValidateEmailVerificationToken_WithInvalidToken_ShouldReturnNull()
    {
        // Act
        var payload = _service.ValidateEmailVerificationToken("not-a-jwt");

        // Assert
        payload.Should().BeNull();
    }

    [Fact]
    public void GenerateEmailVerificationToken_WithEmptyEmail_ShouldThrowArgumentException()
    {
        // Act
        var act = () => _service.GenerateEmailVerificationToken(Guid.NewGuid(), " ");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GeneratePasswordResetToken_WithValidInput_ShouldReturnValidPayloadOnValidation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string email = "john@example.com";
        const string fingerprint = "ABC123";

        // Act
        var token = _service.GeneratePasswordResetToken(userId, email, fingerprint);
        var payload = _service.ValidatePasswordResetToken(token);

        // Assert
        payload.Should().NotBeNull();
        payload!.UserId.Should().Be(userId);
        payload.Email.Should().Be(email);
        payload.PasswordHashFingerprint.Should().Be(fingerprint);
    }

    [Fact]
    public void ValidatePasswordResetToken_WhenTokenPurposeIsDifferent_ShouldReturnNull()
    {
        // Arrange
        var user = User.Create("john@example.com", "hash", "John");
        var loginToken = _service.GenerateToken(user, Array.Empty<Guid>());

        // Act
        var payload = _service.ValidatePasswordResetToken(loginToken);

        // Assert
        payload.Should().BeNull();
    }

    [Fact]
    public void ValidatePasswordResetToken_WhenTokenIsExpired_ShouldReturnNull()
    {
        // Arrange
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiredToken = new JwtSecurityToken(
            issuer: "orgs-api",
            audience: "saas-template",
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, "john@example.com"),
                new Claim("purpose", "password-reset"),
                new Claim("pwd_fgp", "ABC123")
            ],
            expires: DateTime.UtcNow.AddMinutes(-1),
            signingCredentials: credentials);
        var token = new JwtSecurityTokenHandler().WriteToken(expiredToken);

        // Act
        var payload = _service.ValidatePasswordResetToken(token);

        // Assert
        payload.Should().BeNull();
    }

    private static string CreateExpiredVerificationToken()
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Email, "expired@example.com"),
            new("purpose", "email-verification"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: "orgs-api",
            audience: "saas-template",
            claims: claims,
            notBefore: DateTime.UtcNow.AddHours(-2),
            expires: DateTime.UtcNow.AddHours(-1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
