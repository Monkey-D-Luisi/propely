// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.Application.Actions.Interfaces;

/// <summary>
/// Classifies user natural language input into a structured intent with action type and parameters.
/// Uses OpenAI function calling to determine which action the user wants to perform.
/// </summary>
public interface IIntentClassifier
{
    Task<ClassifiedIntent> ClassifyAsync(string text, CancellationToken ct = default);
}
