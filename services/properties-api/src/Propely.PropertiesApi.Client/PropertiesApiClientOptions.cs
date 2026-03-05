// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.PropertiesApi.Client;

/// <summary>
/// Configuration options for the Properties API SDK client.
/// </summary>
public sealed class PropertiesApiClientOptions
{
    /// <summary>
    /// Base URL of the Properties API service.
    /// </summary>
    public string BaseUrl { get; set; } = "http://localhost:5030";

    /// <summary>
    /// HTTP request timeout.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Maximum number of retry attempts for transient failures.
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Minimum number of requests in the sampling window before the circuit breaker evaluates the failure ratio.
    /// </summary>
    public int CircuitBreakerFailureThreshold { get; set; } = 5;

    /// <summary>
    /// Duration the circuit breaker stays open before transitioning to half-open.
    /// </summary>
    public TimeSpan CircuitBreakerDuration { get; set; } = TimeSpan.FromSeconds(30);
}
