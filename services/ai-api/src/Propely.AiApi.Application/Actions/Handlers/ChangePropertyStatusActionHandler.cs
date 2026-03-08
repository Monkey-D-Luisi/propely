// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Microsoft.Extensions.Logging;
using Propely.AiApi.Application.Actions.Commands.PropertyActions;
using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.Application.Actions.Handlers;

/// <summary>
/// Handles the ChangePropertyStatusActionCommand by extracting the property identifier
/// and target status. Since the Properties SDK is currently read-only,
/// this handler extracts and validates parameters and returns structured data
/// describing the status change. Write support will be added when the SDK
/// is extended with status change endpoints.
/// </summary>
public sealed class ChangePropertyStatusActionHandler : IRequestHandler<ChangePropertyStatusActionCommand, ActionResult>
{
    private static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Draft", "Active", "Reserved", "Sold", "Rented", "Archived", "Withdrawn"
    };

    private readonly ILogger<ChangePropertyStatusActionHandler> _logger;

    public ChangePropertyStatusActionHandler(ILogger<ChangePropertyStatusActionHandler> logger)
    {
        _logger = logger;
    }

    public Task<ActionResult> Handle(ChangePropertyStatusActionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling ChangePropertyStatus action for tenant {TenantId} by agent {AgentId}",
            request.TenantId, request.AgentId);

        var propertyId = request.Parameters.PropertyId;
        var reference = request.Parameters.Reference;
        var targetStatus = request.Parameters.Status;

        // Validate property identifier
        if (!propertyId.HasValue && string.IsNullOrWhiteSpace(reference))
        {
            return Task.FromResult(ActionResult.Fail(
                ["A property identifier is required. Please specify the property ID or reference."],
                ActionType.ChangePropertyStatus,
                "I need to know which property to update. Could you provide the property ID or reference?"));
        }

        // Validate target status
        if (string.IsNullOrWhiteSpace(targetStatus))
        {
            return Task.FromResult(ActionResult.Fail(
                [$"A target status is required. Valid statuses are: {string.Join(", ", ValidStatuses)}."],
                ActionType.ChangePropertyStatus,
                $"I need to know the target status. Valid options are: {string.Join(", ", ValidStatuses)}."));
        }

        if (!ValidStatuses.Contains(targetStatus))
        {
            return Task.FromResult(ActionResult.Fail(
                [$"Invalid status \"{targetStatus}\". Valid statuses are: {string.Join(", ", ValidStatuses)}."],
                ActionType.ChangePropertyStatus,
                $"\"{targetStatus}\" is not a valid status. Please choose from: {string.Join(", ", ValidStatuses)}."));
        }

        var statusChangeData = new Dictionary<string, object?>
        {
            ["propertyId"] = propertyId,
            ["reference"] = reference,
            ["targetStatus"] = targetStatus,
            ["agentId"] = request.AgentId,
            ["tenantId"] = request.TenantId
        };

        var identifier = propertyId?.ToString() ?? reference;
        var message = $"I'll change the status of property {identifier} to \"{targetStatus}\".";

        _logger.LogInformation(
            "ChangePropertyStatus action prepared: {TargetStatus} for property {PropertyId} in tenant {TenantId}",
            targetStatus, identifier, request.TenantId);

        return Task.FromResult(ActionResult.Ok(
            data: statusChangeData,
            message: message,
            type: ActionType.ChangePropertyStatus));
    }
}
