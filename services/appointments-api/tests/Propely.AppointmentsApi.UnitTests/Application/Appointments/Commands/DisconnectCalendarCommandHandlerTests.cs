// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using NSubstitute;
using Propely.AppointmentsApi.Application.Appointments.Commands.DisconnectCalendar;
using Propely.AppointmentsApi.Application.Appointments.Interfaces;
using Propely.AppointmentsApi.Application.Common.Interfaces;
using Propely.AppointmentsApi.Domain.Appointments;
using Propely.AppointmentsApi.Domain.Common.Exceptions;

namespace Propely.AppointmentsApi.UnitTests.Application.Appointments.Commands;

public class DisconnectCalendarCommandHandlerTests
{
    private readonly ICalendarConnectionRepository _connectionRepository = Substitute.For<ICalendarConnectionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly DisconnectCalendarCommandHandler _handler;

    private static readonly Guid AgentId = Guid.NewGuid();
    private static readonly Guid TenantId = Guid.NewGuid();

    public DisconnectCalendarCommandHandlerTests()
    {
        _handler = new DisconnectCalendarCommandHandler(_connectionRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ExistingConnection_SoftDeletesAndReturnsUnit()
    {
        var connection = CalendarConnection.Create(
            AgentId, TenantId, CalendarProvider.Google, "token", "refresh", DateTime.UtcNow.AddHours(1), "primary");

        _connectionRepository.GetByAgentAndProviderAsync(
            AgentId, TenantId, CalendarProvider.Google, Arg.Any<CancellationToken>())
            .Returns(connection);

        var command = new DisconnectCalendarCommand(AgentId, TenantId, CalendarProvider.Google);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(MediatR.Unit.Value);
        connection.IsDeleted.Should().BeTrue();
        connection.SyncState.Should().Be(CalendarSyncState.Disabled);
        _connectionRepository.Received(1).Update(connection);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NonExistentConnection_ThrowsNotFoundException()
    {
        _connectionRepository.GetByAgentAndProviderAsync(
            AgentId, TenantId, CalendarProvider.Google, Arg.Any<CancellationToken>())
            .Returns((CalendarConnection?)null);

        var command = new DisconnectCalendarCommand(AgentId, TenantId, CalendarProvider.Google);
        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Google*calendar connection*found*");
    }
}
