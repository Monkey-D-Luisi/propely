// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Refit;

namespace Propely.AiApi.Client.Configuration;

/// <summary>
/// Extension methods for registering the AI API SDK client in the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the <see cref="IActionApi"/>, <see cref="IVoiceApi"/>, and <see cref="IContentApi"/>
    /// Refit clients with tenant header propagation and a Polly timeout policy.
    /// AI calls are NOT retried because they are not idempotent.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Action to configure <see cref="AiApiClientOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAiApiClient(
        this IServiceCollection services,
        Action<AiApiClientOptions> configure)
    {
        var options = new AiApiClientOptions();
        configure(options);

        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddTransient<TenantDelegatingHandler>();

        // Register IActionApi
        services
            .AddRefitClient<IActionApi>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = System.Threading.Timeout.InfiniteTimeSpan;
            })
            .AddHttpMessageHandler<TenantDelegatingHandler>()
            .AddResilienceHandler("ai-api-actions", (builder, _) =>
            {
                builder.AddTimeout(options.Timeout);
            });

        // Register IVoiceApi
        services
            .AddRefitClient<IVoiceApi>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = System.Threading.Timeout.InfiniteTimeSpan;
            })
            .AddHttpMessageHandler<TenantDelegatingHandler>()
            .AddResilienceHandler("ai-api-voice", (builder, _) =>
            {
                builder.AddTimeout(options.Timeout);
            });

        // Register IContentApi
        services
            .AddRefitClient<IContentApi>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = System.Threading.Timeout.InfiniteTimeSpan;
            })
            .AddHttpMessageHandler<TenantDelegatingHandler>()
            .AddResilienceHandler("ai-api-content", (builder, _) =>
            {
                builder.AddTimeout(options.Timeout);
            });

        return services;
    }
}
