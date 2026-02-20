// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Propely.OrgsApi.Client;

/// <summary>
/// HTTP message handler that propagates the tenant ID (X-Tenant-Id) header
/// from the incoming HTTP request to outgoing inter-service calls.
/// </summary>
public sealed class TenantDelegatingHandler : DelegatingHandler
{
    public const string TenantIdHeaderName = "X-Tenant-Id";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<TenantDelegatingHandler> _logger;

    public TenantDelegatingHandler(
        IHttpContextAccessor httpContextAccessor,
        ILogger<TenantDelegatingHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Only add the header if not already present (allow explicit override)
        if (!request.Headers.Contains(TenantIdHeaderName))
        {
            var tenantId = _httpContextAccessor.HttpContext?
                .Request.Headers[TenantIdHeaderName]
                .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(tenantId))
            {
                request.Headers.TryAddWithoutValidation(TenantIdHeaderName, tenantId);
                _logger.LogDebug("Propagated {Header} header with value {TenantId}", TenantIdHeaderName, tenantId);
            }
            else
            {
                _logger.LogDebug("No {Header} header found in incoming request; skipping propagation", TenantIdHeaderName);
            }
        }

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
