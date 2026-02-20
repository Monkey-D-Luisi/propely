// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace Propely.OrgsApi.Api.Configuration;

public static class OAuthConfiguration
{
    public static AuthenticationBuilder AddOAuthProviders(
        this AuthenticationBuilder authBuilder,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var externalCookieSecurePolicy = environment.IsDevelopment() || environment.IsEnvironment("Testing")
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;

        authBuilder.AddCookie(OAuthConstants.ExternalCookieScheme, options =>
        {
            options.Cookie.Name = "orgsapi_external_oauth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = externalCookieSecurePolicy;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
        });

        if (IsGoogleConfigured(configuration))
        {
            authBuilder.AddGoogle(OAuthConstants.GoogleScheme, options =>
            {
                options.ClientId = configuration["OAuth:Google:ClientId"]!;
                options.ClientSecret = configuration["OAuth:Google:ClientSecret"]!;
                options.CallbackPath = "/auth/oauth/google-callback";
                options.SignInScheme = OAuthConstants.ExternalCookieScheme;
                options.ClaimActions.MapJsonKey(OAuthConstants.EmailVerifiedClaim, "email_verified");
                options.ClaimActions.MapJsonKey(OAuthConstants.LegacyVerifiedClaim, "verified_email");
                options.Events = new OAuthEvents
                {
                    OnCreatingTicket = context =>
                    {
                        context.Identity?.AddClaim(new Claim(OAuthConstants.ProviderClaim, OAuthConstants.GoogleProvider));
                        return Task.CompletedTask;
                    }
                };
            });
        }

        if (IsGitHubConfigured(configuration))
        {
            authBuilder.AddOAuth(OAuthConstants.GitHubScheme, options =>
            {
                options.ClientId = configuration["OAuth:GitHub:ClientId"]!;
                options.ClientSecret = configuration["OAuth:GitHub:ClientSecret"]!;
                options.CallbackPath = "/auth/oauth/github-callback";
                options.SignInScheme = OAuthConstants.ExternalCookieScheme;
                options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
                options.TokenEndpoint = "https://github.com/login/oauth/access_token";
                options.UserInformationEndpoint = "https://api.github.com/user";
                options.Scope.Add("read:user");
                options.Scope.Add("user:email");
                options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");

                options.Events = new OAuthEvents
                {
                    OnCreatingTicket = async context =>
                    {
                        var userRequest = CreateGitHubRequest(context.Options.UserInformationEndpoint, context.AccessToken);
                        using var userResponse = await context.Backchannel.SendAsync(userRequest, context.HttpContext.RequestAborted);
                        userResponse.EnsureSuccessStatusCode();

                        using var userPayload = JsonDocument.Parse(await userResponse.Content.ReadAsStringAsync(context.HttpContext.RequestAborted));
                        context.RunClaimActions(userPayload.RootElement);

                        var emailRequest = CreateGitHubRequest("https://api.github.com/user/emails", context.AccessToken);
                        using var emailResponse = await context.Backchannel.SendAsync(emailRequest, context.HttpContext.RequestAborted);
                        emailResponse.EnsureSuccessStatusCode();

                        using var emailsPayload = JsonDocument.Parse(await emailResponse.Content.ReadAsStringAsync(context.HttpContext.RequestAborted));
                        var (verifiedEmail, hasVerifiedEmail) = SelectVerifiedGitHubEmail(emailsPayload.RootElement);

                        if (context.Identity is not null)
                        {
                            if (!string.IsNullOrWhiteSpace(verifiedEmail) &&
                                !context.Identity.HasClaim(c => c.Type == ClaimTypes.Email))
                            {
                                context.Identity.AddClaim(new Claim(ClaimTypes.Email, verifiedEmail));
                            }

                            context.Identity.AddClaim(new Claim(OAuthConstants.EmailVerifiedClaim, hasVerifiedEmail ? "true" : "false"));
                            context.Identity.AddClaim(new Claim(OAuthConstants.ProviderClaim, OAuthConstants.GitHubProvider));
                        }
                    }
                };
            });
        }

        return authBuilder;
    }

    public static bool IsProviderConfigured(IConfiguration configuration, string provider)
    {
        return provider.Trim().ToLowerInvariant() switch
        {
            OAuthConstants.GoogleProvider => IsGoogleConfigured(configuration),
            OAuthConstants.GitHubProvider => IsGitHubConfigured(configuration),
            _ => false
        };
    }

    private static bool IsGoogleConfigured(IConfiguration configuration) =>
        !string.IsNullOrWhiteSpace(configuration["OAuth:Google:ClientId"]) &&
        !string.IsNullOrWhiteSpace(configuration["OAuth:Google:ClientSecret"]);

    private static bool IsGitHubConfigured(IConfiguration configuration) =>
        !string.IsNullOrWhiteSpace(configuration["OAuth:GitHub:ClientId"]) &&
        !string.IsNullOrWhiteSpace(configuration["OAuth:GitHub:ClientSecret"]);

    private static HttpRequestMessage CreateGitHubRequest(string endpoint, string? accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("Propely", "1.0"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    private static (string? Email, bool HasVerifiedEmail) SelectVerifiedGitHubEmail(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Array)
        {
            return (null, false);
        }

        string? fallbackVerifiedEmail = null;

        foreach (var item in element.EnumerateArray())
        {
            if (!item.TryGetProperty("verified", out var verifiedElement) || !verifiedElement.GetBoolean())
            {
                continue;
            }

            if (!item.TryGetProperty("email", out var emailElement))
            {
                continue;
            }

            var email = emailElement.GetString();
            if (string.IsNullOrWhiteSpace(email))
            {
                continue;
            }

            if (item.TryGetProperty("primary", out var primaryElement) && primaryElement.GetBoolean())
            {
                return (email, true);
            }

            fallbackVerifiedEmail ??= email;
        }

        return (fallbackVerifiedEmail, fallbackVerifiedEmail is not null);
    }
}
