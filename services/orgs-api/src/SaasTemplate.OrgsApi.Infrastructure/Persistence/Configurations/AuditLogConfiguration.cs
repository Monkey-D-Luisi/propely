// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SaasTemplate.OrgsApi.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(a => a.UserId)
            .HasColumnName("user_id");

        builder.Property(a => a.OrganizationId)
            .HasColumnName("organization_id");

        builder.Property(a => a.Action)
            .HasColumnName("action")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.EntityType)
            .HasColumnName("entity_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.EntityId)
            .HasColumnName("entity_id")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(a => a.Changes)
            .HasColumnName("changes")
            .HasColumnType("jsonb");

        builder.Property(a => a.CorrelationId)
            .HasColumnName("correlation_id")
            .HasMaxLength(64);

        builder.Property(a => a.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.HasIndex(a => new { a.EntityType, a.EntityId })
            .HasDatabaseName("idx_audit_logs_entity");

        builder.HasIndex(a => a.OrganizationId)
            .HasDatabaseName("idx_audit_logs_organization_id");

        builder.HasIndex(a => a.UserId)
            .HasDatabaseName("idx_audit_logs_user_id");

        builder.HasIndex(a => a.CreatedAtUtc)
            .HasDatabaseName("idx_audit_logs_created_at");
    }
}
