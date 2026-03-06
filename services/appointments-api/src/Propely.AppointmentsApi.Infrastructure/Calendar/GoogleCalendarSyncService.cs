// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Microsoft.Extensions.Logging;
using Propely.AppointmentsApi.Application.Appointments.Dtos;
using Propely.AppointmentsApi.Application.Appointments.Interfaces;
using Propely.AppointmentsApi.Domain.Appointments;

namespace Propely.AppointmentsApi.Infrastructure.Calendar;

public sealed class GoogleCalendarSyncService : ICalendarSyncService
{
    private readonly ILogger<GoogleCalendarSyncService> _logger;

    public GoogleCalendarSyncService(ILogger<GoogleCalendarSyncService> logger)
    {
        _logger = logger;
    }

    public CalendarProvider Provider => CalendarProvider.Google;

    public Task<string> CreateEventAsync(Appointment appointment, CalendarConnection connection, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "GoogleCalendarSyncService.CreateEventAsync -- Stub: Would create Google Calendar event " +
            "for appointment {AppointmentId} (Summary: {Title}, Start: {Start}, End: {End}, Description: {Description}, Location: {Location})",
            appointment.Id, appointment.Title, appointment.StartTimeUtc, appointment.EndTimeUtc,
            appointment.Description, appointment.Location);

        // Return a generated external event ID for now
        var externalEventId = $"google-event-{Guid.NewGuid():N}";
        return Task.FromResult(externalEventId);
    }

    public Task UpdateEventAsync(Appointment appointment, CalendarConnection connection, string externalEventId, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "GoogleCalendarSyncService.UpdateEventAsync -- Stub: Would update Google Calendar event {ExternalEventId} " +
            "for appointment {AppointmentId} (Summary: {Title})",
            externalEventId, appointment.Id, appointment.Title);

        return Task.CompletedTask;
    }

    public Task DeleteEventAsync(CalendarConnection connection, string externalEventId, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "GoogleCalendarSyncService.DeleteEventAsync -- Stub: Would delete Google Calendar event {ExternalEventId}",
            externalEventId);

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ExternalCalendarChange>> GetChangesAsync(CalendarConnection connection, string? syncToken, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "GoogleCalendarSyncService.GetChangesAsync -- Stub: Would fetch changes from Google Calendar " +
            "for connection {ConnectionId} with syncToken {SyncToken}",
            connection.Id, syncToken ?? "(none)");

        return Task.FromResult<IReadOnlyList<ExternalCalendarChange>>(Array.Empty<ExternalCalendarChange>());
    }
}
