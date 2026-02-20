// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Api.Middleware;

/// <summary>
/// Middleware that adds security headers to all responses.
/// </summary>
public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// Initializes the middleware.
    /// </summary>
    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Adds security headers and invokes the next middleware.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'");

        // Note: HSTS is handled by UseHsts() middleware in non-dev environments

        await _next(context);
    }
}

/// <summary>
/// Extension method for registering SecurityHeadersMiddleware.
/// </summary>
public static class SecurityHeadersMiddlewareExtensions
{
    /// <summary>
    /// Adds the security headers middleware to the pipeline.
    /// </summary>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder) =>
        builder.UseMiddleware<SecurityHeadersMiddleware>();
}
