// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Security.Claims;
using SaasTemplate.OrgsApi.Application.Common.Interfaces;

namespace SaasTemplate.OrgsApi.Api.Services;

/// <summary>
/// Provides audit context from the current HTTP request (user identity and correlation ID).
/// </summary>
public sealed class HttpAuditContext : IAuditContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICorrelationIdAccessor _correlationIdAccessor;

    public HttpAuditContext(IHttpContextAccessor httpContextAccessor, ICorrelationIdAccessor correlationIdAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _correlationIdAccessor = correlationIdAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var sub = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value;

            return Guid.TryParse(sub, out var userId) ? userId : null;
        }
    }

    public string? CorrelationId => _correlationIdAccessor.CorrelationId;
}
