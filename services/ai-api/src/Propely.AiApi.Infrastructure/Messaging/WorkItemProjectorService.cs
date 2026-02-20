// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Text;
using Propely.AiApi.Infrastructure.Messaging.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Propely.AiApi.Infrastructure.Messaging;

/// <summary>
/// Background service that consumes domain events from RabbitMQ and delegates to event projectors.
/// Responsibilities: RabbitMQ connection management, message consumption, delegation to projectors.
/// </summary>
public sealed class WorkItemProjectorService : BackgroundService
{
    private readonly IEventProjector _eventProjector;
    private readonly RabbitMqConfiguration _rabbitMqConfiguration;
    private readonly ProjectorConfiguration _projectorConfiguration;
    private readonly ILogger<WorkItemProjectorService> _logger;

    private IConnection? _connection;
    private IChannel? _channel;
    private CancellationToken _stoppingToken;

    public WorkItemProjectorService(
        IEventProjector eventProjector,
        IOptions<RabbitMqConfiguration> rabbitMqConfiguration,
        IOptions<ProjectorConfiguration> projectorConfiguration,
        ILogger<WorkItemProjectorService> logger)
    {
        _eventProjector = eventProjector;
        _rabbitMqConfiguration = rabbitMqConfiguration.Value;
        _projectorConfiguration = projectorConfiguration.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _stoppingToken = stoppingToken;

        if (!_projectorConfiguration.Enabled)
        {
            _logger.LogInformation("WorkItem Projector is disabled");
            return;
        }

        _logger.LogInformation(
            "WorkItem Projector starting. Queue: {Queue}",
            _projectorConfiguration.QueueName);

        try
        {
            // Retry RabbitMQ connection with exponential backoff to handle
            // transient failures without crashing the host (.NET 10 defaults
            // BackgroundServiceExceptionBehavior to StopHost).
            const int maxRetries = 5;
            for (var attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    await InitializeRabbitMqAsync(stoppingToken);
                    break; // Connection succeeded
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    throw; // Shutdown requested, don't retry
                }
                catch (Exception ex) when (attempt < maxRetries)
                {
                    var delay = TimeSpan.FromSeconds(5 * Math.Pow(2, attempt - 1));
                    _logger.LogWarning(ex,
                        "RabbitMQ connection attempt {Attempt}/{MaxRetries} failed. Retrying in {Delay}s",
                        attempt, maxRetries, delay.TotalSeconds);
                    await Task.Delay(delay, stoppingToken);
                    await CleanupAsync(); // Clean up partial connection state
                }
            }

            await StartConsumingAsync(stoppingToken);

            // Keep running until cancellation
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Expected during shutdown
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in WorkItem Projector after all retries exhausted");
            throw;
        }
        finally
        {
            await CleanupAsync();
        }

        _logger.LogInformation("WorkItem Projector stopped");
    }

    private async Task InitializeRabbitMqAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _rabbitMqConfiguration.Host,
            Port = _rabbitMqConfiguration.Port,
            UserName = _rabbitMqConfiguration.Username,
            Password = _rabbitMqConfiguration.Password,
            VirtualHost = _rabbitMqConfiguration.VirtualHost,
            RequestedConnectionTimeout = TimeSpan.FromSeconds(_rabbitMqConfiguration.ConnectionTimeoutSeconds),
            Ssl = _rabbitMqConfiguration.UseSsl
                ? new SslOption { Enabled = true, ServerName = _rabbitMqConfiguration.Host }
                : new SslOption { Enabled = false }
        };

        _logger.LogInformation(
            "Connecting to RabbitMQ at {Host}:{Port}",
            _rabbitMqConfiguration.Host,
            _rabbitMqConfiguration.Port);

        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        // Set prefetch count
        await _channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: _projectorConfiguration.PrefetchCount,
            global: false,
            cancellationToken: cancellationToken);

        // Declare dead-letter exchange and queue
        await _channel.ExchangeDeclareAsync(
            exchange: _projectorConfiguration.DeadLetterExchange,
            type: ExchangeType.Fanout,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await _channel.QueueDeclareAsync(
            queue: _projectorConfiguration.DeadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await _channel.QueueBindAsync(
            queue: _projectorConfiguration.DeadLetterQueue,
            exchange: _projectorConfiguration.DeadLetterExchange,
            routingKey: string.Empty,
            cancellationToken: cancellationToken);

        // Declare main queue with dead-letter configuration
        var queueArgs = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = _projectorConfiguration.DeadLetterExchange
        };

        await _channel.QueueDeclareAsync(
            queue: _projectorConfiguration.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: queueArgs,
            cancellationToken: cancellationToken);

        // Ensure the WorkItems exchange exists before binding
        await _channel.ExchangeDeclareAsync(
            exchange: _rabbitMqConfiguration.WorkItemsExchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        // Bind to work items exchange
        await _channel.QueueBindAsync(
            queue: _projectorConfiguration.QueueName,
            exchange: _rabbitMqConfiguration.WorkItemsExchange,
            routingKey: "#", // Receive all events
            cancellationToken: cancellationToken);

        _logger.LogInformation("RabbitMQ infrastructure initialized for projector");
    }

    private async Task StartConsumingAsync(CancellationToken cancellationToken)
    {
        if (_channel is null)
        {
            throw new InvalidOperationException("Channel not initialized");
        }

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnMessageReceivedAsync;

        await _channel.BasicConsumeAsync(
            queue: _projectorConfiguration.QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Started consuming from queue {Queue}", _projectorConfiguration.QueueName);
    }

    /// <summary>
    /// Handles incoming RabbitMQ messages by delegating to the event projector.
    /// </summary>
    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs args)
    {
        var routingKey = args.RoutingKey;
        var deliveryTag = args.DeliveryTag;

        _logger.LogDebug("Received message with routing key {RoutingKey}", routingKey);

        try
        {
            var payload = Encoding.UTF8.GetString(args.Body.ToArray());

            // Delegate to the event projector
            await _eventProjector.ProjectAsync(routingKey, payload, _stoppingToken);

            if (_channel is not null)
            {
                await _channel.BasicAckAsync(deliveryTag, multiple: false);
                _logger.LogDebug("Successfully processed message with routing key {RoutingKey}", routingKey);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message with routing key {RoutingKey}", routingKey);

            if (_channel is not null)
            {
                // Reject and send to dead-letter queue
                await _channel.BasicNackAsync(deliveryTag, multiple: false, requeue: false);
            }
        }
    }

    private async Task CleanupAsync()
    {
        if (_channel is not null)
        {
            try
            {
                await _channel.CloseAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error closing RabbitMQ channel");
            }
        }

        if (_connection is not null)
        {
            try
            {
                await _connection.CloseAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error closing RabbitMQ connection");
            }
        }
    }
}
