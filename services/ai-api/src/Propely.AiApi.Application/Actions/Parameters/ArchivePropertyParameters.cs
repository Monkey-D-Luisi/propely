// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Application.Actions.Parameters;

/// <summary>
/// Typed parameters for the ArchiveProperty action.
/// </summary>
public sealed record ArchivePropertyParameters(
    Guid? PropertyId,
    string? Reference) : IActionParameters;
