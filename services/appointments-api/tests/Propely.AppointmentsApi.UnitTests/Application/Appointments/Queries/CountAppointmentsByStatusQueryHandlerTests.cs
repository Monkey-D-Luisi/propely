// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using NSubstitute;
using Propely.AppointmentsApi.Application.Appointments.Interfaces;
using Propely.AppointmentsApi.Application.Appointments.Queries.CountAppointmentsByStatus;
using Propely.AppointmentsApi.Domain.Appointments;

namespace Propely.AppointmentsApi.UnitTests.Application.Appointments.Queries;

public class CountAppointmentsByStatusQueryHandlerTests
{
    private readonly IAppointmentReadRepository _readRepository = Substitute.For<IAppointmentReadRepository>();
    private readonly CountAppointmentsByStatusQueryHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();

    public CountAppointmentsByStatusQueryHandlerTests()
    {
        _handler = new CountAppointmentsByStatusQueryHandler(_readRepository);
    }

    [Fact]
    public async Task Handle_ReturnsCountsWithStringKeys()
    {
        var counts = new Dictionary<AppointmentStatus, int>
        {
            { AppointmentStatus.Scheduled, 4 },
            { AppointmentStatus.Confirmed, 2 },
            { AppointmentStatus.Completed, 7 },
            { AppointmentStatus.Cancelled, 1 }
        };

        _readRepository.CountByStatusAsync(TenantId, Arg.Any<CancellationToken>())
            .Returns(counts);

        var query = new CountAppointmentsByStatusQuery(TenantId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(4);
        result["Scheduled"].Should().Be(4);
        result["Confirmed"].Should().Be(2);
        result["Completed"].Should().Be(7);
        result["Cancelled"].Should().Be(1);
    }

    [Fact]
    public async Task Handle_EmptyResults_ReturnsEmptyDictionary()
    {
        _readRepository.CountByStatusAsync(TenantId, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<AppointmentStatus, int>());

        var query = new CountAppointmentsByStatusQuery(TenantId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_PassesTenantIdToRepository()
    {
        _readRepository.CountByStatusAsync(TenantId, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<AppointmentStatus, int>());

        var query = new CountAppointmentsByStatusQuery(TenantId);
        await _handler.Handle(query, CancellationToken.None);

        await _readRepository.Received(1).CountByStatusAsync(TenantId, Arg.Any<CancellationToken>());
    }
}
