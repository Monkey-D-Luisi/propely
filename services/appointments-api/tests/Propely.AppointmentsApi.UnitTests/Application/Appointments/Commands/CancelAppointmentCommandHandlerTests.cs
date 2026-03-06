// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using NSubstitute;
using Propely.AppointmentsApi.Application.Appointments.Commands.CancelAppointment;
using Propely.AppointmentsApi.Application.Appointments.Interfaces;
using Propely.AppointmentsApi.Application.Common.Interfaces;
using Propely.AppointmentsApi.Domain.Appointments;
using Propely.AppointmentsApi.Domain.Common.Exceptions;

namespace Propely.AppointmentsApi.UnitTests.Application.Appointments.Commands;

public class CancelAppointmentCommandHandlerTests
{
    private readonly IAppointmentRepository _appointmentRepository = Substitute.For<IAppointmentRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CancelAppointmentCommandHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AgentId = Guid.NewGuid();

    public CancelAppointmentCommandHandlerTests()
    {
        _handler = new CancelAppointmentCommandHandler(_appointmentRepository, _unitOfWork);
    }

    private static Appointment CreateScheduledAppointment()
    {
        var start = DateTime.UtcNow.AddDays(1);
        var end = start.AddHours(1);
        return Appointment.Create(
            "Test Appointment", AppointmentType.Generic, start, end,
            AgentId, TenantId);
    }

    [Fact]
    public async Task Handle_ExistingAppointment_CancelsWithReasonAndReturnsDto()
    {
        var appointment = CreateScheduledAppointment();
        _appointmentRepository.GetByIdAsync(appointment.Id, TenantId, Arg.Any<CancellationToken>())
            .Returns(appointment);

        var command = new CancelAppointmentCommand
        {
            AppointmentId = appointment.Id,
            TenantId = TenantId,
            Reason = "Client unavailable"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Status.Should().Be(AppointmentStatus.Cancelled);
        result.CancellationReason.Should().Be("Client unavailable");

        _appointmentRepository.Received(1).Update(appointment);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AppointmentNotFound_ThrowsNotFoundException()
    {
        _appointmentRepository.GetByIdAsync(Arg.Any<Guid>(), TenantId, Arg.Any<CancellationToken>())
            .Returns((Appointment?)null);

        var command = new CancelAppointmentCommand
        {
            AppointmentId = Guid.NewGuid(),
            TenantId = TenantId,
            Reason = "No longer needed"
        };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
