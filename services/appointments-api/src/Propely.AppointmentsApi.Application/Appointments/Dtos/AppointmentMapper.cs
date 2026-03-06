// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AppointmentsApi.Domain.Appointments;

namespace Propely.AppointmentsApi.Application.Appointments.Dtos;

public static class AppointmentMapper
{
    public static AppointmentDto ToDto(Appointment appointment) => new()
    {
        Id = appointment.Id,
        Title = appointment.Title,
        Description = appointment.Description,
        Type = appointment.Type,
        Status = appointment.Status,
        StartTimeUtc = appointment.StartTimeUtc,
        EndTimeUtc = appointment.EndTimeUtc,
        Location = appointment.Location,
        IsAllDay = appointment.IsAllDay,
        PropertyId = appointment.PropertyId,
        ContactId = appointment.ContactId,
        AgentId = appointment.AgentId,
        TenantId = appointment.TenantId,
        CancellationReason = appointment.CancellationReason,
        Notes = appointment.Notes,
        CalendarSyncInfos = appointment.CalendarSyncInfos.Select(s => new CalendarSyncInfoDto
        {
            ExternalEventId = s.ExternalEventId,
            Provider = s.Provider,
            LastSyncedUtc = s.LastSyncedUtc
        }).ToList(),
        CreatedAtUtc = appointment.CreatedAtUtc,
        UpdatedAtUtc = appointment.UpdatedAtUtc
    };

    public static AppointmentListItemDto ToListItemDto(Appointment appointment) => new()
    {
        Id = appointment.Id,
        Title = appointment.Title,
        Type = appointment.Type,
        Status = appointment.Status,
        StartTimeUtc = appointment.StartTimeUtc,
        EndTimeUtc = appointment.EndTimeUtc,
        Location = appointment.Location,
        IsAllDay = appointment.IsAllDay,
        PropertyId = appointment.PropertyId,
        ContactId = appointment.ContactId,
        AgentId = appointment.AgentId,
        CreatedAtUtc = appointment.CreatedAtUtc
    };
}
