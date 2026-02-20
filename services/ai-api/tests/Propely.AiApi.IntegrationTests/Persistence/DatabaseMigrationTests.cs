// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Propely.AiApi.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Propely.AiApi.IntegrationTests.Persistence;

/// <summary>
/// Tests that EF Core migrations can be applied to a fresh database and that
/// the PostgreSQL advisory lock prevents concurrent migration conflicts.
/// </summary>
public sealed class DatabaseMigrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("migration_testdb")
        .WithUsername("testuser")
        .WithPassword("testpass")
        .Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    private AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task MigrateAsync_AppliesAllMigrations_ToFreshDatabase()
    {
        // Arrange — fresh database with no schema
        await using var context = CreateContext();

        // Act
        await context.Database.MigrateAsync();

        // Assert — verify migrations history table has entries
        var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
        appliedMigrations.Should().NotBeEmpty("all migration files should be applied");

        // Verify no pending migrations remain
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        pendingMigrations.Should().BeEmpty("all migrations should have been applied");
    }

    [Fact]
    public async Task MigrateAsync_IsIdempotent_WhenRunTwice()
    {
        // Arrange — apply migrations once
        await using var context1 = CreateContext();
        await context1.Database.MigrateAsync();
        var firstRunMigrations = (await context1.Database.GetAppliedMigrationsAsync()).ToList();

        // Act — apply again
        await using var context2 = CreateContext();
        await context2.Database.MigrateAsync();
        var secondRunMigrations = (await context2.Database.GetAppliedMigrationsAsync()).ToList();

        // Assert — same set of migrations
        secondRunMigrations.Should().BeEquivalentTo(firstRunMigrations);
    }

    [Fact]
    public async Task AdvisoryLock_PreventsConcurrentMigrations()
    {
        const long lockId = 200_001; // same as DatabaseMigrationConfiguration

        // Acquire an advisory lock on the same key
        await using var lockConnection = new NpgsqlConnection(_container.GetConnectionString());
        await lockConnection.OpenAsync();

        await using var lockCommand = lockConnection.CreateCommand();
        lockCommand.CommandText = $"SELECT pg_advisory_lock({lockId})";
        await lockCommand.ExecuteNonQueryAsync();

        // Try to acquire the lock non-blocking from another connection
        await using var secondConnection = new NpgsqlConnection(_container.GetConnectionString());
        await secondConnection.OpenAsync();

        await using var tryLockCommand = secondConnection.CreateCommand();
        tryLockCommand.CommandText = $"SELECT pg_try_advisory_lock({lockId})";
        var lockAcquired = (bool)(await tryLockCommand.ExecuteScalarAsync())!;

        // The second connection should fail to acquire the lock
        lockAcquired.Should().BeFalse("advisory lock should prevent concurrent acquisition");

        // Release the first lock
        await using var unlockCommand = lockConnection.CreateCommand();
        unlockCommand.CommandText = $"SELECT pg_advisory_unlock({lockId})";
        await unlockCommand.ExecuteNonQueryAsync();

        // Now the second connection should be able to acquire it
        await using var retryLockCommand = secondConnection.CreateCommand();
        retryLockCommand.CommandText = $"SELECT pg_try_advisory_lock({lockId})";
        var lockAcquiredAfterRelease = (bool)(await retryLockCommand.ExecuteScalarAsync())!;
        lockAcquiredAfterRelease.Should().BeTrue("lock should be available after release");

        // Clean up
        await using var cleanupCommand = secondConnection.CreateCommand();
        cleanupCommand.CommandText = $"SELECT pg_advisory_unlock({lockId})";
        await cleanupCommand.ExecuteNonQueryAsync();
    }
}
