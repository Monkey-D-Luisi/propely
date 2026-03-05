// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Microsoft.Extensions.Logging;
using Propely.AiApi.Application.Actions.Commands.PropertyActions;
using Propely.AiApi.Application.Actions.Helpers;
using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.Application.Actions.Handlers;

/// <summary>
/// Handles the UpdatePropertyActionCommand by extracting the property identifier
/// and the fields to update. Since the Properties SDK is currently read-only,
/// this handler extracts and validates parameters and returns structured data
/// describing what would be updated. Write support will be added when the SDK
/// is extended with update endpoints.
/// </summary>
public sealed class UpdatePropertyActionHandler : IRequestHandler<UpdatePropertyActionCommand, ActionResult>
{
    private readonly ILogger<UpdatePropertyActionHandler> _logger;

    public UpdatePropertyActionHandler(ILogger<UpdatePropertyActionHandler> logger)
    {
        _logger = logger;
    }

    public Task<ActionResult> Handle(UpdatePropertyActionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling UpdateProperty action for tenant {TenantId} by agent {AgentId}",
            request.TenantId, request.AgentId);

        var parameters = request.Parameters;

        var propertyId = ParameterExtractor.GetGuid(parameters, "property_id");
        var reference = ParameterExtractor.GetString(parameters, "reference");
        var field = ParameterExtractor.GetString(parameters, "field");
        var value = ParameterExtractor.GetString(parameters, "value");

        // Validate that we have some identifier for the property
        if (!propertyId.HasValue && string.IsNullOrWhiteSpace(reference))
        {
            return Task.FromResult(ActionResult.Fail(
                ["A property identifier is required. Please specify the property ID or reference."],
                ActionType.UpdateProperty,
                "I need to know which property to update. Could you provide the property ID or reference?"));
        }

        // Validate that we have something to update
        if (string.IsNullOrWhiteSpace(field) && string.IsNullOrWhiteSpace(value))
        {
            // Check if there are any other parameters that look like field updates
            var updateFields = parameters
                .Where(p => p.Key is not ("property_id" or "reference"))
                .Where(p => p.Value is not null)
                .ToDictionary(p => p.Key, p => p.Value);

            if (updateFields.Count == 0)
            {
                return Task.FromResult(ActionResult.Fail(
                    ["No update fields specified. Please specify what to change."],
                    ActionType.UpdateProperty,
                    "I need to know what to update. Could you specify the field and new value?"));
            }

            // Build structured update data from all non-identifier parameters
            var updateData = new Dictionary<string, object?>
            {
                ["propertyId"] = propertyId,
                ["reference"] = reference,
                ["updates"] = updateFields,
                ["agentId"] = request.AgentId,
                ["tenantId"] = request.TenantId
            };

            var fieldNames = string.Join(", ", updateFields.Keys);
            var message = $"I'll update {fieldNames} for property {propertyId?.ToString() ?? reference}.";

            return Task.FromResult(ActionResult.Ok(
                data: updateData,
                message: message,
                type: ActionType.UpdateProperty));
        }

        // Single field update
        var singleUpdateData = new Dictionary<string, object?>
        {
            ["propertyId"] = propertyId,
            ["reference"] = reference,
            ["field"] = field,
            ["value"] = value,
            ["agentId"] = request.AgentId,
            ["tenantId"] = request.TenantId
        };

        var identifier = propertyId?.ToString() ?? reference;
        var singleMessage = $"I'll update the {field} to \"{value}\" for property {identifier}.";

        _logger.LogInformation(
            "UpdateProperty action prepared: field {Field} for property {PropertyId} in tenant {TenantId}",
            field, identifier, request.TenantId);

        return Task.FromResult(ActionResult.Ok(
            data: singleUpdateData,
            message: singleMessage,
            type: ActionType.UpdateProperty));
    }
}
