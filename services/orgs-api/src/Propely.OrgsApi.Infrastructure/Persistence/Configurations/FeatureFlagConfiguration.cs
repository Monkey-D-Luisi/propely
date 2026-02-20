// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Domain.FeatureFlags;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Propely.OrgsApi.Infrastructure.Persistence.Configurations;

public sealed class FeatureFlagConfiguration : IEntityTypeConfiguration<FeatureFlag>
{
    public void Configure(EntityTypeBuilder<FeatureFlag> builder)
    {
        builder.ToTable("feature_flags");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(f => f.Name)
            .HasColumnName("name")
            .HasMaxLength(FeatureFlag.NameMaxLength)
            .IsRequired();

        builder.Property(f => f.Description)
            .HasColumnName("description")
            .HasMaxLength(FeatureFlag.DescriptionMaxLength);

        builder.Property(f => f.IsEnabled)
            .HasColumnName("is_enabled")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(f => f.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(f => f.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        builder.HasIndex(f => f.Name)
            .IsUnique()
            .HasDatabaseName("idx_feature_flags_name");

        builder.Ignore(f => f.DomainEvents);
    }
}
