// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using NSubstitute;
using Propely.AppointmentsApi.Application.Appointments.Interfaces;
using Propely.AppointmentsApi.Application.Appointments.Queries.GetCalendarStatus;
using Propely.AppointmentsApi.Domain.Appointments;

namespace Propely.AppointmentsApi.UnitTests.Application.Appointments.Queries;

public class GetCalendarStatusQueryHandlerTests
{
    private readonly ICalendarConnectionRepository _connectionRepository = Substitute.For<ICalendarConnectionRepository>();
    private readonly GetCalendarStatusQueryHandler _handler;

    private static readonly Guid AgentId = Guid.NewGuid();
    private static readonly Guid TenantId = Guid.NewGuid();

    public GetCalendarStatusQueryHandlerTests()
    {
        _handler = new GetCalendarStatusQueryHandler(_connectionRepository);
    }

    [Fact]
    public async Task Handle_WithConnections_ReturnsDtoList()
    {
        var googleConnection = CalendarConnection.Create(
            AgentId, TenantId, CalendarProvider.Google, "token", "refresh", DateTime.UtcNow.AddHours(1), "primary");
        var msConnection = CalendarConnection.Create(
            AgentId, TenantId, CalendarProvider.Microsoft, "token", "refresh", DateTime.UtcNow.AddHours(1), "user@outlook.com");

        _connectionRepository.GetActiveByAgentAsync(AgentId, TenantId, Arg.Any<CancellationToken>())
            .Returns(new List<CalendarConnection> { googleConnection, msConnection });

        var query = new GetCalendarStatusQuery(AgentId, TenantId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Provider == CalendarProvider.Google);
        result.Should().Contain(c => c.Provider == CalendarProvider.Microsoft);
    }

    [Fact]
    public async Task Handle_WithNoConnections_ReturnsEmptyList()
    {
        _connectionRepository.GetActiveByAgentAsync(AgentId, TenantId, Arg.Any<CancellationToken>())
            .Returns(new List<CalendarConnection>());

        var query = new GetCalendarStatusQuery(AgentId, TenantId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }
}
