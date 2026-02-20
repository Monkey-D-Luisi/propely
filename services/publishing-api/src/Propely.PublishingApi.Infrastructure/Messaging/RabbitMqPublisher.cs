// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Text;
using Propely.PublishingApi.Application.Common.Interfaces;
using Propely.PublishingApi.Infrastructure.Messaging.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Propely.PublishingApi.Infrastructure.Messaging;

/// <summary>
/// RabbitMQ implementation of the message publisher.
/// </summary>
public sealed class RabbitMqPublisher : IMessagePublisher, IAsyncDisposable
{
    private readonly RabbitMqConfiguration _configuration;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    private IConnection? _connection;
    private IChannel? _channel;
    private bool _exchangeDeclared;
    private bool _disposed;

    public RabbitMqPublisher(
        IOptions<RabbitMqConfiguration> configuration,
        ILogger<RabbitMqPublisher> logger)
    {
        _configuration = configuration.Value;
        _logger = logger;
    }

    public async Task PublishAsync(
        string exchange,
        string routingKey,
        string payload,
        Guid? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        await EnsureConnectionAsync(cancellationToken);

        if (_channel is null)
        {
            throw new InvalidOperationException("RabbitMQ channel is not available.");
        }

        var body = Encoding.UTF8.GetBytes(payload);
        var properties = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        };

        if (correlationId.HasValue)
        {
            properties.CorrelationId = correlationId.Value.ToString();
        }

        await _channel.BasicPublishAsync(
            exchange: exchange,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);

        _logger.LogDebug(
            "Published message to exchange {Exchange} with routing key {RoutingKey}",
            exchange,
            routingKey);
    }

    private async Task EnsureConnectionAsync(CancellationToken cancellationToken)
    {
        if (_connection is { IsOpen: true } && _channel is { IsOpen: true } && _exchangeDeclared)
        {
            return;
        }

        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (_connection is { IsOpen: true } && _channel is { IsOpen: true } && _exchangeDeclared)
            {
                return;
            }

            await CloseExistingConnectionAsync();
            await CreateConnectionAsync(cancellationToken);
            await DeclareExchangeAsync(cancellationToken);
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    private async Task CreateConnectionAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration.Host,
            Port = _configuration.Port,
            UserName = _configuration.Username,
            Password = _configuration.Password,
            VirtualHost = _configuration.VirtualHost,
            RequestedConnectionTimeout = TimeSpan.FromSeconds(_configuration.ConnectionTimeoutSeconds),
            Ssl = _configuration.UseSsl
                ? new SslOption { Enabled = true, ServerName = _configuration.Host }
                : new SslOption { Enabled = false }
        };

        _logger.LogInformation(
            "Connecting to RabbitMQ at {Host}:{Port}",
            _configuration.Host,
            _configuration.Port);

        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        _logger.LogInformation("Successfully connected to RabbitMQ");
    }

    private async Task DeclareExchangeAsync(CancellationToken cancellationToken)
    {
        if (_channel is null)
        {
            throw new InvalidOperationException("Channel must be created before declaring exchange.");
        }

        await _channel.ExchangeDeclareAsync(
            exchange: _configuration.EventsExchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        _exchangeDeclared = true;

        _logger.LogInformation(
            "Declared exchange {Exchange} of type {Type}",
            _configuration.EventsExchange,
            ExchangeType.Topic);
    }

    private async Task CloseExistingConnectionAsync()
    {
        _exchangeDeclared = false;

        if (_channel is not null)
        {
            try { await _channel.CloseAsync(); }
            catch (Exception ex) { _logger.LogWarning(ex, "Error closing RabbitMQ channel"); }
            _channel = null;
        }

        if (_connection is not null)
        {
            try { await _connection.CloseAsync(); }
            catch (Exception ex) { _logger.LogWarning(ex, "Error closing RabbitMQ connection"); }
            _connection = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        await CloseExistingConnectionAsync();
        _connectionLock.Dispose();
    }
}
