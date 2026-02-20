// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Propely.OrgsApi.Infrastructure.Persistence.Configurations;

public sealed class UserExternalLoginConfiguration : IEntityTypeConfiguration<UserExternalLogin>
{
    public void Configure(EntityTypeBuilder<UserExternalLogin> builder)
    {
        builder.ToTable("user_external_logins");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(l => l.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(l => l.Provider)
            .HasColumnName("provider")
            .HasMaxLength(UserExternalLogin.ProviderMaxLength)
            .IsRequired();

        builder.Property(l => l.ExternalId)
            .HasColumnName("external_id")
            .HasMaxLength(UserExternalLogin.ExternalIdMaxLength)
            .IsRequired();

        builder.Property(l => l.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.HasIndex(l => new { l.Provider, l.ExternalId })
            .IsUnique()
            .HasDatabaseName("idx_user_external_logins_provider_external");

        builder.HasIndex(l => new { l.UserId, l.Provider })
            .IsUnique()
            .HasDatabaseName("idx_user_external_logins_user_provider");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(l => l.DomainEvents);
    }
}
