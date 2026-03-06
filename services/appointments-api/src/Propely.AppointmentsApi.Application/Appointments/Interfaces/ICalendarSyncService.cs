// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AppointmentsApi.Application.Appointments.Dtos;
using Propely.AppointmentsApi.Domain.Appointments;

namespace Propely.AppointmentsApi.Application.Appointments.Interfaces;

public interface ICalendarSyncService
{
    CalendarProvider Provider { get; }
    Task<string> CreateEventAsync(Appointment appointment, CalendarConnection connection, CancellationToken ct = default);
    Task UpdateEventAsync(Appointment appointment, CalendarConnection connection, string externalEventId, CancellationToken ct = default);
    Task DeleteEventAsync(CalendarConnection connection, string externalEventId, CancellationToken ct = default);
    Task<IReadOnlyList<ExternalCalendarChange>> GetChangesAsync(CalendarConnection connection, string? syncToken, CancellationToken ct = default);
}
