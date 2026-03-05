// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Microsoft.Extensions.Logging;
using Propely.AiApi.Application.Actions.Commands.Content;
using Propely.AiApi.Application.Actions.Commands.Operations;
using Propely.AiApi.Application.Actions.Commands.PropertyActions;
using Propely.AiApi.Application.Actions.Interfaces;
using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.Infrastructure.AI;

/// <summary>
/// Routes classified intents to the appropriate action handler via MediatR.
/// Property action types and operation action types are dispatched to their
/// respective MediatR command handlers.
/// Remaining action types return "not yet implemented" placeholders.
/// </summary>
public sealed class ActionRouter : IActionRouter
{
    private readonly IMediator _mediator;
    private readonly ILogger<ActionRouter> _logger;

    public ActionRouter(IMediator mediator, ILogger<ActionRouter> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<ActionResult> RouteAsync(
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
            return ActionResult.Fail(
                ["Unknown action type cannot be routed."],
                ActionType.Unknown,
                "I could not determine what action to perform.");
        }

        return intent.ActionType switch
        {
            // Property actions — dispatched to MediatR handlers
            ActionType.CreateProperty => await _mediator.Send(
                new CreatePropertyActionCommand(intent.Parameters, tenantId, agentId), ct),
            ActionType.QueryProperties => await _mediator.Send(
                new QueryPropertiesActionCommand(intent.Parameters, tenantId, agentId), ct),
            ActionType.UpdateProperty => await _mediator.Send(
                new UpdatePropertyActionCommand(intent.Parameters, tenantId, agentId), ct),
            ActionType.ChangePropertyStatus => await _mediator.Send(
                new ChangePropertyStatusActionCommand(intent.Parameters, tenantId, agentId), ct),

            // Operation actions — dispatched to MediatR handlers
            ActionType.ReserveProperty => await _mediator.Send(
                new ReservePropertyActionCommand(intent.Parameters, tenantId, agentId), ct),
            ActionType.CloseOperation => await _mediator.Send(
                new CloseOperationActionCommand(intent.Parameters, tenantId, agentId), ct),
            ActionType.ArchiveProperty => await _mediator.Send(
                new ArchivePropertyActionCommand(intent.Parameters, tenantId, agentId), ct),
            ActionType.ReactivateProperty => await _mediator.Send(
                new ReactivatePropertyActionCommand(intent.Parameters, tenantId, agentId), ct),

            // Content / AI generation actions — dispatched to MediatR handlers
            ActionType.ExtractFromText => await _mediator.Send(
                new ExtractFromTextActionCommand(intent.Parameters, tenantId, agentId), ct),
            ActionType.ExtractFromPhotos => await _mediator.Send(
                new ExtractFromPhotosActionCommand(intent.Parameters, tenantId, agentId), ct),
            ActionType.GenerateCopy => await _mediator.Send(
                new GenerateCopyActionCommand(intent.Parameters, tenantId, agentId), ct),

            // All other action types — placeholder until subsequent tasks implement them
            _ => ActionResult.Ok(
                data: new { intent.Parameters },
                message: GetPlaceholderMessage(intent.ActionType),
                type: intent.ActionType,
                confidence: intent.Confidence)
        };
    }

    private static string GetPlaceholderMessage(ActionType actionType) => actionType switch
    {
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
