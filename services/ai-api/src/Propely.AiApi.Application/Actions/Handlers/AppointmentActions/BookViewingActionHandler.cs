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
/// Handles the BookViewingActionCommand by creating a PropertyViewing appointment
/// via the Appointments API SDK.
/// </summary>
public sealed class BookViewingActionHandler : IRequestHandler<BookViewingActionCommand, ActionResult>
{
    private const int DefaultViewingDurationMinutes = 30;

    private readonly IAppointmentsApiClient _appointmentsClient;
    private readonly ILogger<BookViewingActionHandler> _logger;

    public BookViewingActionHandler(
        IAppointmentsApiClient appointmentsClient,
        ILogger<BookViewingActionHandler> logger)
    {
        _appointmentsClient = appointmentsClient;
        _logger = logger;
    }

    public async Task<ActionResult> Handle(BookViewingActionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling BookViewing action for tenant {TenantId} by agent {AgentId}",
            request.TenantId, request.AgentId);

        var parameters = request.Parameters;

        var propertyId = ParameterExtractor.GetGuid(parameters, "property_id");
        var contactId = ParameterExtractor.GetGuid(parameters, "contact_id");
        var startTimeUtcParsed = ParameterExtractor.GetDateTimeUtc(parameters, "start_time");
        var endTimeUtcParsed = ParameterExtractor.GetDateTimeUtc(parameters, "end_time");
        var title = ParameterExtractor.GetString(parameters, "title");
        var location = ParameterExtractor.GetString(parameters, "location");
        var notes = ParameterExtractor.GetString(parameters, "notes");

        if (!propertyId.HasValue)
        {
            return ActionResult.Fail(
                ["A property ID is required to book a viewing."],
                ActionType.BookViewing,
                "I need to know which property the viewing is for. Could you specify the property?");
        }

        if (!startTimeUtcParsed.HasValue)
        {
            return ActionResult.Fail(
                ["A valid start time is required to book a viewing."],
                ActionType.BookViewing,
                "I need to know when to schedule the viewing. Could you provide a date and time like '2026-03-15 10:00'?");
        }

        var startTimeUtc = startTimeUtcParsed.Value;
        var endTimeUtc = endTimeUtcParsed ?? startTimeUtc.AddMinutes(DefaultViewingDurationMinutes);

        AppointmentResponse result;
        try
        {
            result = await _appointmentsClient.CreateAppointmentAsync(new CreateAppointmentClientRequest
            {
                Title = title ?? "Property Viewing",
                Type = "PropertyViewing",
                StartTimeUtc = startTimeUtc,
                EndTimeUtc = endTimeUtc,
                PropertyId = propertyId,
                ContactId = contactId,
                Location = location,
                Notes = notes
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create appointment via Appointments API");

            return ActionResult.Fail(
                ["Failed to book the viewing. The Appointments service may be unavailable."],
                ActionType.BookViewing,
                "I encountered an error while booking the viewing. Please try again later.");
        }

        var confirmationMessage = $"Booked viewing for {result.StartTimeUtc:yyyy-MM-dd HH:mm} to {result.EndTimeUtc:HH:mm}.";

        _logger.LogInformation(
            "BookViewing action completed: appointment {AppointmentId} for tenant {TenantId}",
            result.Id, request.TenantId);

        return ActionResult.Ok(
            data: new { result.Id, result.Title, result.StartTimeUtc, result.EndTimeUtc, result.PropertyId, result.ContactId, result.Status },
            message: confirmationMessage,
            type: ActionType.BookViewing);
    }
}
