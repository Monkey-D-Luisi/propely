// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using NSubstitute;
using Propely.AppointmentsApi.Application.Appointments.Interfaces;
using Propely.AppointmentsApi.Application.Appointments.Queries.CountUpcomingAppointments;

namespace Propely.AppointmentsApi.UnitTests.Application.Appointments.Queries;

public class CountUpcomingAppointmentsQueryHandlerTests
{
    private readonly IAppointmentReadRepository _readRepository = Substitute.For<IAppointmentReadRepository>();
    private readonly CountUpcomingAppointmentsQueryHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();

    public CountUpcomingAppointmentsQueryHandlerTests()
    {
        _handler = new CountUpcomingAppointmentsQueryHandler(_readRepository);
    }

    [Fact]
    public async Task Handle_ReturnsUpcomingCount()
    {
        _readRepository.CountUpcomingAsync(TenantId, 7, Arg.Any<CancellationToken>())
            .Returns(5);

        var query = new CountUpcomingAppointmentsQuery(TenantId, 7);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().Be(5);
    }

    [Fact]
    public async Task Handle_DefaultDaysIsSeven()
    {
        _readRepository.CountUpcomingAsync(TenantId, 7, Arg.Any<CancellationToken>())
            .Returns(0);

        var query = new CountUpcomingAppointmentsQuery(TenantId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().Be(0);
        await _readRepository.Received(1).CountUpcomingAsync(TenantId, 7, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CustomDays_PassedToRepository()
    {
        _readRepository.CountUpcomingAsync(TenantId, 14, Arg.Any<CancellationToken>())
            .Returns(3);

        var query = new CountUpcomingAppointmentsQuery(TenantId, 14);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().Be(3);
        await _readRepository.Received(1).CountUpcomingAsync(TenantId, 14, Arg.Any<CancellationToken>());
    }
}
