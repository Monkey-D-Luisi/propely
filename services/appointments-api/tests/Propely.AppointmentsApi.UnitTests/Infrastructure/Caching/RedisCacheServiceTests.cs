// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Propely.AppointmentsApi.Infrastructure.Caching;
using Propely.AppointmentsApi.Infrastructure.Caching.Configuration;

namespace Propely.AppointmentsApi.UnitTests.Infrastructure.Caching;

public sealed class RedisCacheServiceTests : IAsyncDisposable
{
    private readonly ILogger<RedisCacheService> _logger = Substitute.For<ILogger<RedisCacheService>>();

    public async ValueTask DisposeAsync()
    {
        // Ensure any created services are disposed
        await Task.CompletedTask;
    }

    [Fact]
    public async Task GetAsync_WhenDisabled_ShouldReturnNull()
    {
        var config = Options.Create(new RedisConfiguration { Enabled = false });
        await using var service = new RedisCacheService(config, _logger);

        var result = await service.GetAsync<string>("test-key");

        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_WhenDisabled_ShouldReturnWithoutError()
    {
        var config = Options.Create(new RedisConfiguration { Enabled = false });
        await using var service = new RedisCacheService(config, _logger);

        var act = () => service.SetAsync("test-key", "test-value", TimeSpan.FromMinutes(5));

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RemoveAsync_WhenDisabled_ShouldReturnWithoutError()
    {
        var config = Options.Create(new RedisConfiguration { Enabled = false });
        await using var service = new RedisCacheService(config, _logger);

        var act = () => service.RemoveAsync("test-key");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetAsync_WhenConnectionFails_ShouldReturnNull()
    {
        var config = Options.Create(new RedisConfiguration
        {
            Enabled = true,
            ConnectionString = "nonexistent-host-12345:1",
            KeyPrefix = "test:"
        });
        await using var service = new RedisCacheService(config, _logger);

        var result = await service.GetAsync<string>("test-key");

        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_WhenConnectionFails_ShouldNotThrow()
    {
        var config = Options.Create(new RedisConfiguration
        {
            Enabled = true,
            ConnectionString = "nonexistent-host-12345:1",
            KeyPrefix = "test:"
        });
        await using var service = new RedisCacheService(config, _logger);

        var act = () => service.SetAsync("test-key", "test-value", TimeSpan.FromMinutes(5));

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RemoveAsync_WhenConnectionFails_ShouldNotThrow()
    {
        var config = Options.Create(new RedisConfiguration
        {
            Enabled = true,
            ConnectionString = "nonexistent-host-12345:1",
            KeyPrefix = "test:"
        });
        await using var service = new RedisCacheService(config, _logger);

        var act = () => service.RemoveAsync("test-key");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DisposeAsync_ShouldBeIdempotent()
    {
        var config = Options.Create(new RedisConfiguration { Enabled = false });
        var service = new RedisCacheService(config, _logger);

        await service.DisposeAsync();
        // Second dispose should not throw
        await service.DisposeAsync();
    }
}
