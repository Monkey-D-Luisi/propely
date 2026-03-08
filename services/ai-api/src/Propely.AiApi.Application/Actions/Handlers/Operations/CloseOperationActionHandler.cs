// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Microsoft.Extensions.Logging;
using Propely.AiApi.Application.Actions.Commands.Operations;
using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.Application.Actions.Handlers.Operations;

/// <summary>
/// Handles the CloseOperationActionCommand by extracting the property identifier
/// and inferring the target status from the operation type.
/// Sale operations result in "Sold" status; Rent operations result in "Rented" status.
/// When no operation type is specified, defaults to "Sold".
/// </summary>
public sealed class CloseOperationActionHandler : IRequestHandler<CloseOperationActionCommand, ActionResult>
{
    private static readonly HashSet<string> ValidOperationTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "sale", "rent"
    };

    private readonly ILogger<CloseOperationActionHandler> _logger;

    public CloseOperationActionHandler(ILogger<CloseOperationActionHandler> logger)
    {
        _logger = logger;
    }

    public Task<ActionResult> Handle(CloseOperationActionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling CloseOperation action for tenant {TenantId} by agent {AgentId}",
            request.TenantId, request.AgentId);

        var propertyId = request.Parameters.PropertyId;
        var reference = request.Parameters.Reference;
        var operationType = request.Parameters.OperationType;

        // Validate property identifier
        if (!propertyId.HasValue && string.IsNullOrWhiteSpace(reference))
        {
            return Task.FromResult(ActionResult.Fail(
                ["A property identifier is required. Please specify the property ID or reference."],
                ActionType.CloseOperation,
                "I need to know which property to close the operation for. Could you provide the property ID or reference?"));
        }

        // Validate operation type if provided
        if (!string.IsNullOrWhiteSpace(operationType) && !ValidOperationTypes.Contains(operationType))
        {
            return Task.FromResult(ActionResult.Fail(
                [$"Invalid operation type \"{operationType}\". Valid types are: sale, rent."],
                ActionType.CloseOperation,
                $"\"{operationType}\" is not a valid operation type. Please specify either \"sale\" or \"rent\"."));
        }

        // Infer target status from operation type (default to Sold if not specified)
        var targetStatus = InferTargetStatus(operationType);

        var closeData = new Dictionary<string, object?>
        {
            ["propertyId"] = propertyId,
            ["reference"] = reference,
            ["operationType"] = operationType,
            ["targetStatus"] = targetStatus,
            ["agentId"] = request.AgentId,
            ["tenantId"] = request.TenantId
        };

        var identifier = propertyId?.ToString() ?? reference;
        var message = $"I'll close the operation for property {identifier}. Status will be changed to \"{targetStatus}\".";

        _logger.LogInformation(
            "CloseOperation action prepared: {TargetStatus} for property {PropertyId} (operation: {OperationType}) in tenant {TenantId}",
            targetStatus, identifier, operationType ?? "default(sale)", request.TenantId);

        return Task.FromResult(ActionResult.Ok(
            data: closeData,
            message: message,
            type: ActionType.CloseOperation));
    }

    private static string InferTargetStatus(string? operationType)
    {
        if (string.IsNullOrWhiteSpace(operationType))
            return "Sold"; // Default to Sold when operation type is not specified

        return operationType.Equals("rent", StringComparison.OrdinalIgnoreCase)
            ? "Rented"
            : "Sold";
    }
}
