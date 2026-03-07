// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Propely.AppointmentsApi.Application.Common.Interfaces;
using Propely.AppointmentsApi.Application.Common.Models;
using Propely.AppointmentsApi.Infrastructure.Messaging;
using Propely.AppointmentsApi.Infrastructure.Messaging.Configuration;

namespace Propely.AppointmentsApi.UnitTests.Infrastructure.Messaging;

public sealed class OutboxDispatcherServiceTests
{
    private readonly IServiceScopeFactory _scopeFactory = Substitute.For<IServiceScopeFactory>();
    private readonly IMessagePublisher _publisher = Substitute.For<IMessagePublisher>();
    private readonly ILogger<OutboxDispatcherService> _logger = Substitute.For<ILogger<OutboxDispatcherService>>();
    private readonly IOutboxRepository _outboxRepository = Substitute.For<IOutboxRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    public OutboxDispatcherServiceTests()
    {
        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IOutboxRepository)).Returns(_outboxRepository);
        serviceProvider.GetService(typeof(IUnitOfWork)).Returns(_unitOfWork);

        var scope = Substitute.For<IServiceScope>();
        scope.ServiceProvider.Returns(serviceProvider);

        _scopeFactory.CreateScope().Returns(scope);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDisabled_ShouldNotProcessMessages()
    {
        var config = Options.Create(new OutboxDispatcherConfiguration { Enabled = false });
        var rabbitConfig = Options.Create(new RabbitMqConfiguration());
        var service = new OutboxDispatcherService(_scopeFactory, _publisher, config, rabbitConfig, _logger);

        await service.StartAsync(CancellationToken.None);
        await Task.Delay(100);
        await service.StopAsync(CancellationToken.None);

        _scopeFactory.DidNotReceive().CreateScope();
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoMessages_ShouldNotPublish()
    {
        var config = Options.Create(new OutboxDispatcherConfiguration
        {
            Enabled = true,
            PollingIntervalSeconds = 300
        });
        var rabbitConfig = Options.Create(new RabbitMqConfiguration());

        _outboxRepository.GetUnprocessedMessagesAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<OutboxMessage>());

        var service = new OutboxDispatcherService(_scopeFactory, _publisher, config, rabbitConfig, _logger);

        using var cts = new CancellationTokenSource();
        await service.StartAsync(cts.Token);
        await Task.Delay(200);
        cts.Cancel();
        try { await service.StopAsync(CancellationToken.None); } catch (OperationCanceledException) { }

        await _publisher.DidNotReceive().PublishAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<Guid?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithMessages_ShouldPublishAndMarkProcessed()
    {
        var config = Options.Create(new OutboxDispatcherConfiguration
        {
            Enabled = true,
            PollingIntervalSeconds = 300
        });
        var rabbitConfig = Options.Create(new RabbitMqConfiguration());

        var message = OutboxMessage.Create(
            Guid.NewGuid(), "TestEvent", "{\"test\":true}",
            DateTime.UtcNow, Guid.NewGuid(), null);

        _outboxRepository.GetUnprocessedMessagesAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<OutboxMessage> { message });

        var service = new OutboxDispatcherService(_scopeFactory, _publisher, config, rabbitConfig, _logger);

        using var cts = new CancellationTokenSource();
        await service.StartAsync(cts.Token);
        await Task.Delay(1000);
        cts.Cancel();
        try { await service.StopAsync(CancellationToken.None); } catch (OperationCanceledException) { }

        await _publisher.Received(1).PublishAsync(
            "appointments.events",
            "TestEvent",
            "{\"test\":true}",
            Arg.Any<Guid?>(),
            Arg.Any<CancellationToken>());

        message.ProcessedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenPublishFails_ShouldContinueWithNextMessage()
    {
        var config = Options.Create(new OutboxDispatcherConfiguration
        {
            Enabled = true,
            PollingIntervalSeconds = 300
        });
        var rabbitConfig = Options.Create(new RabbitMqConfiguration());

        var message1 = OutboxMessage.Create(
            Guid.NewGuid(), "FailEvent", "{}", DateTime.UtcNow, null, null);
        var message2 = OutboxMessage.Create(
            Guid.NewGuid(), "SuccessEvent", "{}", DateTime.UtcNow, null, null);

        _outboxRepository.GetUnprocessedMessagesAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<OutboxMessage> { message1, message2 });

        _publisher.PublishAsync(
            Arg.Any<string>(), "FailEvent", Arg.Any<string>(),
            Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("Publish failed")));

        _publisher.PublishAsync(
            Arg.Any<string>(), "SuccessEvent", Arg.Any<string>(),
            Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var service = new OutboxDispatcherService(_scopeFactory, _publisher, config, rabbitConfig, _logger);

        using var cts = new CancellationTokenSource();
        await service.StartAsync(cts.Token);
        await Task.Delay(1000);
        cts.Cancel();
        try { await service.StopAsync(CancellationToken.None); } catch (OperationCanceledException) { }

        message1.ProcessedAtUtc.Should().BeNull();
        message2.ProcessedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenSaveChangesFails_ShouldLogAndContinue()
    {
        var config = Options.Create(new OutboxDispatcherConfiguration
        {
            Enabled = true,
            PollingIntervalSeconds = 300
        });
        var rabbitConfig = Options.Create(new RabbitMqConfiguration());

        var message = OutboxMessage.Create(
            Guid.NewGuid(), "TestEvent", "{}", DateTime.UtcNow, null, null);

        _outboxRepository.GetUnprocessedMessagesAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<OutboxMessage> { message });

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<int>(new Exception("DB save failed")));

        var service = new OutboxDispatcherService(_scopeFactory, _publisher, config, rabbitConfig, _logger);

        using var cts = new CancellationTokenSource();
        await service.StartAsync(cts.Token);
        await Task.Delay(1000);
        cts.Cancel();
        try { await service.StopAsync(CancellationToken.None); } catch (OperationCanceledException) { }

        await _publisher.Received(1).PublishAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<Guid?>(), Arg.Any<CancellationToken>());
    }
}
