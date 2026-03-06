// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Text.Json;
using Propely.ContactsApi.Domain.Contacts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Propely.ContactsApi.Infrastructure.Persistence.Configurations;

public sealed class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.ToTable("contacts");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(c => c.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(Contact.NameMaxLength)
            .IsRequired();

        builder.Property(c => c.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(Contact.NameMaxLength)
            .IsRequired();

        builder.Property(c => c.Email)
            .HasColumnName("email")
            .HasMaxLength(Contact.EmailMaxLength)
            .IsRequired();

        builder.Property(c => c.Phone)
            .HasColumnName("phone")
            .HasMaxLength(Contact.PhoneMaxLength);

        builder.Property(c => c.SecondaryPhone)
            .HasColumnName("secondary_phone")
            .HasMaxLength(Contact.PhoneMaxLength);

        builder.Property(c => c.Company)
            .HasColumnName("company")
            .HasMaxLength(Contact.CompanyMaxLength);

        builder.Property(c => c.Notes)
            .HasColumnName("notes")
            .HasMaxLength(Contact.NotesMaxLength);

        builder.Property(c => c.PreferredLanguage)
            .HasColumnName("preferred_language")
            .HasMaxLength(10);

        builder.Property(c => c.Source)
            .HasColumnName("source")
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(c => c.AssignedAgentId)
            .HasColumnName("assigned_agent_id");

        builder.Property(c => c.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(c => c.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(c => c.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        builder.Property(c => c.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(c => c.DeletedAtUtc)
            .HasColumnName("deleted_at_utc");

        // Roles backing field stored as JSON
        builder.Property(typeof(List<ContactRole>), "_roles")
            .HasColumnName("roles")
            .HasConversion(
                new ValueConverter<List<ContactRole>, string>(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<List<ContactRole>>(v, (JsonSerializerOptions)null!) ?? new List<ContactRole>()))
            .HasColumnType("jsonb");
        builder.Ignore(c => c.Roles);

        // PropertyInterests navigation
        builder.HasMany(c => c.PropertyInterests)
            .WithOne()
            .HasForeignKey(pi => pi.ContactId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(c => c.PropertyInterests).UsePropertyAccessMode(PropertyAccessMode.Field);

        // Soft-delete query filter
        builder.HasQueryFilter(c => !c.IsDeleted);

        // Indexes
        builder.HasIndex(c => c.TenantId)
            .HasDatabaseName("ix_contacts_tenant_id");

        builder.HasIndex(c => new { c.TenantId, c.Email })
            .IsUnique()
            .HasDatabaseName("ix_contacts_tenant_email");

        builder.HasIndex(c => c.AssignedAgentId)
            .HasDatabaseName("ix_contacts_assigned_agent_id");

        // Ignore domain events
        builder.Ignore(c => c.DomainEvents);
    }
}
