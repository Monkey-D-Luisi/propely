// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Propely.AppointmentsApi.Client;

/// <summary>
/// HTTP message handler that propagates the tenant ID (X-Org-Id) header
/// from the incoming HTTP request to outgoing inter-service calls.
/// </summary>
public sealed class TenantDelegatingHandler : DelegatingHandler
{
    /// <summary>
    /// The name of the tenant ID header.
    /// </summary>
    public const string TenantIdHeaderName = "X-Org-Id";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<TenantDelegatingHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantDelegatingHandler"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    /// <param name="logger">The logger.</param>
    public TenantDelegatingHandler(
        IHttpContextAccessor httpContextAccessor,
        ILogger<TenantDelegatingHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
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
