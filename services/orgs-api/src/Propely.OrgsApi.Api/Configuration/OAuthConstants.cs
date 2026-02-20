// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Api.Configuration;

public static class OAuthConstants
{
    public const string GoogleProvider = "google";
    public const string GitHubProvider = "github";

    public const string GoogleScheme = "Google";
    public const string GitHubScheme = "GitHub";
    public const string ExternalCookieScheme = "ExternalOAuth";
    public const string ProviderPropertyKey = "oauth_provider";
    public const string ProviderClaim = "oauth_provider";

    public const string EmailVerifiedClaim = "email_verified";
    public const string LegacyVerifiedClaim = "verified_email";

    public static bool TryGetSchemeForProvider(string provider, out string scheme)
    {
        switch (provider.Trim().ToLowerInvariant())
        {
            case GoogleProvider:
                scheme = GoogleScheme;
                return true;
            case GitHubProvider:
                scheme = GitHubScheme;
                return true;
            default:
                scheme = string.Empty;
                return false;
        }
    }
}
