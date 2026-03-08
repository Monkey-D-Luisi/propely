// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AppointmentsApi.Domain.Appointments;

namespace Propely.AppointmentsApi.Api.Dtos;

public sealed record CreateAppointmentApiRequest
{
    public string Title { get; init; } = null!;
    public AppointmentType Type { get; init; }
    public DateTime StartTimeUtc { get; init; }
    public DateTime EndTimeUtc { get; init; }
    public string? Description { get; init; }
    public string? Location { get; init; }
    public bool? IsAllDay { get; init; }
    public Guid? PropertyId { get; init; }
    public Guid? ContactId { get; init; }
    public string? Notes { get; init; }
}

public sealed record UpdateAppointmentApiRequest
{
    public string Title { get; init; } = null!;
    public DateTime StartTimeUtc { get; init; }
    public DateTime EndTimeUtc { get; init; }
    public string? Description { get; init; }
    public string? Location { get; init; }
    public bool? IsAllDay { get; init; }
    public Guid? PropertyId { get; init; }
    public Guid? ContactId { get; init; }
    public string? Notes { get; init; }
}

public sealed record CancelAppointmentApiRequest
{
    public string Reason { get; init; } = null!;
}

public sealed record CompleteAppointmentApiRequest
{
    public string? Notes { get; init; }
}

public sealed record CountUpcomingApiResponse(int Count);
