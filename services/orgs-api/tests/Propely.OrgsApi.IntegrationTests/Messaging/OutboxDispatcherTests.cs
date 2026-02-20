// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Text;
using Propely.OrgsApi.Application.Common.Models;
using Propely.OrgsApi.Infrastructure.Messaging;
using Propely.OrgsApi.Infrastructure.Messaging.Configuration;
using Propely.OrgsApi.Infrastructure.Persistence;
using Propely.OrgsApi.Infrastructure.Persistence.Repositories;
using Propely.OrgsApi.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Propely.OrgsApi.IntegrationTests.Messaging;

[Collection("PostgresAndRabbitMQ")]
public sealed class OutboxDispatcherTests : IAsyncLifetime
{
    private readonly PostgresFixture _postgresFixture;
    private readonly RabbitMqFixture _rabbitMqFixture;
    private AppDbContext _dbContext = null!;
    private RabbitMqPublisher _publisher = null!;
    private IConnection? _consumerConnection;
    private IChannel? _consumerChannel;

    public OutboxDispatcherTests(PostgresFixture postgresFixture, RabbitMqFixture rabbitMqFixture)
    {
        _postgresFixture = postgresFixture;
        _rabbitMqFixture = rabbitMqFixture;
    }

    public async Task InitializeAsync()
    {
        _dbContext = _postgresFixture.CreateContext();

        var rabbitMqConfig = new RabbitMqConfiguration
        {
            Host = _rabbitMqFixture.Host,
            Port = _rabbitMqFixture.Port,
            Username = _rabbitMqFixture.Username,
            Password = _rabbitMqFixture.Password,
            WorkItemsExchange = "test.workitems.events"
        };

        _publisher = new RabbitMqPublisher(
            Options.Create(rabbitMqConfig),
            NullLogger<RabbitMqPublisher>.Instance);

        // Set up a consumer connection for verification
        var factory = new ConnectionFactory
        {
            HostName = _rabbitMqFixture.Host,
            Port = _rabbitMqFixture.Port,
            UserName = _rabbitMqFixture.Username,
            Password = _rabbitMqFixture.Password
        };

        _consumerConnection = await factory.CreateConnectionAsync();
        _consumerChannel = await _consumerConnection.CreateChannelAsync();

        // Declare exchange and queue for testing
        await _consumerChannel.ExchangeDeclareAsync(
            exchange: "test.workitems.events",
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false);

        await _consumerChannel.QueueDeclareAsync(
            queue: "test.queue",
            durable: true,
            exclusive: false,
            autoDelete: false);

        await _consumerChannel.QueueBindAsync(
            queue: "test.queue",
            exchange: "test.workitems.events",
            routingKey: "#");
    }

    public async Task DisposeAsync()
    {
        if (_consumerChannel is not null)
        {
            await _consumerChannel.CloseAsync();
        }

        if (_consumerConnection is not null)
        {
            await _consumerConnection.CloseAsync();
        }

        await _publisher.DisposeAsync();
        await _dbContext.DisposeAsync();
    }

    [Fact]
    public async Task PublishAsync_ShouldPublishMessageToRabbitMq()
    {
        // Arrange
        var payload = """{"eventId": "123", "eventType": "TestEvent"}""";

        // Act
        await _publisher.PublishAsync(
            exchange: "test.workitems.events",
            routingKey: "TestEvent",
            payload: payload);

        // Assert - verify message was received
        await Task.Delay(500); // Allow message to propagate

        var result = await _consumerChannel!.BasicGetAsync("test.queue", autoAck: true);
        result.Should().NotBeNull();

        var body = Encoding.UTF8.GetString(result!.Body.ToArray());

        // Parse JSON and assert properties instead of raw string comparison
        // This is more resilient to formatting/whitespace changes
        using var doc = System.Text.Json.JsonDocument.Parse(body);
        doc.RootElement.GetProperty("eventId").GetString().Should().Be("123");
        doc.RootElement.GetProperty("eventType").GetString().Should().Be("TestEvent");
    }

    [Fact]
    public async Task PublishAsync_ShouldSetCorrectMessageProperties()
    {
        // Arrange
        var payload = """{"data": "test"}""";
        var correlationId = Guid.NewGuid();

        // Act
        await _publisher.PublishAsync(
            exchange: "test.workitems.events",
            routingKey: "WorkItemCreatedV1",
            payload: payload,
            correlationId: correlationId);

        // Assert
        await Task.Delay(500);

        var result = await _consumerChannel!.BasicGetAsync("test.queue", autoAck: true);
        result.Should().NotBeNull();
        result!.BasicProperties.ContentType.Should().Be("application/json");
        result.BasicProperties.DeliveryMode.Should().Be(DeliveryModes.Persistent);
        result.BasicProperties.CorrelationId.Should().Be(correlationId.ToString());
    }

