// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Propely.OrgsApi.Application.Common.Auth;

namespace Propely.OrgsApi.Api.Configuration;

/// <summary>
/// Testing-only authentication handler that auto-authenticates all requests.
/// Guarded at runtime: throws <see cref="InvalidOperationException"/> if the
/// hosting environment is not "Testing" or "Development".
/// </summary>
public sealed class DevAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private static readonly HashSet<string> AllowedEnvironments = new(StringComparer.OrdinalIgnoreCase)
    {
        "Development",
        "Testing"
    };

    private readonly IHostEnvironment _environment;

    public DevAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IHostEnvironment environment)
        : base(options, logger, encoder)
    {
        _environment = environment;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!AllowedEnvironments.Contains(_environment.EnvironmentName))
        {
            throw new InvalidOperationException(
                $"DevAuthenticationHandler must not be used in '{_environment.EnvironmentName}' environment. " +
                "It is only allowed in Development and Testing environments.");
        }

        // Create a synthetic identity with all required claims for testing
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "dev-user"),
            new Claim(ClaimTypes.NameIdentifier, "00000000-0000-0000-0000-000000000000"), // Guid.Empty
            new Claim(AuthClaimTypes.SystemAdmin, "true"),
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
