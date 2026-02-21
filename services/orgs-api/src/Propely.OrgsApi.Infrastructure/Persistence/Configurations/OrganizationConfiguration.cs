// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Domain.Agencies;
using Propely.OrgsApi.Domain.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Propely.OrgsApi.Infrastructure.Persistence.Configurations;

public sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("organizations");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(o => o.Name)
            .HasColumnName("name")
            .HasMaxLength(Organization.NameMaxLength)
            .IsRequired();

        builder.Property(o => o.Description)
            .HasColumnName("description")
            .HasMaxLength(Organization.DescriptionMaxLength);

        builder.Property(o => o.AgencyId)
            .HasColumnName("agency_id");

        // FK to agencies table with SetNull on delete (agency deletion clears branch assignment)
        builder.HasOne<Agency>()
            .WithMany()
            .HasForeignKey(o => o.AgencyId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(o => o.AgencyId)
            .HasDatabaseName("ix_organizations_agency_id");

        builder.Property(o => o.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(o => o.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        builder.Property(o => o.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(o => o.DeletedAtUtc)
            .HasColumnName("deleted_at_utc");

        builder.HasQueryFilter(o => !o.IsDeleted);

        // Case-insensitive unique index on name (excludes soft-deleted rows).
        // The actual DB index uses LOWER(name) via raw SQL in the migration;
        // this HasIndex keeps EF Core's model snapshot in sync.
        builder.HasIndex(o => o.Name)
            .IsUnique()
            .HasFilter("is_deleted = false")
            .HasDatabaseName("ix_organizations_name_unique");

        builder.Ignore(o => o.DomainEvents);
    }
}
