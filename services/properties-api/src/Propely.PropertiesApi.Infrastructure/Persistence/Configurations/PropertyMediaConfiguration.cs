// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.PropertiesApi.Domain.Properties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Propely.PropertiesApi.Infrastructure.Persistence.Configurations;

public sealed class PropertyMediaConfiguration : IEntityTypeConfiguration<PropertyMedia>
{
    public void Configure(EntityTypeBuilder<PropertyMedia> builder)
    {
        builder.ToTable("property_media");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(m => m.PropertyId)
            .HasColumnName("property_id")
            .IsRequired();

        builder.Property(m => m.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(m => m.MediaType)
            .HasColumnName("media_type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(m => m.StoragePath)
            .HasColumnName("storage_path")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(m => m.ThumbnailPath)
            .HasColumnName("thumbnail_path")
            .HasMaxLength(500);

        builder.Property(m => m.FileName)
            .HasColumnName("file_name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(m => m.ContentType)
            .HasColumnName("content_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(m => m.SizeBytes)
            .HasColumnName("size_bytes")
            .IsRequired();

        builder.Property(m => m.Width)
            .HasColumnName("width");

        builder.Property(m => m.Height)
            .HasColumnName("height");

        builder.Property(m => m.DisplayOrder)
            .HasColumnName("display_order")
            .IsRequired();

        builder.Property(m => m.UploadedAtUtc)
            .HasColumnName("uploaded_at_utc")
            .IsRequired();

        builder.HasIndex(m => new { m.PropertyId, m.TenantId })
            .HasDatabaseName("ix_property_media_property_tenant");

        builder.HasIndex(m => m.TenantId)
            .HasDatabaseName("ix_property_media_tenant");

        builder.Ignore(m => m.DomainEvents);
    }
}
