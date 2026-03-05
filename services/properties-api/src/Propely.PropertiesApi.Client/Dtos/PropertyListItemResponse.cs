// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.PropertiesApi.Client.Dtos;

/// <summary>
/// Lightweight property summary for list views.
/// </summary>
public sealed record PropertyListItemResponse
{
    /// <summary>The unique property identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>The property title.</summary>
    public string Title { get; init; } = null!;

    /// <summary>The property type (e.g., Apartment, House, Villa).</summary>
    public string PropertyType { get; init; } = null!;

    /// <summary>The operation type (e.g., Sale, Rent).</summary>
    public string OperationType { get; init; } = null!;

    /// <summary>The current property status.</summary>
    public string Status { get; init; } = null!;

    /// <summary>Property price in euros.</summary>
    public decimal? Price { get; init; }

    /// <summary>City where the property is located.</summary>
    public string? City { get; init; }

    /// <summary>Built area in square meters.</summary>
    public decimal? BuiltArea { get; init; }

    /// <summary>Number of bedrooms.</summary>
    public int? Bedrooms { get; init; }

    /// <summary>Number of bathrooms.</summary>
    public int? Bathrooms { get; init; }

    /// <summary>The assigned agent's user ID.</summary>
    public Guid AgentId { get; init; }

    /// <summary>Date the property was created (UTC).</summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>Date the property was last updated (UTC).</summary>
    public DateTime? UpdatedAtUtc { get; init; }
}
