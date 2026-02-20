// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Security.Claims;

namespace Propely.ContactsApi.Api.Middleware;

public sealed class OrgContextMiddleware
{
    private const string OrgIdHeaderName = "X-Org-Id";
    private readonly RequestDelegate _next;

    public OrgContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity is ClaimsIdentity { IsAuthenticated: true } identity)
        {
            if (identity.FindFirst("org_id") is null)
            {
                var orgIdHeader = context.Request.Headers[OrgIdHeaderName].FirstOrDefault();
                if (Guid.TryParse(orgIdHeader, out var orgId))
                {
                    var orgIdsClaim = identity.FindFirst("org_ids")?.Value;
                    if (orgIdsClaim is null || IsMemberOf(orgIdsClaim, orgId))
                    {
                        identity.AddClaim(new Claim("org_id", orgIdHeader!));
                    }
                }
            }
        }

        await _next(context);
    }

    private static bool IsMemberOf(string orgIdsClaim, Guid orgId)
    {
        var target = orgId.ToString();
        foreach (var segment in orgIdsClaim.Split(','))
        {
            if (string.Equals(segment, target, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}

public static class OrgContextMiddlewareExtensions
{
    public static IApplicationBuilder UseOrgContext(this IApplicationBuilder builder) =>
        builder.UseMiddleware<OrgContextMiddleware>();
}
