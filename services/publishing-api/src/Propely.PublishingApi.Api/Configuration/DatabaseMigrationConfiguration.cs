// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Microsoft.EntityFrameworkCore;
using Npgsql;
using Propely.PublishingApi.Infrastructure.Persistence;

namespace Propely.PublishingApi.Api.Configuration;

public static class DatabaseMigrationConfiguration
{
    private const long MigrationLockId = 400_001;

    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        if (app.Environment.EnvironmentName == "Testing")
            return;

        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("DatabaseMigration");

        var pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();

        if (pendingMigrations.Count == 0)
        {
            logger.LogInformation("Database is up to date, no pending migrations");
            return;
        }

        logger.LogInformation(
            "Found {Count} pending migration(s): {Migrations}",
            pendingMigrations.Count,
            string.Join(", ", pendingMigrations));

        var connectionString = context.Database.GetConnectionString();

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            logger.LogError("Database connection string is unavailable; cannot acquire advisory lock for migrations");
            throw new InvalidOperationException("Database connection string is not configured.");
        }

        await using var lockConnection = new NpgsqlConnection(connectionString);
        await lockConnection.OpenAsync();

        logger.LogInformation("Acquiring advisory lock for migration...");

        await using var lockCommand = lockConnection.CreateCommand();
        lockCommand.CommandText = $"SELECT pg_advisory_lock({MigrationLockId})";
        await lockCommand.ExecuteNonQueryAsync();

        try
        {
            pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();

            if (pendingMigrations.Count > 0)
            {
                await context.Database.MigrateAsync();
                logger.LogInformation("Migrations applied successfully");
            }
            else
            {
                logger.LogInformation("Migrations were already applied by another instance");
            }
        }
        finally
        {
            await using var unlockCommand = lockConnection.CreateCommand();
            unlockCommand.CommandText = $"SELECT pg_advisory_unlock({MigrationLockId})";
            await unlockCommand.ExecuteNonQueryAsync();
            logger.LogInformation("Advisory lock released");
        }
    }
}
