// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Microsoft.Extensions.Logging;
using Propely.AppointmentsApi.Application.Appointments.Dtos;
using Propely.AppointmentsApi.Application.Appointments.Interfaces;
using Propely.AppointmentsApi.Domain.Appointments;

namespace Propely.AppointmentsApi.Infrastructure.Calendar;

public sealed class MicrosoftCalendarSyncService : ICalendarSyncService
{
    private readonly ILogger<MicrosoftCalendarSyncService> _logger;

    public MicrosoftCalendarSyncService(ILogger<MicrosoftCalendarSyncService> logger)
    {
        _logger = logger;
    }

    public CalendarProvider Provider => CalendarProvider.Microsoft;

    public Task<string> CreateEventAsync(Appointment appointment, CalendarConnection connection, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "MicrosoftCalendarSyncService.CreateEventAsync -- Stub: Would create Outlook Calendar event " +
            "for appointment {AppointmentId} (Subject: {Title}, Start: {Start}, End: {End}, " +
            "Body: {Description}, Location: {Location})",
            appointment.Id, appointment.Title, appointment.StartTimeUtc, appointment.EndTimeUtc,
            appointment.Description, appointment.Location);

        // Return a generated external event ID for now
        var externalEventId = $"ms-event-{Guid.NewGuid():N}";
        return Task.FromResult(externalEventId);
    }

    public Task UpdateEventAsync(Appointment appointment, CalendarConnection connection, string externalEventId, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "MicrosoftCalendarSyncService.UpdateEventAsync -- Stub: Would update Outlook Calendar event {ExternalEventId} " +
            "for appointment {AppointmentId} (Subject: {Title})",
            externalEventId, appointment.Id, appointment.Title);

        return Task.CompletedTask;
    }

    public Task DeleteEventAsync(CalendarConnection connection, string externalEventId, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "MicrosoftCalendarSyncService.DeleteEventAsync -- Stub: Would delete Outlook Calendar event {ExternalEventId}",
            externalEventId);

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ExternalCalendarChange>> GetChangesAsync(CalendarConnection connection, string? syncToken, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "MicrosoftCalendarSyncService.GetChangesAsync -- Stub: Would fetch changes from Outlook Calendar " +
            "for connection {ConnectionId} with deltaLink {SyncToken}",
            connection.Id, syncToken ?? "(none)");

        return Task.FromResult<IReadOnlyList<ExternalCalendarChange>>(Array.Empty<ExternalCalendarChange>());
    }
}
