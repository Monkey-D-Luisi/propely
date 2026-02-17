// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SaasTemplate.OrgsApi.Infrastructure.Persistence.Configurations;

public sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("subscriptions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(s => s.OrganizationId)
            .HasColumnName("organization_id")
            .IsRequired();

        builder.Property(s => s.StripeCustomerId)
            .HasColumnName("stripe_customer_id")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(s => s.StripeSubscriptionId)
            .HasColumnName("stripe_subscription_id")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(s => s.PlanId)
            .HasColumnName("plan_id")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.CurrentPeriodEnd)
            .HasColumnName("current_period_end")
            .IsRequired();

        builder.Property(s => s.CancelledAtUtc)
            .HasColumnName("cancelled_at_utc");

        builder.Property(s => s.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(s => s.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        builder.HasIndex(s => s.OrganizationId)
            .IsUnique()
            .HasDatabaseName("ix_subscriptions_organization_id");

        builder.HasIndex(s => s.StripeSubscriptionId)
            .IsUnique()
            .HasDatabaseName("ix_subscriptions_stripe_subscription_id");

        builder.Ignore(s => s.DomainEvents);
    }
}
