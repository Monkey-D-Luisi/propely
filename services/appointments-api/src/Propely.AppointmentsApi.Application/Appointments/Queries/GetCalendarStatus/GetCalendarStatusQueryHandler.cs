// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.AppointmentsApi.Application.Appointments.Dtos;
using Propely.AppointmentsApi.Application.Appointments.Interfaces;

namespace Propely.AppointmentsApi.Application.Appointments.Queries.GetCalendarStatus;

public sealed class GetCalendarStatusQueryHandler : IRequestHandler<GetCalendarStatusQuery, IReadOnlyList<CalendarConnectionDto>>
{
    private readonly ICalendarConnectionRepository _connectionRepository;

    public GetCalendarStatusQueryHandler(ICalendarConnectionRepository connectionRepository)
    {
        _connectionRepository = connectionRepository;
    }

    public async Task<IReadOnlyList<CalendarConnectionDto>> Handle(GetCalendarStatusQuery request, CancellationToken cancellationToken)
    {
        var connections = await _connectionRepository.GetActiveByAgentAsync(
            request.AgentId, request.TenantId, cancellationToken);

        return connections.Select(CalendarConnectionMapper.ToDto).ToList();
    }
}
