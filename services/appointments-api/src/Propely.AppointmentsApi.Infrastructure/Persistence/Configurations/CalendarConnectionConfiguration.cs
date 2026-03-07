// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AppointmentsApi.Domain.Appointments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Propely.AppointmentsApi.Infrastructure.Persistence.Configurations;

public sealed class CalendarConnectionConfiguration : IEntityTypeConfiguration<CalendarConnection>
{
    public void Configure(EntityTypeBuilder<CalendarConnection> builder)
    {
        builder.ToTable("calendar_connections");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(c => c.AgentId)
            .HasColumnName("agent_id")
            .IsRequired();

        builder.Property(c => c.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(c => c.Provider)
            .HasColumnName("provider")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.EncryptedAccessToken)
            .HasColumnName("encrypted_access_token")
            .HasMaxLength(CalendarConnection.EncryptedTokenMaxLength)
            .IsRequired();

        builder.Property(c => c.EncryptedRefreshToken)
            .HasColumnName("encrypted_refresh_token")
            .HasMaxLength(CalendarConnection.EncryptedTokenMaxLength)
            .IsRequired();

        builder.Property(c => c.TokenExpiresAtUtc)
            .HasColumnName("token_expires_at_utc")
            .IsRequired();

        builder.Property(c => c.ExternalCalendarId)
            .HasColumnName("external_calendar_id")
            .HasMaxLength(CalendarConnection.ExternalCalendarIdMaxLength)
            .IsRequired();

        builder.Property(c => c.LastSyncedUtc)
            .HasColumnName("last_synced_utc");

        builder.Property(c => c.LastSyncError)
            .HasColumnName("last_sync_error")
            .HasMaxLength(CalendarConnection.LastSyncErrorMaxLength);

        builder.Property(c => c.SyncState)
            .HasColumnName("sync_state")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(c => c.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        builder.Property(c => c.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(c => c.DeletedAtUtc)
            .HasColumnName("deleted_at_utc");

        // NOTE: Query filter (soft-delete + tenant isolation) is defined in AppDbContext.OnModelCreating

        // Unique index: one connection per agent per provider
        builder.HasIndex(c => new { c.AgentId, c.Provider })
            .HasDatabaseName("ix_calendar_connections_agent_id_provider")
            .IsUnique()
            .HasFilter("is_deleted = false");

        builder.HasIndex(c => c.TenantId)
            .HasDatabaseName("ix_calendar_connections_tenant_id");

        // Ignore domain events
        builder.Ignore(c => c.DomainEvents);
    }
}
