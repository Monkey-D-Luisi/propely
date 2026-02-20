// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Propely.OrgsApi.Api.Configuration;
using Propely.OrgsApi.Api.Extensions;
using Propely.OrgsApi.Api.Services;
using Propely.OrgsApi.Application.Users.Commands.OAuthLogin;
using Propely.OrgsApi.Domain.Common.Exceptions;

namespace Propely.OrgsApi.Api.Controllers;

[ApiController]
[Route("auth/oauth")]
public sealed class OAuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OAuthController> _logger;
    private readonly bool _useSecureCookies;
    private readonly SameSiteMode _sameSiteMode;

    public OAuthController(
        IMediator mediator,
        IConfiguration configuration,
        CookieSettings cookieSettings,
        ILogger<OAuthController> logger)
    {
        _mediator = mediator;
        _configuration = configuration;
        _logger = logger;
        _useSecureCookies = cookieSettings.UseSecureCookies;
        _sameSiteMode = cookieSettings.SameSiteMode;
    }

    [HttpGet("google")]
    [AllowAnonymous]
    public IActionResult GoogleOAuth([FromQuery] string? next = null) =>
        StartOAuthChallenge(OAuthConstants.GoogleProvider, next);

    [HttpGet("github")]
    [AllowAnonymous]
    public IActionResult GitHubOAuth([FromQuery] string? next = null) =>
        StartOAuthChallenge(OAuthConstants.GitHubProvider, next);

    [HttpGet("callback")]
    [AllowAnonymous]
    public async Task<IActionResult> OAuthCallback(
        [FromQuery] string provider,
        [FromQuery] string? next = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedProvider = provider.Trim().ToLowerInvariant();
        if (!OAuthConstants.TryGetSchemeForProvider(normalizedProvider, out _) ||
            !OAuthConfiguration.IsProviderConfigured(_configuration, normalizedProvider))
        {
            return await RedirectWithOAuthErrorAsync("provider_not_configured", next);
        }

        var authResult = await HttpContext.AuthenticateAsync(OAuthConstants.ExternalCookieScheme);
        if (!authResult.Succeeded || authResult.Principal is null)
        {
            return await RedirectWithOAuthErrorAsync("external_auth_failed", next);
        }

        var principal = authResult.Principal;
        if (!IsProviderBindingValid(authResult.Properties, principal, normalizedProvider))
        {
            _logger.LogWarning(
                "OAuth provider mismatch in callback. Expected provider {ExpectedProvider}",
                normalizedProvider);
            return await RedirectWithOAuthErrorAsync("provider_mismatch", next);
        }

        var externalId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = principal.FindFirstValue(ClaimTypes.Email);
        var name = principal.FindFirstValue(ClaimTypes.Name);

        if (string.IsNullOrWhiteSpace(externalId))
        {
            return await RedirectWithOAuthErrorAsync("missing_external_id", next);
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return await RedirectWithOAuthErrorAsync("missing_email", next);
        }

        if (!HasVerifiedEmail(principal))
        {
            return await RedirectWithOAuthErrorAsync("email_not_verified", next);
        }

        try
        {
            var command = new OAuthLoginCommand(email, name, normalizedProvider, externalId);
            var result = await _mediator.Send(command, cancellationToken);

            Response.SetAccessTokenCookie(result.Token, _useSecureCookies, _sameSiteMode);
            Response.SetRefreshTokenCookie(result.RefreshToken, _useSecureCookies, _sameSiteMode);
            await HttpContext.SignOutAsync(OAuthConstants.ExternalCookieScheme);

            return Redirect(BuildSuccessRedirect(next));
        }
        catch (ConflictException)
        {
            _logger.LogWarning(
                "OAuth provider conflict for provider {Provider} with email {MaskedEmail}",
                normalizedProvider, MaskEmail(email));
            return await RedirectWithOAuthErrorAsync("provider_already_linked", next);
        }
        catch (OperationCanceledException) when (
            cancellationToken.IsCancellationRequested ||
            HttpContext.RequestAborted.IsCancellationRequested)
        {
            _logger.LogDebug(
                "OAuth callback cancelled for provider {Provider}",
                normalizedProvider);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "OAuth login failed for provider {Provider} with email {MaskedEmail}",
                normalizedProvider, MaskEmail(email));
            return await RedirectWithOAuthErrorAsync("oauth_login_failed", next);
        }
    }

    private IActionResult StartOAuthChallenge(string provider, string? next)
    {
        var normalizedProvider = provider.Trim().ToLowerInvariant();
        if (!OAuthConstants.TryGetSchemeForProvider(normalizedProvider, out var scheme) ||
            !OAuthConfiguration.IsProviderConfigured(_configuration, normalizedProvider))
        {
            return Redirect(BuildFailureRedirect("provider_not_configured", next));
        }

        var safeNext = SanitizeNext(next);
        var callbackUrl = Url.Action(
            nameof(OAuthCallback),
            "OAuth",
            new { provider = normalizedProvider, next = safeNext })
            ?? $"/auth/oauth/callback?provider={normalizedProvider}&next={Uri.EscapeDataString(safeNext)}";

        var properties = new AuthenticationProperties
        {
            RedirectUri = callbackUrl
        };
        properties.Items[OAuthConstants.ProviderPropertyKey] = normalizedProvider;

        return Challenge(properties, scheme);
    }

    private async Task<IActionResult> RedirectWithOAuthErrorAsync(string errorCode, string? next)
    {
        await HttpContext.SignOutAsync(OAuthConstants.ExternalCookieScheme);
        return Redirect(BuildFailureRedirect(errorCode, next));
    }

    private string BuildSuccessRedirect(string? next)
    {
        var frontendBaseUrl = (_configuration["Auth:FrontendBaseUrl"] ?? "http://localhost:3000").TrimEnd('/');
        var safeNext = SanitizeNext(next);
        return $"{frontendBaseUrl}{safeNext}";
    }

    private string BuildFailureRedirect(string errorCode, string? next)
    {
        var frontendBaseUrl = (_configuration["Auth:FrontendBaseUrl"] ?? "http://localhost:3000").TrimEnd('/');
        var safeNext = SanitizeNext(next);
        var locale = ResolveLocale(safeNext);

        var queryString = QueryString.Create("oauthError", errorCode);
        if (!string.Equals(safeNext, "/", StringComparison.Ordinal))
        {
            queryString = queryString.Add("next", safeNext);
        }

        return $"{frontendBaseUrl}/{locale}/login{queryString}";
    }

    private static string SanitizeNext(string? next)
    {
        if (string.IsNullOrWhiteSpace(next))
        {
            return "/";
        }

        string decoded;
        try
        {
            decoded = Uri.UnescapeDataString(next);
        }
        catch (FormatException)
        {
            decoded = next;
        }

        if (!decoded.StartsWith("/", StringComparison.Ordinal) ||
            decoded.StartsWith("//", StringComparison.Ordinal) ||
            decoded.Contains('\r') ||
            decoded.Contains('\n'))
        {
            return "/";
        }

        return decoded;
    }

    private static string ResolveLocale(string path)
    {
        if (path.StartsWith("/es/", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(path, "/es", StringComparison.OrdinalIgnoreCase))
        {
            return "es";
        }

        if (path.StartsWith("/en/", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(path, "/en", StringComparison.OrdinalIgnoreCase))
        {
            return "en";
        }

        return "en";
    }

    private static bool HasVerifiedEmail(ClaimsPrincipal principal)
    {
        var verifiedClaim =
            principal.FindFirstValue(OAuthConstants.EmailVerifiedClaim) ??
            principal.FindFirstValue(OAuthConstants.LegacyVerifiedClaim);

        return bool.TryParse(verifiedClaim, out var isVerified) && isVerified;
    }

    private static bool IsProviderBindingValid(
        AuthenticationProperties? properties,
        ClaimsPrincipal principal,
        string expectedProvider)
    {
        if (properties?.Items is not null &&
            properties.Items.TryGetValue(OAuthConstants.ProviderPropertyKey, out var providerFromProperties) &&
            !string.IsNullOrWhiteSpace(providerFromProperties))
        {
            return string.Equals(
                providerFromProperties.Trim(),
                expectedProvider,
                StringComparison.OrdinalIgnoreCase);
        }

        var providerFromClaim = principal.FindFirstValue(OAuthConstants.ProviderClaim);
        return !string.IsNullOrWhiteSpace(providerFromClaim) &&
               string.Equals(
                   providerFromClaim.Trim(),
                   expectedProvider,
                   StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Masks an email address for safe logging (e.g., "t***@example.com").
    /// </summary>
    private static string MaskEmail(string email)
    {
        var atIndex = email.IndexOf('@');
        if (atIndex <= 0)
        {
            return "***";
        }

        var local = email[..atIndex];
        var domain = email[atIndex..];
        var visiblePrefix = local.Length >= 1 ? local[..1] : "";
        return $"{visiblePrefix}***{domain}";
    }
}
