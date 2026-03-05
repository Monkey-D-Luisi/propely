// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Client.Dtos;

/// <summary>
/// Structured property data extracted from unstructured text or photos.
/// Each field is wrapped in <see cref="ExtractedFieldDto{T}"/> with a confidence score.
/// </summary>
public sealed record ExtractedPropertyDto
{
    /// <summary>The property type (e.g., Apartment, House, Villa).</summary>
    public ExtractedFieldDto<string>? PropertyType { get; init; }

    /// <summary>The operation type (e.g., Sale, Rent).</summary>
    public ExtractedFieldDto<string>? OperationType { get; init; }

    /// <summary>Number of bedrooms.</summary>
    public ExtractedFieldDto<int?>? Bedrooms { get; init; }

    /// <summary>Number of bathrooms.</summary>
    public ExtractedFieldDto<int?>? Bathrooms { get; init; }

    /// <summary>Property price.</summary>
    public ExtractedFieldDto<decimal?>? Price { get; init; }

    /// <summary>Property area in square meters.</summary>
    public ExtractedFieldDto<decimal?>? Area { get; init; }

    /// <summary>City name.</summary>
    public ExtractedFieldDto<string>? City { get; init; }

    /// <summary>Street address.</summary>
    public ExtractedFieldDto<string>? Address { get; init; }

    /// <summary>Property description text.</summary>
    public ExtractedFieldDto<string>? Description { get; init; }

    /// <summary>Property title.</summary>
    public ExtractedFieldDto<string>? Title { get; init; }

    /// <summary>List of property features.</summary>
    public ExtractedFieldDto<List<string>>? Features { get; init; }

    /// <summary>Floor number.</summary>
    public ExtractedFieldDto<int?>? Floor { get; init; }

    /// <summary>Whether the property has an elevator.</summary>
    public ExtractedFieldDto<bool?>? HasElevator { get; init; }

    /// <summary>Whether the property has parking.</summary>
    public ExtractedFieldDto<bool?>? HasParking { get; init; }

    /// <summary>Whether the property has a pool.</summary>
    public ExtractedFieldDto<bool?>? HasPool { get; init; }

    /// <summary>Year the property was built.</summary>
    public ExtractedFieldDto<int?>? YearBuilt { get; init; }
}
