// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.ContactsApi.Application.Common.Interfaces;
using Propely.ContactsApi.Infrastructure.Messaging.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Propely.ContactsApi.Infrastructure.Messaging;

/// <summary>
/// Background service that polls the outbox table and publishes messages to RabbitMQ.
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
            "Outbox Dispatcher started. Polling every {Interval} seconds with batch size {BatchSize}",
            _configuration.PollingIntervalSeconds,
            _configuration.BatchSize);

        var pollingInterval = TimeSpan.FromSeconds(_configuration.PollingIntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
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

        foreach (var message in messages)
        {
            try
            {
                await _messagePublisher.PublishAsync(
                    exchange: _rabbitMqConfiguration.EventsExchange,
                    routingKey: message.EventType,
                    payload: message.Payload,
                    correlationId: message.CorrelationId,
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                failedCount++;
                _logger.LogError(
                    ex,
                    "Failed to publish outbox message {MessageId} to RabbitMQ with event type {EventType}",
                    message.Id,
                    message.EventType);
                continue;
            }

            try
            {
                message.MarkAsProcessed(DateTime.UtcNow);
                await outboxRepository.UpdateAsync(message, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                processedCount++;

                _logger.LogDebug(
                    "Published outbox message {MessageId} with event type {EventType}",
                    message.Id,
                    message.EventType);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to mark outbox message {MessageId} as processed after successful publish. " +
                    "Message may be delivered again (at-least-once semantics)",
                    message.Id);
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
