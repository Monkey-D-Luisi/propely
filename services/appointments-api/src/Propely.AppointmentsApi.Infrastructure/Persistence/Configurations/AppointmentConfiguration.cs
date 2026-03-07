// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AppointmentsApi.Domain.Appointments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Propely.AppointmentsApi.Infrastructure.Persistence.Configurations;

public sealed class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("appointments");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(a => a.Title)
            .HasColumnName("title")
            .HasMaxLength(Appointment.TitleMaxLength)
            .IsRequired();

        builder.Property(a => a.Description)
            .HasColumnName("description")
            .HasMaxLength(Appointment.DescriptionMaxLength);

        builder.Property(a => a.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(a => a.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(a => a.StartTimeUtc)
            .HasColumnName("start_time_utc")
            .IsRequired();

        builder.Property(a => a.EndTimeUtc)
            .HasColumnName("end_time_utc")
            .IsRequired();

        builder.Property(a => a.Location)
            .HasColumnName("location")
            .HasMaxLength(Appointment.LocationMaxLength);

        builder.Property(a => a.IsAllDay)
            .HasColumnName("is_all_day")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(a => a.PropertyId)
            .HasColumnName("property_id");

        builder.Property(a => a.ContactId)
            .HasColumnName("contact_id");

        builder.Property(a => a.AgentId)
            .HasColumnName("agent_id")
            .IsRequired();

        builder.Property(a => a.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(a => a.CancellationReason)
            .HasColumnName("cancellation_reason")
            .HasMaxLength(Appointment.CancellationReasonMaxLength);

        builder.Property(a => a.Notes)
            .HasColumnName("notes")
            .HasMaxLength(Appointment.NotesMaxLength);

        builder.Property(a => a.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(a => a.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        builder.Property(a => a.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(a => a.DeletedAtUtc)
            .HasColumnName("deleted_at_utc");

        // CalendarSyncInfos as owned entity collection
        builder.OwnsMany(a => a.CalendarSyncInfos, sync =>
        {
            sync.ToTable("appointment_calendar_syncs");

            sync.WithOwner().HasForeignKey("appointment_id");

            sync.Property<int>("Id")
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
            sync.HasKey("Id");

            sync.Property(s => s.ExternalEventId)
                .HasColumnName("external_event_id")
                .HasMaxLength(500)
                .IsRequired();

            sync.Property(s => s.Provider)
                .HasColumnName("provider")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            sync.Property(s => s.LastSyncedUtc)
                .HasColumnName("last_synced_utc")
                .IsRequired();
        });

        // NOTE: Query filter (soft-delete + tenant isolation) is defined in AppDbContext.OnModelCreating

        // Indexes
        builder.HasIndex(a => new { a.AgentId, a.StartTimeUtc })
            .HasDatabaseName("ix_appointments_agent_id_start_time_utc");

        builder.HasIndex(a => a.PropertyId)
            .HasDatabaseName("ix_appointments_property_id");

        builder.HasIndex(a => a.ContactId)
            .HasDatabaseName("ix_appointments_contact_id");

        builder.HasIndex(a => a.TenantId)
            .HasDatabaseName("ix_appointments_tenant_id");

        // Ignore domain events
        builder.Ignore(a => a.DomainEvents);
    }
}
