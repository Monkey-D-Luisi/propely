// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Propely.OrgsApi.Application.Permissions.Interfaces;

namespace Propely.OrgsApi.Api.Authorization;

/// <summary>
/// Handles permission-based authorization requirements by evaluating
/// the user's effective permissions via IPermissionEvaluator.
/// </summary>
public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionEvaluator _permissionEvaluator;
    private readonly ILogger<PermissionAuthorizationHandler> _logger;

    public PermissionAuthorizationHandler(
        IPermissionEvaluator permissionEvaluator,
        ILogger<PermissionAuthorizationHandler> logger)
    {
        _permissionEvaluator = permissionEvaluator;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? context.User.FindFirst("sub")?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Permission check failed: no valid user ID in claims");
            return;
        }

        // Extract organization ID from route data
        var orgId = ExtractOrganizationId(context);
        if (orgId is null)
        {
            _logger.LogWarning("Permission check failed: no organization ID found in route for permission {Permission}", requirement.Permission);
            return;
        }

        var cancellationToken = (context.Resource as HttpContext)?.RequestAborted ?? CancellationToken.None;

        var hasPermission = await _permissionEvaluator.HasPermissionAsync(
            userId, orgId.Value, requirement.Permission, cancellationToken);

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning(
                "Permission denied: user {UserId} lacks {Permission} in org {OrgId}",
                userId, requirement.Permission, orgId.Value);
        }
    }

    private static Guid? ExtractOrganizationId(AuthorizationHandlerContext context)
    {
        // Try to get orgId from HttpContext route values
        if (context.Resource is HttpContext httpContext)
        {
            if (httpContext.Request.RouteValues.TryGetValue("orgId", out var orgIdValue) &&
                Guid.TryParse(orgIdValue?.ToString(), out var orgId))
            {
                return orgId;
            }
        }

        // Try from DefaultHttpContext for filter-based auth
        if (context.Resource is Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext filterContext)
        {
            if (filterContext.RouteData.Values.TryGetValue("orgId", out var routeOrgId) &&
                Guid.TryParse(routeOrgId?.ToString(), out var filteredOrgId))
            {
                return filteredOrgId;
            }
        }

        return null;
    }
}
