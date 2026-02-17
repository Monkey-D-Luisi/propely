// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace SaasTemplate.OrgsApi.Api.Configuration;

/// <summary>
/// Extension methods for configuring OpenTelemetry tracing and metrics.
/// </summary>
public static class TelemetryConfiguration
{
    /// <summary>
    /// The service name used for telemetry identification.
    /// </summary>
    public const string ServiceName = "SaasTemplate.OrgsApi";

    /// <summary>
    /// Adds OpenTelemetry tracing and metrics to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddTelemetry(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var otlpEndpoint = configuration["OpenTelemetry:OtlpEndpoint"];

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: ServiceName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation();

                // Add console exporter for development
                if (configuration.GetValue<bool>("OpenTelemetry:UseConsoleExporter"))
                {
                    tracing.AddConsoleExporter();
                }

                // Add OTLP exporter if endpoint is configured
                if (!string.IsNullOrEmpty(otlpEndpoint))
                {
                    tracing.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                    });
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();

                // Add console exporter for development
                if (configuration.GetValue<bool>("OpenTelemetry:UseConsoleExporter"))
                {
                    metrics.AddConsoleExporter();
                }

                // Add OTLP exporter if endpoint is configured
                if (!string.IsNullOrEmpty(otlpEndpoint))
                {
                    metrics.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                    });
                }
            });

        return services;
    }
}
