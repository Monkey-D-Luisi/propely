// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Propely.OrgsApi.Api.Services;

/// <summary>
/// Provides cookie security settings computed once from the host environment.
/// Eliminates duplication of the _useSecureCookies boolean computation across
/// AuthController and OAuthController.
/// </summary>
public sealed class CookieSettings
{
    /// <summary>
    /// Whether cookies should be marked as Secure (HTTPS-only).
    /// Returns false in Development and Testing environments.
    /// </summary>
    public bool UseSecureCookies { get; }

    /// <summary>
    /// SameSite mode for auth cookies (access_token, refresh_token, csrf_token).
    /// Defaults to <see cref="SameSiteMode.Lax"/> for same-site deployments.
    /// Set to <see cref="SameSiteMode.None"/> when frontend and backend are on
    /// different registrable domains (e.g. separate .run.app subdomains).
    /// Configure via <c>Cookie:SameSite</c> ("None", "Lax", or "Strict").
    /// </summary>
    public SameSiteMode SameSiteMode { get; }

    public CookieSettings(IWebHostEnvironment environment, IConfiguration configuration)
    {
        UseSecureCookies = !environment.IsDevelopment() && !environment.IsEnvironment("Testing");

        var sameSiteValue = configuration["Cookie:SameSite"];
        SameSiteMode = sameSiteValue?.Trim().ToLowerInvariant() switch
        {
            "none" => SameSiteMode.None,
            "strict" => SameSiteMode.Strict,
            _ => SameSiteMode.Lax
        };
    }
}
