// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Propely.AppointmentsApi.Application.Appointments.Dtos;
using Propely.AppointmentsApi.Application.Appointments.Interfaces;
using Propely.AppointmentsApi.Application.Common.Interfaces;
using Propely.AppointmentsApi.Domain.Appointments;
using Propely.AppointmentsApi.Infrastructure.Calendar;

namespace Propely.AppointmentsApi.UnitTests.Infrastructure.Calendar;

public class CalendarSyncOrchestratorTests
{
    private readonly ICalendarConnectionRepository _connectionRepository = Substitute.For<ICalendarConnectionRepository>();
    private readonly IAppointmentRepository _appointmentRepository = Substitute.For<IAppointmentRepository>();
    private readonly ISyncOperationRepository _syncOperationRepository = Substitute.For<ISyncOperationRepository>();
    private readonly ICalendarSyncService _googleSyncService = Substitute.For<ICalendarSyncService>();
    private readonly ICalendarSyncService _microsoftSyncService = Substitute.For<ICalendarSyncService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ILogger<CalendarSyncOrchestrator> _logger = Substitute.For<ILogger<CalendarSyncOrchestrator>>();
    private readonly CalendarSyncOrchestrator _orchestrator;

    private static readonly Guid AgentId = Guid.NewGuid();
    private static readonly Guid TenantId = Guid.NewGuid();

    public CalendarSyncOrchestratorTests()
    {
        _googleSyncService.Provider.Returns(CalendarProvider.Google);
        _microsoftSyncService.Provider.Returns(CalendarProvider.Microsoft);

        _orchestrator = new CalendarSyncOrchestrator(
            _connectionRepository,
            _appointmentRepository,
            _syncOperationRepository,
            new[] { _googleSyncService, _microsoftSyncService },
            _unitOfWork,
            _logger);
    }

    // --- SyncOutboundAsync tests ---

    [Fact]
    public async Task SyncOutboundAsync_WithActiveConnections_CreatesOperationsForEach()
    {
        var appointment = Appointment.Create(
            "Test Viewing", AppointmentType.Generic,
            DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1),
            AgentId, TenantId);

        var googleConnection = CalendarConnection.Create(
            AgentId, TenantId, CalendarProvider.Google, "token", "refresh", DateTime.UtcNow.AddHours(1), "primary");
        var msConnection = CalendarConnection.Create(
            AgentId, TenantId, CalendarProvider.Microsoft, "token", "refresh", DateTime.UtcNow.AddHours(1), "user@outlook.com");

        _connectionRepository.GetActiveByAgentAsync(AgentId, TenantId, Arg.Any<CancellationToken>())
            .Returns(new List<CalendarConnection> { googleConnection, msConnection });

        await _orchestrator.SyncOutboundAsync(appointment, SyncOperationType.Create);

        await _syncOperationRepository.Received(2).AddAsync(
            Arg.Is<SyncOperation>(o =>
                o.Direction == SyncDirection.Outbound &&
                o.OperationType == SyncOperationType.Create),
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SyncOutboundAsync_WithNoConnections_DoesNotCreateOperations()
    {
        var appointment = Appointment.Create(
            "Test Viewing", AppointmentType.Generic,
            DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1),
            AgentId, TenantId);

        _connectionRepository.GetActiveByAgentAsync(AgentId, TenantId, Arg.Any<CancellationToken>())
            .Returns(new List<CalendarConnection>());

        await _orchestrator.SyncOutboundAsync(appointment, SyncOperationType.Create);

        await _syncOperationRepository.DidNotReceive().AddAsync(
            Arg.Any<SyncOperation>(), Arg.Any<CancellationToken>());
    }

    // --- SyncInboundAsync tests ---

    [Fact]
    public async Task SyncInboundAsync_WithActiveConnections_CallsGetChangesForEach()
    {
        var googleConnection = CalendarConnection.Create(
            AgentId, TenantId, CalendarProvider.Google, "token", "refresh", DateTime.UtcNow.AddHours(1), "primary");

        _connectionRepository.ListActiveAsync(Arg.Any<CancellationToken>())
            .Returns(new List<CalendarConnection> { googleConnection });

        _googleSyncService.GetChangesAsync(googleConnection, null, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<ExternalCalendarChange>());

        await _orchestrator.SyncInboundAsync();

        await _googleSyncService.Received(1).GetChangesAsync(
            googleConnection, null, Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SyncInboundAsync_WhenProviderFails_MarksConnectionError()
    {
        var googleConnection = CalendarConnection.Create(
            AgentId, TenantId, CalendarProvider.Google, "token", "refresh", DateTime.UtcNow.AddHours(1), "primary");

        _connectionRepository.ListActiveAsync(Arg.Any<CancellationToken>())
            .Returns(new List<CalendarConnection> { googleConnection });

        _googleSyncService.GetChangesAsync(googleConnection, null, Arg.Any<CancellationToken>())
            .Returns<IReadOnlyList<ExternalCalendarChange>>(_ => throw new Exception("API rate limit"));

        await _orchestrator.SyncInboundAsync();

        googleConnection.SyncState.Should().Be(CalendarSyncState.Error);
        googleConnection.LastSyncError.Should().Be("API rate limit");
        _connectionRepository.Received(1).Update(googleConnection);
    }

    // --- ProcessPendingOperationsAsync tests ---

    [Fact]
    public async Task ProcessPendingOperationsAsync_WithNoPending_DoesNothing()
    {
        _syncOperationRepository.GetPendingAsync(Arg.Any<CancellationToken>())
            .Returns(new List<SyncOperation>());

        await _orchestrator.ProcessPendingOperationsAsync();

        await _googleSyncService.DidNotReceive().CreateEventAsync(
            Arg.Any<Appointment>(), Arg.Any<CalendarConnection>(), Arg.Any<CancellationToken>());
    }
}
