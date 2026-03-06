// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using NSubstitute;
using Propely.AppointmentsApi.Application.Appointments.Commands.MarkNoShowAppointment;
using Propely.AppointmentsApi.Application.Appointments.Interfaces;
using Propely.AppointmentsApi.Application.Common.Interfaces;
using Propely.AppointmentsApi.Domain.Appointments;
using Propely.AppointmentsApi.Domain.Common.Exceptions;

namespace Propely.AppointmentsApi.UnitTests.Application.Appointments.Commands;

public class MarkNoShowAppointmentCommandHandlerTests
{
    private readonly IAppointmentRepository _appointmentRepository = Substitute.For<IAppointmentRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly MarkNoShowAppointmentCommandHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AgentId = Guid.NewGuid();

    public MarkNoShowAppointmentCommandHandlerTests()
    {
        _handler = new MarkNoShowAppointmentCommandHandler(_appointmentRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ScheduledAppointment_MarksNoShowAndReturnsDto()
    {
        var start = DateTime.UtcNow.AddDays(1);
        var end = start.AddHours(1);
        var appointment = Appointment.Create(
            "Property Viewing", AppointmentType.PropertyViewing,
            start, end, AgentId, TenantId, propertyId: Guid.NewGuid());

        _appointmentRepository.GetByIdAsync(appointment.Id, TenantId, Arg.Any<CancellationToken>())
            .Returns(appointment);

        var command = new MarkNoShowAppointmentCommand(appointment.Id, TenantId);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Status.Should().Be(AppointmentStatus.NoShow);
        result.Id.Should().Be(appointment.Id);
        result.Title.Should().Be("Property Viewing");
        _appointmentRepository.Received(1).Update(appointment);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NonExistentAppointment_ThrowsNotFoundException()
    {
        var appointmentId = Guid.NewGuid();

        _appointmentRepository.GetByIdAsync(appointmentId, TenantId, Arg.Any<CancellationToken>())
            .Returns((Appointment?)null);

        var command = new MarkNoShowAppointmentCommand(appointmentId, TenantId);
        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*'{appointmentId}'*not found*");
    }
}
