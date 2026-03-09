// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Globalization;
using MediatR;
using Microsoft.Extensions.Logging;
using Propely.AiApi.Application.Actions.Commands.PropertyActions;
using Propely.AiApi.Domain.Actions;
using Propely.PropertiesApi.Client;
using Propely.PropertiesApi.Client.Dtos;

namespace Propely.AiApi.Application.Actions.Handlers;

/// <summary>
/// Handles the CreatePropertyActionCommand by mapping AI-extracted parameters
/// to a Properties API create request and persisting the property.
/// </summary>
public sealed class CreatePropertyActionHandler : IRequestHandler<CreatePropertyActionCommand, ActionResult>
{
    private readonly IPropertiesApiClient _propertiesClient;
    private readonly ILogger<CreatePropertyActionHandler> _logger;

    public CreatePropertyActionHandler(
        IPropertiesApiClient propertiesClient,
        ILogger<CreatePropertyActionHandler> logger)
    {
        _propertiesClient = propertiesClient;
        _logger = logger;
    }

    public async Task<ActionResult> Handle(CreatePropertyActionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling CreateProperty action for tenant {TenantId} by agent {AgentId}",
            request.TenantId, request.AgentId);

        var propertyType = request.Parameters.PropertyType;
        var operationType = request.Parameters.OperationType;
        var bedrooms = request.Parameters.Bedrooms;
        var bathrooms = request.Parameters.Bathrooms;
        var price = request.Parameters.Price;
        var city = request.Parameters.City;
        var description = request.Parameters.Description;

        // Validate minimum required fields
        if (string.IsNullOrWhiteSpace(propertyType))
        {
            return ActionResult.Fail(
                ["Property type is required. Please specify the type (e.g., apartment, house, villa)."],
                ActionType.CreateProperty,
                "I need to know the property type to create a listing. Could you specify if it's an apartment, house, villa, or another type?");
        }

        // Normalize enum values to PascalCase for the Properties API
        var normalizedType = ToPascalCase(propertyType);
        var normalizedOperation = ToPascalCase(operationType ?? "sale");
        var title = request.Parameters.Title
            ?? $"{normalizedType} in {city ?? "unspecified location"}";

        var createRequest = new CreatePropertyRequest
        {
            Title = title,
            PropertyType = normalizedType,
            OperationType = normalizedOperation,
            Description = description is not null
                ? new CreateLocalizedTextRequest { Es = description }
                : null,
            Address = city is not null
                ? new CreateAddressRequest { City = city }
                : null,
            Features = (bedrooms.HasValue || bathrooms.HasValue)
                ? new CreatePropertyFeaturesRequest { Bedrooms = bedrooms, Bathrooms = bathrooms }
                : null,
            Financials = price.HasValue
                ? new CreatePropertyFinancialsRequest { Price = price }
                : null
        };

        PropertyResponse created;
        try
        {
            created = await _propertiesClient.CreateAsync(createRequest, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create property via Properties API for tenant {TenantId}", request.TenantId);
            return ActionResult.Fail(
                ["Failed to save the property. Please try again."],
                ActionType.CreateProperty,
                "I understood your request but couldn't save the property right now. Please try again.");
        }

        // Build human-readable confirmation message
        var message = BuildConfirmationMessage(normalizedType, normalizedOperation, city, bedrooms, bathrooms, price);

        _logger.LogInformation(
            "CreateProperty action completed: {PropertyId} ({PropertyType}) in {City} for tenant {TenantId}",
            created.Id, normalizedType, city ?? "unspecified", request.TenantId);

        var responseData = new Dictionary<string, object?>
        {
            ["id"] = created.Id,
            ["propertyType"] = created.PropertyType,
            ["operationType"] = created.OperationType,
            ["title"] = created.Title,
            ["status"] = created.Status,
            ["agentId"] = created.AgentId,
            ["tenantId"] = created.TenantId
        };

        return ActionResult.Ok(
            data: responseData,
            message: message,
            type: ActionType.CreateProperty);
    }

    private static string BuildConfirmationMessage(
        string propertyType, string operationType, string? city,
        int? bedrooms, int? bathrooms, decimal? price)
    {
        var parts = new List<string> { $"Created a {propertyType.ToLowerInvariant()} for {operationType.ToLowerInvariant()}" };

        if (!string.IsNullOrWhiteSpace(city))
            parts.Add($"in {city}");

        if (bedrooms.HasValue)
            parts.Add($"with {bedrooms} bedroom{(bedrooms > 1 ? "s" : "")}");

        if (bathrooms.HasValue)
            parts.Add($"{bathrooms} bathroom{(bathrooms > 1 ? "s" : "")}");

        if (price.HasValue)
            parts.Add(string.Format(CultureInfo.InvariantCulture, "priced at {0:N0} EUR", price.Value));

        return string.Join(", ", parts) + ".";
    }

    private static string ToPascalCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;
        return char.ToUpperInvariant(value[0]) + value[1..].ToLowerInvariant();
    }
}
