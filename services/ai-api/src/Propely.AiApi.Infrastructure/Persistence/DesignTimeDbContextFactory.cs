// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Propely.AiApi.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for creating DbContext instances during migrations.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // Use environment variable or default development connection string
        var connectionString = Environment.GetEnvironmentVariable("AIAPI_ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=propely_aiapi;Username=propely;Password=propely_dev_password";

        optionsBuilder.UseNpgsql(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
