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
/// Handles extraction of structured property data from unstructured text.
/// Validates input text length (max 10,000 characters), calls IOpenAiService
/// with the property extraction prompt, and parses the JSON response into
/// an <see cref="ExtractedPropertyDto"/> with per-field confidence scores.
/// </summary>
public sealed class ExtractFromTextActionHandler : IRequestHandler<ExtractFromTextActionCommand, ActionResult>
{
    private const int MaxTextLength = 10_000;

    internal const string PropertyExtractionSystemPrompt = """
        You are a real estate data extraction specialist. Your task is to extract structured property data from unstructured text.

        Analyze the provided text and extract as many property fields as possible. For each field, provide the extracted value and a confidence score between 0.0 and 1.0 indicating how certain you are about the extraction.

        Return a JSON object with the following structure. Only include fields that you can extract from the text. Do not invent or guess values — if a field is not present in the text, omit it entirely.

        {
            "property_type": { "value": "apartment|house|villa|studio|penthouse|duplex|commercial|land|garage|storage", "confidence": 0.95 },
            "operation_type": { "value": "sale|rent|transfer", "confidence": 0.9 },
            "bedrooms": { "value": 3, "confidence": 0.85 },
            "bathrooms": { "value": 2, "confidence": 0.8 },
            "price": { "value": 250000.00, "confidence": 0.9 },
            "area": { "value": 120.5, "confidence": 0.85 },
            "city": { "value": "Malaga", "confidence": 0.95 },
            "address": { "value": "Calle Mayor 15", "confidence": 0.7 },
            "description": { "value": "Spacious apartment with sea views", "confidence": 0.9 },
            "title": { "value": "Sea-view apartment in Malaga center", "confidence": 0.8 },
            "features": { "value": ["terrace", "sea view", "air conditioning"], "confidence": 0.85 },
            "floor": { "value": 5, "confidence": 0.9 },
            "has_elevator": { "value": true, "confidence": 0.7 },
            "has_parking": { "value": true, "confidence": 0.8 },
            "has_pool": { "value": false, "confidence": 0.6 },
            "year_built": { "value": 2015, "confidence": 0.75 }
        }

        Rules:
        - Confidence 0.9-1.0: Explicitly stated in text
        - Confidence 0.7-0.89: Strongly implied or inferable
        - Confidence 0.5-0.69: Somewhat uncertain, derived from context
        - Confidence below 0.5: Do not include the field
        - Prices should be in EUR unless another currency is explicitly mentioned
        - Area should be in square meters
        - Always respond with valid JSON only
        """;

    private readonly IOpenAiService _openAiService;
    private readonly ILogger<ExtractFromTextActionHandler> _logger;

    public ExtractFromTextActionHandler(IOpenAiService openAiService, ILogger<ExtractFromTextActionHandler> logger)
    {
        _openAiService = openAiService;
        _logger = logger;
    }

    public async Task<ActionResult> Handle(ExtractFromTextActionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling ExtractFromText action for tenant {TenantId} by agent {AgentId}",
            request.TenantId, request.AgentId);

        var text = ParameterExtractor.GetString(request.Parameters, "text");

        if (string.IsNullOrWhiteSpace(text))
        {
            return ActionResult.Fail(
                ["The text parameter is required. Please provide unstructured text to extract property data from."],
                ActionType.ExtractFromText,
                "I need some text to extract property data from. Please paste the listing text or description.");
        }

        if (text.Length > MaxTextLength)
        {
            return ActionResult.Fail(
                [$"Text exceeds the maximum length of 10,000 characters (provided: {text.Length})."],
                ActionType.ExtractFromText,
                "The provided text exceeds the maximum limit of 10,000 characters. Please shorten it and try again.");
        }

        try
        {
            var systemPrompt = PropertyExtractionSystemPrompt;

            var response = await _openAiService.GenerateWithJsonResponseAsync(
                systemPrompt, text, cancellationToken);

            if (string.IsNullOrWhiteSpace(response))
            {
                return ActionResult.Fail(
                    ["AI service returned an empty response."],
                    ActionType.ExtractFromText,
                    "I was unable to extract property data from the provided text. Please try again.");
            }

            var extracted = ParseExtractionResponse(response);

            _logger.LogInformation(
                "ExtractFromText completed for tenant {TenantId}: extracted property data",
                request.TenantId);

            return ActionResult.Ok(
                data: extracted,
                message: BuildConfirmationMessage(extracted),
                type: ActionType.ExtractFromText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ExtractFromText failed for tenant {TenantId}", request.TenantId);

            return ActionResult.Fail(
                [$"Failed to extract property data: {ex.Message}"],
                ActionType.ExtractFromText,
                "I encountered an error while extracting property data. Please try again.");
        }
    }

