// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.ContactsApi.Domain.Contacts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Propely.ContactsApi.Infrastructure.Persistence.Configurations;

public sealed class ContactPropertyInterestConfiguration : IEntityTypeConfiguration<ContactPropertyInterest>
{
    public void Configure(EntityTypeBuilder<ContactPropertyInterest> builder)
    {
        builder.ToTable("contact_property_interests");

        builder.HasKey(pi => pi.Id);
        builder.Property(pi => pi.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(pi => pi.ContactId)
            .HasColumnName("contact_id")
            .IsRequired();

        builder.Property(pi => pi.PropertyId)
            .HasColumnName("property_id")
            .IsRequired();

        builder.Property(pi => pi.InterestType)
            .HasColumnName("interest_type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(pi => pi.Notes)
            .HasColumnName("notes")
            .HasMaxLength(2000);

        builder.Property(pi => pi.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        // Unique index on (contact_id, property_id, interest_type)
        builder.HasIndex(pi => new { pi.ContactId, pi.PropertyId, pi.InterestType })
            .IsUnique()
            .HasDatabaseName("ix_contact_property_interests_contact_property_type");
    }
}
