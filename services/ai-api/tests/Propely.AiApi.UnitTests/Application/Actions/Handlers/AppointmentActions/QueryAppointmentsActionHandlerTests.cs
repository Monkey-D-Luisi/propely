// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Propely.AiApi.Application.Actions.Commands.AppointmentActions;
using Propely.AiApi.Application.Actions.Handlers.AppointmentActions;
using Propely.AiApi.Application.Actions.Parameters;
using Propely.AiApi.Domain.Actions;
using Propely.AppointmentsApi.Client;
using Propely.AppointmentsApi.Client.Models;

namespace Propely.AiApi.UnitTests.Application.Actions.Handlers.AppointmentActions;

public sealed class QueryAppointmentsActionHandlerTests
{
    private readonly IAppointmentsApiClient _appointmentsClient;
    private readonly QueryAppointmentsActionHandler _handler;
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AgentId = Guid.NewGuid();

    public QueryAppointmentsActionHandlerTests()
    {
        _appointmentsClient = Substitute.For<IAppointmentsApiClient>();
        var logger = Substitute.For<ILogger<QueryAppointmentsActionHandler>>();
        _handler = new QueryAppointmentsActionHandler(_appointmentsClient, logger);
    }

    [Fact]
    public async Task Handle_WithResults_ShouldReturnAppointmentsSummary()
    {
        // Arrange
        var parameters = new QueryAppointmentsParameters(
            Status: "Scheduled",
            Type: null,
            PropertyId: null,
            FromDate: new DateTime(2026, 3, 1),
            ToDate: null);
        var command = new QueryAppointmentsActionCommand(parameters, TenantId, AgentId);

        var response = new AppointmentListResponse
        {
            Items = new List<AppointmentResponse>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Viewing at Calle Mayor",
                    StartTimeUtc = new DateTime(2026, 3, 15, 10, 0, 0),
                    EndTimeUtc = new DateTime(2026, 3, 15, 10, 30, 0),
                    Status = "Scheduled",
                    Type = "PropertyViewing"
                }
            },
            TotalCount = 1,
            PageNumber = 1,
            TotalPages = 1
        };

        _appointmentsClient.GetAppointmentsAsync(
            page: 1, pageSize: 20, status: "Scheduled", type: Arg.Any<string?>(),
            agentId: null, propertyId: Arg.Any<Guid?>(), contactId: null,
            fromUtc: Arg.Any<DateTime?>(), toUtc: Arg.Any<DateTime?>(),
            sortBy: "start", sortDescending: false, ct: Arg.Any<CancellationToken>())
            .Returns(response);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.ActionType.Should().Be(ActionType.QueryAppointments);
        result.Message.Should().Contain("1 appointment");
        result.Message.Should().Contain("Viewing at Calle Mayor");
    }

    [Fact]
    public async Task Handle_WithNoResults_ShouldReturnEmptyMessage()
    {
        // Arrange
        var parameters = new QueryAppointmentsParameters(
            Status: "Completed",
            Type: null,
            PropertyId: null,
            FromDate: null,
            ToDate: null);
        var command = new QueryAppointmentsActionCommand(parameters, TenantId, AgentId);

        _appointmentsClient.GetAppointmentsAsync(
            page: Arg.Any<int>(), pageSize: Arg.Any<int>(), status: Arg.Any<string?>(),
            type: Arg.Any<string?>(), agentId: Arg.Any<Guid?>(), propertyId: Arg.Any<Guid?>(),
            contactId: Arg.Any<Guid?>(), fromUtc: Arg.Any<DateTime?>(), toUtc: Arg.Any<DateTime?>(),
            sortBy: Arg.Any<string?>(), sortDescending: Arg.Any<bool>(), ct: Arg.Any<CancellationToken>())
            .Returns(new AppointmentListResponse
            {
                Items = new List<AppointmentResponse>(),
                TotalCount = 0,
                PageNumber = 1,
                TotalPages = 0
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.ActionType.Should().Be(ActionType.QueryAppointments);
        result.Message.Should().Contain("No appointments found");
    }

    [Fact]
    public async Task Handle_WhenSdkThrows_ShouldReturnFailureGracefully()
    {
        // Arrange
        var parameters = new QueryAppointmentsParameters(
            Status: null,
            Type: null,
            PropertyId: null,
            FromDate: null,
            ToDate: null);
        var command = new QueryAppointmentsActionCommand(parameters, TenantId, AgentId);

        _appointmentsClient.GetAppointmentsAsync(
            page: Arg.Any<int>(), pageSize: Arg.Any<int>(), status: Arg.Any<string?>(),
            type: Arg.Any<string?>(), agentId: Arg.Any<Guid?>(), propertyId: Arg.Any<Guid?>(),
            contactId: Arg.Any<Guid?>(), fromUtc: Arg.Any<DateTime?>(), toUtc: Arg.Any<DateTime?>(),
            sortBy: Arg.Any<string?>(), sortDescending: Arg.Any<bool>(), ct: Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.QueryAppointments);
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WithMoreResultsThanShown_ShouldIndicatePagination()
    {
        // Arrange
        var parameters = new QueryAppointmentsParameters(
            Status: null,
            Type: null,
            PropertyId: null,
            FromDate: null,
            ToDate: null);
        var command = new QueryAppointmentsActionCommand(parameters, TenantId, AgentId);

        var items = Enumerable.Range(1, 5).Select(i => new AppointmentResponse
        {
            Id = Guid.NewGuid(),
            Title = $"Appointment {i}",
            StartTimeUtc = DateTime.UtcNow.AddDays(i),
            EndTimeUtc = DateTime.UtcNow.AddDays(i).AddHours(1),
            Status = "Scheduled",
            Type = "PropertyViewing"
        }).ToList();

        _appointmentsClient.GetAppointmentsAsync(
            page: Arg.Any<int>(), pageSize: Arg.Any<int>(), status: Arg.Any<string?>(),
            type: Arg.Any<string?>(), agentId: Arg.Any<Guid?>(), propertyId: Arg.Any<Guid?>(),
            contactId: Arg.Any<Guid?>(), fromUtc: Arg.Any<DateTime?>(), toUtc: Arg.Any<DateTime?>(),
            sortBy: Arg.Any<string?>(), sortDescending: Arg.Any<bool>(), ct: Arg.Any<CancellationToken>())
            .Returns(new AppointmentListResponse
            {
                Items = items,
                TotalCount = 25,
                PageNumber = 1,
                TotalPages = 5
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("25 appointments");
        result.Message.Should().Contain("Showing 5 of 25 total");
    }
}
