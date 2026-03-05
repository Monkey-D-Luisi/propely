// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Microsoft.Extensions.Logging;
using Propely.AiApi.Application.Actions.Interfaces;
using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.Infrastructure.AI;

/// <summary>
/// Routes classified intents to the appropriate action handler.
/// Currently returns "not yet implemented" for all known action types,
/// providing the routing plumbing that subsequent tasks (3.2, 3.3, etc.) will plug handlers into.
/// </summary>
public sealed class ActionRouter : IActionRouter
{
    private readonly ILogger<ActionRouter> _logger;

    public ActionRouter(ILogger<ActionRouter> logger)
    {
        _logger = logger;
    }

    public Task<ActionResult> RouteAsync(
        ClassifiedIntent intent,
        Guid tenantId,
        Guid agentId,
        CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Routing action {ActionType} for tenant {TenantId} by agent {AgentId} with {ParamCount} parameters",
            intent.ActionType, tenantId, agentId, intent.Parameters.Count);

        if (intent.ActionType == ActionType.Unknown)
        {
            return Task.FromResult(ActionResult.Fail(
                ["Unknown action type cannot be routed."],
                ActionType.Unknown,
                "I could not determine what action to perform."));
        }

        // For now, all known action types return a placeholder result.
        // Individual action handlers will be implemented in subsequent tasks (3.2, 3.3, etc.)
        var message = GetPlaceholderMessage(intent.ActionType);

        return Task.FromResult(ActionResult.Ok(
            data: new { intent.Parameters },
            message: message,
            type: intent.ActionType,
            confidence: intent.Confidence));
    }

    private static string GetPlaceholderMessage(ActionType actionType) => actionType switch
    {
        ActionType.CreateProperty => "I understood you want to create a property. This action will be available soon.",
        ActionType.UpdateProperty => "I understood you want to update a property. This action will be available soon.",
        ActionType.QueryProperties => "I understood you want to search properties. This action will be available soon.",
        ActionType.ChangePropertyStatus => "I understood you want to change a property's status. This action will be available soon.",
        ActionType.GenerateCopy => "I understood you want to generate marketing copy. This action will be available soon.",
        ActionType.ExtractFromText => "I understood you want to extract property data from text. This action will be available soon.",
        ActionType.ReserveProperty => "I understood you want to reserve a property. This action will be available soon.",
        ActionType.CloseOperation => "I understood you want to close an operation. This action will be available soon.",
        ActionType.ArchiveProperty => "I understood you want to archive a property. This action will be available soon.",
        ActionType.CreateLead => "I understood you want to create a lead. This action will be available soon.",
        ActionType.CreateContact => "I understood you want to create a contact. This action will be available soon.",
        ActionType.QualifyLead => "I understood you want to qualify a lead. This action will be available soon.",
        ActionType.ConvertLead => "I understood you want to convert a lead. This action will be available soon.",
        ActionType.QueryLeads => "I understood you want to search leads. This action will be available soon.",
        ActionType.BookViewing => "I understood you want to book a viewing. This action will be available soon.",
        ActionType.QueryAppointments => "I understood you want to search appointments. This action will be available soon.",
        ActionType.CancelAppointment => "I understood you want to cancel an appointment. This action will be available soon.",
        ActionType.RescheduleAppointment => "I understood you want to reschedule an appointment. This action will be available soon.",
        _ => "Action understood but not yet implemented."
    };
}
