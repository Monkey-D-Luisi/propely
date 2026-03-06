// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.AppointmentsApi.Application.Appointments.Dtos;
using Propely.AppointmentsApi.Application.Appointments.Interfaces;
using Propely.AppointmentsApi.Application.Common.Models;

namespace Propely.AppointmentsApi.Application.Appointments.Queries.ListAppointments;

public sealed class ListAppointmentsQueryHandler : IRequestHandler<ListAppointmentsQuery, PagedResult<AppointmentListItemDto>>
{
    private readonly IAppointmentReadRepository _appointmentReadRepository;

    public ListAppointmentsQueryHandler(IAppointmentReadRepository appointmentReadRepository)
    {
        _appointmentReadRepository = appointmentReadRepository;
    }

    public async Task<PagedResult<AppointmentListItemDto>> Handle(ListAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var filter = new AppointmentListFilter
        {
            TenantId = request.TenantId,
            Search = request.Search,
            Status = request.Status,
            Type = request.Type,
            AgentId = request.AgentId,
            PropertyId = request.PropertyId,
            ContactId = request.ContactId,
            FromUtc = request.FromUtc,
            ToUtc = request.ToUtc,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending,
            Page = request.Page,
            PageSize = request.PageSize
        };

        var result = await _appointmentReadRepository.ListAsync(filter, cancellationToken);

        return new PagedResult<AppointmentListItemDto>(
            result.Items.Select(AppointmentMapper.ToListItemDto).ToList(),
            result.TotalCount,
            result.PageNumber,
            request.PageSize);
    }
}
