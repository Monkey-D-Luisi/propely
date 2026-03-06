// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AppointmentsApi.Domain.Appointments;

namespace Propely.AppointmentsApi.Application.Appointments.Dtos;

public sealed record CalendarConnectionDto
{
    public Guid Id { get; init; }
    public Guid AgentId { get; init; }
    public CalendarProvider Provider { get; init; }
    public CalendarSyncState SyncState { get; init; }
    public DateTime? LastSyncedUtc { get; init; }
    public string ExternalCalendarId { get; init; } = null!;
    public DateTime CreatedAtUtc { get; init; }
}
