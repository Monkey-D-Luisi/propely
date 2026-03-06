// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.AppointmentsApi.Application.Appointments.Dtos;
using Propely.AppointmentsApi.Application.Common.Models;
using Propely.AppointmentsApi.Domain.Appointments;

namespace Propely.AppointmentsApi.Application.Appointments.Queries.ListAppointments;

public sealed record ListAppointmentsQuery : IRequest<PagedResult<AppointmentListItemDto>>
{
    public Guid TenantId { get; init; }
    public string? Search { get; init; }
    public AppointmentStatus? Status { get; init; }
    public AppointmentType? Type { get; init; }
    public Guid? AgentId { get; init; }
    public Guid? PropertyId { get; init; }
    public Guid? ContactId { get; init; }
    public DateTime? FromUtc { get; init; }
    public DateTime? ToUtc { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