    internal static ExtractedPropertyDto ParseExtractionResponse(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        return new ExtractedPropertyDto
        {
            PropertyType = ParseStringField(root, "property_type"),
            OperationType = ParseStringField(root, "operation_type"),
            Bedrooms = ParseIntField(root, "bedrooms"),
            Bathrooms = ParseIntField(root, "bathrooms"),
            Price = ParseDecimalField(root, "price"),
            Area = ParseDecimalField(root, "area"),
            City = ParseStringField(root, "city"),
            Address = ParseStringField(root, "address"),
            Description = ParseStringField(root, "description"),
            Title = ParseStringField(root, "title"),
            Features = ParseStringListField(root, "features"),
            Floor = ParseIntField(root, "floor"),
            HasElevator = ParseBoolField(root, "has_elevator"),
            HasParking = ParseBoolField(root, "has_parking"),
            HasPool = ParseBoolField(root, "has_pool"),
            YearBuilt = ParseIntField(root, "year_built")
        };
    }

    private static ExtractedField<string>? ParseStringField(JsonElement root, string fieldName)
    {
        if (!root.TryGetProperty(fieldName, out var field)) return null;
        if (!field.TryGetProperty("value", out var value)) return null;

        var confidence = field.TryGetProperty("confidence", out var conf) ? conf.GetDouble() : 0.5;
        return new ExtractedField<string>(value.GetString(), confidence);
    }

    private static ExtractedField<int?>? ParseIntField(JsonElement root, string fieldName)
    {
        if (!root.TryGetProperty(fieldName, out var field)) return null;
        if (!field.TryGetProperty("value", out var value)) return null;

        var confidence = field.TryGetProperty("confidence", out var conf) ? conf.GetDouble() : 0.5;
        return value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var intVal)
            ? new ExtractedField<int?>(intVal, confidence)
            : null;
    }

    private static ExtractedField<decimal?>? ParseDecimalField(JsonElement root, string fieldName)
    {
        if (!root.TryGetProperty(fieldName, out var field)) return null;
        if (!field.TryGetProperty("value", out var value)) return null;

        var confidence = field.TryGetProperty("confidence", out var conf) ? conf.GetDouble() : 0.5;
        return value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var decVal)
            ? new ExtractedField<decimal?>(decVal, confidence)
            : null;
    }

    private static ExtractedField<bool?>? ParseBoolField(JsonElement root, string fieldName)
    {
        if (!root.TryGetProperty(fieldName, out var field)) return null;
        if (!field.TryGetProperty("value", out var value)) return null;

        var confidence = field.TryGetProperty("confidence", out var conf) ? conf.GetDouble() : 0.5;
        return value.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? new ExtractedField<bool?>(value.GetBoolean(), confidence)
            : null;
    }

    private static ExtractedField<List<string>>? ParseStringListField(JsonElement root, string fieldName)
    {
        if (!root.TryGetProperty(fieldName, out var field)) return null;
        if (!field.TryGetProperty("value", out var value)) return null;
        if (value.ValueKind != JsonValueKind.Array) return null;

        var confidence = field.TryGetProperty("confidence", out var conf) ? conf.GetDouble() : 0.5;
        var list = new List<string>();
        foreach (var item in value.EnumerateArray())
        {
            var str = item.GetString();
            if (str != null) list.Add(str);
        }
        return new ExtractedField<List<string>>(list, confidence);
    }

    private static string BuildConfirmationMessage(ExtractedPropertyDto dto)
    {
        var parts = new List<string>();

        if (dto.PropertyType?.Value != null)
            parts.Add($"property type: {dto.PropertyType.Value}");
        if (dto.OperationType?.Value != null)
            parts.Add($"operation: {dto.OperationType.Value}");
        if (dto.Bedrooms?.Value != null)
            parts.Add($"{dto.Bedrooms.Value} bedrooms");
        if (dto.City?.Value != null)
            parts.Add($"in {dto.City.Value}");

        return parts.Count > 0
            ? $"I extracted property data from the text: {string.Join(", ", parts)}."
            : "I extracted property data from the text.";
    }
}
