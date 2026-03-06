// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Propely.AppointmentsApi.Infrastructure.Calendar;

public sealed class CalendarSyncWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CalendarSyncWorker> _logger;
    private readonly TimeSpan _syncInterval;

    public CalendarSyncWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<CalendarConfiguration> calendarOptions,
        ILogger<CalendarSyncWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _syncInterval = TimeSpan.FromMinutes(calendarOptions.Value.SyncIntervalMinutes);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "CalendarSyncWorker started. Sync interval: {SyncInterval} minutes",
            _syncInterval.TotalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_syncInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var orchestrator = scope.ServiceProvider.GetRequiredService<CalendarSyncOrchestrator>();

                _logger.LogDebug("CalendarSyncWorker: Processing pending operations...");
                await orchestrator.ProcessPendingOperationsAsync(stoppingToken);

                _logger.LogDebug("CalendarSyncWorker: Syncing inbound changes...");
                await orchestrator.SyncInboundAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CalendarSyncWorker encountered an error during sync cycle");
            }
        }

        _logger.LogInformation("CalendarSyncWorker stopped.");
    }
}
