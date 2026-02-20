// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Text;
using System.Text.Json.Serialization;
using System.Reflection;
using Propely.ContactsApi.Api.Configuration;
using Propely.ContactsApi.Api.Middleware;
using Propely.ContactsApi.Api.Services;
using Propely.ContactsApi.Application.Common.Interfaces;
using AspNetCoreRateLimit;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Propely.ContactsApi.Api;

public static class DependencyInjection
{
    public static void AddApiServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddHttpContextAccessor();

        services.AddScoped<CorrelationIdAccessor>();
        services.AddScoped<ICorrelationIdAccessor>(sp => sp.GetRequiredService<CorrelationIdAccessor>());
        services.AddScoped<ITenantAccessor, HttpTenantAccessor>();

        AddCors(services, configuration);
        AddRateLimiting(services);

        services.AddTelemetry(configuration);
        services.AddApplicationHealthChecks(configuration);
        AddAuthenticationAndAuthorization(services, configuration, environment);

        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

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

    private static void AddRateLimiting(IServiceCollection services)
    {
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
                }
            ];
        });
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        services.AddInMemoryRateLimiting();
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

        if (useJwt && Encoding.UTF8.GetByteCount(jwtSecret!) < 32)
        {
            throw new InvalidOperationException(
                "Jwt:Secret must be at least 32 bytes (256 bits) for HS256. " +
                "Current length: " + Encoding.UTF8.GetByteCount(jwtSecret!) + " bytes.");
        }

        var authBuilder = services.AddAuthentication(options =>
        {
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

        if (allowAnonymous)
        {
            authBuilder.AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, DevAuthenticationHandler>("DevScheme", options => { });
        }

        services.AddAuthorizationBuilder()
            .SetDefaultPolicy(new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());
    }

    public static void AddSwaggerGenWithAuth(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

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
