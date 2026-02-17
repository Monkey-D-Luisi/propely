// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Application.Common.Interfaces;
using SaasTemplate.OrgsApi.Application.Common.Models;
using SaasTemplate.OrgsApi.Infrastructure.Messaging;
using SaasTemplate.OrgsApi.Infrastructure.Messaging.Configuration;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace SaasTemplate.OrgsApi.UnitTests.Infrastructure.Messaging;

public sealed class OutboxDispatcherServiceTests
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IServiceScope _scope;
    private readonly IServiceProvider _serviceProvider;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IOptions<OutboxDispatcherConfiguration> _dispatcherConfiguration;
    private readonly IOptions<RabbitMqConfiguration> _rabbitMqConfiguration;
    private readonly ILogger<OutboxDispatcherService> _logger;

    public OutboxDispatcherServiceTests()
    {
        _outboxRepository = Substitute.For<IOutboxRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _messagePublisher = Substitute.For<IMessagePublisher>();

        _serviceProvider = Substitute.For<IServiceProvider>();
        _serviceProvider.GetService(typeof(IOutboxRepository)).Returns(_outboxRepository);
        _serviceProvider.GetService(typeof(IUnitOfWork)).Returns(_unitOfWork);

        _scope = Substitute.For<IServiceScope>();
        _scope.ServiceProvider.Returns(_serviceProvider);

        _scopeFactory = Substitute.For<IServiceScopeFactory>();
        _scopeFactory.CreateScope().Returns(_scope);
        _scopeFactory.CreateAsyncScope().Returns(new AsyncServiceScope(_scope));

        _dispatcherConfiguration = Options.Create(new OutboxDispatcherConfiguration
        {
            Enabled = true,
            PollingIntervalSeconds = 1,
            BatchSize = 100
        });

        _rabbitMqConfiguration = Options.Create(new RabbitMqConfiguration
        {
            WorkItemsExchange = "test.events"
        });

        _logger = NullLogger<OutboxDispatcherService>.Instance;
    }

    [Fact]
    public async Task ExecuteAsync_WhenDisabled_ShouldNotProcessMessages()
    {
        // Arrange
        var disabledConfiguration = Options.Create(new OutboxDispatcherConfiguration
        {
            Enabled = false
        });

        var service = new OutboxDispatcherService(
            _scopeFactory,
            _messagePublisher,
            disabledConfiguration,
            _rabbitMqConfiguration,
            _logger);

        using var cts = new CancellationTokenSource();

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(100);
        await cts.CancelAsync();
        await service.StopAsync(CancellationToken.None);

        // Assert
        await _outboxRepository.DidNotReceive()
            .GetUnprocessedMessagesAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoMessages_ShouldNotPublish()
    {
        // Arrange
        _outboxRepository
            .GetUnprocessedMessagesAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<OutboxMessage>>([]));

        var service = new OutboxDispatcherService(
            _scopeFactory,
            _messagePublisher,
            _dispatcherConfiguration,
            _rabbitMqConfiguration,
            _logger);

        using var cts = new CancellationTokenSource();

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(200);
        await cts.CancelAsync();
        await service.StopAsync(CancellationToken.None);

        // Assert
        await _messagePublisher.DidNotReceive()
            .PublishAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<Guid?>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithMessages_ShouldPublishToCorrectExchange()
    {
        // Arrange
        var message = OutboxMessage.Create(
            Guid.NewGuid(),
            "WorkItemCreatedV1",
            """{"data": "test"}""",
            DateTime.UtcNow,
            Guid.NewGuid());

        _outboxRepository
            .GetUnprocessedMessagesAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<IReadOnlyList<OutboxMessage>>([message]),
                Task.FromResult<IReadOnlyList<OutboxMessage>>([]));

        var service = new OutboxDispatcherService(
            _scopeFactory,
            _messagePublisher,
            _dispatcherConfiguration,
            _rabbitMqConfiguration,
            _logger);

        using var cts = new CancellationTokenSource();

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(200);
        await cts.CancelAsync();
        await service.StopAsync(CancellationToken.None);

        // Assert
        await _messagePublisher.Received(1)
            .PublishAsync(
                "test.events",
                "WorkItemCreatedV1",
                """{"data": "test"}""",
                message.CorrelationId,
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithMessages_ShouldMarkAsProcessed()
    {
        // Arrange
        var message = OutboxMessage.Create(
            Guid.NewGuid(),
            "WorkItemCreatedV1",
            """{"data": "test"}""",
            DateTime.UtcNow);

        _outboxRepository
            .GetUnprocessedMessagesAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<IReadOnlyList<OutboxMessage>>([message]),
                Task.FromResult<IReadOnlyList<OutboxMessage>>([]));

        var service = new OutboxDispatcherService(
            _scopeFactory,
            _messagePublisher,
            _dispatcherConfiguration,
            _rabbitMqConfiguration,
            _logger);

        using var cts = new CancellationTokenSource();

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(200);
        await cts.CancelAsync();
        await service.StopAsync(CancellationToken.None);

        // Assert
        await _outboxRepository.Received(1)
            .UpdateAsync(
                Arg.Is<OutboxMessage>(m => m.ProcessedAtUtc != null),
                Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1)
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithMultipleMessages_ShouldProcessAllMessages()
    {
        // Arrange
        var messages = new List<OutboxMessage>
        {
            OutboxMessage.Create(Guid.NewGuid(), "Event1", """{"id": 1}""", DateTime.UtcNow),
            OutboxMessage.Create(Guid.NewGuid(), "Event2", """{"id": 2}""", DateTime.UtcNow),
            OutboxMessage.Create(Guid.NewGuid(), "Event3", """{"id": 3}""", DateTime.UtcNow)
        };

        _outboxRepository
            .GetUnprocessedMessagesAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<IReadOnlyList<OutboxMessage>>(messages),
                Task.FromResult<IReadOnlyList<OutboxMessage>>([]));

        var service = new OutboxDispatcherService(
            _scopeFactory,
            _messagePublisher,
            _dispatcherConfiguration,
            _rabbitMqConfiguration,
            _logger);

        using var cts = new CancellationTokenSource();

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(200);
        await cts.CancelAsync();
        await service.StopAsync(CancellationToken.None);

        // Assert
        await _messagePublisher.Received(3)
            .PublishAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<Guid?>(),
                Arg.Any<CancellationToken>());

        await _outboxRepository.Received(3)
            .UpdateAsync(Arg.Any<OutboxMessage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenPublishFails_ShouldContinueWithNextMessage()
    {
        // Arrange
        var messages = new List<OutboxMessage>
        {
            OutboxMessage.Create(Guid.NewGuid(), "FailingEvent", """{"id": 1}""", DateTime.UtcNow),
            OutboxMessage.Create(Guid.NewGuid(), "SuccessEvent", """{"id": 2}""", DateTime.UtcNow)
        };

        _outboxRepository
            .GetUnprocessedMessagesAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<IReadOnlyList<OutboxMessage>>(messages),
                Task.FromResult<IReadOnlyList<OutboxMessage>>([]));

        _messagePublisher
            .When(x => x.PublishAsync(
                Arg.Any<string>(),
                "FailingEvent",
                Arg.Any<string>(),
                Arg.Any<Guid?>(),
                Arg.Any<CancellationToken>()))
            .Throw(new InvalidOperationException("Connection failed"));

        var service = new OutboxDispatcherService(
            _scopeFactory,
            _messagePublisher,
            _dispatcherConfiguration,
            _rabbitMqConfiguration,
            _logger);

        using var cts = new CancellationTokenSource();

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(200);
        await cts.CancelAsync();
        await service.StopAsync(CancellationToken.None);

        // Assert - second message should still be processed
        await _messagePublisher.Received(1)
            .PublishAsync(
                Arg.Any<string>(),
                "SuccessEvent",
                Arg.Any<string>(),
                Arg.Any<Guid?>(),
                Arg.Any<CancellationToken>());

        // Both messages should be updated: failed one for retry count, successful one as processed
        await _outboxRepository.Received(2)
            .UpdateAsync(Arg.Any<OutboxMessage>(), Arg.Any<CancellationToken>());

        // Failed message should have retry count incremented
        messages[0].RetryCount.Should().Be(1);
        messages[0].FailedAtUtc.Should().BeNull(); // Not yet dead-lettered (under max retries)
    }

    [Fact]
    public async Task ExecuteAsync_WhenPublishFailsExceedingMaxRetries_ShouldDeadLetterMessage()
    {
        // Arrange - create a message that has already been retried 4 times (MaxRetries default is 5)
        var message = OutboxMessage.Create(
            Guid.NewGuid(),
            "PersistentlyFailingEvent",
            """{"id": 1}""",
            DateTime.UtcNow);

        // Simulate 4 prior retries by incrementing retry count
        for (var i = 0; i < 4; i++)
        {
            message.IncrementRetryCount();
        }

        message.RetryCount.Should().Be(4);

        _outboxRepository
            .GetUnprocessedMessagesAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<IReadOnlyList<OutboxMessage>>([message]),
                Task.FromResult<IReadOnlyList<OutboxMessage>>([]));

        _messagePublisher
            .PublishAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<Guid?>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Connection failed"));

        var service = new OutboxDispatcherService(
            _scopeFactory,
            _messagePublisher,
            _dispatcherConfiguration,
            _rabbitMqConfiguration,
            _logger);

        using var cts = new CancellationTokenSource();

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(200);
        await cts.CancelAsync();
        await service.StopAsync(CancellationToken.None);

        // Assert - message should be dead-lettered (retry count hits 5 which equals MaxRetries)
        message.RetryCount.Should().Be(5);
        message.FailedAtUtc.Should().NotBeNull();
        message.ProcessedAtUtc.Should().BeNull(); // Not processed, it was dead-lettered

        await _outboxRepository.Received()
            .UpdateAsync(
                Arg.Is<OutboxMessage>(m => m.FailedAtUtc != null),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUseBatchSizeFromConfiguration()
    {
        // Arrange
        var configWithSmallBatch = Options.Create(new OutboxDispatcherConfiguration
        {
            Enabled = true,
            PollingIntervalSeconds = 1,
            BatchSize = 25
        });

        _outboxRepository
            .GetUnprocessedMessagesAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<OutboxMessage>>([]));

        var service = new OutboxDispatcherService(
            _scopeFactory,
            _messagePublisher,
            configWithSmallBatch,
            _rabbitMqConfiguration,
            _logger);

        using var cts = new CancellationTokenSource();

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(200);
        await cts.CancelAsync();
        await service.StopAsync(CancellationToken.None);

        // Assert
        await _outboxRepository.Received()
            .GetUnprocessedMessagesAsync(25, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUseCorrectRoutingKey()
    {
        // Arrange
        var message = OutboxMessage.Create(
            Guid.NewGuid(),
            "WorkItemUpdatedV1",
            """{"data": "test"}""",
            DateTime.UtcNow);

        _outboxRepository
            .GetUnprocessedMessagesAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<IReadOnlyList<OutboxMessage>>([message]),
                Task.FromResult<IReadOnlyList<OutboxMessage>>([]));

        var service = new OutboxDispatcherService(
            _scopeFactory,
            _messagePublisher,
            _dispatcherConfiguration,
            _rabbitMqConfiguration,
            _logger);

        using var cts = new CancellationTokenSource();

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(200);
        await cts.CancelAsync();
        await service.StopAsync(CancellationToken.None);

        // Assert - routing key should be the event type
        await _messagePublisher.Received(1)
            .PublishAsync(
                Arg.Any<string>(),
                "WorkItemUpdatedV1",
                Arg.Any<string>(),
                Arg.Any<Guid?>(),
                Arg.Any<CancellationToken>());
    }
}
