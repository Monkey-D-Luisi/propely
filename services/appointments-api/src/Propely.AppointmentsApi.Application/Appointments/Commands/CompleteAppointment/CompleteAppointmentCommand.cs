// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.AppointmentsApi.Application.Appointments.Dtos;

namespace Propely.AppointmentsApi.Application.Appointments.Commands.CompleteAppointment;

public sealed record CompleteAppointmentCommand : IRequest<AppointmentDto>
{
    public Guid AppointmentId { get; init; }
    public Guid TenantId { get; init; }
    public string? Notes { get; init; }
}
