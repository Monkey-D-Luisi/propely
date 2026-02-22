// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Text.Json;
using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Application.Common.Models;
using Propely.OrgsApi.Domain.Agencies;
using Propely.OrgsApi.Domain.Billing;
using Propely.OrgsApi.Domain.Common;
using Propely.OrgsApi.Domain.FeatureFlags;
using Propely.OrgsApi.Domain.Notifications;
using Propely.OrgsApi.Domain.Organizations;
using Propely.OrgsApi.Domain.Permissions;
using Propely.OrgsApi.Domain.Users;
using Propely.OrgsApi.Infrastructure.Persistence.Configurations;
using Propely.OrgsApi.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;

namespace Propely.OrgsApi.Infrastructure.Persistence;

/// <summary>
/// Application database context with automatic domain event dispatch and audit logging.
/// </summary>
public sealed class AppDbContext : DbContext
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Properties excluded from audit log serialization to prevent secret/PII leakage.
    /// </summary>
    private static readonly HashSet<string> SensitiveProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "PasswordHash",
        "Token",
        "TokenHash"
    };

    // Audit context and logger are resolved lazily from HttpContext to support DbContext pooling.
    // With AddDbContextPool, only the DbContextOptions constructor is used.
    private IAuditContext? _auditContext;
    private ILogger<AppDbContext>? _logger;

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Sets the audit context for the current scope. Called after the pooled context is rented.
    /// </summary>
    public void SetAuditContext(IAuditContext? auditContext, ILogger<AppDbContext>? logger)
    {
        _auditContext = auditContext;
        _logger = logger;
    }

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<ProcessedEvent> ProcessedEvents => Set<ProcessedEvent>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserExternalLogin> UserExternalLogins => Set<UserExternalLogin>();
    public DbSet<Agency> Agencies => Set<Agency>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<Invitation> Invitations => Set<Invitation>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<FeatureFlag> FeatureFlags => Set<FeatureFlag>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<ProcessedWebhookEvent> ProcessedWebhookEvents => Set<ProcessedWebhookEvent>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PermissionOverride> PermissionOverrides => Set<PermissionOverride>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new ProcessedEventConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new UserExternalLoginConfiguration());
        modelBuilder.ApplyConfiguration(new AgencyConfiguration());
        modelBuilder.ApplyConfiguration(new OrganizationConfiguration());
        modelBuilder.ApplyConfiguration(new MembershipConfiguration());
        modelBuilder.ApplyConfiguration(new InvitationConfiguration());
        modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
        modelBuilder.ApplyConfiguration(new NotificationConfiguration());
        modelBuilder.ApplyConfiguration(new FeatureFlagConfiguration());
        modelBuilder.ApplyConfiguration(new SubscriptionConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentConfiguration());
        modelBuilder.ApplyConfiguration(new ProcessedWebhookEventConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
        modelBuilder.ApplyConfiguration(new PermissionOverrideConfiguration());
    }

    /// <summary>
    /// Saves changes and automatically dispatches domain events to the outbox and creates audit log entries.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Collect domain events and entities in a single pass over ChangeTracker
        var (domainEvents, entitiesWithEvents) = GetDomainEventsAndEntities();

        // Add domain events to outbox using LINQ
        OutboxMessages.AddRange(domainEvents.Select(CreateOutboxMessage));

        // Create audit log entries (best-effort per R5: must not fail the main operation)
        try
        {
            CreateAuditEntries();
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to create audit log entries. Main operation will proceed.");
        }

        // Save everything in a single transaction
        var result = await base.SaveChangesAsync(cancellationToken);

        // Clear domain events only after successful save
        foreach (var entity in entitiesWithEvents)
        {
            entity.ClearDomainEvents();
        }

        return result;
    }

    private void CreateAuditEntries()
    {
        var userId = _auditContext?.UserId;
        var correlationId = _auditContext?.CorrelationId;
        if (correlationId?.Length > 64) correlationId = correlationId[..64];

        var entries = ChangeTracker.Entries<Entity>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
        {
            try
            {
                var entityType = Truncate(entry.Entity.GetType().Name, 100);
                var entityId = Truncate(GetEntityId(entry), 200);
                var action = entry.State switch
                {
                    EntityState.Added => "Created",
                    EntityState.Modified => "Updated",
                    EntityState.Deleted => "Deleted",
                    _ => "Unknown"
                };

                string? changes = entry.State switch
                {
                    EntityState.Modified => SerializeModifiedProperties(entry),
                    EntityState.Added => SerializeAddedProperties(entry),
                    EntityState.Deleted => SerializeDeletedProperties(entry),
                    _ => null
                };

                var auditLog = AuditLog.Create(userId, TryGetOrganizationId(entry), action, entityType, entityId, changes, correlationId);
                AuditLogs.Add(auditLog);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to create audit entry for {EntityType}. Skipping.", entry.Entity.GetType().Name);
            }
        }
    }

    private static string GetEntityId(EntityEntry entry)
    {
        var keyProperties = entry.Metadata.FindPrimaryKey()?.Properties;
        if (keyProperties is null || keyProperties.Count == 0)
            return "unknown";

        var keyValues = keyProperties.Select(p => entry.Property(p.Name).CurrentValue?.ToString() ?? "null");
        return string.Join(",", keyValues);
    }

    private static string? SerializeModifiedProperties(EntityEntry entry)
    {
        var changes = new Dictionary<string, object?>();

        foreach (var property in entry.Properties
            .Where(p => p.IsModified
                && !SensitiveProperties.Contains(p.Metadata.Name)
                && p.Metadata.Name != "DomainEvents"))
        {
            changes[property.Metadata.Name] = new
            {
                Old = property.OriginalValue,
                New = property.CurrentValue
            };
        }

        return changes.Count > 0 ? JsonSerializer.Serialize(changes, JsonOptions) : null;
    }

    private static string? SerializeAddedProperties(EntityEntry entry)
    {
        var values = new Dictionary<string, object?>();

        foreach (var property in entry.Properties
            .Where(p => !SensitiveProperties.Contains(p.Metadata.Name)
                && p.Metadata.Name != "DomainEvents"
                && p.CurrentValue is not null))
        {
            values[property.Metadata.Name] = property.CurrentValue;
        }

        return values.Count > 0 ? JsonSerializer.Serialize(values, JsonOptions) : null;
    }

    private static string? SerializeDeletedProperties(EntityEntry entry)
    {
        var values = new Dictionary<string, object?>();

        foreach (var property in entry.Properties
            .Where(p => !SensitiveProperties.Contains(p.Metadata.Name)
                && p.Metadata.Name != "DomainEvents"
                && p.OriginalValue is not null))
        {
            values[property.Metadata.Name] = property.OriginalValue;
        }

        return values.Count > 0 ? JsonSerializer.Serialize(values, JsonOptions) : null;
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length > maxLength ? value[..maxLength] : value;

    private static Guid? TryGetOrganizationId(EntityEntry entry)
    {
        var orgIdProperty = entry.Properties
            .FirstOrDefault(p => p.Metadata.Name == "OrganizationId");

        if (orgIdProperty?.CurrentValue is Guid orgId)
            return orgId;

        return null;
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
