// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace SaasTemplate.OrgsApi.Application.Users;

internal static class EmailVerificationLinkBuilder
{
    public static string Build(string frontendBaseUrl, string locale, string token)
    {
        var normalizedBaseUrl = frontendBaseUrl.Trim().TrimEnd('/');
        if (string.IsNullOrWhiteSpace(normalizedBaseUrl))
        {
            normalizedBaseUrl = "http://localhost:3000";
        }

        var normalizedLocale = locale.Trim().ToLowerInvariant().StartsWith("es", StringComparison.Ordinal)
            ? "es"
            : "en";
        var encodedToken = Uri.EscapeDataString(token);

        return $"{normalizedBaseUrl}/{normalizedLocale}/verify-email?token={encodedToken}";
    }
}
