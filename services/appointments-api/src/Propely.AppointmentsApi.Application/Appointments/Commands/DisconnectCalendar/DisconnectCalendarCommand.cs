// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.AppointmentsApi.Domain.Appointments;

namespace Propely.AppointmentsApi.Application.Appointments.Commands.DisconnectCalendar;

public sealed record DisconnectCalendarCommand(Guid AgentId, Guid TenantId, CalendarProvider Provider) : IRequest<Unit>;
