// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Propely.ContactsApi.Infrastructure.Messaging.Configuration;

namespace Propely.ContactsApi.UnitTests.Infrastructure.Messaging.Configuration;

public sealed class RabbitMqConfigurationTests
{
    [Fact]
    public void Defaults_ShouldBeSetCorrectly()
    {
        var config = new RabbitMqConfiguration();

        config.Host.Should().Be("localhost");
        config.Port.Should().Be(5672);
        config.Username.Should().Be("guest");
        config.Password.Should().Be("guest");
        config.VirtualHost.Should().Be("/");
        config.EventsExchange.Should().Be("contacts.events");
        config.UseSsl.Should().BeFalse();
        config.ConnectionTimeoutSeconds.Should().Be(30);
    }

    [Fact]
    public void Host_ShouldTrimWhitespace()
    {
        var config = new RabbitMqConfiguration { Host = "  myhost  " };

        config.Host.Should().Be("myhost");
    }

    [Fact]
    public void Host_WhenNull_ShouldFallbackToDefault()
    {
        var config = new RabbitMqConfiguration { Host = null! };

        config.Host.Should().Be("localhost");
    }

    [Fact]
    public void Username_ShouldTrimWhitespace()
    {
        var config = new RabbitMqConfiguration { Username = "  admin  " };

        config.Username.Should().Be("admin");
    }

    [Fact]
    public void Username_WhenNull_ShouldFallbackToDefault()
    {
        var config = new RabbitMqConfiguration { Username = null! };

        config.Username.Should().Be("guest");
    }

    [Fact]
    public void Password_ShouldTrimWhitespace()
    {
        var config = new RabbitMqConfiguration { Password = "  secret  " };

        config.Password.Should().Be("secret");
    }

    [Fact]
    public void Password_WhenNull_ShouldFallbackToEmpty()
    {
        var config = new RabbitMqConfiguration { Password = null! };

        config.Password.Should().BeEmpty();
    }

    [Fact]
    public void VirtualHost_ShouldTrimWhitespace()
    {
        var config = new RabbitMqConfiguration { VirtualHost = "  /myvhost  " };

        config.VirtualHost.Should().Be("/myvhost");
    }

    [Fact]
    public void VirtualHost_WhenNull_ShouldFallbackToDefault()
    {
        var config = new RabbitMqConfiguration { VirtualHost = null! };

        config.VirtualHost.Should().Be("/");
    }

    [Fact]
    public void SectionName_ShouldBeRabbitMQ()
    {
        RabbitMqConfiguration.SectionName.Should().Be("RabbitMQ");
    }
}
