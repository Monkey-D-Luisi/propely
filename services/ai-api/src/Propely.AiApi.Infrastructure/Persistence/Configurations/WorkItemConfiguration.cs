// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Domain.WorkItems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Propely.AiApi.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the WorkItem entity.
/// </summary>
public sealed class WorkItemConfiguration : IEntityTypeConfiguration<WorkItem>
{
    public void Configure(EntityTypeBuilder<WorkItem> builder)
    {
        builder.ToTable("work_items");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(w => w.OrgId)
            .HasColumnName("org_id")
            .IsRequired();

        builder.Property(w => w.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(w => w.Title)
            .HasColumnName("title")
            .HasMaxLength(WorkItem.TitleMaxLength)
            .IsRequired();

        builder.Property(w => w.Description)
            .HasColumnName("description")
            .HasMaxLength(WorkItem.DescriptionMaxLength);

        builder.Property(w => w.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(w => w.Priority)
            .HasColumnName("priority")
            .HasMaxLength(20)
            .HasConversion<string?>();

        builder.Property(w => w.Type)
            .HasColumnName("type")
            .HasMaxLength(20)
            .HasConversion<string?>();

        builder.Property(w => w.DueDateUtc)
            .HasColumnName("due_date_utc");

        builder.Property(w => w.EstimatedEffort)
            .HasColumnName("estimated_effort")
            .HasMaxLength(5)
            .HasConversion<string?>();

        builder.Property(w => w.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(w => w.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .IsRequired();

        builder.Property(w => w.Version)
            .HasColumnName("version")
            .IsConcurrencyToken()
            .IsRequired();

        builder.Property(w => w.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(w => w.DeletedAtUtc)
            .HasColumnName("deleted_at_utc");

        builder.HasIndex(w => w.OrgId)
            .HasDatabaseName("idx_work_items_org_id");

        builder.HasIndex(w => w.Status)
            .HasDatabaseName("idx_work_items_status");

        // Query filter (soft-delete + tenant) is configured in AppDbContext.OnModelCreating

        // Ignore domain events (not persisted)
        builder.Ignore(w => w.DomainEvents);
    }
}
