// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Microsoft.Extensions.Logging;
using Propely.AiApi.Application.Actions.Commands.Operations;
using Propely.AiApi.Application.Actions.Helpers;
using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.Application.Actions.Handlers.Operations;

/// <summary>
/// Handles the ReservePropertyActionCommand by extracting the property identifier
/// and optional contact name. Sets the target status to "Reserved" and returns
/// structured data describing the reservation.
/// </summary>
public sealed class ReservePropertyActionHandler : IRequestHandler<ReservePropertyActionCommand, ActionResult>
{
    private readonly ILogger<ReservePropertyActionHandler> _logger;

    public ReservePropertyActionHandler(ILogger<ReservePropertyActionHandler> logger)
    {
        _logger = logger;
    }

    public Task<ActionResult> Handle(ReservePropertyActionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling ReserveProperty action for tenant {TenantId} by agent {AgentId}",
            request.TenantId, request.AgentId);

        var parameters = request.Parameters;

        var propertyId = ParameterExtractor.GetGuid(parameters, "property_id");
        var reference = ParameterExtractor.GetString(parameters, "reference");
        var contactName = ParameterExtractor.GetString(parameters, "contact_name");

        // Validate property identifier
        if (!propertyId.HasValue && string.IsNullOrWhiteSpace(reference))
        {
            return Task.FromResult(ActionResult.Fail(
                ["A property identifier is required. Please specify the property ID or reference."],
                ActionType.ReserveProperty,
                "I need to know which property to reserve. Could you provide the property ID or reference?"));
        }

        var reservationData = new Dictionary<string, object?>
        {
            ["propertyId"] = propertyId,
            ["reference"] = reference,
            ["contactName"] = contactName,
            ["targetStatus"] = "Reserved",
            ["agentId"] = request.AgentId,
            ["tenantId"] = request.TenantId
        };

        var identifier = propertyId?.ToString() ?? reference;
        var contactPart = string.IsNullOrWhiteSpace(contactName)
            ? string.Empty
            : $" for {contactName}";
        var message = $"I'll reserve property {identifier}{contactPart}. Reserved status will be applied.";

        _logger.LogInformation(
            "ReserveProperty action prepared: property {PropertyId}, contact {ContactName} in tenant {TenantId}",
            identifier, contactName ?? "(none)", request.TenantId);

        return Task.FromResult(ActionResult.Ok(
            data: reservationData,
            message: message,
            type: ActionType.ReserveProperty));
    }
}
