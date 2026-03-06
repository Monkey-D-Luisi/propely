// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using NSubstitute;
using Propely.AppointmentsApi.Application.Appointments.Commands.CreateAppointment;
using Propely.AppointmentsApi.Application.Appointments.Interfaces;
using Propely.AppointmentsApi.Application.Common.Interfaces;
using Propely.AppointmentsApi.Domain.Appointments;

namespace Propely.AppointmentsApi.UnitTests.Application.Appointments.Commands;

public class CreateAppointmentCommandHandlerTests
{
    private readonly IAppointmentRepository _appointmentRepository = Substitute.For<IAppointmentRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateAppointmentCommandHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AgentId = Guid.NewGuid();
    private static readonly Guid PropertyId = Guid.NewGuid();

    public CreateAppointmentCommandHandlerTests()
    {
        _handler = new CreateAppointmentCommandHandler(_appointmentRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesAppointmentAndReturnsDto()
    {
        var start = DateTime.UtcNow.AddDays(1);
        var end = start.AddHours(1);

        var command = new CreateAppointmentCommand
        {
            Title = "Property Viewing - Calle Mayor",
            Type = AppointmentType.PropertyViewing,
            StartTimeUtc = start,
            EndTimeUtc = end,
            AgentId = AgentId,
            TenantId = TenantId,
            Description = "Viewing for client",
            Location = "Calle Mayor 10",
            IsAllDay = false,
            PropertyId = PropertyId,
            Notes = "Bring documents"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Title.Should().Be("Property Viewing - Calle Mayor");
        result.Type.Should().Be(AppointmentType.PropertyViewing);
        result.Status.Should().Be(AppointmentStatus.Scheduled);
        result.StartTimeUtc.Should().Be(start);
        result.EndTimeUtc.Should().Be(end);
        result.AgentId.Should().Be(AgentId);
        result.TenantId.Should().Be(TenantId);
        result.PropertyId.Should().Be(PropertyId);
        result.Description.Should().Be("Viewing for client");
        result.Location.Should().Be("Calle Mayor 10");
        result.Notes.Should().Be("Bring documents");

        await _appointmentRepository.Received(1).AddAsync(Arg.Any<Appointment>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GenericAppointment_DoesNotRequirePropertyId()
    {
        var start = DateTime.UtcNow.AddDays(1);
        var end = start.AddHours(1);

        var command = new CreateAppointmentCommand
        {
            Title = "Team Meeting",
            Type = AppointmentType.Generic,
            StartTimeUtc = start,
            EndTimeUtc = end,
            AgentId = AgentId,
            TenantId = TenantId
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.PropertyId.Should().BeNull();
        result.Type.Should().Be(AppointmentType.Generic);
    }
}
