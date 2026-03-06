// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using NSubstitute;
using Propely.AppointmentsApi.Application.Appointments.Commands.UpdateAppointment;
using Propely.AppointmentsApi.Application.Appointments.Interfaces;
using Propely.AppointmentsApi.Application.Common.Interfaces;
using Propely.AppointmentsApi.Domain.Appointments;
using Propely.AppointmentsApi.Domain.Common.Exceptions;

namespace Propely.AppointmentsApi.UnitTests.Application.Appointments.Commands;

public class UpdateAppointmentCommandHandlerTests
{
    private readonly IAppointmentRepository _appointmentRepository = Substitute.For<IAppointmentRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateAppointmentCommandHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AgentId = Guid.NewGuid();
    private static readonly Guid AppointmentId = Guid.NewGuid();

    public UpdateAppointmentCommandHandlerTests()
    {
        _handler = new UpdateAppointmentCommandHandler(_appointmentRepository, _unitOfWork);
    }

    private static Appointment CreateScheduledAppointment()
    {
        var start = DateTime.UtcNow.AddDays(1);
        var end = start.AddHours(1);
        return Appointment.Create(
            "Original Title", AppointmentType.Generic, start, end,
            AgentId, TenantId);
    }

    [Fact]
    public async Task Handle_ExistingAppointment_UpdatesAndReturnsDto()
    {
        var appointment = CreateScheduledAppointment();
        _appointmentRepository.GetByIdAsync(Arg.Any<Guid>(), TenantId, Arg.Any<CancellationToken>())
            .Returns(appointment);

        var newStart = DateTime.UtcNow.AddDays(2);
        var newEnd = newStart.AddHours(2);

        var command = new UpdateAppointmentCommand
        {
            AppointmentId = appointment.Id,
            TenantId = TenantId,
            Title = "Updated Title",
            StartTimeUtc = newStart,
            EndTimeUtc = newEnd,
            Description = "Updated description",
            Location = "New location"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Title.Should().Be("Updated Title");
        result.StartTimeUtc.Should().Be(newStart);
        result.EndTimeUtc.Should().Be(newEnd);
        result.Description.Should().Be("Updated description");
        result.Location.Should().Be("New location");

        _appointmentRepository.Received(1).Update(appointment);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AppointmentNotFound_ThrowsNotFoundException()
    {
        _appointmentRepository.GetByIdAsync(Arg.Any<Guid>(), TenantId, Arg.Any<CancellationToken>())
            .Returns((Appointment?)null);

        var command = new UpdateAppointmentCommand
        {
            AppointmentId = Guid.NewGuid(),
            TenantId = TenantId,
            Title = "Updated Title",
            StartTimeUtc = DateTime.UtcNow.AddDays(1),
            EndTimeUtc = DateTime.UtcNow.AddDays(1).AddHours(1)
        };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
