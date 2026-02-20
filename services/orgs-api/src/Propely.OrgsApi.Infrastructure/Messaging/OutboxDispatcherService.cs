// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Infrastructure.Messaging.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Propely.OrgsApi.Infrastructure.Messaging;

/// <summary>
/// Background service that polls the outbox table and publishes messages to RabbitMQ.
/// Messages that fail to publish are retried up to MaxRetries times (configurable,
/// default: 5). After exceeding the retry limit, messages are marked as dead-lettered
/// (FailedAtUtc is set) and excluded from future polling.
/// </summary>
public sealed class OutboxDispatcherService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMessagePublisher _messagePublisher;
    private readonly OutboxDispatcherConfiguration _configuration;
    private readonly RabbitMqConfiguration _rabbitMqConfiguration;
    private readonly ILogger<OutboxDispatcherService> _logger;

    public OutboxDispatcherService(
        IServiceScopeFactory scopeFactory,
        IMessagePublisher messagePublisher,
        IOptions<OutboxDispatcherConfiguration> configuration,
        IOptions<RabbitMqConfiguration> rabbitMqConfiguration,
        ILogger<OutboxDispatcherService> logger)
    {
        _scopeFactory = scopeFactory;
        _messagePublisher = messagePublisher;
        _configuration = configuration.Value;
        _rabbitMqConfiguration = rabbitMqConfiguration.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_configuration.Enabled)
        {
            _logger.LogInformation("Outbox Dispatcher is disabled");
            return;
        }

        _logger.LogInformation(
            "Outbox Dispatcher started. Polling every {Interval} seconds with batch size {BatchSize}, max retries {MaxRetries}",
            _configuration.PollingIntervalSeconds,
            _configuration.BatchSize,
            _configuration.MaxRetries);

        var pollingInterval = TimeSpan.FromSeconds(_configuration.PollingIntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Expected during shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox messages");
            }

            try
            {
                await Task.Delay(pollingInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        _logger.LogInformation("Outbox Dispatcher stopped");
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var messages = await outboxRepository.GetUnprocessedMessagesAsync(
            _configuration.BatchSize,
            cancellationToken);

        if (messages.Count == 0)
        {
            _logger.LogDebug("No unprocessed outbox messages found");
            return;
        }

        _logger.LogInformation("Processing {Count} outbox message(s)", messages.Count);

        var processedCount = 0;
        var failedCount = 0;
        var hasSuccessfulUpdates = false;

        foreach (var message in messages)
        {
            // Separate try-catch blocks to distinguish publish vs database failures
            try
            {
                await _messagePublisher.PublishAsync(
                    exchange: _rabbitMqConfiguration.WorkItemsExchange,
                    routingKey: message.EventType,
                    payload: message.Payload,
                    correlationId: message.CorrelationId,
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                failedCount++;
                message.IncrementRetryCount();

                if (message.RetryCount >= _configuration.MaxRetries)
                {
                    message.MarkAsFailed(DateTime.UtcNow);
                    _logger.LogError(
                        ex,
                        "Outbox message {MessageId} dead-lettered after {RetryCount} failed attempts (event type {EventType})",
                        message.Id,
                        message.RetryCount,
                        message.EventType);
                }
                else
                {
                    _logger.LogWarning(
                        ex,
                        "Failed to publish outbox message {MessageId} (attempt {RetryCount}/{MaxRetries}, event type {EventType})",
                        message.Id,
                        message.RetryCount,
                        _configuration.MaxRetries,
                        message.EventType);
                }

                // Save failed message state immediately to persist retry count
                try
                {
                    await outboxRepository.UpdateAsync(message, cancellationToken);
                    await unitOfWork.SaveChangesAsync(cancellationToken);
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(
                        dbEx,
                        "Failed to persist retry count update for outbox message {MessageId}",
                        message.Id);
                }

                continue;
            }

            // Mark as processed but defer the database save to batch at end of loop
            message.MarkAsProcessed(DateTime.UtcNow);
            await outboxRepository.UpdateAsync(message, cancellationToken);
            hasSuccessfulUpdates = true;
            processedCount++;

            _logger.LogDebug(
                "Published outbox message {MessageId} with event type {EventType}",
                message.Id,
                message.EventType);
        }

        // Batch-save all successfully processed messages in a single SaveChangesAsync call
        if (hasSuccessfulUpdates)
        {
            try
            {
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                // Messages were published but database update failed.
                // Will result in duplicate delivery (at-least-once semantics).
                _logger.LogError(
                    ex,
                    "Failed to batch-save {Count} processed outbox messages after successful publish. " +
                    "Messages may be delivered again (at-least-once semantics)",
                    processedCount);
            }
        }

        if (processedCount > 0 || failedCount > 0)
        {
            _logger.LogInformation(
                "Outbox processing complete. Processed: {Processed}, Failed: {Failed}",
                processedCount,
                failedCount);
        }
    }
}
