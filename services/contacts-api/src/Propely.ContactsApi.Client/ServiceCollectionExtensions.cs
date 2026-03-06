// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Refit;

namespace Propely.ContactsApi.Client;

/// <summary>
/// Extension methods for registering the Contacts API SDK client in the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the <see cref="IContactsApiClient"/> and <see cref="ILeadsApiClient"/> Refit clients
    /// with tenant header propagation and Polly resilience policies (retry + circuit breaker).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Action to configure <see cref="ContactsApiClientOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddContactsApiClient(
        this IServiceCollection services,
        Action<ContactsApiClientOptions> configure)
    {
        var options = new ContactsApiClientOptions();
        configure(options);

        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddTransient<TenantDelegatingHandler>();

        // Register IContactsApiClient
        services
            .AddRefitClient<IContactsApiClient>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = System.Threading.Timeout.InfiniteTimeSpan;
            })
            .AddHttpMessageHandler<TenantDelegatingHandler>()
            .AddResilienceHandler("contacts-api", (builder, _) =>
            {
                ConfigureResilience(builder, options);
            });

        // Register ILeadsApiClient
        services
            .AddRefitClient<ILeadsApiClient>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = System.Threading.Timeout.InfiniteTimeSpan;
            })
            .AddHttpMessageHandler<TenantDelegatingHandler>()
            .AddResilienceHandler("leads-api", (builder, _) =>
            {
                ConfigureResilience(builder, options);
            });

        return services;
    }

    private static void ConfigureResilience(
        ResiliencePipelineBuilder<HttpResponseMessage> builder,
        ContactsApiClientOptions options)
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
