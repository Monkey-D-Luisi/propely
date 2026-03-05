// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using Propely.AiApi.Application.Actions.Commands.Content;
using Propely.AiApi.Application.Actions.Dtos;
using Propely.AiApi.Application.Actions.Helpers;
using Propely.AiApi.Application.Common.Interfaces;
using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.Application.Actions.Handlers;

/// <summary>
/// Handles generation of marketing copy for property listings in multiple languages.
/// Validates tone and language parameters, calls IOpenAiService.GenerateWithJsonResponseAsync,
/// and parses the JSON response into a <see cref="GeneratedCopyDto"/> with language variants.
/// </summary>
public sealed class GenerateCopyActionHandler : IRequestHandler<GenerateCopyActionCommand, ActionResult>
{
    internal static readonly string[] ValidTones = ["professional", "luxury", "casual", "concise"];
    internal static readonly string[] ValidLanguages = ["es", "en", "fr", "de", "nl"];
    private const string DefaultTone = "professional";

    private readonly IOpenAiService _openAiService;
    private readonly ILogger<GenerateCopyActionHandler> _logger;

    public GenerateCopyActionHandler(IOpenAiService openAiService, ILogger<GenerateCopyActionHandler> logger)
    {
        _openAiService = openAiService;
        _logger = logger;
    }

    public async Task<ActionResult> Handle(GenerateCopyActionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling GenerateCopy action for tenant {TenantId} by agent {AgentId}",
            request.TenantId, request.AgentId);

        var propertyData = ParameterExtractor.GetString(request.Parameters, "property_data");
        var tone = ParameterExtractor.GetString(request.Parameters, "tone")?.ToLowerInvariant() ?? DefaultTone;
        var languages = ParameterExtractor.GetStringList(request.Parameters, "languages");

        if (string.IsNullOrWhiteSpace(propertyData))
        {
            return ActionResult.Fail(
                ["Property data is required. Please provide a description of the property to generate copy for."],
                ActionType.GenerateCopy,
                "I need some property information to generate marketing copy. Please describe the property.");
        }

        if (!ValidTones.Contains(tone))
        {
            return ActionResult.Fail(
                [$"Invalid tone '{tone}'. Valid options are: {string.Join(", ", ValidTones)}."],
                ActionType.GenerateCopy,
                $"The tone '{tone}' is not supported. Please choose from: {string.Join(", ", ValidTones)}.");
        }

        var targetLanguages = languages != null && languages.Count > 0
            ? languages.Where(l => ValidLanguages.Contains(l.ToLowerInvariant())).ToList()
            : ValidLanguages.ToList();

        if (targetLanguages.Count == 0)
        {
            targetLanguages = ValidLanguages.ToList();
        }

        try
        {
            var systemPrompt = BuildSystemPrompt(tone);
            var userPrompt = BuildUserPrompt(propertyData, targetLanguages);

            var response = await _openAiService.GenerateWithJsonResponseAsync(
                systemPrompt, userPrompt, cancellationToken);

            if (string.IsNullOrWhiteSpace(response))
            {
                return ActionResult.Fail(
                    ["AI service returned an empty response."],
                    ActionType.GenerateCopy,
                    "I was unable to generate marketing copy. Please try again.");
            }

            var copy = ParseCopyResponse(response, tone);

            _logger.LogInformation(
                "GenerateCopy completed for tenant {TenantId}: {LanguageCount} language variants with tone {Tone}",
                request.TenantId, copy.Variants.Count, tone);

            return ActionResult.Ok(
                data: copy,
                message: $"I generated {copy.Variants.Count} language variant{(copy.Variants.Count > 1 ? "s" : "")} of marketing copy with a {tone} tone.",
                type: ActionType.GenerateCopy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GenerateCopy failed for tenant {TenantId}", request.TenantId);

            return ActionResult.Fail(
                [$"Failed to generate copy: {ex.Message}"],
                ActionType.GenerateCopy,
                "I encountered an error while generating marketing copy. Please try again.");
        }
    }

    internal static string BuildSystemPrompt(string tone) => $$"""
        You are a real estate marketing copywriter. Your task is to generate compelling property listing descriptions.

        Tone: {{tone}}
        {{GetToneGuidance(tone)}}

        Rules:
        - Write engaging, accurate marketing copy based on the provided property data
        - Each language variant should be a natural, native-quality translation (not a literal translation)
        - Adapt cultural references and phrasing to each target language and market
        - Keep descriptions between 100-300 words per language variant
        - Highlight key selling points: location, size, features, condition
        - Do not invent features or amenities not present in the property data
        - Always respond with valid JSON only

        Return a JSON object with this structure:
        {
            "variants": {
                "es": "Texto en espanol...",
                "en": "English text...",
                "fr": "Texte en francais...",
                "de": "Deutscher Text...",
                "nl": "Nederlandse tekst..."
            },
            "tone": "{{tone}}"
        }

        Only include language variants that were requested.
        """;

    internal static string BuildUserPrompt(string propertyData, List<string> languages)
    {
        var languageList = string.Join(", ", languages);
        return $"""
            Generate marketing copy for the following property in these languages: {languageList}

            Property information:
            {propertyData}
            """;
    }

    private static string GetToneGuidance(string tone) => tone switch
    {
        "professional" => """
            Professional tone guidance:
            - Use formal, business-appropriate language
            - Focus on facts, specifications, and investment value
            - Emphasize location advantages, build quality, and market positioning
            """,
        "luxury" => """
            Luxury tone guidance:
            - Use aspirational, evocative language
            - Emphasize exclusivity, prestige, and lifestyle
            - Paint a picture of refined living and exceptional quality
            - Use sensory descriptions: views, light, materials, finishes
            """,
        "casual" => """
            Casual tone guidance:
            - Use warm, friendly, conversational language
            - Make the reader feel at home
            - Highlight livability, neighborhood charm, and daily life benefits
            """,
        "concise" => """
            Concise tone guidance:
            - Be brief and to the point
            - Lead with the most important selling points
            - Maximum 100 words per language variant
            """,
        _ => "Use a balanced, professional tone."
    };

    internal static GeneratedCopyDto ParseCopyResponse(string json, string requestedTone)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var variants = new Dictionary<string, string>();

        if (root.TryGetProperty("variants", out var variantsElement))
        {
            foreach (var property in variantsElement.EnumerateObject())
            {
                var text = property.Value.GetString();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    variants[property.Name] = text;
                }
            }
        }

        var tone = root.TryGetProperty("tone", out var toneElement)
            ? toneElement.GetString() ?? requestedTone
            : requestedTone;

        return new GeneratedCopyDto
        {
            Variants = variants,
            Tone = tone
        };
    }
}
