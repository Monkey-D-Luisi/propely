// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Domain.Suggestions;
using Propely.AiApi.Application.Suggestions.Interfaces;
using Propely.ContactsApi.Client;

namespace Propely.AiApi.Application.Suggestions.Rules;

/// <summary>
/// Triggers when there are leads in "New" status that haven't been contacted.
/// Uses the count-by-status endpoint to check for stale leads.
/// </summary>
public sealed class StaleLeadsRule : ISuggestionRule
{
    private readonly ILeadsApiClient _leadsClient;
    private const int StaleThreshold = 5;

    public SuggestionType Type => SuggestionType.StaleLeads;

    public StaleLeadsRule(ILeadsApiClient leadsClient)
    {
        _leadsClient = leadsClient;
    }

    public async Task<IReadOnlyList<Suggestion>> EvaluateAsync(Guid tenantId, CancellationToken ct = default)
    {
        var counts = await _leadsClient.CountByStatusAsync(ct);

        counts.TryGetValue("New", out var newCount);

        if (newCount < StaleThreshold)
            return [];

        return
        [
            new Suggestion(
                SuggestionType.StaleLeads,
                SuggestionPriority.High,
                $"You have {newCount} new leads waiting to be contacted. Reaching out quickly improves conversion rates.",
                "/leads?status=New",
                "View new leads")
        ];
    }
}
