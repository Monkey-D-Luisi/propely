// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Domain.Suggestions;
using Propely.AiApi.Application.Suggestions.Interfaces;
using Propely.PropertiesApi.Client;

namespace Propely.AiApi.Application.Suggestions.Rules;

/// <summary>
/// Triggers when there are properties stuck in Draft status.
/// </summary>
public sealed class DraftPropertyRule : ISuggestionRule
{
    private readonly IPropertiesApiClient _propertiesClient;
    private const int DraftThreshold = 3;

    public SuggestionType Type => SuggestionType.DraftProperty;

    public DraftPropertyRule(IPropertiesApiClient propertiesClient)
    {
        _propertiesClient = propertiesClient;
    }

    public async Task<IReadOnlyList<Suggestion>> EvaluateAsync(Guid tenantId, CancellationToken ct = default)
    {
        var counts = await _propertiesClient.CountByStatusAsync(ct);

        counts.TryGetValue("Draft", out var draftCount);

        if (draftCount < DraftThreshold)
            return [];

        return
        [
            new Suggestion(
                SuggestionType.DraftProperty,
                SuggestionPriority.Medium,
                $"You have {draftCount} properties in Draft status. Consider activating them to start receiving leads.",
                "/properties?status=Draft",
                "View draft properties")
        ];
    }
}
