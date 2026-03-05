// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Domain.Actions;

/// <summary>
/// Represents the result of intent classification from user natural language input.
/// Contains the classified action type, extracted parameters, and confidence score.
/// </summary>
public sealed record ClassifiedIntent(
    ActionType ActionType,
    Dictionary<string, object?> Parameters,
    double Confidence,
    string? RawFunctionName = null);
