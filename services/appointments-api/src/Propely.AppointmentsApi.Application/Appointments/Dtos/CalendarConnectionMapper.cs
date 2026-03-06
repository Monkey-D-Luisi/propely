// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AppointmentsApi.Domain.Appointments;

namespace Propely.AppointmentsApi.Application.Appointments.Dtos;

public static class CalendarConnectionMapper
{
    public static CalendarConnectionDto ToDto(CalendarConnection connection)
    {
        return new CalendarConnectionDto
        {
            Id = connection.Id,
            AgentId = connection.AgentId,
            Provider = connection.Provider,
            SyncState = connection.SyncState,
            LastSyncedUtc = connection.LastSyncedUtc,
            ExternalCalendarId = connection.ExternalCalendarId,
            CreatedAtUtc = connection.CreatedAtUtc
        };
    }
}
