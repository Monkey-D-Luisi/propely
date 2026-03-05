// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Application.Actions.Dtos;

/// <summary>
/// Structured property data extracted from unstructured text or photos.
/// Each field is wrapped in <see cref="ExtractedField{T}"/> with a confidence score.
/// </summary>
public sealed record ExtractedPropertyDto
{
    public ExtractedField<string>? PropertyType { get; init; }
    public ExtractedField<string>? OperationType { get; init; }
    public ExtractedField<int?>? Bedrooms { get; init; }
    public ExtractedField<int?>? Bathrooms { get; init; }
    public ExtractedField<decimal?>? Price { get; init; }
    public ExtractedField<decimal?>? Area { get; init; }
    public ExtractedField<string>? City { get; init; }
    public ExtractedField<string>? Address { get; init; }
    public ExtractedField<string>? Description { get; init; }
    public ExtractedField<string>? Title { get; init; }
    public ExtractedField<List<string>>? Features { get; init; }
    public ExtractedField<int?>? Floor { get; init; }
    public ExtractedField<bool?>? HasElevator { get; init; }
    public ExtractedField<bool?>? HasParking { get; init; }
    public ExtractedField<bool?>? HasPool { get; init; }
    public ExtractedField<int?>? YearBuilt { get; init; }
}
