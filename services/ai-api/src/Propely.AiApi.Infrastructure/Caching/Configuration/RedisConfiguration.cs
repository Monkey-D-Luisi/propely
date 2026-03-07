// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Application.Common.Interfaces;

namespace Propely.AiApi.Infrastructure.Caching.Configuration;

/// <summary>
/// Configuration options for Redis caching.
/// </summary>
public sealed class RedisConfiguration : ICacheSettings
{
    /// <summary>
    /// The configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "Redis";

    /// <summary>
    /// The Redis connection string.
    /// </summary>
    public string ConnectionString { get; set; } = "localhost:6379";

    /// <summary>
    /// Whether Redis caching is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Default TTL for cached items in minutes.
    /// </summary>
    public int DefaultTtlMinutes { get; set; } = 5;

    /// <summary>
    /// Key prefix for all cache keys.
    /// </summary>
    public string KeyPrefix { get; set; } = "aiapi:";

    /// <summary>
    /// Whether to validate SSL certificates when connecting to Redis over TLS.
    /// Defaults to true. Set to false only in Development/Testing environments
    /// (e.g., when using self-signed certs or Google Memorystore without public CA).
    /// </summary>
    public bool SslCertValidation { get; set; } = true;
}
