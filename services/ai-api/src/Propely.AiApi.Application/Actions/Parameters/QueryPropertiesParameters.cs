// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Application.Actions.Parameters;

/// <summary>
/// Typed parameters for the QueryProperties action.
/// </summary>
public sealed record QueryPropertiesParameters(
    string? PropertyType,
    string? OperationType,
    string? Status,
    string? City,
    decimal? MinPrice,
    decimal? MaxPrice,
    int? MinBedrooms) : IActionParameters;
