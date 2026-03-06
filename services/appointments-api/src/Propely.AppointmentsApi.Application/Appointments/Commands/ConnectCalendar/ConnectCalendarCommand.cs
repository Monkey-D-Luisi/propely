// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.AppointmentsApi.Application.Appointments.Dtos;
using Propely.AppointmentsApi.Domain.Appointments;

namespace Propely.AppointmentsApi.Application.Appointments.Commands.ConnectCalendar;

public sealed record ConnectCalendarCommand : IRequest<CalendarConnectionDto>
{
    public Guid AgentId { get; init; }
    public Guid TenantId { get; init; }
    public CalendarProvider Provider { get; init; }
    public string AuthorizationCode { get; init; } = null!;
    public string RedirectUri { get; init; } = null!;
}
