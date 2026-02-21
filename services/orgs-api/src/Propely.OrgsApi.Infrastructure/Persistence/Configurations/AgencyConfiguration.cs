// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Domain.Agencies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Propely.OrgsApi.Infrastructure.Persistence.Configurations;

public sealed class AgencyConfiguration : IEntityTypeConfiguration<Agency>
{
    public void Configure(EntityTypeBuilder<Agency> builder)
    {
        builder.ToTable("agencies");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(a => a.Name)
            .HasColumnName("name")
            .HasMaxLength(Agency.NameMaxLength)
            .IsRequired();

        builder.Property(a => a.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .IsRequired();

        builder.Property(a => a.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(a => a.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        builder.Property(a => a.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(a => a.DeletedAtUtc)
            .HasColumnName("deleted_at_utc");

        // AgencySlug is a value object stored as a string column
        builder.Property(a => a.Slug)
            .HasColumnName("slug")
            .HasMaxLength(AgencySlug.MaxLength)
            .IsRequired()
            .HasConversion(
                slug => slug.Value,
                value => AgencySlug.Create(value));

        builder.HasQueryFilter(a => !a.IsDeleted);

        // Unique index on slug (excludes soft-deleted rows)
        builder.HasIndex(a => a.Slug)
            .IsUnique()
            .HasFilter("is_deleted = false")
            .HasDatabaseName("ix_agencies_slug_unique");

        // BranchIds is a collection of Guid tracked separately via Organization.AgencyId FK
        // We ignore it in EF Core since the relationship is managed through Organization
        builder.Ignore(a => a.BranchIds);
        builder.Ignore(a => a.DomainEvents);
    }
}
