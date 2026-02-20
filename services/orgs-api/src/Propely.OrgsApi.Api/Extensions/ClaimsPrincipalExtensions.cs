// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Propely.OrgsApi.Api.Extensions;

/// <summary>
/// Shared extension method for extracting the authenticated user ID from claims.
/// Eliminates duplication across AuthController, OrgsController, BillingController,
/// NotificationsController, and AuditLogsController.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Extracts the user ID (Guid) from the current ClaimsPrincipal.
    /// Looks for NameIdentifier or "sub" claim.
    /// </summary>
    public static Guid? GetUserId(this ControllerBase controller)
    {
        var sub = controller.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? controller.User.FindFirst("sub")?.Value;

        return Guid.TryParse(sub, out var userId) ? userId : null;
    }
}
