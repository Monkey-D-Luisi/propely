// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Application.Actions.Models;
using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.Application.Actions.Interfaces;

/// <summary>
/// Classifies user natural language input into a structured intent with action type and parameters.
/// Provider-agnostic interface — implementations may use OpenAI, Claude, Gemini, or other LLMs.
/// </summary>
public interface IIntentClassifier
{
    Task<ClassifiedIntent> ClassifyAsync(string text, IReadOnlyList<ConversationExchange>? history = null, CancellationToken ct = default);
}
