// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.PropertiesApi.Client.Dtos;

/// <summary>
/// Represents a paginated result set from the Properties API.
/// </summary>
/// <typeparam name="T">The type of items in the result.</typeparam>
public sealed class PagedResult<T>
{
    /// <summary>The items in the current page.</summary>
    public IReadOnlyList<T> Items { get; init; } = [];

    /// <summary>The current page number (1-based).</summary>
    public int PageNumber { get; init; }

    /// <summary>The total number of pages.</summary>
    public int TotalPages { get; init; }

    /// <summary>The total number of items across all pages.</summary>
    public int TotalCount { get; init; }

    /// <summary>Whether there is a previous page.</summary>
    public bool HasPreviousPage { get; init; }

    /// <summary>Whether there is a next page.</summary>
    public bool HasNextPage { get; init; }
}
