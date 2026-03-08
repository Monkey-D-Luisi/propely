// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Microsoft.Extensions.Logging;
using Propely.AiApi.Application.Actions.Commands.AppointmentActions;
using Propely.AiApi.Domain.Actions;
using Propely.AppointmentsApi.Client;
using Propely.AppointmentsApi.Client.Models;

namespace Propely.AiApi.Application.Actions.Handlers.AppointmentActions;

/// <summary>
/// Handles the CancelAppointmentActionCommand by cancelling an appointment
/// via the Appointments API SDK.
/// </summary>
public sealed class CancelAppointmentActionHandler : IRequestHandler<CancelAppointmentActionCommand, ActionResult>
{
    private readonly IAppointmentsApiClient _appointmentsClient;
    private readonly ILogger<CancelAppointmentActionHandler> _logger;

    public CancelAppointmentActionHandler(
        IAppointmentsApiClient appointmentsClient,
        ILogger<CancelAppointmentActionHandler> logger)
    {
        _appointmentsClient = appointmentsClient;
        _logger = logger;
    }

    public async Task<ActionResult> Handle(CancelAppointmentActionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling CancelAppointment action for tenant {TenantId} by agent {AgentId}",
            request.TenantId, request.AgentId);

        var appointmentId = request.Parameters.AppointmentId;
        var reason = request.Parameters.Reason;

        if (!appointmentId.HasValue)
        {
            return ActionResult.Fail(
                ["Appointment ID is required to cancel an appointment."],
                ActionType.CancelAppointment,
                "I need to know which appointment to cancel. Could you specify the appointment?");
        }

        AppointmentResponse result;
        try
        {
            result = await _appointmentsClient.CancelAppointmentAsync(
                appointmentId.Value,
                new CancelAppointmentClientRequest { Reason = reason ?? "Cancelled via voice/text command" },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel appointment {AppointmentId} via Appointments API", appointmentId.Value);

            return ActionResult.Fail(
                ["Failed to cancel the appointment. It may not exist or may already be cancelled."],
                ActionType.CancelAppointment,
                "I encountered an error while cancelling the appointment. Please verify it exists and is not already cancelled.");
        }

        var confirmationMessage = $"Appointment \"{result.Title}\" ({result.StartTimeUtc:yyyy-MM-dd HH:mm}) has been cancelled.";

        _logger.LogInformation(
            "CancelAppointment action completed: appointment {AppointmentId} for tenant {TenantId}",
            appointmentId.Value, request.TenantId);

        return ActionResult.Ok(
            data: new { result.Id, result.Title, result.Status, result.StartTimeUtc },
            message: confirmationMessage,
            type: ActionType.CancelAppointment);
    }
}
