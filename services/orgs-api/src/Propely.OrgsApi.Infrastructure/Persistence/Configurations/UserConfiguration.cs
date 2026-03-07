// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Propely.OrgsApi.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasMaxLength(User.EmailMaxLength)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .IsRequired();

        builder.Property(u => u.Name)
            .HasColumnName("name")
            .HasMaxLength(User.NameMaxLength);

        builder.Property(u => u.EmailVerified)
            .HasColumnName("email_verified")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(u => u.EmailVerifiedAtUtc)
            .HasColumnName("email_verified_at_utc");

        builder.Property(u => u.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(u => u.DeletedAtUtc)
            .HasColumnName("deleted_at_utc");

        builder.Property(u => u.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(u => u.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        builder.Property(u => u.PasswordVersion)
            .HasColumnName("password_version")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(u => u.IsSystemAdmin)
            .HasColumnName("is_system_admin")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(u => u.FailedLoginAttempts)
            .HasColumnName("failed_login_attempts")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(u => u.LockoutEndUtc)
            .HasColumnName("lockout_end_utc");

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("idx_users_email");

        builder.HasQueryFilter(u => !u.IsDeleted);

        builder.Ignore(u => u.DomainEvents);
    }
}
