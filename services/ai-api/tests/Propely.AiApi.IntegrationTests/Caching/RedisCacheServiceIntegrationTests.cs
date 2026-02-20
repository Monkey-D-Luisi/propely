// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Infrastructure.Caching;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Testcontainers.Redis;
using RedisConfig = Propely.AiApi.Infrastructure.Caching.Configuration.RedisConfiguration;

namespace Propely.AiApi.IntegrationTests.Caching;

/// <summary>
/// Integration tests for RedisCacheService using Testcontainers.
/// Tests actual Redis operations with a real container.
/// </summary>
public sealed class RedisCacheServiceIntegrationTests : IAsyncLifetime
{
    private readonly RedisContainer _redisContainer;
    private RedisCacheService _cacheService = null!;

    public RedisCacheServiceIntegrationTests()
    {
        _redisContainer = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _redisContainer.StartAsync();

        var config = new RedisConfig
        {
            Enabled = true,
            ConnectionString = _redisContainer.GetConnectionString()
        };

        var options = Options.Create(config);
        var logger = NullLogger<RedisCacheService>.Instance;

        _cacheService = new RedisCacheService(options, logger);
    }

    public async Task DisposeAsync()
    {
        if (_cacheService is not null)
        {
            await _cacheService.DisposeAsync();
        }
        await _redisContainer.DisposeAsync();
    }

    [Fact]
    public async Task SetAsync_AndGetAsync_ShouldRoundtripValue()
    {
        // Arrange
        var key = $"test-key-{Guid.NewGuid()}";
        var dto = new TestCacheDto("id-123", "Test Name", 42);

        // Act
        await _cacheService.SetAsync(key, dto, TimeSpan.FromMinutes(5));
        var result = await _cacheService.GetAsync<TestCacheDto>(key);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("id-123");
        result.Name.Should().Be("Test Name");
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task GetAsync_WhenKeyDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var key = $"nonexistent-{Guid.NewGuid()}";

        // Act
        var result = await _cacheService.GetAsync<TestCacheDto>(key);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task RemoveAsync_ShouldDeleteKey()
    {
        // Arrange
        var key = $"removable-{Guid.NewGuid()}";
        var dto = new TestCacheDto("id-456", "To Remove", 100);

        await _cacheService.SetAsync(key, dto, TimeSpan.FromMinutes(5));

        // Verify it was set
        var beforeRemove = await _cacheService.GetAsync<TestCacheDto>(key);
        beforeRemove.Should().NotBeNull();

        // Act
        await _cacheService.RemoveAsync(key);

        // Assert
        var afterRemove = await _cacheService.GetAsync<TestCacheDto>(key);
        afterRemove.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_WithShortTtl_ShouldExpire()
    {
        // Arrange
        var key = $"expiring-{Guid.NewGuid()}";
        var dto = new TestCacheDto("id-789", "Expiring", 999);

        // Act - set with short TTL
        await _cacheService.SetAsync(key, dto, TimeSpan.FromSeconds(1));

        // Should exist immediately
        var immediate = await _cacheService.GetAsync<TestCacheDto>(key);
        immediate.Should().NotBeNull();

        // Poll for expiration (more reliable than fixed delay)
        TestCacheDto? afterExpiry = null;
        var timeout = DateTime.UtcNow.AddSeconds(5);
        while (DateTime.UtcNow < timeout)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(200));
            afterExpiry = await _cacheService.GetAsync<TestCacheDto>(key);
            if (afterExpiry is null)
            {
                break;
            }
        }

        // Assert - should be expired
        afterExpiry.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_CanOverwriteExistingKey()
    {
        // Arrange
        var key = $"overwrite-{Guid.NewGuid()}";
        var original = new TestCacheDto("original", "Original", 1);
        var updated = new TestCacheDto("updated", "Updated", 2);

        await _cacheService.SetAsync(key, original, TimeSpan.FromMinutes(5));

        // Act
        await _cacheService.SetAsync(key, updated, TimeSpan.FromMinutes(5));

        // Assert
        var result = await _cacheService.GetAsync<TestCacheDto>(key);
        result.Should().NotBeNull();
        result!.Id.Should().Be("updated");
        result.Name.Should().Be("Updated");
        result.Value.Should().Be(2);
    }

    [Fact]
    public async Task MultipleOperations_ShouldWorkConcurrently()
    {
        // Arrange
        var tasks = new List<Task>();

        // Act - perform multiple concurrent operations
        for (int i = 0; i < 10; i++)
        {
            var index = i;
            tasks.Add(Task.Run(async () =>
            {
                var key = $"concurrent-{index}-{Guid.NewGuid()}";
                var dto = new TestCacheDto($"id-{index}", $"Name-{index}", index);

                await _cacheService.SetAsync(key, dto, TimeSpan.FromMinutes(5));
                var result = await _cacheService.GetAsync<TestCacheDto>(key);

                result.Should().NotBeNull();
                result!.Id.Should().Be($"id-{index}");
            }));
        }

        // Assert - all operations should complete without error
        await Task.WhenAll(tasks);
    }
}

/// <summary>
/// Test DTO for cache operations.
/// </summary>
public sealed record TestCacheDto(string Id, string Name, int Value);
