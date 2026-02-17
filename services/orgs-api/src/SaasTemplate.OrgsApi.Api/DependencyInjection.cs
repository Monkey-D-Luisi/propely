// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Reflection;
using System.Text;
using SaasTemplate.OrgsApi.Api.Configuration;
using SaasTemplate.OrgsApi.Api.Middleware;
using SaasTemplate.OrgsApi.Api.Services;
using SaasTemplate.OrgsApi.Application.Common.Interfaces;
using AspNetCoreRateLimit;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.DependencyInjection;
using SaasTemplate.OrgsApi.Application.Common.Auth;

namespace SaasTemplate.OrgsApi.Api;

public static class DependencyInjection
{
    public static void AddApiServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        // FluentValidation for API DTOs
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Correlation ID accessor (scoped per request)
        services.AddScoped<CorrelationIdAccessor>();
        services.AddScoped<ICorrelationIdAccessor>(sp => sp.GetRequiredService<CorrelationIdAccessor>());

        // Shared cookie security settings (singleton, environment-based)
        services.AddSingleton<CookieSettings>();

        // Audit context (scoped per request — reads user claims + correlation ID)
        services.AddHttpContextAccessor();
        services.AddScoped<IAuditContext, HttpAuditContext>();

        // Rate limiting
        services.AddMemoryCache();
        services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        services.AddInMemoryRateLimiting();

        // OpenTelemetry tracing and metrics
        services.AddTelemetry(configuration);

        // Health checks
        services.AddApplicationHealthChecks(configuration);

        // CORS
        AddCors(services, configuration);

        // Authentication & Authorization
        AddAuthenticationAndAuthorization(services, configuration, environment);

        // Controllers
        services.AddControllers();

        // Swagger / OpenAPI (available in development)
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGenWithAuth();

        // Version options
        services.Configure<VersionOptions>(configuration.GetSection("Version"));

        // HttpClient for GitHub API (update checks)
        services.AddHttpClient("github", client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", "SaaSStarterKit");
            client.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
        });
    }

    private static void AddCors(IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:3000"];

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });
    }

    private static void AddAuthenticationAndAuthorization(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var jwtSecret = configuration["Jwt:Secret"];
        var useJwt = !string.IsNullOrEmpty(jwtSecret);
        var isTesting = environment.EnvironmentName == "Testing";

        // Fail-fast: validate JWT secret length at startup
        if (useJwt)
        {
            var secretBytes = Encoding.UTF8.GetBytes(jwtSecret!);
            if (secretBytes.Length < 32)
            {
                throw new InvalidOperationException(
                    "Jwt:Secret must be at least 32 bytes (256 bits) for HMAC-SHA256. " +
                    $"Current length: {secretBytes.Length} bytes.");
            }
        }

        var authBuilder = services.AddAuthentication(options =>
        {
            if (isTesting)
            {
                options.DefaultAuthenticateScheme = "DevScheme";
                options.DefaultChallengeScheme = "DevScheme";
            }
            else
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }
        });

        if (useJwt)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret!));

            authBuilder.AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt:Issuer"] ?? "orgs-api",
                    ValidateAudience = true,
                    ValidAudience = configuration["Jwt:Audience"] ?? "saas-template",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };

                // Read JWT from cookie
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.TryGetValue("access_token", out var token))
                        {
                            context.Token = token;
                        }
                        return Task.CompletedTask;
                    }
                };
            });
        }
        else
        {
            authBuilder.AddJwtBearer();
        }

        authBuilder.AddOAuthProviders(configuration, environment);

        if (isTesting)
        {
            authBuilder.AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, DevAuthenticationHandler>("DevScheme", options => { });
        }

        services.AddAuthorizationBuilder()
            .SetDefaultPolicy(new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build())
            .AddPolicy(AuthorizationPolicies.AdminOnly, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim(AuthClaimTypes.SystemAdmin, "true"));
    }

    public static void AddSwaggerGenWithAuth(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            // Security Definition
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            // Security Requirement
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header,
                    },
                    new List<string>()
                }
            });
        });
    }
}