    [Fact]
    public async Task OutboxRepository_ShouldRetrieveUnprocessedMessages()
    {
        // Arrange
        var repository = new OutboxRepository(_dbContext);
        var testRunId = Guid.NewGuid().ToString();
        var message = OutboxMessage.Create(
            Guid.NewGuid(),
            $"TestEvent_{testRunId}",
            """{"test": true}""",
            DateTime.UtcNow);

        await repository.AddAsync(message);
        await _dbContext.SaveChangesAsync();

        // Act
        var unprocessedMessages = await repository.GetUnprocessedMessagesAsync(100);

        // Assert
        unprocessedMessages.Should().Contain(m => m.Id == message.Id);
    }

    [Fact]
    public async Task OutboxRepository_ShouldNotRetrieveProcessedMessages()
    {
        // Arrange
        var repository = new OutboxRepository(_dbContext);
        var message = OutboxMessage.Create(
            Guid.NewGuid(),
            "ProcessedEvent",
            """{"test": true}""",
            DateTime.UtcNow);

        message.MarkAsProcessed(DateTime.UtcNow);
        await repository.AddAsync(message);
        await _dbContext.SaveChangesAsync();

        // Act
        var unprocessedMessages = await repository.GetUnprocessedMessagesAsync(10);

        // Assert
        unprocessedMessages.Should().NotContain(m => m.Id == message.Id);
    }

    [Fact]
    public async Task OutboxRepository_UpdateAsync_ShouldPersistProcessedAtUtc()
    {
        // Arrange
        var repository = new OutboxRepository(_dbContext);
        var message = OutboxMessage.Create(
            Guid.NewGuid(),
            "EventToProcess",
            """{"test": true}""",
            DateTime.UtcNow);

        await repository.AddAsync(message);
        await _dbContext.SaveChangesAsync();

        // Act
        message.MarkAsProcessed(DateTime.UtcNow);
        await repository.UpdateAsync(message);
        await _dbContext.SaveChangesAsync();

        // Assert - use a fresh context to verify persistence
        await using var verifyContext = _postgresFixture.CreateContext();
        var savedMessage = await verifyContext.OutboxMessages.FindAsync(message.Id);
        savedMessage.Should().NotBeNull();
        savedMessage!.ProcessedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task OutboxRepository_GetUnprocessedMessagesAsync_ShouldRespectBatchSize()
    {
        // Arrange
        var repository = new OutboxRepository(_dbContext);
        var testRunId = Guid.NewGuid().ToString();

        for (int i = 0; i < 10; i++)
        {
            var message = OutboxMessage.Create(
                Guid.NewGuid(),
                $"BatchEvent_{testRunId}_{i}",
                $$$"""{"index": {{{i}}}}""",
                DateTime.UtcNow);
            await repository.AddAsync(message);
        }
        await _dbContext.SaveChangesAsync();

        // Act
        var messages = await repository.GetUnprocessedMessagesAsync(5);

        // Assert - should return at most the requested batch size
        messages.Should().HaveCountLessThanOrEqualTo(5);
    }

    [Fact]
    public async Task OutboxRepository_GetUnprocessedMessagesAsync_ShouldOrderByOccurredAtUtc()
    {
        // Arrange
        var repository = new OutboxRepository(_dbContext);
        var testRunId = Guid.NewGuid().ToString();
        var now = DateTime.UtcNow;

        var olderMessage = OutboxMessage.Create(
            Guid.NewGuid(),
            $"OlderEvent_{testRunId}",
            """{"order": "first"}""",
            now.AddMinutes(-10));

        var newerMessage = OutboxMessage.Create(
            Guid.NewGuid(),
            $"NewerEvent_{testRunId}",
            """{"order": "second"}""",
            now);

        // Add in reverse order
        await repository.AddAsync(newerMessage);
        await repository.AddAsync(olderMessage);
        await _dbContext.SaveChangesAsync();

        // Act
        var messages = await repository.GetUnprocessedMessagesAsync(100);

        // Assert - older message should come first
        var orderedMessages = messages.Where(m =>
            m.EventType == $"OlderEvent_{testRunId}" || m.EventType == $"NewerEvent_{testRunId}").ToList();

        orderedMessages.Should().HaveCount(2);
        var olderIndex = orderedMessages.FindIndex(m => m.EventType == $"OlderEvent_{testRunId}");
        var newerIndex = orderedMessages.FindIndex(m => m.EventType == $"NewerEvent_{testRunId}");
        olderIndex.Should().BeLessThan(newerIndex);
    }
}
