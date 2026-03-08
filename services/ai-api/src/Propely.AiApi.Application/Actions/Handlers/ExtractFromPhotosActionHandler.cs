// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using Propely.AiApi.Application.Actions.Commands.Content;
using Propely.AiApi.Application.Actions.Dtos;
using Propely.AiApi.Application.Common.Interfaces;
using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.Application.Actions.Handlers;

/// <summary>
/// Handles extraction of structured property data from photographs via OpenAI vision API.
/// Validates image URL count (1-10), calls IOpenAiService.AnalyzeImagesAsync,
/// and parses the JSON response into an <see cref="ExtractedPropertyDto"/>
/// with per-field confidence scores.
/// </summary>
public sealed class ExtractFromPhotosActionHandler : IRequestHandler<ExtractFromPhotosActionCommand, ActionResult>
{
    private const int MaxImages = 10;

    internal const string PhotoExtractionSystemPrompt = """
        You are a real estate photo analysis specialist. Your task is to analyze property photographs and extract structured property data.

        Examine the provided images carefully and extract as many property attributes as possible. For each field, provide the extracted value and a confidence score between 0.0 and 1.0 indicating how certain you are about the observation.

        Return a JSON object with the following structure. Only include fields that you can determine from the images. Do not guess — if you cannot determine a field from the photos, omit it.

        {
            "property_type": { "value": "apartment|house|villa|studio|penthouse|duplex|commercial|land|garage|storage", "confidence": 0.95 },
            "bedrooms": { "value": 3, "confidence": 0.8 },
            "bathrooms": { "value": 2, "confidence": 0.7 },
            "area": { "value": 120.5, "confidence": 0.5 },
            "description": { "value": "Modern apartment with open-plan kitchen and hardwood floors", "confidence": 0.9 },
            "features": { "value": ["terrace", "hardwood floors", "modern kitchen", "natural light"], "confidence": 0.85 },
            "floor": { "value": 5, "confidence": 0.6 },
            "has_elevator": { "value": true, "confidence": 0.5 },
            "has_parking": { "value": true, "confidence": 0.7 },
            "has_pool": { "value": true, "confidence": 0.9 },
            "year_built": { "value": 2020, "confidence": 0.5 }
        }

        Rules:
        - Confidence 0.9-1.0: Clearly visible in photos
        - Confidence 0.7-0.89: Likely based on visual evidence
        - Confidence 0.5-0.69: Inferred from visual cues
        - Confidence below 0.5: Do not include the field
        - Count visible rooms as accurately as possible (bedrooms have beds, bathrooms have fixtures)
        - Note visible features: pool, terrace, garden, parking, elevator, appliances, flooring type
        - Estimate property type from overall appearance and layout
        - Generate a natural description summarizing what you see
        - Always respond with valid JSON only
        """;

    private readonly IOpenAiService _openAiService;
    private readonly ILogger<ExtractFromPhotosActionHandler> _logger;

    public ExtractFromPhotosActionHandler(IOpenAiService openAiService, ILogger<ExtractFromPhotosActionHandler> logger)
    {
        _openAiService = openAiService;
        _logger = logger;
    }

    public async Task<ActionResult> Handle(ExtractFromPhotosActionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling ExtractFromPhotos action for tenant {TenantId} by agent {AgentId}",
            request.TenantId, request.AgentId);

        var imageUrls = request.Parameters.ImageUrls;

        if (imageUrls == null || imageUrls.Count == 0)
        {
            return ActionResult.Fail(
                ["At least one image URL is required. Please provide image URLs to extract property data from."],
                ActionType.ExtractFromPhotos,
                "I need at least one image to analyze. Please provide the image URLs.");
        }

        if (imageUrls.Count > MaxImages)
        {
            return ActionResult.Fail(
                [$"Too many images provided ({imageUrls.Count}). Maximum allowed is 10."],
                ActionType.ExtractFromPhotos,
                "You can upload a maximum of 10 images at a time. Please reduce the number of images.");
        }

        try
        {
            var response = await _openAiService.AnalyzeImagesAsync(
                PhotoExtractionSystemPrompt, imageUrls.ToArray(), cancellationToken);

            if (string.IsNullOrWhiteSpace(response))
            {
                return ActionResult.Fail(
                    ["AI service returned an empty response."],
                    ActionType.ExtractFromPhotos,
                    "I was unable to extract property data from the photos. Please try again.");
            }

            var extracted = ExtractFromTextActionHandler.ParseExtractionResponse(response);

            _logger.LogInformation(
                "ExtractFromPhotos completed for tenant {TenantId}: analyzed {ImageCount} images",
                request.TenantId, imageUrls.Count);

            return ActionResult.Ok(
                data: extracted,
                message: BuildConfirmationMessage(extracted, imageUrls.Count),
                type: ActionType.ExtractFromPhotos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ExtractFromPhotos failed for tenant {TenantId}", request.TenantId);

            return ActionResult.Fail(
                ["Failed to analyze the provided photos."],
                ActionType.ExtractFromPhotos,
                "I encountered an error while analyzing the photos. Please try again.");
        }
    }

    private static string BuildConfirmationMessage(ExtractedPropertyDto dto, int imageCount)
    {
        var parts = new List<string>();

        if (dto.PropertyType?.Value != null)
            parts.Add($"property type: {dto.PropertyType.Value}");
        if (dto.Bedrooms?.Value != null)
            parts.Add($"{dto.Bedrooms.Value} bedrooms");
        if (dto.Features?.Value != null && dto.Features.Value.Count > 0)
            parts.Add($"features: {string.Join(", ", dto.Features.Value.Take(3))}");

        var summary = parts.Count > 0
            ? $": {string.Join(", ", parts)}"
            : string.Empty;

        return $"I analyzed {imageCount} photo{(imageCount > 1 ? "s" : "")} and extracted property data{summary}.";
    }
}
