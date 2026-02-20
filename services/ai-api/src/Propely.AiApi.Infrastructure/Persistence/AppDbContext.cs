// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Text.Json;
using Propely.AiApi.Application.Common.Interfaces;
using Propely.AiApi.Application.Common.Models;
using Propely.AiApi.Domain.Common;
using Propely.AiApi.Domain.Common.Exceptions;
using Propely.AiApi.Domain.WorkItems;
using Propely.AiApi.Infrastructure.Persistence.Configurations;
using Propely.AiApi.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Propely.AiApi.Infrastructure.Persistence;

/// <summary>
/// Application database context with automatic domain event dispatch and tenant-scoped query filters.
/// </summary>
public sealed class AppDbContext : DbContext
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly Guid? _currentOrgId;

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
        _currentOrgId = null;
    }

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantAccessor tenantAccessor)
        : base(options)
    {
        _currentOrgId = tenantAccessor.GetCurrentOrgId();
    }

    public DbSet<WorkItem> WorkItems => Set<WorkItem>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<WorkItemRead> WorkItemsRead => Set<WorkItemRead>();
    public DbSet<ProcessedEvent> ProcessedEvents => Set<ProcessedEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new WorkItemConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new WorkItemReadConfiguration());
        modelBuilder.ApplyConfiguration(new ProcessedEventConfiguration());

        // Tenant + soft-delete combined filter for WorkItem
        // When _currentOrgId is null (system/background), the filter passes all tenants
        modelBuilder.Entity<WorkItem>()
            .HasQueryFilter(w => !w.IsDeleted && (_currentOrgId == null || w.OrgId == _currentOrgId));

        // Tenant filter for WorkItemRead (no soft-delete on read model)
        modelBuilder.Entity<WorkItemRead>()
            .HasQueryFilter(w => _currentOrgId == null || w.OrgId == _currentOrgId);
    }

    /// <summary>
    /// Saves changes, auto-sets OrgId on new tenant-scoped entities, and dispatches domain events to the outbox.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetTenantIdOnNewEntities();

        // Collect domain events and entities in a single pass over ChangeTracker
        var (domainEvents, entitiesWithEvents) = GetDomainEventsAndEntities();

        // Add domain events to outbox using LINQ
        OutboxMessages.AddRange(domainEvents.Select(CreateOutboxMessage));

        // Save everything in a single transaction
        var result = await base.SaveChangesAsync(cancellationToken);

        // Clear domain events only after successful save
        foreach (var entity in entitiesWithEvents)
        {
            entity.ClearDomainEvents();
        }

        return result;
    }

    private void SetTenantIdOnNewEntities()
    {
        if (_currentOrgId is null || _currentOrgId == Guid.Empty)
            return;

        foreach (var entry in ChangeTracker.Entries<WorkItem>()
            .Where(e => e.State == EntityState.Added))
        {
            var entityOrgId = entry.Entity.OrgId;

            if (entityOrgId == Guid.Empty)
            {
                // Auto-set OrgId when not explicitly provided
                entry.Property(w => w.OrgId).CurrentValue = _currentOrgId.Value;
            }
            else if (entityOrgId != _currentOrgId.Value)
            {
                // Prevent cross-tenant data injection
                throw new TenantMismatchException(
                    $"Cannot save WorkItem with OrgId '{entityOrgId}' in tenant context '{_currentOrgId.Value}'.");
            }
        }
    }

    private (List<IDomainEvent> Events, List<Entity> Entities) GetDomainEventsAndEntities()
    {
        var entitiesWithEvents = ChangeTracker
            .Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entitiesWithEvents
            .SelectMany(e => e.DomainEvents)
            .ToList();

        return (domainEvents, entitiesWithEvents);
    }

    private static OutboxMessage CreateOutboxMessage(IDomainEvent domainEvent)
    {
        var payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), JsonOptions);

        return OutboxMessage.Create(
            domainEvent.EventId,
            domainEvent.EventType,
            payload,
            domainEvent.OccurredAtUtc,
            domainEvent.CorrelationId,
            domainEvent.CausationId);
    }
}
