// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Microsoft.Extensions.Logging;
using Propely.AiApi.Application.Actions.Commands.AppointmentActions;
using Propely.AiApi.Application.Actions.Commands.ContactActions;
using Propely.AiApi.Application.Actions.Commands.Content;
using Propely.AiApi.Application.Actions.Commands.Operations;
using Propely.AiApi.Application.Actions.Commands.PropertyActions;
using Propely.AiApi.Application.Actions.Interfaces;
using Propely.AiApi.Application.Actions.Parameters;
using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.Infrastructure.AI;

/// <summary>
/// Routes classified intents to the appropriate action handler via MediatR.
/// Binds untyped parameter dictionaries to strongly-typed parameter records
/// before dispatching to handlers.
/// </summary>
public sealed class ActionRouter : IActionRouter
{
    private readonly IMediator _mediator;
    private readonly IParameterBinder _binder;
    private readonly ILogger<ActionRouter> _logger;

    public ActionRouter(IMediator mediator, IParameterBinder binder, ILogger<ActionRouter> logger)
    {
        _mediator = mediator;
        _binder = binder;
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
            // Property actions
            ActionType.CreateProperty => await _mediator.Send(
                new CreatePropertyActionCommand(_binder.Bind<CreatePropertyParameters>(intent.Parameters), tenantId, agentId), ct),
            ActionType.QueryProperties => await _mediator.Send(
                new QueryPropertiesActionCommand(_binder.Bind<QueryPropertiesParameters>(intent.Parameters), tenantId, agentId), ct),
            ActionType.UpdateProperty => await _mediator.Send(
                new UpdatePropertyActionCommand(_binder.Bind<UpdatePropertyParameters>(intent.Parameters), tenantId, agentId), ct),
            ActionType.ChangePropertyStatus => await _mediator.Send(
                new ChangePropertyStatusActionCommand(_binder.Bind<ChangePropertyStatusParameters>(intent.Parameters), tenantId, agentId), ct),

            // Operation actions
            ActionType.ReserveProperty => await _mediator.Send(
                new ReservePropertyActionCommand(_binder.Bind<ReservePropertyParameters>(intent.Parameters), tenantId, agentId), ct),
            ActionType.CloseOperation => await _mediator.Send(
                new CloseOperationActionCommand(_binder.Bind<CloseOperationParameters>(intent.Parameters), tenantId, agentId), ct),
            ActionType.ArchiveProperty => await _mediator.Send(
                new ArchivePropertyActionCommand(_binder.Bind<ArchivePropertyParameters>(intent.Parameters), tenantId, agentId), ct),
            ActionType.ReactivateProperty => await _mediator.Send(
                new ReactivatePropertyActionCommand(_binder.Bind<ReactivatePropertyParameters>(intent.Parameters), tenantId, agentId), ct),

            // Content / AI generation actions
            ActionType.ExtractFromText => await _mediator.Send(
                new ExtractFromTextActionCommand(_binder.Bind<ExtractFromTextParameters>(intent.Parameters), tenantId, agentId), ct),
            ActionType.ExtractFromPhotos => await _mediator.Send(
                new ExtractFromPhotosActionCommand(_binder.Bind<ExtractFromPhotosParameters>(intent.Parameters), tenantId, agentId), ct),
            ActionType.GenerateCopy => await _mediator.Send(
                new GenerateCopyActionCommand(_binder.Bind<GenerateCopyParameters>(intent.Parameters), tenantId, agentId), ct),

            // Contact & Lead actions
            ActionType.CreateLead => await _mediator.Send(
                new CreateLeadActionCommand(_binder.Bind<CreateLeadParameters>(intent.Parameters), tenantId, agentId), ct),
            ActionType.CreateContact => await _mediator.Send(
                new CreateContactActionCommand(_binder.Bind<CreateContactParameters>(intent.Parameters), tenantId, agentId), ct),
            ActionType.QualifyLead => await _mediator.Send(
                new QualifyLeadActionCommand(_binder.Bind<QualifyLeadParameters>(intent.Parameters), tenantId, agentId), ct),
            ActionType.ConvertLead => await _mediator.Send(
                new ConvertLeadActionCommand(_binder.Bind<ConvertLeadParameters>(intent.Parameters), tenantId, agentId), ct),
            ActionType.QueryLeads => await _mediator.Send(
                new QueryLeadsActionCommand(_binder.Bind<QueryLeadsParameters>(intent.Parameters), tenantId, agentId), ct),

            // Appointment actions
            ActionType.BookViewing => await _mediator.Send(
                new BookViewingActionCommand(_binder.Bind<BookViewingParameters>(intent.Parameters), tenantId, agentId), ct),
            ActionType.QueryAppointments => await _mediator.Send(
                new QueryAppointmentsActionCommand(_binder.Bind<QueryAppointmentsParameters>(intent.Parameters), tenantId, agentId), ct),
            ActionType.CancelAppointment => await _mediator.Send(
                new CancelAppointmentActionCommand(_binder.Bind<CancelAppointmentParameters>(intent.Parameters), tenantId, agentId), ct),
            ActionType.RescheduleAppointment => await _mediator.Send(
                new RescheduleAppointmentActionCommand(_binder.Bind<RescheduleAppointmentParameters>(intent.Parameters), tenantId, agentId), ct),

            // All other action types — placeholder
            _ => ActionResult.Ok(
                data: new { intent.Parameters },
                message: "Action understood but not yet implemented.",
                type: intent.ActionType,
                confidence: intent.Confidence)
        };
    }
}
