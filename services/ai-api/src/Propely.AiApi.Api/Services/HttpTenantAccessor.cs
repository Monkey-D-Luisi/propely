// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Application.Common.Interfaces;

namespace Propely.AiApi.Api.Services;

/// <summary>
/// Resolves the current tenant (organization) from the HTTP context JWT claims.
/// Returns null when no HTTP context is available (system/background mode - bypasses tenant filters).
/// Returns Guid.Empty when HTTP context exists but org_id claim is missing/invalid (fail-closed - matches nothing).
/// </summary>
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

        // No HTTP context (background job, system operation) → null = bypass tenant filter
        if (httpContext is null)
            return null;

        // HTTP context exists → must resolve org_id or fail closed
        var orgIdClaim = httpContext.User.FindFirst("org_id")?.Value;

        // Valid claim → return the org ID
        if (Guid.TryParse(orgIdClaim, out var orgId) && orgId != Guid.Empty)
            return orgId;

        // Missing or invalid claim in HTTP context → Guid.Empty = matches no tenant (fail-closed)
        return Guid.Empty;
    }
}
