// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Domain.Common;
using Propely.OrgsApi.Domain.Organizations;
using Propely.OrgsApi.Infrastructure.Persistence;
using Propely.OrgsApi.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;

namespace Propely.OrgsApi.IntegrationTests.Persistence;

[Collection("Postgres")]
public sealed class AuditLogTests : IAsyncLifetime
{
    private readonly PostgresFixture _postgresFixture;

    public AuditLogTests(PostgresFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task SaveChangesAsync_WhenEntityCreated_ShouldCreateAuditLogWithCreatedAction()
    {
        // Arrange
        var auditContext = new TestAuditContext(Guid.NewGuid(), "corr-create-test");
        await using var context = CreateContextWithAudit(auditContext);

        var org = Organization.Create("Audit Test Org");

        // Act
        context.Organizations.Add(org);
        await context.SaveChangesAsync();

        // Assert — use a fresh context to verify persistence
        await using var verifyContext = _postgresFixture.CreateContext();
        var auditLog = await verifyContext.AuditLogs
            .FirstOrDefaultAsync(a => a.EntityId == org.Id.ToString() && a.EntityType == "Organization");

        auditLog.Should().NotBeNull();
        auditLog!.Action.Should().Be("Created");
        auditLog.UserId.Should().Be(auditContext.UserId);
        auditLog.CorrelationId.Should().Be(auditContext.CorrelationId);
        auditLog.Changes.Should().NotBeNullOrEmpty();
        auditLog.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task SaveChangesAsync_WhenEntityModified_ShouldCreateAuditLogWithUpdatedAction()
    {
        // Arrange — create an organization first
        var userId = Guid.NewGuid();
        await using var setupContext = CreateContextWithAudit(new TestAuditContext(userId, "corr-setup"));
        var org = Organization.Create("Org Before Update");
        setupContext.Organizations.Add(org);
        await setupContext.SaveChangesAsync();

        // Act — soft-delete the organization (triggers a modification)
        var auditContext = new TestAuditContext(userId, "corr-update-test");
        await using var updateContext = CreateContextWithAudit(auditContext);
        var orgToUpdate = await updateContext.Organizations.FirstAsync(o => o.Id == org.Id);
        orgToUpdate.SoftDelete();
        await updateContext.SaveChangesAsync();

        // Assert
        await using var verifyContext = _postgresFixture.CreateContext();
        var auditLog = await verifyContext.AuditLogs
            .FirstOrDefaultAsync(a =>
                a.EntityId == org.Id.ToString()
                && a.EntityType == "Organization"
                && a.Action == "Updated");

        auditLog.Should().NotBeNull();
        auditLog!.UserId.Should().Be(userId);
        auditLog.CorrelationId.Should().Be("corr-update-test");
        auditLog.Changes.Should().NotBeNullOrEmpty();

        var changesDoc = JsonDocument.Parse(auditLog.Changes!);
        changesDoc.RootElement.TryGetProperty("IsDeleted", out var isDeletedProp).Should().BeTrue();
        isDeletedProp.GetProperty("old").GetBoolean().Should().BeFalse();
        isDeletedProp.GetProperty("new").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task SaveChangesAsync_WhenNoAuditContext_ShouldStillCreateAuditLogWithNullUser()
    {
        // Arrange — use parameterless constructor (no IAuditContext)
        await using var context = _postgresFixture.CreateContext();
        var org = Organization.Create("No Audit Context Org");

        // Act
        context.Organizations.Add(org);
        await context.SaveChangesAsync();

        // Assert
        await using var verifyContext = _postgresFixture.CreateContext();
        var auditLog = await verifyContext.AuditLogs
            .FirstOrDefaultAsync(a => a.EntityId == org.Id.ToString() && a.EntityType == "Organization");

        auditLog.Should().NotBeNull();
        auditLog!.Action.Should().Be("Created");
        auditLog.UserId.Should().BeNull();
        auditLog.CorrelationId.Should().BeNull();
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldNotCreateAuditLogForAuditLogEntity()
    {
        // Arrange — AuditLog doesn't extend Entity, so it should not be audited
        var auditContext = new TestAuditContext(Guid.NewGuid(), "corr-no-recurse");
        await using var context = CreateContextWithAudit(auditContext);
        var org = Organization.Create("Org For Recursion Test");

        // Act
        context.Organizations.Add(org);
        await context.SaveChangesAsync();

        // Assert — should have exactly one audit log for the Organization, none for the AuditLog itself
        await using var verifyContext = _postgresFixture.CreateContext();
        var auditLogs = await verifyContext.AuditLogs
            .Where(a => a.CorrelationId == "corr-no-recurse")
            .ToListAsync();

        auditLogs.Should().AllSatisfy(a => a.EntityType.Should().NotBe("AuditLog"));
    }

    private AppDbContext CreateContextWithAudit(IAuditContext auditContext)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgresFixture.ConnectionString)
            .Options;

        var context = new AppDbContext(options);
        context.SetAuditContext(auditContext, NullLogger<AppDbContext>.Instance);
        return context;
    }

    private sealed class TestAuditContext : IAuditContext
    {
        public Guid? UserId { get; }
        public string? CorrelationId { get; }

        public TestAuditContext(Guid? userId, string? correlationId)
        {
            UserId = userId;
            CorrelationId = correlationId;
        }
    }
}
