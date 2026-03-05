// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Microsoft.Extensions.Logging;
using Propely.AiApi.Application.Actions.Commands.Operations;
using Propely.AiApi.Application.Actions.Helpers;
using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.Application.Actions.Handlers.Operations;

/// <summary>
/// Handles the ReactivatePropertyActionCommand by extracting the property identifier
/// and returning structured data confirming the reactivation.
/// Sets the target status to "Active".
/// </summary>
public sealed class ReactivatePropertyActionHandler : IRequestHandler<ReactivatePropertyActionCommand, ActionResult>
{
    private readonly ILogger<ReactivatePropertyActionHandler> _logger;

    public ReactivatePropertyActionHandler(ILogger<ReactivatePropertyActionHandler> logger)
    {
        _logger = logger;
    }

    public Task<ActionResult> Handle(ReactivatePropertyActionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling ReactivateProperty action for tenant {TenantId} by agent {AgentId}",
            request.TenantId, request.AgentId);

        var parameters = request.Parameters;

        var propertyId = ParameterExtractor.GetGuid(parameters, "property_id");
        var reference = ParameterExtractor.GetString(parameters, "reference");

        // Validate property identifier
        if (!propertyId.HasValue && string.IsNullOrWhiteSpace(reference))
        {
            return Task.FromResult(ActionResult.Fail(
                ["A property identifier is required. Please specify the property ID or reference."],
                ActionType.ReactivateProperty,
                "I need to know which property to reactivate. Could you provide the property ID or reference?"));
        }

        var reactivateData = new Dictionary<string, object?>
        {
            ["propertyId"] = propertyId,
            ["reference"] = reference,
            ["targetStatus"] = "Active",
            ["agentId"] = request.AgentId,
            ["tenantId"] = request.TenantId
        };

        var identifier = propertyId?.ToString() ?? reference;
        var message = $"I'll reactivate property {identifier}. The listing will be returned to Active status.";

        _logger.LogInformation(
            "ReactivateProperty action prepared: property {PropertyId} in tenant {TenantId}",
            identifier, request.TenantId);

        return Task.FromResult(ActionResult.Ok(
            data: reactivateData,
            message: message,
            type: ActionType.ReactivateProperty));
    }
}
