// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.AiApi.Domain.Common.Exceptions;
using SaasTemplate.AiApi.Domain.WorkItems;
using SaasTemplate.AiApi.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace SaasTemplate.AiApi.IntegrationTests.Persistence;

[Collection("Postgres")]
public sealed class TenantQueryFilterTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    private readonly Guid _orgA = Guid.NewGuid();
    private readonly Guid _orgB = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    public TenantQueryFilterTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        // Seed: create work items for two different tenants using a system context (no tenant filter)
        await using var context = _fixture.CreateContext();

        var itemA1 = WorkItem.Create(_orgA, _userId, "Org A Item 1");
        var itemA2 = WorkItem.Create(_orgA, _userId, "Org A Item 2");
        var itemB1 = WorkItem.Create(_orgB, _userId, "Org B Item 1");

        context.WorkItems.AddRange(itemA1, itemA2, itemB1);
        await context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        // Clean up seeded data — use IgnoreQueryFilters to include soft-deleted items
        await using var context = _fixture.CreateContext();
        var all = await context.WorkItems.IgnoreQueryFilters().ToListAsync();
        context.WorkItems.RemoveRange(all);
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task Query_WithTenantA_ShouldReturnOnlyTenantAItems()
    {
        // Arrange
        await using var context = _fixture.CreateContext(_orgA);

        // Act
        var items = await context.WorkItems.ToListAsync();

        // Assert
        items.Should().HaveCount(2);
        items.Should().AllSatisfy(w => w.OrgId.Should().Be(_orgA));
    }

    [Fact]
    public async Task Query_WithTenantB_ShouldReturnOnlyTenantBItems()
    {
        // Arrange
        await using var context = _fixture.CreateContext(_orgB);

        // Act
        var items = await context.WorkItems.ToListAsync();

        // Assert
        items.Should().HaveCount(1);
        items.Should().AllSatisfy(w => w.OrgId.Should().Be(_orgB));
    }

    [Fact]
    public async Task Query_WithNoTenant_ShouldReturnAllItems()
    {
        // Arrange - system context (no tenant)
        await using var context = _fixture.CreateContext();

        // Act
        var items = await context.WorkItems.ToListAsync();

        // Assert
        items.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task Query_WithTenantFilter_ShouldCombineWithSoftDelete()
    {
        // Arrange - soft-delete one of org A's items
        await using var setupContext = _fixture.CreateContext();
        var itemToDelete = await setupContext.WorkItems
            .FirstAsync(w => w.OrgId == _orgA);
        itemToDelete.SoftDelete();
        await setupContext.SaveChangesAsync();

        // Act - query as tenant A
        await using var context = _fixture.CreateContext(_orgA);
        var items = await context.WorkItems.ToListAsync();

        // Assert - should only see the non-deleted org A item
        items.Should().HaveCount(1);
        items.Should().AllSatisfy(w =>
        {
            w.OrgId.Should().Be(_orgA);
            w.IsDeleted.Should().BeFalse();
        });
    }

    [Fact]
    public async Task Query_WithIgnoreQueryFilters_ShouldBypassTenantAndSoftDelete()
    {
        // Arrange - soft-delete one of org A's items
        await using var setupContext = _fixture.CreateContext();
        var nonDeletedItems = await setupContext.WorkItems
            .Where(w => w.OrgId == _orgA && !w.IsDeleted)
            .ToListAsync();
        if (nonDeletedItems.Count > 0)
        {
            nonDeletedItems[0].SoftDelete();
            await setupContext.SaveChangesAsync();
        }

        // Act - query as tenant A but with IgnoreQueryFilters
        await using var context = _fixture.CreateContext(_orgA);
        var items = await context.WorkItems
            .IgnoreQueryFilters()
            .ToListAsync();

        // Assert - should see ALL items across ALL tenants, including deleted
        items.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task ReadModel_WithTenantA_ShouldReturnOnlyTenantAReadItems()
    {
        // Arrange - insert read model entries directly
        await using var setupContext = _fixture.CreateContext();
        var readA = SaasTemplate.AiApi.Infrastructure.Persistence.Entities.WorkItemRead.FromCreatedEvent(
            Guid.NewGuid(), _orgA, _userId, "Read A", null, "New", null, null, null, null, DateTime.UtcNow);
        var readB = SaasTemplate.AiApi.Infrastructure.Persistence.Entities.WorkItemRead.FromCreatedEvent(
            Guid.NewGuid(), _orgB, _userId, "Read B", null, "New", null, null, null, null, DateTime.UtcNow);
        setupContext.WorkItemsRead.AddRange(readA, readB);
        await setupContext.SaveChangesAsync();

        // Act
        await using var context = _fixture.CreateContext(_orgA);
        var items = await context.WorkItemsRead.ToListAsync();

        // Assert
        items.Should().AllSatisfy(w => w.OrgId.Should().Be(_orgA));
        items.Should().Contain(w => w.Title == "Read A");
        items.Should().NotContain(w => w.Title == "Read B");

        // Cleanup
        await using var cleanupContext = _fixture.CreateContext();
        var readItems = await cleanupContext.WorkItemsRead
            .Where(w => w.Id == readA.Id || w.Id == readB.Id)
            .ToListAsync();
        cleanupContext.WorkItemsRead.RemoveRange(readItems);
        await cleanupContext.SaveChangesAsync();
    }

    [Fact]
    public async Task SaveChanges_WithMismatchedOrgId_ShouldThrowTenantMismatchException()
    {
        // Arrange — create a context scoped to tenant A
        await using var context = _fixture.CreateContext(_orgA);

        // Create a WorkItem explicitly assigned to tenant B (cross-tenant injection attempt)
        var crossTenantItem = WorkItem.Create(_orgB, _userId, "Cross-Tenant Injection Attempt");
        context.WorkItems.Add(crossTenantItem);

        // Act
        var act = () => context.SaveChangesAsync();

        // Assert — SetTenantIdOnNewEntities() should detect the mismatch and throw
        await act.Should().ThrowAsync<TenantMismatchException>()
            .WithMessage($"*'{_orgB}'*'{_orgA}'*");
    }
}
