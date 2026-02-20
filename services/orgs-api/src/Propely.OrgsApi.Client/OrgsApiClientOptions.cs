// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Client;

/// <summary>
/// Configuration options for the Orgs API SDK client.
/// </summary>
public sealed class OrgsApiClientOptions
{
    /// <summary>
    /// Base URL of the Orgs API service.
    /// </summary>
    public string BaseUrl { get; set; } = "http://localhost:5020";

    /// <summary>
    /// HTTP request timeout.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Maximum number of retry attempts for transient failures.
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Number of consecutive failures before the circuit breaker opens.
    /// </summary>
    public int CircuitBreakerFailureThreshold { get; set; } = 5;

    /// <summary>
    /// Duration the circuit breaker stays open before transitioning to half-open.
    /// </summary>
    public TimeSpan CircuitBreakerDuration { get; set; } = TimeSpan.FromSeconds(30);
}
