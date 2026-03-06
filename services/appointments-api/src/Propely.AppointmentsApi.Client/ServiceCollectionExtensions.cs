// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Refit;

namespace Propely.AppointmentsApi.Client;

/// <summary>
/// Extension methods for registering the Appointments API SDK client in the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the <see cref="IAppointmentsApiClient"/> Refit client
    /// with tenant header propagation and Polly resilience policies (retry + circuit breaker).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Action to configure <see cref="AppointmentsApiClientOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAppointmentsApiClient(
        this IServiceCollection services,
        Action<AppointmentsApiClientOptions> configure)
    {
        var options = new AppointmentsApiClientOptions();
        configure(options);

        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddTransient<TenantDelegatingHandler>();

        // Register IAppointmentsApiClient
        services
            .AddRefitClient<IAppointmentsApiClient>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = System.Threading.Timeout.InfiniteTimeSpan;
            })
            .AddHttpMessageHandler<TenantDelegatingHandler>()
            .AddResilienceHandler("appointments-api", (builder, _) =>
            {
                ConfigureResilience(builder, options);
            });

        return services;
    }

    private static void ConfigureResilience(
        ResiliencePipelineBuilder<HttpResponseMessage> builder,
        AppointmentsApiClientOptions options)
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
            MinimumThroughput = options.CircuitBreakerMinimumThroughput,
            BreakDuration = options.CircuitBreakerDuration,
            ShouldHandle = args => ValueTask.FromResult(ShouldRetry(args.Outcome))
        });

        builder.AddTimeout(options.Timeout);
    }

    private static bool ShouldRetry(Outcome<HttpResponseMessage> outcome)
    {
        if (outcome.Exception is not null)
            return true;

        var statusCode = (int?)outcome.Result?.StatusCode;
        return statusCode is >= 500 or 408 or 429;
    }
}
