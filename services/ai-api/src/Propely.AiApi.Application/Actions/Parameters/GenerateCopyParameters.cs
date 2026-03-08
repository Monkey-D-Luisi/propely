// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Application.Actions.Parameters;

/// <summary>
/// Typed parameters for the GenerateCopy action.
/// </summary>
public sealed record GenerateCopyParameters(
    string? PropertyData,
    string? Tone,
    List<string>? Languages) : IActionParameters;
