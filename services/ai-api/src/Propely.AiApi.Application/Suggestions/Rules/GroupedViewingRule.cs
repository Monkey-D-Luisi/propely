// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Domain.Suggestions;
using Propely.AiApi.Application.Suggestions.Interfaces;
using Propely.ContactsApi.Client;

namespace Propely.AiApi.Application.Suggestions.Rules;

/// <summary>
/// Triggers when a contact has property interests in 3+ properties, suggesting a grouped viewing.
/// </summary>
public sealed class GroupedViewingRule : ISuggestionRule
{
    private readonly IContactsApiClient _contactsClient;
    private const int InterestThreshold = 3;

    public SuggestionType Type => SuggestionType.GroupedViewing;

    public GroupedViewingRule(IContactsApiClient contactsClient)
    {
        _contactsClient = contactsClient;
    }

    public async Task<IReadOnlyList<Suggestion>> EvaluateAsync(Guid tenantId, CancellationToken ct = default)
    {
        var contacts = await _contactsClient.GetContactsAsync(pageSize: 100, ct: ct);

        var suggestions = new List<Suggestion>();

        foreach (var contact in contacts.Items)
        {
            if (contact.PropertyInterests is null || contact.PropertyInterests.Count < InterestThreshold)
                continue;

            suggestions.Add(new Suggestion(
                SuggestionType.GroupedViewing,
                SuggestionPriority.Medium,
                $"{contact.FirstName} {contact.LastName} is interested in {contact.PropertyInterests.Count} properties. Consider scheduling a grouped viewing.",
                $"/contacts/{contact.Id}",
                "View contact"));
        }

        return suggestions;
    }
}
