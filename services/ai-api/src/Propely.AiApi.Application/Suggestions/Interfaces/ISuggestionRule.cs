// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Domain.Suggestions;

namespace Propely.AiApi.Application.Suggestions.Interfaces;

/// <summary>
/// Interface for a suggestion rule that evaluates live data and produces actionable suggestions.
/// </summary>
public interface ISuggestionRule
{
    SuggestionType Type { get; }

    Task<IReadOnlyList<Suggestion>> EvaluateAsync(Guid tenantId, CancellationToken ct = default);
}
