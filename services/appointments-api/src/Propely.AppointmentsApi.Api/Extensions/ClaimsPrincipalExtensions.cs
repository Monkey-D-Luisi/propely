// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Propely.AppointmentsApi.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ControllerBase controller)
    {
        var sub = controller.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? controller.User.FindFirst("sub")?.Value;

        return Guid.TryParse(sub, out var userId) ? userId : null;
    }

    public static Guid? GetTenantId(this ControllerBase controller)
    {
        var orgId = controller.User.FindFirst("org_id")?.Value;
        return Guid.TryParse(orgId, out var tenantId) && tenantId != Guid.Empty ? tenantId : null;
    }
}
