// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Application.Actions.Dtos;

/// <summary>
/// Generic wrapper for an extracted field value with a confidence score (0.0 to 1.0).
/// Used by AI extraction handlers to indicate certainty of each extracted data point.
/// </summary>
public sealed record ExtractedField<T>(T? Value, double Confidence);
