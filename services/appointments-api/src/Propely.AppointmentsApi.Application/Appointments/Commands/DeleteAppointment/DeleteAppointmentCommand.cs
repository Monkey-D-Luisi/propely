// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;

namespace Propely.AppointmentsApi.Application.Appointments.Commands.DeleteAppointment;

public sealed record DeleteAppointmentCommand(Guid AppointmentId, Guid TenantId) : IRequest;
