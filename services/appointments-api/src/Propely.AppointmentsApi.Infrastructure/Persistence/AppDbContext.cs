// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Text.Json;
using Propely.AppointmentsApi.Application.Common.Interfaces;
using Propely.AppointmentsApi.Application.Common.Models;
using Propely.AppointmentsApi.Domain.Common;
using Propely.AppointmentsApi.Infrastructure.Persistence.Configurations;
using Propely.AppointmentsApi.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Propely.AppointmentsApi.Infrastructure.Persistence;

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

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<ProcessedEvent> ProcessedEvents => Set<ProcessedEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new ProcessedEventConfiguration());
    }

    /// <summary>
    /// Saves changes and dispatches domain events to the outbox.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Collect domain events and entities in a single pass over ChangeTracker
        var (domainEvents, entitiesWithEvents) = GetDomainEventsAndEntities();

        // Add domain events to outbox
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
