// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.AppointmentsApi.Application.Appointments.Dtos;

namespace Propely.AppointmentsApi.Application.Appointments.Commands.MarkNoShowAppointment;

public sealed record MarkNoShowAppointmentCommand(Guid AppointmentId, Guid TenantId) : IRequest<AppointmentDto>;
