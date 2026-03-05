// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Client.Dtos;

/// <summary>
/// Generic wrapper for an extracted field value with a confidence score (0.0 to 1.0).
/// Used by AI extraction results to indicate certainty of each extracted data point.
/// </summary>
/// <typeparam name="T">The type of the extracted value.</typeparam>
public sealed record ExtractedFieldDto<T>
{
    /// <summary>The extracted value.</summary>
    public T? Value { get; init; }

    /// <summary>Confidence score for the extraction (0.0 to 1.0).</summary>
    public double Confidence { get; init; }
}
