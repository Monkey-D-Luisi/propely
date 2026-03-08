// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.AppointmentsApi.Application.Appointments.Interfaces;

namespace Propely.AppointmentsApi.Application.Appointments.Queries.CountAppointmentsByStatus;

public sealed record CountAppointmentsByStatusQuery(Guid TenantId) : IRequest<Dictionary<string, int>>;

public sealed class CountAppointmentsByStatusQueryHandler : IRequestHandler<CountAppointmentsByStatusQuery, Dictionary<string, int>>
{
    private readonly IAppointmentReadRepository _readRepository;

    public CountAppointmentsByStatusQueryHandler(IAppointmentReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<Dictionary<string, int>> Handle(CountAppointmentsByStatusQuery request, CancellationToken cancellationToken)
    {
        var counts = await _readRepository.CountByStatusAsync(request.TenantId, cancellationToken);

        return counts.ToDictionary(
            kvp => kvp.Key.ToString(),
            kvp => kvp.Value);
    }
}
