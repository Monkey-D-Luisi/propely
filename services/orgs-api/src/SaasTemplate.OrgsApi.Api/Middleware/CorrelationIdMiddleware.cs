// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Api.Services;

namespace SaasTemplate.OrgsApi.Api.Middleware;

/// <summary>
/// Middleware that extracts or generates a correlation ID for each request.
/// The correlation ID is used for end-to-end request tracing.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    private const string CorrelationIdHeaderName = "X-Correlation-ID";
    private const int MaxCorrelationIdLength = 64;
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, CorrelationIdAccessor correlationIdAccessor)
    {
        // Extract correlation ID from request header or generate a new one
        var correlationId = context.Request.Headers[CorrelationIdHeaderName].FirstOrDefault();

        // Validate and sanitize: generate new ID if missing, empty, or too long (security)
        if (string.IsNullOrWhiteSpace(correlationId) || correlationId.Length > MaxCorrelationIdLength)
        {
            if (correlationId?.Length > MaxCorrelationIdLength)
            {
                _logger.LogWarning(
                    "Correlation ID from header exceeded max length ({MaxLength}), generating new ID",
                    MaxCorrelationIdLength);
            }
            correlationId = Guid.NewGuid().ToString("N");
            _logger.LogDebug("Generated new correlation ID: {CorrelationId}", correlationId);
        }
        else
        {
            _logger.LogDebug("Using correlation ID from request header: {CorrelationId}", correlationId);
        }

        // Store in accessor for DI access
        correlationIdAccessor.CorrelationId = correlationId;

        // Add to response headers
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeaderName] = correlationId;
            return Task.CompletedTask;
        });

        // Enrich log context with correlation ID for all logs in this request scope
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        }))
        {
            await _next(context);
        }
    }
}

/// <summary>
/// Extension methods for CorrelationIdMiddleware registration.
/// </summary>
public static class CorrelationIdMiddlewareExtensions
{
    /// <summary>
    /// Adds the correlation ID middleware to the pipeline.
    /// </summary>
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder) =>
        builder.UseMiddleware<CorrelationIdMiddleware>();
}
