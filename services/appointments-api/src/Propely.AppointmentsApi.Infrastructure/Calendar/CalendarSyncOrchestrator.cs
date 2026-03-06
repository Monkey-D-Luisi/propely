// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Microsoft.Extensions.Logging;
using Propely.AppointmentsApi.Application.Appointments.Interfaces;
using Propely.AppointmentsApi.Application.Common.Interfaces;
using Propely.AppointmentsApi.Domain.Appointments;

namespace Propely.AppointmentsApi.Infrastructure.Calendar;

public sealed class CalendarSyncOrchestrator
{
    private const int MaxRetryCount = 3;

    private readonly ICalendarConnectionRepository _connectionRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ISyncOperationRepository _syncOperationRepository;
    private readonly IEnumerable<ICalendarSyncService> _syncServices;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CalendarSyncOrchestrator> _logger;

    public CalendarSyncOrchestrator(
        ICalendarConnectionRepository connectionRepository,
        IAppointmentRepository appointmentRepository,
        ISyncOperationRepository syncOperationRepository,
        IEnumerable<ICalendarSyncService> syncServices,
        IUnitOfWork unitOfWork,
        ILogger<CalendarSyncOrchestrator> logger)
    {
        _connectionRepository = connectionRepository;
        _appointmentRepository = appointmentRepository;
        _syncOperationRepository = syncOperationRepository;
        _syncServices = syncServices;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task SyncOutboundAsync(Appointment appointment, SyncOperationType operationType, CancellationToken ct = default)
    {
        var connections = await _connectionRepository.GetActiveByAgentAsync(
            appointment.AgentId, appointment.TenantId, ct);

        foreach (var connection in connections)
        {
            var operation = SyncOperation.Create(
                calendarConnectionId: connection.Id,
                appointmentId: appointment.Id,
                direction: SyncDirection.Outbound,
                operationType: operationType);

            await _syncOperationRepository.AddAsync(operation, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task SyncInboundAsync(CancellationToken ct = default)
    {
        var connections = await _connectionRepository.ListActiveAsync(ct);

        foreach (var connection in connections)
        {
            try
            {
                var syncService = GetSyncService(connection.Provider);
                var changes = await syncService.GetChangesAsync(connection, syncToken: null, ct);

                if (changes.Count > 0)
                {
                    _logger.LogInformation(
                        "Retrieved {ChangeCount} inbound changes for connection {ConnectionId} (Provider: {Provider})",
                        changes.Count, connection.Id, connection.Provider);
                }

                connection.MarkSynced();
                _connectionRepository.Update(connection);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to sync inbound changes for connection {ConnectionId} (Provider: {Provider})",
                    connection.Id, connection.Provider);

                connection.MarkError(ex.Message);
                _connectionRepository.Update(connection);
            }
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task ProcessPendingOperationsAsync(CancellationToken ct = default)
    {
        var pendingOperations = await _syncOperationRepository.GetPendingAsync(ct);

        foreach (var operation in pendingOperations)
        {
            try
            {
                operation.Start();
                _syncOperationRepository.Update(operation);
                await _unitOfWork.SaveChangesAsync(ct);

                var connection = await _connectionRepository.GetByIdAsync(operation.CalendarConnectionId, ct);
                if (connection is null || connection.SyncState == CalendarSyncState.Disabled)
                {
                    operation.Fail("Calendar connection not found or disabled.");
                    _syncOperationRepository.Update(operation);
                    await _unitOfWork.SaveChangesAsync(ct);
                    continue;
                }

                var syncService = GetSyncService(connection.Provider);

                await ProcessOperationAsync(operation, connection, syncService, ct);

                operation.Complete();
                connection.MarkSynced();
                _syncOperationRepository.Update(operation);
                _connectionRepository.Update(connection);
                await _unitOfWork.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to process sync operation {OperationId} (Type: {OperationType}, Direction: {Direction})",
                    operation.Id, operation.OperationType, operation.Direction);

                operation.Fail(ex.Message);
                _syncOperationRepository.Update(operation);
                await _unitOfWork.SaveChangesAsync(ct);

                if (operation.RetryCount < MaxRetryCount)
                {
                    operation.IncrementRetry();
                    _syncOperationRepository.Update(operation);
                    await _unitOfWork.SaveChangesAsync(ct);
                }
            }
        }
    }

    private async Task ProcessOperationAsync(
        SyncOperation operation,
        CalendarConnection connection,
        ICalendarSyncService syncService,
        CancellationToken ct)
    {
        if (operation.Direction == SyncDirection.Outbound && operation.AppointmentId.HasValue)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(
                operation.AppointmentId.Value, connection.TenantId, ct);

            if (appointment is null)
            {
                _logger.LogWarning(
                    "Appointment {AppointmentId} not found for sync operation {OperationId}",
                    operation.AppointmentId, operation.Id);
                return;
            }

            switch (operation.OperationType)
            {
                case SyncOperationType.Create:
                    var externalEventId = await syncService.CreateEventAsync(appointment, connection, ct);
                    appointment.SetCalendarSync(externalEventId, connection.Provider);
                    _appointmentRepository.Update(appointment);
                    break;

                case SyncOperationType.Update:
                    var existingSync = appointment.CalendarSyncInfos
                        .FirstOrDefault(s => s.Provider == connection.Provider);
                    if (existingSync is not null)
                    {
                        await syncService.UpdateEventAsync(appointment, connection, existingSync.ExternalEventId, ct);
                        appointment.SetCalendarSync(existingSync.ExternalEventId, connection.Provider);
                        _appointmentRepository.Update(appointment);
                    }
                    break;

                case SyncOperationType.Delete:
                    var syncToDelete = appointment.CalendarSyncInfos
                        .FirstOrDefault(s => s.Provider == connection.Provider);
                    if (syncToDelete is not null)
                    {
                        await syncService.DeleteEventAsync(connection, syncToDelete.ExternalEventId, ct);
                    }
                    break;
            }
        }
    }

    private ICalendarSyncService GetSyncService(CalendarProvider provider)
    {
        var service = _syncServices.FirstOrDefault(s => s.Provider == provider);
        if (service is null)
            throw new InvalidOperationException($"No calendar sync service registered for provider '{provider}'.");

        return service;
    }
}
