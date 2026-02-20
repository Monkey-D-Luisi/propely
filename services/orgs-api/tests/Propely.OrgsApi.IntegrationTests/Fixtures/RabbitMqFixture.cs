// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Testcontainers.RabbitMq;

namespace Propely.OrgsApi.IntegrationTests.Fixtures;

/// <summary>
/// Shared RabbitMQ container fixture for integration tests.
/// </summary>
public sealed class RabbitMqFixture : IAsyncLifetime
{
    private readonly RabbitMqContainer _container = new RabbitMqBuilder()
        .WithImage("rabbitmq:3-management-alpine")
        .WithUsername("testuser")
        .WithPassword("testpass")
        .Build();

    public string Host => _container.Hostname;
    public int Port => _container.GetMappedPublicPort(5672);
    public string Username => "testuser";
    public string Password => "testpass";
    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}

[CollectionDefinition("RabbitMQ")]
public class RabbitMqCollection : ICollectionFixture<RabbitMqFixture>
{
}

[CollectionDefinition("PostgresAndRabbitMQ")]
public class PostgresAndRabbitMqCollection : ICollectionFixture<PostgresFixture>, ICollectionFixture<RabbitMqFixture>
{
}
