// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Text.Json;
using Propely.AiApi.Domain.WorkItems;
using Propely.AiApi.Infrastructure.Persistence;
using Propely.AiApi.Infrastructure.Persistence.Repositories;
using Propely.AiApi.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Propely.AiApi.IntegrationTests.Persistence;

[Collection("Postgres")]
public sealed class WorkItemRepositoryTests
{
    private readonly PostgresFixture _fixture;

    public WorkItemRepositoryTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AddAsync_ShouldPersistWorkItem()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        var repository = new WorkItemRepository(context);
        var workItem = WorkItem.Create(Guid.NewGuid(), Guid.NewGuid(), "Test Title", "Test Description");

        // Act
        await repository.AddAsync(workItem);
        await context.SaveChangesAsync();

        // Assert
        await using var verifyContext = _fixture.CreateContext();
        var persisted = await verifyContext.WorkItems.FirstOrDefaultAsync(w => w.Id == workItem.Id);
        persisted.Should().NotBeNull();
        persisted!.Title.Should().Be("Test Title");
        persisted.Description.Should().Be("Test Description");
        persisted.Status.Should().Be(WorkItemStatus.Pending);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ShouldReturnWorkItem()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        var repository = new WorkItemRepository(context);
        var workItem = WorkItem.Create(Guid.NewGuid(), Guid.NewGuid(), "Retrievable Item");
        await repository.AddAsync(workItem);
        await context.SaveChangesAsync();

        // Act
        await using var queryContext = _fixture.CreateContext();
        var queryRepository = new WorkItemRepository(queryContext);
        var retrieved = await queryRepository.GetByIdAsync(workItem.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(workItem.Id);
        retrieved.Title.Should().Be("Retrievable Item");
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotExists_ShouldReturnNull()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        var repository = new WorkItemRepository(context);
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldDispatchDomainEventsToOutbox()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        var repository = new WorkItemRepository(context);
        var workItem = WorkItem.Create(Guid.NewGuid(), Guid.NewGuid(), "Event Test Item");

        // Act
        await repository.AddAsync(workItem);
        await context.SaveChangesAsync();

        // Assert
        await using var verifyContext = _fixture.CreateContext();
        var outboxMessages = await verifyContext.OutboxMessages
            .Where(m => m.EventType == "WorkItemCreatedV1")
            .OrderByDescending(m => m.OccurredAtUtc)
            .ToListAsync();

        var relevantMessage = outboxMessages.FirstOrDefault(m =>
        {
            var json = JsonDocument.Parse(m.Payload);
            return json.RootElement.TryGetProperty("data", out var data) &&
                   data.TryGetProperty("workItemId", out var idProp) &&
                   idProp.GetGuid() == workItem.Id;
        });
        relevantMessage.Should().NotBeNull();
        relevantMessage!.ProcessedAtUtc.Should().BeNull();
    }

    [Fact]
    public async Task WorkItemTable_ShouldUseSnakeCaseNaming()
    {
        // Arrange
        await using var context = _fixture.CreateContext();

        // Act - Query raw SQL to verify column names
        var columnNames = await context.Database
            .SqlQueryRaw<string>(
                "SELECT column_name FROM information_schema.columns WHERE table_name = 'work_items'")
            .ToListAsync();

        // Assert
        columnNames.Should().Contain("id");
        columnNames.Should().Contain("org_id");
        columnNames.Should().Contain("title");
        columnNames.Should().Contain("description");
        columnNames.Should().Contain("status");
        columnNames.Should().Contain("created_at_utc");
        columnNames.Should().Contain("updated_at_utc");
        columnNames.Should().Contain("version");
    }

    [Fact]
    public async Task OutboxTable_ShouldUseSnakeCaseNaming()
    {
        // Arrange
        await using var context = _fixture.CreateContext();

        // Act - Query raw SQL to verify column names
        var columnNames = await context.Database
            .SqlQueryRaw<string>(
                "SELECT column_name FROM information_schema.columns WHERE table_name = 'outbox_messages'")
            .ToListAsync();

        // Assert
        columnNames.Should().Contain("id");
        columnNames.Should().Contain("event_type");
        columnNames.Should().Contain("payload");
        columnNames.Should().Contain("occurred_at_utc");
        columnNames.Should().Contain("processed_at_utc");
        columnNames.Should().Contain("correlation_id");
        columnNames.Should().Contain("causation_id");
    }

}
