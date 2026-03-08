// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Domain.Suggestions;
using Propely.AiApi.Application.Suggestions.Interfaces;
using Propely.AppointmentsApi.Client;

namespace Propely.AiApi.Application.Suggestions.Rules;

/// <summary>
/// Triggers when the agent has no upcoming appointments in the next 7 days.
/// </summary>
public sealed class EmptyCalendarRule : ISuggestionRule
{
    private readonly IAppointmentsApiClient _appointmentsClient;

    public SuggestionType Type => SuggestionType.EmptyCalendar;

    public EmptyCalendarRule(IAppointmentsApiClient appointmentsClient)
    {
        _appointmentsClient = appointmentsClient;
    }

    public async Task<IReadOnlyList<Suggestion>> EvaluateAsync(Guid tenantId, CancellationToken ct = default)
    {
        var result = await _appointmentsClient.CountUpcomingAsync(7, ct);

        if (result.Count > 0)
            return [];

        return
        [
            new Suggestion(
                SuggestionType.EmptyCalendar,
                SuggestionPriority.Medium,
                "You have no appointments scheduled for the next 7 days. Consider scheduling property viewings with your leads.",
                "/appointments",
                "View calendar")
        ];
    }
}
