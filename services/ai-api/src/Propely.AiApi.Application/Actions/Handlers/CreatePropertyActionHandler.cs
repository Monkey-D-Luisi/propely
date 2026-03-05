// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Globalization;
using MediatR;
using Microsoft.Extensions.Logging;
using Propely.AiApi.Application.Actions.Commands.PropertyActions;
using Propely.AiApi.Application.Actions.Helpers;
using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.Application.Actions.Handlers;

/// <summary>
/// Handles the CreatePropertyActionCommand by extracting property parameters
/// and returning structured data for property creation.
/// Since the Properties SDK is currently read-only, this handler extracts and validates
/// parameters and returns them as structured data. Write support will be added
/// when the SDK is extended with create endpoints.
/// </summary>
public sealed class CreatePropertyActionHandler : IRequestHandler<CreatePropertyActionCommand, ActionResult>
{
    private readonly ILogger<CreatePropertyActionHandler> _logger;

    public CreatePropertyActionHandler(ILogger<CreatePropertyActionHandler> logger)
    {
        _logger = logger;
    }

    public Task<ActionResult> Handle(CreatePropertyActionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling CreateProperty action for tenant {TenantId} by agent {AgentId}",
            request.TenantId, request.AgentId);

        var parameters = request.Parameters;

        var title = ParameterExtractor.GetString(parameters, "title");
        var propertyType = ParameterExtractor.GetString(parameters, "property_type");
        var operationType = ParameterExtractor.GetString(parameters, "operation_type");
        var bedrooms = ParameterExtractor.GetInt(parameters, "bedrooms");
        var bathrooms = ParameterExtractor.GetInt(parameters, "bathrooms");
        var price = ParameterExtractor.GetDecimal(parameters, "price");
        var city = ParameterExtractor.GetString(parameters, "city");
        var description = ParameterExtractor.GetString(parameters, "description");

        // Validate minimum required fields
        if (string.IsNullOrWhiteSpace(propertyType))
        {
            return Task.FromResult(ActionResult.Fail(
                ["Property type is required. Please specify the type (e.g., apartment, house, villa)."],
                ActionType.CreateProperty,
                "I need to know the property type to create a listing. Could you specify if it's an apartment, house, villa, or another type?"));
        }

        // Build structured property data
        var propertyData = new Dictionary<string, object?>
        {
            ["propertyType"] = propertyType,
            ["operationType"] = operationType ?? "Sale",
            ["title"] = title ?? $"{propertyType} in {city ?? "unspecified location"}",
            ["bedrooms"] = bedrooms,
            ["bathrooms"] = bathrooms,
            ["price"] = price,
            ["city"] = city,
            ["description"] = description,
            ["agentId"] = request.AgentId,
            ["tenantId"] = request.TenantId
        };

        // Build human-readable confirmation message
        var messageParts = new List<string>
        {
            $"I'll create a {propertyType}"
        };

        if (!string.IsNullOrWhiteSpace(operationType))
            messageParts[0] += $" for {operationType.ToLowerInvariant()}";

        if (!string.IsNullOrWhiteSpace(city))
            messageParts.Add($"located in {city}");

        if (bedrooms.HasValue)
            messageParts.Add($"with {bedrooms} bedroom{(bedrooms > 1 ? "s" : "")}");

        if (bathrooms.HasValue)
            messageParts.Add($"{bathrooms} bathroom{(bathrooms > 1 ? "s" : "")}");

        if (price.HasValue)
            messageParts.Add(string.Format(CultureInfo.InvariantCulture, "priced at {0:N0} EUR", price.Value));

        var message = string.Join(", ", messageParts) + ".";

        _logger.LogInformation(
            "CreateProperty action prepared: {PropertyType} in {City} for tenant {TenantId}",
            propertyType, city ?? "unspecified", request.TenantId);

        return Task.FromResult(ActionResult.Ok(
            data: propertyData,
            message: message,
            type: ActionType.CreateProperty));
    }
}
