// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using NSubstitute;
using Propely.AppointmentsApi.Application.Appointments.Commands.CompleteAppointment;
using Propely.AppointmentsApi.Application.Appointments.Interfaces;
using Propely.AppointmentsApi.Application.Common.Interfaces;
using Propely.AppointmentsApi.Domain.Appointments;
using Propely.AppointmentsApi.Domain.Common.Exceptions;

namespace Propely.AppointmentsApi.UnitTests.Application.Appointments.Commands;

public class CompleteAppointmentCommandHandlerTests
{
    private readonly IAppointmentRepository _appointmentRepository = Substitute.For<IAppointmentRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CompleteAppointmentCommandHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AgentId = Guid.NewGuid();

    public CompleteAppointmentCommandHandlerTests()
    {
        _handler = new CompleteAppointmentCommandHandler(_appointmentRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ScheduledAppointment_CompletesAndReturnsDto()
    {
        var start = DateTime.UtcNow.AddDays(1);
        var end = start.AddHours(1);
        var appointment = Appointment.Create(
            "Team Meeting", AppointmentType.Generic,
            start, end, AgentId, TenantId);

        _appointmentRepository.GetByIdAsync(appointment.Id, TenantId, Arg.Any<CancellationToken>())
            .Returns(appointment);

        var command = new CompleteAppointmentCommand
        {
            AppointmentId = appointment.Id,
            TenantId = TenantId,
            Notes = "Meeting went well"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Status.Should().Be(AppointmentStatus.Completed);
        result.Notes.Should().Be("Meeting went well");
        _appointmentRepository.Received(1).Update(appointment);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithoutNotes_CompletesSuccessfully()
    {
        var start = DateTime.UtcNow.AddDays(1);
        var end = start.AddHours(1);
        var appointment = Appointment.Create(
            "Team Meeting", AppointmentType.Generic,
            start, end, AgentId, TenantId);

        _appointmentRepository.GetByIdAsync(appointment.Id, TenantId, Arg.Any<CancellationToken>())
            .Returns(appointment);

        var command = new CompleteAppointmentCommand
        {
            AppointmentId = appointment.Id,
            TenantId = TenantId,
            Notes = null
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Status.Should().Be(AppointmentStatus.Completed);
        result.Notes.Should().BeNull();
    }

    [Fact]
    public async Task Handle_NonExistentAppointment_ThrowsNotFoundException()
    {
        var appointmentId = Guid.NewGuid();

        _appointmentRepository.GetByIdAsync(appointmentId, TenantId, Arg.Any<CancellationToken>())
            .Returns((Appointment?)null);

        var command = new CompleteAppointmentCommand
        {
            AppointmentId = appointmentId,
            TenantId = TenantId
        };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*'{appointmentId}'*not found*");
    }
}
