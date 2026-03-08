// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Propely.Shared.Http;
using Refit;

namespace Propely.PropertiesApi.Client;

/// <summary>
/// Extension methods for registering the Properties API SDK client in the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the <see cref="IPropertiesApiClient"/> Refit client
    /// with tenant header propagation and Polly resilience policies (retry + circuit breaker).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Action to configure <see cref="PropertiesApiClientOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPropertiesApiClient(
        this IServiceCollection services,
        Action<PropertiesApiClientOptions> configure)
    {
        var options = new PropertiesApiClientOptions();
        configure(options);

        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddTransient<TenantDelegatingHandler>();

        services
            .AddRefitClient<IPropertiesApiClient>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(100);
            })
            .AddHttpMessageHandler<TenantDelegatingHandler>()
            .AddResilienceHandler("properties-api", (builder, _) =>
            {
                builder.AddRetry(new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = options.RetryCount,
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    Delay = TimeSpan.FromMilliseconds(500),
                    ShouldHandle = args => ValueTask.FromResult(ShouldRetry(args.Outcome))
                });

                builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                {
                    FailureRatio = 0.5,
                    SamplingDuration = TimeSpan.FromSeconds(30),
                    MinimumThroughput = options.CircuitBreakerFailureThreshold,
                    BreakDuration = options.CircuitBreakerDuration,
                    ShouldHandle = args => ValueTask.FromResult(ShouldRetry(args.Outcome))
                });

                builder.AddTimeout(options.Timeout);
            });

        return services;
    }

    private static bool ShouldRetry(Outcome<HttpResponseMessage> outcome)
    {
        if (outcome.Exception is not null)
            return true;

        var statusCode = (int?)outcome.Result?.StatusCode;
        return statusCode is >= 500 or 408 or 429;
    }
}
