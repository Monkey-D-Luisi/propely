// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.AppointmentsApi.Application.Appointments.Dtos;

namespace Propely.AppointmentsApi.Application.Appointments.Queries.GetCalendarStatus;

public sealed record GetCalendarStatusQuery(Guid AgentId, Guid TenantId) : IRequest<IReadOnlyList<CalendarConnectionDto>>;
