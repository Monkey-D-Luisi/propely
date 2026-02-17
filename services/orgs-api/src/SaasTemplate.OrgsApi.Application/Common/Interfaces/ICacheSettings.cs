// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace SaasTemplate.OrgsApi.Application.Common.Interfaces;

/// <summary>
/// Provides cache configuration settings.
/// </summary>
public interface ICacheSettings
{
    /// <summary>
    /// Gets the default cache TTL in minutes.
    /// </summary>
    int DefaultTtlMinutes { get; }
}
