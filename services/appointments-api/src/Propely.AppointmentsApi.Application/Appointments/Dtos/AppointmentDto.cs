// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AppointmentsApi.Domain.Appointments;

namespace Propely.AppointmentsApi.Application.Appointments.Dtos;

public sealed record AppointmentDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public string? Description { get; init; }
    public AppointmentType Type { get; init; }
    public AppointmentStatus Status { get; init; }
    public DateTime StartTimeUtc { get; init; }
    public DateTime EndTimeUtc { get; init; }
    public string? Location { get; init; }
    public bool IsAllDay { get; init; }
    public Guid? PropertyId { get; init; }
    public Guid? ContactId { get; init; }
    public Guid AgentId { get; init; }
    public Guid TenantId { get; init; }
    public string? CancellationReason { get; init; }
    public string? Notes { get; init; }
    public IReadOnlyCollection<CalendarSyncInfoDto> CalendarSyncInfos { get; init; } = [];
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
}

public sealed record CalendarSyncInfoDto
{
    public string ExternalEventId { get; init; } = null!;
    public CalendarProvider Provider { get; init; }
    public DateTime LastSyncedUtc { get; init; }
}

public sealed record AppointmentListItemDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public AppointmentType Type { get; init; }
    public AppointmentStatus Status { get; init; }
    public DateTime StartTimeUtc { get; init; }
    public DateTime EndTimeUtc { get; init; }
    public string? Location { get; init; }
    public bool IsAllDay { get; init; }
    public Guid? PropertyId { get; init; }
    public Guid? ContactId { get; init; }
    public Guid AgentId { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}
