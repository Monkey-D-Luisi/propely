// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Domain.Permissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Propely.OrgsApi.Infrastructure.Persistence.Configurations;

public sealed class PermissionOverrideConfiguration : IEntityTypeConfiguration<PermissionOverride>
{
    public void Configure(EntityTypeBuilder<PermissionOverride> builder)
    {
        builder.ToTable("permission_overrides");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(p => p.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(p => p.OrganizationId)
            .HasColumnName("organization_id")
            .IsRequired();

        builder.Property(p => p.Permission)
            .HasColumnName("permission")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Granted)
            .HasColumnName("granted")
            .IsRequired();

        builder.Property(p => p.GrantedBy)
            .HasColumnName("granted_by")
            .IsRequired();

        builder.Property(p => p.GrantedAtUtc)
            .HasColumnName("granted_at_utc")
            .IsRequired();

        // Composite unique index: one override per user per permission per org
        builder.HasIndex(p => new { p.UserId, p.OrganizationId, p.Permission })
            .IsUnique()
            .HasDatabaseName("ix_permission_overrides_user_org_permission");

        builder.Ignore(p => p.DomainEvents);
    }
}
