// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Application.Actions.Parameters;

/// <summary>
/// Typed parameters for the CreateProperty action.
/// </summary>
public sealed record CreatePropertyParameters(
    string? Title,
    string? PropertyType,
    string? OperationType,
    int? Bedrooms,
    int? Bathrooms,
    decimal? AreaM2,
    decimal? Price,
    string? City,
    string? Description) : IActionParameters;
