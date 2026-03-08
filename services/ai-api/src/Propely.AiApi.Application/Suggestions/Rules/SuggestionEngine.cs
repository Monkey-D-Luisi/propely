// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Domain.Suggestions;
using Propely.AiApi.Application.Suggestions.Interfaces;
using Microsoft.Extensions.Logging;

namespace Propely.AiApi.Application.Suggestions.Rules;

/// <summary>
/// Orchestrates all suggestion rules and returns aggregated, deduplicated results.
/// </summary>
public sealed class SuggestionEngine
{
    private readonly IEnumerable<ISuggestionRule> _rules;
    private readonly ILogger<SuggestionEngine> _logger;

    public SuggestionEngine(IEnumerable<ISuggestionRule> rules, ILogger<SuggestionEngine> logger)
    {
        _rules = rules;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Suggestion>> GenerateAsync(Guid tenantId, CancellationToken ct = default)
    {
        var allSuggestions = new List<Suggestion>();

        foreach (var rule in _rules)
        {
            try
            {
                var suggestions = await rule.EvaluateAsync(tenantId, ct);
                allSuggestions.AddRange(suggestions);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Suggestion rule {RuleType} failed for tenant {TenantId}", rule.Type, tenantId);
            }
        }

        return allSuggestions
            .OrderByDescending(s => s.Priority)
            .ThenBy(s => s.Type)
            .ToList();
    }
}
