// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Security.Claims;

namespace SaasTemplate.AiApi.Api.Middleware;

/// <summary>
/// Middleware that reads the X-Org-Id header and injects it as an "org_id" claim
/// on the authenticated user's identity. This allows multi-org users to specify
/// their active organization per request without embedding it in the JWT.
/// </summary>
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
            // Only inject from header if org_id is not already present in the JWT
            if (identity.FindFirst("org_id") is null)
            {
                var orgIdHeader = context.Request.Headers[OrgIdHeaderName].FirstOrDefault();
                if (Guid.TryParse(orgIdHeader, out var orgId))
                {
                    // Verify the user is a member of the requested organization.
                    // The JWT contains an "org_ids" claim with a comma-separated list
                    // of organization IDs the user belongs to.
                    // When org_ids is absent (old tokens pre-rollover), allow injection
                    // for backward compatibility until users refresh/re-login.
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

/// <summary>
/// Extension methods for OrgContextMiddleware registration.
/// </summary>
public static class OrgContextMiddlewareExtensions
{
    /// <summary>
    /// Adds the org context middleware to the pipeline.
    /// </summary>
    public static IApplicationBuilder UseOrgContext(this IApplicationBuilder builder) =>
        builder.UseMiddleware<OrgContextMiddleware>();
}
