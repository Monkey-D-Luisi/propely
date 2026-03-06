// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.ContactsApi.Domain.Leads;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Propely.ContactsApi.Infrastructure.Persistence.Configurations;

public sealed class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.ToTable("leads");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(l => l.Name)
            .HasColumnName("name")
            .HasMaxLength(Lead.NameMaxLength)
            .IsRequired();

        builder.Property(l => l.Email)
            .HasColumnName("email")
            .HasMaxLength(Lead.EmailMaxLength)
            .IsRequired();

        builder.Property(l => l.Phone)
            .HasColumnName("phone")
            .HasMaxLength(Lead.PhoneMaxLength);

        builder.Property(l => l.Message)
            .HasColumnName("message")
            .HasMaxLength(Lead.MessageMaxLength);

        builder.Property(l => l.Source)
            .HasColumnName("source")
            .HasMaxLength(Lead.SourceMaxLength);

        builder.Property(l => l.PropertyId)
            .HasColumnName("property_id")
            .IsRequired();

        builder.Property(l => l.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(l => l.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(l => l.AssignedAgentId)
            .HasColumnName("assigned_agent_id");

        builder.Property(l => l.ContactId)
            .HasColumnName("contact_id");

        builder.Property(l => l.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(l => l.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        builder.Property(l => l.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(l => l.DeletedAtUtc)
            .HasColumnName("deleted_at_utc");

        // Soft-delete query filter
        builder.HasQueryFilter(l => !l.IsDeleted);

        // Indexes
        builder.HasIndex(l => l.TenantId)
            .HasDatabaseName("ix_leads_tenant_id");

        builder.HasIndex(l => l.PropertyId)
            .HasDatabaseName("ix_leads_property_id");

        builder.HasIndex(l => l.Status)
            .HasDatabaseName("ix_leads_status");

        builder.HasIndex(l => l.AssignedAgentId)
            .HasDatabaseName("ix_leads_assigned_agent_id");

        builder.HasIndex(l => new { l.TenantId, l.Status })
            .HasDatabaseName("ix_leads_tenant_status");

        builder.HasIndex(l => new { l.Email, l.PropertyId, l.TenantId })
            .IsUnique()
            .HasDatabaseName("ix_leads_email_property_tenant");

        // Ignore domain events
        builder.Ignore(l => l.DomainEvents);
    }
}
