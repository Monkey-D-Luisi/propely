// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Propely.ContactsApi.Infrastructure.Messaging;
using Propely.ContactsApi.Infrastructure.Messaging.Configuration;

namespace Propely.ContactsApi.UnitTests.Infrastructure.Messaging;

public sealed class RabbitMqPublisherTests
{
    [Fact]
    public async Task DisposeAsync_ShouldMarkAsDisposed()
    {
        var config = Options.Create(new RabbitMqConfiguration());
        var logger = Substitute.For<ILogger<RabbitMqPublisher>>();
        var publisher = new RabbitMqPublisher(config, logger);

        await publisher.DisposeAsync();

        var act = () => publisher.PublishAsync("exchange", "key", "payload");
        await act.Should().ThrowAsync<ObjectDisposedException>();
    }

    [Fact]
    public async Task DisposeAsync_ShouldBeIdempotent()
    {
        var config = Options.Create(new RabbitMqConfiguration());
        var logger = Substitute.For<ILogger<RabbitMqPublisher>>();
        var publisher = new RabbitMqPublisher(config, logger);

        await publisher.DisposeAsync();
        // Second dispose should not throw
        await publisher.DisposeAsync();
    }
}
