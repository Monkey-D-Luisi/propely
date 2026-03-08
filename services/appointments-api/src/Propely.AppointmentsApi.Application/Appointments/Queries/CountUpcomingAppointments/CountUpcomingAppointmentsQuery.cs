// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.AppointmentsApi.Application.Appointments.Interfaces;

namespace Propely.AppointmentsApi.Application.Appointments.Queries.CountUpcomingAppointments;

public sealed record CountUpcomingAppointmentsQuery(Guid TenantId, int Days = 7) : IRequest<int>;

public sealed class CountUpcomingAppointmentsQueryHandler : IRequestHandler<CountUpcomingAppointmentsQuery, int>
{
    private readonly IAppointmentReadRepository _readRepository;

    public CountUpcomingAppointmentsQueryHandler(IAppointmentReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<int> Handle(CountUpcomingAppointmentsQuery request, CancellationToken cancellationToken)
    {
        return await _readRepository.CountUpcomingAsync(request.TenantId, request.Days, cancellationToken);
    }
}
