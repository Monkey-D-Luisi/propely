// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Propely.PropertiesApi.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for creating DbContext instances during migrations.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        var connectionString = Environment.GetEnvironmentVariable("PROPERTIESAPI_ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=propely_propertiesapi;Username=propely;Password=propely_dev_password";

        optionsBuilder.UseNpgsql(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
