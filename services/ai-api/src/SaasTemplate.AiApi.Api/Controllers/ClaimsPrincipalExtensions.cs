// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Security.Claims;

namespace SaasTemplate.AiApi.Api.Controllers;

/// <summary>
/// Extension methods for ClaimsPrincipal to extract user context.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Tries to extract the user ID from the NameIdentifier claim.
    /// </summary>
    public static bool TryGetUserId(this ClaimsPrincipal user, out Guid userId)
    {
        var userIdString = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdString, out userId);
    }

    /// <summary>
    /// Tries to extract the organization ID from the org_id claim.
    /// </summary>
    public static bool TryGetOrgId(this ClaimsPrincipal user, out Guid orgId)
    {
        var orgIdString = user.FindFirst("org_id")?.Value;
        return Guid.TryParse(orgIdString, out orgId);
    }
}
