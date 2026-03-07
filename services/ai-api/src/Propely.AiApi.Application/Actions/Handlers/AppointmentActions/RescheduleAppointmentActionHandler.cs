// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Microsoft.Extensions.Logging;
using Propely.AiApi.Application.Actions.Commands.AppointmentActions;
using Propely.AiApi.Application.Actions.Helpers;
using Propely.AiApi.Domain.Actions;
using Propely.AppointmentsApi.Client;
using Propely.AppointmentsApi.Client.Models;

namespace Propely.AiApi.Application.Actions.Handlers.AppointmentActions;

/// <summary>
/// Handles the RescheduleAppointmentActionCommand by updating an appointment's time
/// via the Appointments API SDK.
/// </summary>
public sealed class RescheduleAppointmentActionHandler : IRequestHandler<RescheduleAppointmentActionCommand, ActionResult>
{
    private readonly IAppointmentsApiClient _appointmentsClient;
    private readonly ILogger<RescheduleAppointmentActionHandler> _logger;

    public RescheduleAppointmentActionHandler(
        IAppointmentsApiClient appointmentsClient,
        ILogger<RescheduleAppointmentActionHandler> logger)
    {
        _appointmentsClient = appointmentsClient;
        _logger = logger;
    }

    public async Task<ActionResult> Handle(RescheduleAppointmentActionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling RescheduleAppointment action for tenant {TenantId} by agent {AgentId}",
            request.TenantId, request.AgentId);

        var parameters = request.Parameters;
        var appointmentId = ParameterExtractor.GetGuid(parameters, "appointment_id");
        var newStartTime = ParameterExtractor.GetString(parameters, "new_start_time");
        var newEndTime = ParameterExtractor.GetString(parameters, "new_end_time");

        if (!appointmentId.HasValue)
        {
            return ActionResult.Fail(
                ["Appointment ID is required to reschedule."],
                ActionType.RescheduleAppointment,
                "I need to know which appointment to reschedule. Could you specify the appointment?");
        }

        if (string.IsNullOrWhiteSpace(newStartTime))
        {
            return ActionResult.Fail(
                ["A new start time is required to reschedule."],
                ActionType.RescheduleAppointment,
                "I need the new date and time for the appointment.");
        }

        if (!DateTime.TryParse(newStartTime, out var newStartTimeUtc))
        {
            return ActionResult.Fail(
                ["Could not parse the new start time."],
                ActionType.RescheduleAppointment,
                "I couldn't understand the date/time format. Could you try again?");
        }

        // Fetch existing appointment to preserve its data
        AppointmentResponse existing;
        try
        {
            existing = await _appointmentsClient.GetAppointmentByIdAsync(appointmentId.Value, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch appointment {AppointmentId} for rescheduling", appointmentId.Value);

            return ActionResult.Fail(
                ["Could not find the appointment to reschedule."],
                ActionType.RescheduleAppointment,
                "I couldn't find the appointment. Please verify it exists.");
        }

        // Calculate duration from original appointment
        var originalDuration = existing.EndTimeUtc - existing.StartTimeUtc;
        var newEndTimeUtc = newStartTimeUtc + originalDuration;
        if (!string.IsNullOrWhiteSpace(newEndTime) && DateTime.TryParse(newEndTime, out var parsedEnd))
        {
            newEndTimeUtc = parsedEnd;
        }

        AppointmentResponse result;
        try
        {
            result = await _appointmentsClient.UpdateAppointmentAsync(
                appointmentId.Value,
                new UpdateAppointmentClientRequest
                {
                    Title = existing.Title,
                    Type = existing.Type,
                    StartTimeUtc = newStartTimeUtc,
                    EndTimeUtc = newEndTimeUtc,
                    Description = existing.Description,
                    Location = existing.Location,
                    PropertyId = existing.PropertyId,
                    ContactId = existing.ContactId,
                    Notes = existing.Notes
                },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reschedule appointment {AppointmentId}", appointmentId.Value);

            return ActionResult.Fail(
                ["Failed to reschedule the appointment."],
                ActionType.RescheduleAppointment,
                "I encountered an error while rescheduling. Please try again later.");
        }

        var confirmationMessage = $"Rescheduled \"{result.Title}\" to {result.StartTimeUtc:yyyy-MM-dd HH:mm} - {result.EndTimeUtc:HH:mm}.";

        _logger.LogInformation(
            "RescheduleAppointment action completed: appointment {AppointmentId} for tenant {TenantId}",
            appointmentId.Value, request.TenantId);

        return ActionResult.Ok(
            data: new { result.Id, result.Title, result.StartTimeUtc, result.EndTimeUtc, result.Status },
            message: confirmationMessage,
            type: ActionType.RescheduleAppointment);
    }
}
