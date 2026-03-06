// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AppointmentsApi.Domain.Appointments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Propely.AppointmentsApi.Infrastructure.Persistence.Configurations;

public sealed class SyncOperationConfiguration : IEntityTypeConfiguration<SyncOperation>
{
    public void Configure(EntityTypeBuilder<SyncOperation> builder)
    {
        builder.ToTable("sync_operations");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(s => s.CalendarConnectionId)
            .HasColumnName("calendar_connection_id")
            .IsRequired();

        builder.Property(s => s.AppointmentId)
            .HasColumnName("appointment_id");

        builder.Property(s => s.Direction)
            .HasColumnName("direction")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.OperationType)
            .HasColumnName("operation_type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(SyncOperation.ErrorMessageMaxLength);

        builder.Property(s => s.StartedAtUtc)
            .HasColumnName("started_at_utc");

        builder.Property(s => s.CompletedAtUtc)
            .HasColumnName("completed_at_utc");

        builder.Property(s => s.RetryCount)
            .HasColumnName("retry_count")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(s => s.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        // Index for processing pending operations
        builder.HasIndex(s => new { s.Status, s.CreatedAtUtc })
            .HasDatabaseName("ix_sync_operations_status_created_at_utc");

        builder.HasIndex(s => s.CalendarConnectionId)
            .HasDatabaseName("ix_sync_operations_calendar_connection_id");

        // Ignore domain events
        builder.Ignore(s => s.DomainEvents);
    }
}
