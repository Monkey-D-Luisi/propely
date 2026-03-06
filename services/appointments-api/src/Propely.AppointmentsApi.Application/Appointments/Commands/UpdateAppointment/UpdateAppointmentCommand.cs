// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.AppointmentsApi.Application.Appointments.Dtos;

namespace Propely.AppointmentsApi.Application.Appointments.Commands.UpdateAppointment;

public sealed record UpdateAppointmentCommand : IRequest<AppointmentDto>
{
    public Guid AppointmentId { get; init; }
    public Guid TenantId { get; init; }
    public string Title { get; init; } = null!;
    public DateTime StartTimeUtc { get; init; }
    public DateTime EndTimeUtc { get; init; }
    public string? Description { get; init; }
    public string? Location { get; init; }
    public bool IsAllDay { get; init; }
    public Guid? PropertyId { get; init; }
    public Guid? ContactId { get; init; }
    public string? Notes { get; init; }
}
