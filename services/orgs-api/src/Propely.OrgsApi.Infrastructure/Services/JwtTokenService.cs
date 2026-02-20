// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Propely.OrgsApi.Application.Users.Interfaces;
using Propely.OrgsApi.Application.Common.Auth;
using Propely.OrgsApi.Domain.Users;

namespace Propely.OrgsApi.Infrastructure.Services;

public sealed class JwtTokenService : IJwtTokenService
{
    private const string PurposeClaimType = "purpose";
    private const string EmailVerificationPurpose = "email-verification";
    private const string PasswordResetPurpose = "password-reset";
    private const string PasswordHashFingerprintClaimType = "pwd_fgp";

    private static readonly JwtSecurityTokenHandler TokenHandler = new();

    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user, IReadOnlyList<Guid> orgIds)
    {
        var claims = BuildBaseClaims(user.Id, user.Email);
        if (user.Name is not null)
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Name, user.Name));
        }

        claims.Add(new Claim(AuthClaimTypes.PasswordVersion, user.PasswordVersion.ToString()));

        if (user.IsSystemAdmin)
        {
            claims.Add(new Claim(AuthClaimTypes.SystemAdmin, "true"));
        }

        if (orgIds.Count > 0)
        {
            claims.Add(new Claim(AuthClaimTypes.OrgIds, string.Join(',', orgIds)));
        }

        return GenerateJwt(claims, DateTime.UtcNow.AddMinutes(15));
    }

    public string GenerateEmailVerificationToken(Guid userId, string email)
    {
        var claims = BuildBaseClaims(userId, email);
        claims.Add(new Claim(PurposeClaimType, EmailVerificationPurpose));

        return GenerateJwt(claims, DateTime.UtcNow.AddHours(24));
    }

    public EmailVerificationTokenPayload? ValidateEmailVerificationToken(string token)
    {
        var principal = ValidateJwt(token);
        if (principal is null)
        {
            return null;
        }

        var purpose = GetClaimValue(principal, PurposeClaimType);
        if (!string.Equals(purpose, EmailVerificationPurpose, StringComparison.Ordinal))
        {
            return null;
        }

        var userIdRaw = GetClaimValue(principal, JwtRegisteredClaimNames.Sub)
            ?? GetClaimValue(principal, ClaimTypes.NameIdentifier);
        var email = GetClaimValue(principal, JwtRegisteredClaimNames.Email)
            ?? GetClaimValue(principal, ClaimTypes.Email);

        if (!Guid.TryParse(userIdRaw, out var userId) || string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        return new EmailVerificationTokenPayload(userId, email.Trim().ToLowerInvariant());
    }

    public string GeneratePasswordResetToken(Guid userId, string email, string passwordHashFingerprint)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHashFingerprint);

        var claims = BuildBaseClaims(userId, email);
        claims.Add(new Claim(PurposeClaimType, PasswordResetPurpose));
        claims.Add(new Claim(PasswordHashFingerprintClaimType, passwordHashFingerprint));

        return GenerateJwt(claims, DateTime.UtcNow.AddMinutes(15));
    }

    public PasswordResetTokenPayload? ValidatePasswordResetToken(string token)
    {
        var principal = ValidateJwt(token);
        if (principal is null)
        {
            return null;
        }

        var purpose = GetClaimValue(principal, PurposeClaimType);
        if (!string.Equals(purpose, PasswordResetPurpose, StringComparison.Ordinal))
        {
            return null;
        }

        var userIdRaw = GetClaimValue(principal, JwtRegisteredClaimNames.Sub)
            ?? GetClaimValue(principal, ClaimTypes.NameIdentifier);
        var email = GetClaimValue(principal, JwtRegisteredClaimNames.Email)
            ?? GetClaimValue(principal, ClaimTypes.Email);
        var passwordHashFingerprint = GetClaimValue(principal, PasswordHashFingerprintClaimType);

        if (!Guid.TryParse(userIdRaw, out var userId) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(passwordHashFingerprint))
        {
            return null;
        }

        return new PasswordResetTokenPayload(
            userId,
            email.Trim().ToLowerInvariant(),
            passwordHashFingerprint);
    }

    private string GenerateJwt(IReadOnlyCollection<Claim> claims, DateTime expiresAtUtc)
    {
        var token = new JwtSecurityToken(
            issuer: GetIssuer(),
            audience: GetAudience(),
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: CreateSigningCredentials());

        return TokenHandler.WriteToken(token);
    }

    private ClaimsPrincipal? ValidateJwt(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        try
        {
            return TokenHandler.ValidateToken(token, CreateValidationParameters(), out _);
        }
        catch (SecurityTokenException)
        {
            return null;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    private SigningCredentials CreateSigningCredentials()
    {
        var key = BuildSigningKey();
        return new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }

    private TokenValidationParameters CreateValidationParameters()
    {
        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = BuildSigningKey(),
            ValidateIssuer = true,
            ValidIssuer = GetIssuer(),
            ValidateAudience = true,
            ValidAudience = GetAudience(),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    }

    private SymmetricSecurityKey BuildSigningKey()
    {
        var secret = _configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret is not configured.");

        var secretBytes = Encoding.UTF8.GetBytes(secret);
        if (secretBytes.Length < 32)
        {
            throw new InvalidOperationException(
                "Jwt:Secret must be at least 32 bytes (256 bits) for HMAC-SHA256. " +
                $"Current length: {secretBytes.Length} bytes.");
        }

        return new SymmetricSecurityKey(secretBytes);
    }

    private string GetIssuer() => _configuration["Jwt:Issuer"] ?? "orgs-api";

    private string GetAudience() => _configuration["Jwt:Audience"] ?? "propely";

    private static string? GetClaimValue(ClaimsPrincipal principal, string claimType)
    {
        return principal.FindFirst(claimType)?.Value;
    }

    private static List<Claim> BuildBaseClaims(Guid userId, string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return
        [
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, normalizedEmail),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        ];
    }
}
