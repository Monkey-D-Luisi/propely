// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Microsoft.AspNetCore.Http;

namespace Propely.OrgsApi.Api.Extensions;

public static class HttpResponseCookieExtensions
{
    public static void SetAccessTokenCookie(this HttpResponse response, string token, bool useSecureCookies, SameSiteMode sameSiteMode = SameSiteMode.Lax)
    {
        response.Cookies.Append("access_token", token, new CookieOptions
        {
            HttpOnly = true,
            SameSite = sameSiteMode,
            Secure = useSecureCookies,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddMinutes(15)
        });
    }

    public static void SetRefreshTokenCookie(this HttpResponse response, string token, bool useSecureCookies, SameSiteMode sameSiteMode = SameSiteMode.Lax)
    {
        // When cross-site (SameSite=None), refresh token must also be None to be sent.
        // When same-site, use Strict for maximum protection.
        var effectiveSameSite = sameSiteMode == SameSiteMode.None
            ? SameSiteMode.None
            : SameSiteMode.Strict;

        response.Cookies.Append("refresh_token", token, new CookieOptions
        {
            HttpOnly = true,
            SameSite = effectiveSameSite,
            Secure = useSecureCookies,
            Path = "/auth/refresh",
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });
    }

    public static void DeleteAccessTokenCookie(this HttpResponse response, bool useSecureCookies, SameSiteMode sameSiteMode = SameSiteMode.Lax)
    {
        response.Cookies.Delete("access_token", new CookieOptions
        {
            Path = "/",
            SameSite = sameSiteMode,
            Secure = useSecureCookies
        });
    }

    public static void DeleteRefreshTokenCookie(this HttpResponse response, bool useSecureCookies, SameSiteMode sameSiteMode = SameSiteMode.Lax)
    {
        var effectiveSameSite = sameSiteMode == SameSiteMode.None
            ? SameSiteMode.None
            : SameSiteMode.Strict;

        response.Cookies.Delete("refresh_token", new CookieOptions
        {
            Path = "/auth/refresh",
            SameSite = effectiveSameSite,
            Secure = useSecureCookies
        });
    }
}
