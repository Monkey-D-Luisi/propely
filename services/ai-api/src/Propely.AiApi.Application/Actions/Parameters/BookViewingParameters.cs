// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Application.Actions.Parameters;

/// <summary>
/// Typed parameters for the BookViewing action.
/// </summary>
public sealed record BookViewingParameters(
    Guid? PropertyId,
    Guid? ContactId,
    DateTime? StartTime,
    DateTime? EndTime,
    string? Title,
    string? Location,
    string? Notes) : IActionParameters;
