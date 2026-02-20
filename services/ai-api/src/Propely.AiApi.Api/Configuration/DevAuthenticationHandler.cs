// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Propely.AiApi.Api.Configuration;

/// <summary>
/// Development-only authentication handler that auto-authenticates all requests.
/// WARNING: This should NEVER be used in production (Security:AllowAnonymous = false).
///
/// Supports per-request tenant/user overrides via headers (for integration tests):
///   X-Test-User-Id: override the NameIdentifier claim
///   X-Test-Org-Id:  override the org_id claim (use "none" to omit it)
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
        // Allow per-request overrides for integration tests
        var userId = Request.Headers["X-Test-User-Id"].FirstOrDefault() ?? DefaultUserId;
        var orgIdHeader = Request.Headers["X-Test-Org-Id"].FirstOrDefault();

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "dev-user"),
            new(ClaimTypes.NameIdentifier, userId),
            new("scope", "workitems:read"),
            new("scope", "workitems:write")
        };

        // "none" (case-insensitive) = omit org_id claim entirely (test fail-closed behavior)
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
