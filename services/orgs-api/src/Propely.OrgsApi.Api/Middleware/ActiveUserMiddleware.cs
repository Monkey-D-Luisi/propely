// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Propely.OrgsApi.Application.Common.Auth;
using Propely.OrgsApi.Application.Users.Interfaces;

namespace Propely.OrgsApi.Api.Middleware;

/// <summary>
/// Ensures authenticated requests are backed by an active (non-soft-deleted) user.
/// </summary>
public sealed class ActiveUserMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ActiveUserMiddleware> _logger;

    public ActiveUserMiddleware(RequestDelegate next, ILogger<ActiveUserMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IUserRepository userRepository)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        var endpoint = context.GetEndpoint();
        var allowAnonymous = endpoint?.Metadata.GetMetadata<IAllowAnonymous>() is not null;

        var sub = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? context.User.FindFirst("sub")?.Value;

        if (!Guid.TryParse(sub, out var userId))
        {
            _logger.LogWarning("Authenticated request missing valid user id claim.");
            InvalidateAuthenticatedPrincipal(context);
            if (allowAnonymous)
            {
                await _next(context);
                return;
            }

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var pwdVerClaim = context.User.FindFirst(AuthClaimTypes.PasswordVersion)?.Value;
        var passwordVersion = int.TryParse(pwdVerClaim, out var pv) ? pv : 0;

        if (await userRepository.IsActiveWithPasswordVersionAsync(userId, passwordVersion, context.RequestAborted))
        {
            await _next(context);
            return;
        }

        InvalidateAuthenticatedPrincipal(context);
        if (allowAnonymous)
        {
            await _next(context);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
    }

    private static void InvalidateAuthenticatedPrincipal(HttpContext context)
    {
        context.User = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity());
        context.Response.Cookies.Delete("access_token", new CookieOptions
        {
            Path = "/",
            SameSite = SameSiteMode.Lax
        });
    }
}

public static class ActiveUserMiddlewareExtensions
{
    public static IApplicationBuilder UseActiveUserValidation(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ActiveUserMiddleware>();
    }
}
