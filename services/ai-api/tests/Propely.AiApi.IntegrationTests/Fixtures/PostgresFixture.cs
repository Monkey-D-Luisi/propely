// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Application.Common.Interfaces;
using Propely.AiApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Propely.AiApi.IntegrationTests.Fixtures;

/// <summary>
/// Shared PostgreSQL container fixture for integration tests.
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("testdb")
        .WithUsername("testuser")
        .WithPassword("testpass")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        // Create database schema from EF Core model (no migrations)
        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    /// <summary>
    /// Creates a context with no tenant filter (system/background mode - sees all data).
    /// </summary>
    public AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new AppDbContext(options);
    }

    /// <summary>
    /// Creates a context scoped to a specific tenant (OrgId).
    /// </summary>
    public AppDbContext CreateContext(Guid orgId)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new AppDbContext(options, new TestTenantAccessor(orgId));
    }
}

/// <summary>
/// Test implementation of ITenantAccessor that returns a fixed OrgId.
/// </summary>
internal sealed class TestTenantAccessor : ITenantAccessor
{
    private readonly Guid? _orgId;

    public TestTenantAccessor(Guid? orgId) => _orgId = orgId;

    public Guid? GetCurrentOrgId() => _orgId;
}

[CollectionDefinition("Postgres")]
public class PostgresCollection : ICollectionFixture<PostgresFixture>
{
}
