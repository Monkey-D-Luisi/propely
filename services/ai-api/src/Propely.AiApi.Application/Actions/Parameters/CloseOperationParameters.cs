// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Application.Actions.Parameters;

/// <summary>
/// Typed parameters for the CloseOperation action.
/// </summary>
public sealed record CloseOperationParameters(
    Guid? PropertyId,
    string? Reference,
    string? OperationType) : IActionParameters;
