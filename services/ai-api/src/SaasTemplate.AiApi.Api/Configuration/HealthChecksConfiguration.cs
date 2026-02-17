// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Text.Json;
using SaasTemplate.AiApi.Infrastructure.Messaging.Configuration;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using StackExchange.Redis;

namespace SaasTemplate.AiApi.Api.Configuration;

/// <summary>
/// Async health check for RabbitMQ that manages its own connection lifecycle.
/// Unlike a singleton IConnection, this gracefully reports Unhealthy when
/// RabbitMQ is unreachable instead of crashing the DI container at startup.
/// </summary>
public sealed class RabbitMqHealthCheck : IHealthCheck, IDisposable
{
    private readonly RabbitMqConfiguration _config;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private IConnection? _connection;

    public RabbitMqHealthCheck(IOptions<RabbitMqConfiguration> config)
    {
        _config = config.Value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (_connection is { IsOpen: true })
            {
                return HealthCheckResult.Healthy("RabbitMQ connection is open.");
            }

            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                // Double-check after acquiring lock
                if (_connection is { IsOpen: true })
                {
                    return HealthCheckResult.Healthy("RabbitMQ connection is open.");
                }

                // Clean up stale connection
                if (_connection is not null)
                {
                    try { await _connection.CloseAsync(cancellationToken); } catch { /* best effort */ }
                    _connection = null;
                }

                var factory = new ConnectionFactory
                {
                    HostName = _config.Host,
                    Port = _config.Port,
                    UserName = _config.Username,
                    Password = _config.Password,
                    VirtualHost = _config.VirtualHost,
                    RequestedConnectionTimeout = TimeSpan.FromSeconds(5),
                    Ssl = _config.UseSsl
                        ? new SslOption { Enabled = true, ServerName = _config.Host }
                        : new SslOption { Enabled = false }
                };

                _connection = await factory.CreateConnectionAsync(cancellationToken);
                return HealthCheckResult.Healthy("RabbitMQ connection is open.");
            }
            finally
            {
                _semaphore.Release();
            }
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("RabbitMQ connection failed.", ex);
        }
    }

    public void Dispose()
    {
        try { _connection?.Dispose(); } catch { /* best effort */ }
        _semaphore.Dispose();
    }
}

/// <summary>
/// Extension methods for configuring health checks.
/// </summary>
public static class HealthChecksConfiguration
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    /// <summary>
    /// Adds health checks for application dependencies.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApplicationHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var healthChecksBuilder = services.AddHealthChecks();

        // Liveness check - always healthy if the process is running
        healthChecksBuilder.AddCheck(
            "self",
            () => HealthCheckResult.Healthy(),
            tags: ["live"]);

        // PostgreSQL readiness check
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (!string.IsNullOrEmpty(connectionString))
        {
            healthChecksBuilder.AddNpgSql(
                connectionString,
                name: "postgresql",
                tags: ["ready", "db"]);
        }

        // Redis readiness check
        var redisConnectionString = configuration["Redis:ConnectionString"];

        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            healthChecksBuilder.AddRedis(
                sp =>
                {
                    var options = ConfigurationOptions.Parse(redisConnectionString);
                    options.AbortOnConnectFail = false;
                    // Trust Google Memorystore CA cert (private VPC traffic, not publicly trusted)
                    if (options.Ssl)
                    {
                        options.CertificateValidation += (_, _, _, _) => true;
                    }
                    return ConnectionMultiplexer.Connect(options);
                },
                name: "redis",
                tags: ["ready", "cache"]);
        }

        // RabbitMQ readiness check — uses IOptions<RabbitMqConfiguration> (registered
        // by Infrastructure layer) instead of a singleton IConnection, so connection
        // failures are reported as Unhealthy instead of crashing the DI container.
        var rabbitHost = configuration["RabbitMQ:Host"];
        var rabbitPort = configuration.GetValue<int?>("RabbitMQ:Port");

        if (!string.IsNullOrEmpty(rabbitHost) && rabbitPort.HasValue)
        {
            healthChecksBuilder.AddCheck<RabbitMqHealthCheck>(
                "rabbitmq",
                tags: ["ready", "messaging"]);
        }

        return services;
    }

    /// <summary>
    /// Maps health check endpoints.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseHealthCheckEndpoints(this IApplicationBuilder app)
    {
        // Liveness endpoint - only checks if process is alive
        app.UseHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live"),
            ResponseWriter = WriteHealthCheckResponse
        });

        // Readiness endpoint - checks all dependencies
        app.UseHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = WriteHealthCheckResponse
        });

        return app;
    }

    private static async Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            entries = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration.TotalMilliseconds,
                description = e.Value.Description,
                exception = e.Value.Exception?.Message
            })
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}
