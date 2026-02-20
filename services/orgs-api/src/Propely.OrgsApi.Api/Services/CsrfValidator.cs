// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Propely.OrgsApi.Api.Services;

public static class CsrfValidator
{
    private static readonly TimeSpan CsrfTokenMaxAge = TimeSpan.FromHours(1);

    /// <summary>
    /// Validates that the x-csrf-token header contains a well-formed, HMAC-signed,
    /// non-expired CSRF token.
    /// </summary>
    public static bool Validate(HttpRequest request)
    {
        var headerToken = request.Headers["x-csrf-token"].FirstOrDefault();

        if (string.IsNullOrEmpty(headerToken))
        {
            return false;
        }

        // Token format: {hex-timestamp}.{random-hex}.{hmac-hex}
        var lastDotIndex = headerToken.LastIndexOf('.');
        if (lastDotIndex <= 0)
        {
            return false;
        }

        var payload = headerToken[..lastDotIndex];
        var signature = headerToken[(lastDotIndex + 1)..];

        // Validate HMAC signature
        var secret = GetCsrfSecret(request);
        var expectedSignature = ComputeHmac(payload, secret);
        if (!CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(signature),
            Encoding.UTF8.GetBytes(expectedSignature)))
        {
            return false;
        }

        // Validate timestamp freshness
        var firstDotIndex = payload.IndexOf('.');
        if (firstDotIndex <= 0)
        {
            return false;
        }

        if (!long.TryParse(payload[..firstDotIndex], System.Globalization.NumberStyles.HexNumber, null, out var unixSeconds))
        {
            return false;
        }

        var tokenAge = DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
        return tokenAge >= TimeSpan.Zero && tokenAge <= CsrfTokenMaxAge;
    }

    /// <summary>
    /// Generates a signed CSRF token: {hex-timestamp}.{random-hex}.{hmac-hex}
    /// </summary>
    public static string GenerateToken(string csrfSecret)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var random = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var payload = $"{timestamp:X}.{random}";
        var signature = ComputeHmac(payload, csrfSecret);
        return $"{payload}.{signature}";
    }

    public static string ComputeHmac(string payload, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        var hash = HMACSHA256.HashData(keyBytes, payloadBytes);
        return Convert.ToHexString(hash);
    }

    private static bool _csrfFallbackWarningLogged;

    private static string GetCsrfSecret(HttpRequest request)
    {
        var configuration = request.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var csrfSecret = configuration["Csrf:Secret"];
        if (!string.IsNullOrEmpty(csrfSecret))
            return csrfSecret;

        if (!_csrfFallbackWarningLogged)
        {
            var logger = request.HttpContext.RequestServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(nameof(CsrfValidator));
            logger.LogWarning("Csrf:Secret is not configured; falling back to Jwt:Secret. Set a dedicated CSRF secret in production.");
            _csrfFallbackWarningLogged = true;
        }

        return configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException(
                "CSRF secret is not configured. Set Csrf:Secret or Jwt:Secret in configuration.");
    }
}
