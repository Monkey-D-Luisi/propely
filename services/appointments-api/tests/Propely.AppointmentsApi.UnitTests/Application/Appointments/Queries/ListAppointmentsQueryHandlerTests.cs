// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using NSubstitute;
using Propely.AppointmentsApi.Application.Appointments.Interfaces;
using Propely.AppointmentsApi.Application.Appointments.Queries.ListAppointments;
using Propely.AppointmentsApi.Application.Common.Models;
using Propely.AppointmentsApi.Domain.Appointments;

namespace Propely.AppointmentsApi.UnitTests.Application.Appointments.Queries;

public class ListAppointmentsQueryHandlerTests
{
    private readonly IAppointmentReadRepository _appointmentReadRepository = Substitute.For<IAppointmentReadRepository>();
    private readonly ListAppointmentsQueryHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AgentId = Guid.NewGuid();

    public ListAppointmentsQueryHandlerTests()
    {
        _handler = new ListAppointmentsQueryHandler(_appointmentReadRepository);
    }

    [Fact]
    public async Task Handle_ReturnsPagedResultWithMappedItems()
    {
        var start = DateTime.UtcNow.AddDays(1);
        var end = start.AddHours(1);
        var appointment = Appointment.Create(
            "Test Appointment", AppointmentType.Generic, start, end,
            AgentId, TenantId);

        var pagedAppointments = new PagedResult<Appointment>(
            [appointment], 1, 1, 20);

        _appointmentReadRepository.ListAsync(Arg.Any<AppointmentListFilter>(), Arg.Any<CancellationToken>())
            .Returns(pagedAppointments);

        var query = new ListAppointmentsQuery
        {
            TenantId = TenantId,
            Page = 1,
            PageSize = 20
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
        result.PageNumber.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().Title.Should().Be("Test Appointment");
        result.Items.First().Type.Should().Be(AppointmentType.Generic);
        result.Items.First().AgentId.Should().Be(AgentId);
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyPagedResult()
    {
        var pagedAppointments = new PagedResult<Appointment>(
            [], 0, 1, 20);

        _appointmentReadRepository.ListAsync(Arg.Any<AppointmentListFilter>(), Arg.Any<CancellationToken>())
            .Returns(pagedAppointments);

        var query = new ListAppointmentsQuery
        {
            TenantId = TenantId,
            Page = 1,
            PageSize = 20
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_PassesFilterCorrectly()
    {
        var fromUtc = DateTime.UtcNow;
        var toUtc = DateTime.UtcNow.AddDays(7);
        var propertyId = Guid.NewGuid();

        var pagedAppointments = new PagedResult<Appointment>([], 0, 1, 20);
        _appointmentReadRepository.ListAsync(Arg.Any<AppointmentListFilter>(), Arg.Any<CancellationToken>())
            .Returns(pagedAppointments);

        var query = new ListAppointmentsQuery
        {
            TenantId = TenantId,
            Search = "viewing",
            Status = AppointmentStatus.Scheduled,
            Type = AppointmentType.PropertyViewing,
            AgentId = AgentId,
            PropertyId = propertyId,
            FromUtc = fromUtc,
            ToUtc = toUtc,
            SortBy = "startTimeUtc",
            SortDescending = true,
            Page = 2,
            PageSize = 10
        };

        await _handler.Handle(query, CancellationToken.None);

        await _appointmentReadRepository.Received(1).ListAsync(
            Arg.Is<AppointmentListFilter>(f =>
                f.TenantId == TenantId &&
                f.Search == "viewing" &&
                f.Status == AppointmentStatus.Scheduled &&
                f.Type == AppointmentType.PropertyViewing &&
                f.AgentId == AgentId &&
                f.PropertyId == propertyId &&
                f.FromUtc == fromUtc &&
                f.ToUtc == toUtc &&
                f.SortBy == "startTimeUtc" &&
                f.SortDescending == true &&
                f.Page == 2 &&
                f.PageSize == 10),
            Arg.Any<CancellationToken>());
    }
}
