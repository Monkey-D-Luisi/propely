// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Infrastructure.Caching;
using Propely.AiApi.Infrastructure.Caching.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Propely.AiApi.IntegrationTests.Caching;

/// <summary>
/// Integration tests for RedisCacheService.
/// Note: These tests use mocked configuration and test the service behavior.
/// For full integration testing with Redis, use Testcontainers.
/// </summary>
public sealed class RedisCacheServiceTests
{
    [Fact]
    public async Task GetAsync_WhenDisabled_ShouldReturnNull()
    {
        // Arrange
        var config = new RedisConfiguration
        {
            Enabled = false,
            ConnectionString = "localhost:6379"
        };
        var options = Options.Create(config);
        var logger = NullLogger<RedisCacheService>.Instance;

        await using var service = new RedisCacheService(options, logger);

        // Act
        var result = await service.GetAsync<TestDto>("test-key");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_WhenDisabled_ShouldNotThrow()
    {
        // Arrange
        var config = new RedisConfiguration
        {
            Enabled = false,
            ConnectionString = "localhost:6379"
        };
        var options = Options.Create(config);
        var logger = NullLogger<RedisCacheService>.Instance;

        await using var service = new RedisCacheService(options, logger);
        var dto = new TestDto("test-id", "Test Name");

        // Act & Assert
        var act = async () => await service.SetAsync("test-key", dto, TimeSpan.FromMinutes(5));
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RemoveAsync_WhenDisabled_ShouldNotThrow()
    {
        // Arrange
        var config = new RedisConfiguration
        {
            Enabled = false,
            ConnectionString = "localhost:6379"
        };
        var options = Options.Create(config);
        var logger = NullLogger<RedisCacheService>.Instance;

        await using var service = new RedisCacheService(options, logger);

        // Act & Assert
        var act = async () => await service.RemoveAsync("test-key");
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void Configuration_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act
        var config = new RedisConfiguration();

        // Assert
        config.ConnectionString.Should().Be("localhost:6379");
        config.Enabled.Should().BeTrue();
        config.DefaultTtlMinutes.Should().Be(5);
        config.KeyPrefix.Should().Be("aiapi:");
    }

    [Fact]
    public void Configuration_ShouldApplySectionName()
    {
        // Arrange & Act
        var sectionName = RedisConfiguration.SectionName;

        // Assert
        sectionName.Should().Be("Redis");
    }

    /// <summary>
    /// Test DTO for caching tests.
    /// </summary>
    private sealed record TestDto(string Id, string Name);
}
