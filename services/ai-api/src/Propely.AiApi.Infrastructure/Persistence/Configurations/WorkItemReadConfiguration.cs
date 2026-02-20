// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Propely.AiApi.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for WorkItemRead entity.
/// </summary>
public sealed class WorkItemReadConfiguration : IEntityTypeConfiguration<WorkItemRead>
{
    public void Configure(EntityTypeBuilder<WorkItemRead> builder)
    {
        builder.ToTable("work_items_read");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.OrgId)
            .HasColumnName("org_id")
            .IsRequired();

        builder.HasIndex(x => x.OrgId)
            .HasDatabaseName("idx_work_items_read_org_id");

        // Composite index for list query: filtered by org + status, ordered by created_at_utc DESC
        builder.HasIndex(x => new { x.OrgId, x.Status, x.CreatedAtUtc })
            .HasDatabaseName("idx_work_items_read_org_status_created")
            .IsDescending(false, false, true);

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(2000);

        builder.Property(x => x.Priority)
            .HasColumnName("priority")
            .HasMaxLength(20);

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .HasMaxLength(20);

        builder.Property(x => x.DueDateUtc)
            .HasColumnName("due_date_utc");

        builder.Property(x => x.EstimatedEffort)
            .HasColumnName("estimated_effort")
            .HasMaxLength(5);

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(x => x.LastProjectedAtUtc)
            .HasColumnName("last_projected_at_utc")
            .IsRequired();
    }
}
