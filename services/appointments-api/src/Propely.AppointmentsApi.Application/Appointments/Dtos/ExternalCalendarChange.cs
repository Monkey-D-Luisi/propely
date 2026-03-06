// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AppointmentsApi.Application.Appointments.Dtos;

public enum ExternalChangeType
{
    Created = 0,
    Updated = 1,
    Deleted = 2
}

public sealed record ExternalCalendarChange
{
    public string ExternalEventId { get; init; } = null!;
    public ExternalChangeType ChangeType { get; init; }
    public string? Title { get; init; }
    public DateTime? StartTimeUtc { get; init; }
    public DateTime? EndTimeUtc { get; init; }
    public string? Description { get; init; }
    public string? Location { get; init; }
    public string? NewSyncToken { get; init; }
}
