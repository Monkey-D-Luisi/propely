// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Text;
using System.Text.Json.Serialization;
using System.Reflection;
using Propely.AiApi.Api.Configuration;
using Propely.AiApi.Api.Middleware;
using Propely.AiApi.Api.Services;
using Propely.AiApi.Application.Common.Interfaces;
using AspNetCoreRateLimit;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Propely.AiApi.Api;

public static class DependencyInjection
{
    public static void AddApiServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        // FluentValidation for API DTOs
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // HTTP context accessor (required by HttpTenantAccessor)
        services.AddHttpContextAccessor();

        // Correlation ID accessor (scoped per request)
        services.AddScoped<CorrelationIdAccessor>();
        services.AddScoped<ICorrelationIdAccessor>(sp => sp.GetRequiredService<CorrelationIdAccessor>());

        // Tenant accessor (scoped per request - resolves OrgId from JWT claims)
        services.AddScoped<ITenantAccessor, HttpTenantAccessor>();

        // CORS
        AddCors(services, configuration);

        // Rate limiting
        services.AddMemoryCache();
        services.Configure<IpRateLimitOptions>(options =>
        {
            options.EnableEndpointRateLimiting = true;
            options.StackBlockedRequests = false;
            options.RealIpHeader = "X-Real-IP";
            options.ClientIdHeader = "X-ClientId";
            options.GeneralRules =
            [
                new RateLimitRule
                {
                    Endpoint = "*",
                    Period = "1m",
                    Limit = 100
                },
                new RateLimitRule
                {
                    Endpoint = "POST:/v1/work-items/parse",
                    Period = "1m",
                    Limit = 10  // Stricter limit for AI-powered endpoint
                }
            ];
        });
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        services.AddInMemoryRateLimiting();

        // OpenTelemetry tracing and metrics
        services.AddTelemetry(configuration);

        // Health checks
        services.AddApplicationHealthChecks(configuration);

        // Authentication & Authorization
        AddAuthenticationAndAuthorization(services, configuration, environment);

        // Controllers
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        // Swagger / OpenAPI (available in development)
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGenWithAuth();
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
        var allowAnonymous = configuration.GetValue<bool>("Security:AllowAnonymous");
        var isDevelopmentOrTesting = environment.IsDevelopment() || environment.EnvironmentName == "Testing";

        if (allowAnonymous && !isDevelopmentOrTesting)
        {
            throw new InvalidOperationException("Security:AllowAnonymous can only be set to true in Development or Testing environments.");
        }

        var jwtSecret = configuration["Jwt:Secret"];
        var useJwt = !string.IsNullOrEmpty(jwtSecret);

        // P1: Validate JWT secret meets minimum length requirement (256 bits = 32 bytes)
        if (useJwt && Encoding.UTF8.GetByteCount(jwtSecret!) < 32)
        {
            throw new InvalidOperationException(
                "Jwt:Secret must be at least 32 bytes (256 bits) for HS256. " +
                "Current length: " + Encoding.UTF8.GetByteCount(jwtSecret!) + " bytes.");
        }

        var authBuilder = services.AddAuthentication(options =>
        {
            // If anonymous access is allowed (Dev/Test), use the DevScheme by default
            if (allowAnonymous)
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

        // JWT Bearer authentication with cookie support (mirrors orgs-api)
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
                    ValidAudience = configuration["Jwt:Audience"] ?? "propely",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };

                // Read JWT from cookie (shared with orgs-api)
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

        // Register Dev Scheme if allowed
        if (allowAnonymous)
        {
            authBuilder.AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, DevAuthenticationHandler>("DevScheme", options => { });
        }

        services.AddAuthorizationBuilder()
            .SetDefaultPolicy(new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build())
            // Named authorization policies.
            // Currently all require only an authenticated user because the JWT issued by
            // orgs-api does not include a "role" claim. When role-based access is needed,
            // add RequireClaim("role", ...) here and update JwtTokenService to emit it.
            .AddPolicy(AuthorizationPolicies.CanCreateWorkItem, policy => policy
                .RequireAuthenticatedUser())
            .AddPolicy(AuthorizationPolicies.CanReadWorkItem, policy => policy
                .RequireAuthenticatedUser())
            .AddPolicy(AuthorizationPolicies.CanUpdateWorkItem, policy => policy
                .RequireAuthenticatedUser())
            .AddPolicy(AuthorizationPolicies.CanDeleteWorkItem, policy => policy
                .RequireAuthenticatedUser());
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
