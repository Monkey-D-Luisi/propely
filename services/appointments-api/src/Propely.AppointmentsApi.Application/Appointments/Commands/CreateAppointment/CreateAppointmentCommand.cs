// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.AppointmentsApi.Application.Appointments.Dtos;
using Propely.AppointmentsApi.Domain.Appointments;

namespace Propely.AppointmentsApi.Application.Appointments.Commands.CreateAppointment;

public sealed record CreateAppointmentCommand : IRequest<AppointmentDto>
{
    public string Title { get; init; } = null!;
    public AppointmentType Type { get; init; }
    public DateTime StartTimeUtc { get; init; }
    public DateTime EndTimeUtc { get; init; }
    public Guid AgentId { get; init; }
    public Guid TenantId { get; init; }
    public string? Description { get; init; }
    public string? Location { get; init; }
    public bool IsAllDay { get; init; }
    public Guid? PropertyId { get; init; }
    public Guid? ContactId { get; init; }
    public string? Notes { get; init; }
}
