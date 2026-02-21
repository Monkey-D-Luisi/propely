// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Propely.PublishingApi.Infrastructure.Caching.Configuration;

namespace Propely.PublishingApi.UnitTests.Infrastructure.Caching.Configuration;

public sealed class RedisConfigurationTests
{
    [Fact]
    public void Defaults_ShouldBeSetCorrectly()
    {
        var config = new RedisConfiguration();

        config.ConnectionString.Should().Be("localhost:6379");
        config.Enabled.Should().BeTrue();
        config.DefaultTtlMinutes.Should().Be(5);
        config.KeyPrefix.Should().Be("publishingapi:");
    }

    [Fact]
    public void SectionName_ShouldBeRedis()
    {
        RedisConfiguration.SectionName.Should().Be("Redis");
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        var config = new RedisConfiguration
        {
            ConnectionString = "redis.example.com:6380",
            Enabled = false,
            DefaultTtlMinutes = 30,
            KeyPrefix = "test:"
        };

        config.ConnectionString.Should().Be("redis.example.com:6380");
        config.Enabled.Should().BeFalse();
        config.DefaultTtlMinutes.Should().Be(30);
        config.KeyPrefix.Should().Be("test:");
    }

    [Fact]
    public void ShouldImplementICacheSettings()
    {
        var config = new RedisConfiguration();

        config.Should().BeAssignableTo<Propely.PublishingApi.Application.Common.Interfaces.ICacheSettings>();
        ((Propely.PublishingApi.Application.Common.Interfaces.ICacheSettings)config).DefaultTtlMinutes.Should().Be(5);
    }
}
