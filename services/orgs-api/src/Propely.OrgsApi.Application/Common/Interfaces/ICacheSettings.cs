// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Application.Common.Interfaces;

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
