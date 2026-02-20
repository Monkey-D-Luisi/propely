// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Api.Services;

/// <summary>
/// Provides access to the current request's correlation ID.
/// </summary>
public interface ICorrelationIdAccessor
{
    /// <summary>
    /// Gets the correlation ID for the current request.
    /// Returns null if accessed outside of a request context.
    /// </summary>
    string? CorrelationId { get; }
}

/// <summary>
/// Scoped service that holds the correlation ID for the current request.
/// </summary>
public sealed class CorrelationIdAccessor : ICorrelationIdAccessor
{
    /// <summary>
    /// Gets or sets the correlation ID for the current request.
    /// </summary>
    public string? CorrelationId { get; set; }
}
