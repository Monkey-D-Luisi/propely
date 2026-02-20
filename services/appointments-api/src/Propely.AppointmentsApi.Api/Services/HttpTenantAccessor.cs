// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AppointmentsApi.Application.Common.Interfaces;

namespace Propely.AppointmentsApi.Api.Services;

public sealed class HttpTenantAccessor : ITenantAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpTenantAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? GetCurrentOrgId()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is null)
            return null;

        var orgIdClaim = httpContext.User.FindFirst("org_id")?.Value;

        if (Guid.TryParse(orgIdClaim, out var orgId) && orgId != Guid.Empty)
            return orgId;

        return Guid.Empty;
    }
}
