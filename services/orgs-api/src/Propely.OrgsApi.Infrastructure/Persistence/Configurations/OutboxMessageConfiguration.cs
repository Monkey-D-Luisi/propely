// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Propely.OrgsApi.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the OutboxMessage entity.
/// </summary>
public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(m => m.EventType)
            .HasColumnName("event_type")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(m => m.Payload)
            .HasColumnName("payload")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(m => m.OccurredAtUtc)
            .HasColumnName("occurred_at_utc")
            .IsRequired();

        builder.Property(m => m.ProcessedAtUtc)
            .HasColumnName("processed_at_utc");

        builder.Property(m => m.CorrelationId)
            .HasColumnName("correlation_id");

        builder.Property(m => m.CausationId)
            .HasColumnName("causation_id");

        builder.Property(m => m.RetryCount)
            .HasColumnName("retry_count")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(m => m.FailedAtUtc)
            .HasColumnName("failed_at_utc");

        // Partial index for outbox polling: covers the unprocessed-message query
        // used by OutboxDispatcherService. The filter narrows the index to pending
        // rows only (not yet processed and not dead-lettered), keeping it small
        // even as the table grows.
        builder.HasIndex(m => m.OccurredAtUtc)
            .HasDatabaseName("idx_outbox_unprocessed")
            .HasFilter("processed_at_utc IS NULL AND failed_at_utc IS NULL");

        // Check constraint for non-empty event type
        builder.ToTable(t => t.HasCheckConstraint(
            "chk_event_type",
            "event_type <> ''"));
    }
}
