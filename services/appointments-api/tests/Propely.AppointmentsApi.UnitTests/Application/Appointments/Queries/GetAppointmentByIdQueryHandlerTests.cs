// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using NSubstitute;
using Propely.AppointmentsApi.Application.Appointments.Interfaces;
using Propely.AppointmentsApi.Application.Appointments.Queries.GetAppointmentById;
using Propely.AppointmentsApi.Domain.Appointments;

namespace Propely.AppointmentsApi.UnitTests.Application.Appointments.Queries;

public class GetAppointmentByIdQueryHandlerTests
{
    private readonly IAppointmentReadRepository _appointmentReadRepository = Substitute.For<IAppointmentReadRepository>();
    private readonly GetAppointmentByIdQueryHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AgentId = Guid.NewGuid();

    public GetAppointmentByIdQueryHandlerTests()
    {
        _handler = new GetAppointmentByIdQueryHandler(_appointmentReadRepository);
    }

    [Fact]
    public async Task Handle_AppointmentFound_ReturnsDtoWithCorrectValues()
    {
        var start = DateTime.UtcNow.AddDays(1);
        var end = start.AddHours(1);
        var appointment = Appointment.Create(
            "Property Viewing", AppointmentType.Generic, start, end,
            AgentId, TenantId, description: "Test desc", location: "Office");

        _appointmentReadRepository.GetByIdAsync(appointment.Id, TenantId, Arg.Any<CancellationToken>())
            .Returns(appointment);

        var query = new GetAppointmentByIdQuery(appointment.Id, TenantId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(appointment.Id);
        result.Title.Should().Be("Property Viewing");
        result.Type.Should().Be(AppointmentType.Generic);
        result.Status.Should().Be(AppointmentStatus.Scheduled);
        result.StartTimeUtc.Should().Be(start);
        result.EndTimeUtc.Should().Be(end);
        result.AgentId.Should().Be(AgentId);
        result.TenantId.Should().Be(TenantId);
        result.Description.Should().Be("Test desc");
        result.Location.Should().Be("Office");
    }

    [Fact]
    public async Task Handle_AppointmentNotFound_ReturnsNull()
    {
        _appointmentReadRepository.GetByIdAsync(Arg.Any<Guid>(), TenantId, Arg.Any<CancellationToken>())
            .Returns((Appointment?)null);

        var query = new GetAppointmentByIdQuery(Guid.NewGuid(), TenantId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }
}
