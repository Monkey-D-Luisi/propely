// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AppointmentsApi.Application.Common.Interfaces;

namespace Propely.AppointmentsApi.Infrastructure.Caching.Configuration;

/// <summary>
/// Configuration options for Redis caching.
/// </summary>
public sealed class RedisConfiguration : ICacheSettings
{
    public const string SectionName = "Redis";

    public string ConnectionString { get; set; } = "localhost:6379";
    public bool Enabled { get; set; } = true;
    public int DefaultTtlMinutes { get; set; } = 5;
    public string KeyPrefix { get; set; } = "appointmentsapi:";
}
