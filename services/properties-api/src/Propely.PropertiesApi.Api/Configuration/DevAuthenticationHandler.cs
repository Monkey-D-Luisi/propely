// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Propely.PropertiesApi.Api.Configuration;

/// <summary>
/// Development-only authentication handler that auto-authenticates all requests.
/// WARNING: This should NEVER be used in production (Security:AllowAnonymous = false).
/// </summary>
public sealed class DevAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private const string DefaultUserId = "00000000-0000-0000-0000-000000000000";
    private const string DefaultOrgId = "00000000-0000-0000-0000-000000000001";

    public DevAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userId = Request.Headers["X-Test-User-Id"].FirstOrDefault() ?? DefaultUserId;
        var orgIdHeader = Request.Headers["X-Test-Org-Id"].FirstOrDefault();

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "dev-user"),
            new(ClaimTypes.NameIdentifier, userId)
        };

        if (!string.Equals(orgIdHeader, "none", StringComparison.OrdinalIgnoreCase))
        {
            claims.Add(new Claim("org_id", orgIdHeader ?? DefaultOrgId));
        }

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
