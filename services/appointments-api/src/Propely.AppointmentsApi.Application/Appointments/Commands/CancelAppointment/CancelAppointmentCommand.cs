// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.AppointmentsApi.Application.Appointments.Dtos;

namespace Propely.AppointmentsApi.Application.Appointments.Commands.CancelAppointment;

public sealed record CancelAppointmentCommand : IRequest<AppointmentDto>
{
    public Guid AppointmentId { get; init; }
    public Guid TenantId { get; init; }
    public string Reason { get; init; } = null!;
}
