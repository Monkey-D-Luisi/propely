// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Domain.Suggestions;
using Propely.AiApi.Application.Suggestions.Interfaces;
using Propely.ContactsApi.Client;

namespace Propely.AiApi.Application.Suggestions.Rules;

/// <summary>
/// Triggers when the lead conversion rate is below 20%.
/// Conversion rate = Converted / (Converted + Lost + Qualified + Contacted + New).
/// </summary>
public sealed class LowConversionRule : ISuggestionRule
{
    private readonly ILeadsApiClient _leadsClient;
    private const double ConversionThreshold = 0.20;
    private const int MinSampleSize = 5;

    public SuggestionType Type => SuggestionType.LowConversion;

    public LowConversionRule(ILeadsApiClient leadsClient)
    {
        _leadsClient = leadsClient;
    }

    public async Task<IReadOnlyList<Suggestion>> EvaluateAsync(Guid tenantId, CancellationToken ct = default)
    {
        var counts = await _leadsClient.CountByStatusAsync(ct);

        counts.TryGetValue("Converted", out var converted);
        counts.TryGetValue("Lost", out var lost);
        counts.TryGetValue("New", out var newLeads);
        counts.TryGetValue("Contacted", out var contacted);
        counts.TryGetValue("Qualified", out var qualified);

        var total = converted + lost + newLeads + contacted + qualified;

        if (total < MinSampleSize)
            return [];

        var rate = (double)converted / total;

        if (rate >= ConversionThreshold)
            return [];

        var pct = (int)(rate * 100);
        return
        [
            new Suggestion(
                SuggestionType.LowConversion,
                SuggestionPriority.High,
                $"Your lead conversion rate is {pct}%, below the target of 20%. Focus on qualifying and following up with your most promising leads.",
                "/leads",
                "Review leads")
        ];
    }
}
