// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Text.Json;
using SaasTemplate.AiApi.Application.Common.Interfaces;
using SaasTemplate.AiApi.Infrastructure.Caching.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace SaasTemplate.AiApi.Infrastructure.Caching;

/// <summary>
/// Redis implementation of ICacheService.
/// </summary>
public sealed class RedisCacheService : ICacheService, IAsyncDisposable
{
    private readonly RedisConfiguration _configuration;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    private ConnectionMultiplexer? _connection;
    private IDatabase? _database;
    private bool _disposed;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public RedisCacheService(
        IOptions<RedisConfiguration> configuration,
        ILogger<RedisCacheService> logger)
    {
        _configuration = configuration.Value;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        if (!_configuration.Enabled)
        {
            return null;
        }

        try
        {
            await EnsureConnectionAsync(cancellationToken);

            if (_database is null)
            {
                return null;
            }

            var fullKey = GetFullKey(key);
            var value = await _database.StringGetAsync(fullKey);

            if (value.IsNullOrEmpty)
            {
                _logger.LogDebug("Cache miss for key {Key}", key);
                return null;
            }

            _logger.LogDebug("Cache hit for key {Key}", key);
            return JsonSerializer.Deserialize<T>((string)value!, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting value from cache for key {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default) where T : class
    {
        if (!_configuration.Enabled)
        {
            return;
        }

        try
        {
            await EnsureConnectionAsync(cancellationToken);

            if (_database is null)
            {
                return;
            }

            var fullKey = GetFullKey(key);
            var serialized = JsonSerializer.Serialize(value, JsonOptions);

            await _database.StringSetAsync(fullKey, serialized, expiration);

            _logger.LogDebug("Cached value for key {Key} with TTL {Ttl}", key, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error setting value in cache for key {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        if (!_configuration.Enabled)
        {
            return;
        }

        try
        {
            await EnsureConnectionAsync(cancellationToken);

            if (_database is null)
            {
                return;
            }

            var fullKey = GetFullKey(key);
            await _database.KeyDeleteAsync(fullKey);

            _logger.LogDebug("Removed cache entry for key {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error removing value from cache for key {Key}", key);
        }
    }

    private string GetFullKey(string key) => $"{_configuration.KeyPrefix}{key}";

    private async Task EnsureConnectionAsync(CancellationToken cancellationToken)
    {
        if (_connection is not null && _connection.IsConnected)
        {
            return;
        }

        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (_connection is not null && _connection.IsConnected)
            {
                return;
            }

            _logger.LogInformation("Connecting to Redis at {ConnectionString}", 
                RedactConnectionString(_configuration.ConnectionString));

            var options = ConfigurationOptions.Parse(_configuration.ConnectionString);
            options.AbortOnConnectFail = false;
            options.ConnectTimeout = 5000;
            options.SyncTimeout = 5000;

            // Trust Google Memorystore CA cert (private VPC traffic, not publicly trusted)
            if (options.Ssl)
            {
                options.CertificateValidation += (_, _, _, _) => true;
            }

            _connection = await ConnectionMultiplexer.ConnectAsync(options);
            _database = _connection.GetDatabase();

            _logger.LogInformation("Connected to Redis");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to Redis");
            // Don't rethrow - allow graceful fallback. Calling methods check for null _database.
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    private static string RedactConnectionString(string connectionString)
    {
        // Simple redaction - hide password if present
        var parts = connectionString.Split(',');
        for (var i = 0; i < parts.Length; i++)
        {
            if (parts[i].StartsWith("password=", StringComparison.OrdinalIgnoreCase))
            {
                parts[i] = "password=***";
            }
        }
        return string.Join(",", parts);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }

        _connectionLock.Dispose();
    }
}
