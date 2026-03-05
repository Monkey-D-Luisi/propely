// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.PropertiesApi.Domain.Properties;
using Propely.PropertiesApi.Domain.Properties.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Propely.PropertiesApi.Infrastructure.Persistence.Configurations;

public sealed class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.ToTable("properties");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(p => p.Title)
            .HasColumnName("title")
            .HasMaxLength(Property.TitleMaxLength)
            .IsRequired();

        builder.Property(p => p.PropertyType)
            .HasColumnName("property_type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.OperationType)
            .HasColumnName("operation_type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(p => p.AgentId)
            .HasColumnName("agent_id")
            .IsRequired();

        builder.Property(p => p.AgencyId)
            .HasColumnName("agency_id");

        builder.Property(p => p.VirtualTourUrl)
            .HasColumnName("virtual_tour_url")
            .HasMaxLength(2000);

        builder.Property(p => p.VideoUrl)
            .HasColumnName("video_url")
            .HasMaxLength(2000);

        builder.Property(p => p.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(p => p.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        builder.Property(p => p.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(p => p.UpdatedBy)
            .HasColumnName("updated_by");

        builder.Property(p => p.PublishedAtUtc)
            .HasColumnName("published_at_utc");

        builder.Property(p => p.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(p => p.DeletedAtUtc)
            .HasColumnName("deleted_at_utc");

        // Description (LocalizedText) as owned entity
        builder.OwnsOne(p => p.Description, desc =>
        {
            desc.Property(d => d.Es).HasColumnName("description_es").HasMaxLength(10000);
            desc.Property(d => d.Pt).HasColumnName("description_pt").HasMaxLength(10000);
            desc.Property(d => d.En).HasColumnName("description_en").HasMaxLength(10000);
            desc.Property(d => d.Fr).HasColumnName("description_fr").HasMaxLength(10000);
            desc.Property(d => d.De).HasColumnName("description_de").HasMaxLength(10000);
            desc.Property(d => d.Nl).HasColumnName("description_nl").HasMaxLength(10000);
        });

        // Address as owned entity
        builder.OwnsOne(p => p.Address, addr =>
        {
            addr.Property(a => a.Street).HasColumnName("address_street").HasMaxLength(500);
            addr.Property(a => a.City).HasColumnName("address_city").HasMaxLength(200);
            addr.Property(a => a.Province).HasColumnName("address_province").HasMaxLength(200);
            addr.Property(a => a.PostalCode).HasColumnName("address_postal_code").HasMaxLength(20);
            addr.Property(a => a.Country).HasColumnName("address_country").HasMaxLength(10);
            addr.Property(a => a.ProvinceCode).HasColumnName("address_province_code").HasMaxLength(10);
            addr.Property(a => a.MunicipalityCode).HasColumnName("address_municipality_code").HasMaxLength(10);
            addr.Property(a => a.Latitude).HasColumnName("address_latitude");
            addr.Property(a => a.Longitude).HasColumnName("address_longitude");
        });

        // Features as owned entity
        builder.OwnsOne(p => p.Features, feat =>
        {
            feat.Property(f => f.Bedrooms).HasColumnName("features_bedrooms");
            feat.Property(f => f.Bathrooms).HasColumnName("features_bathrooms");
            feat.Property(f => f.BuiltArea).HasColumnName("features_built_area").HasPrecision(12, 2);
            feat.Property(f => f.UsableArea).HasColumnName("features_usable_area").HasPrecision(12, 2);
            feat.Property(f => f.PlotArea).HasColumnName("features_plot_area").HasPrecision(12, 2);
            feat.Property(f => f.Floor).HasColumnName("features_floor");
            feat.Property(f => f.Orientation).HasColumnName("features_orientation").HasConversion<string>().HasMaxLength(5);
            feat.Property(f => f.YearBuilt).HasColumnName("features_year_built");
            feat.Property(f => f.EnergyRating).HasColumnName("features_energy_rating").HasConversion<string>().HasMaxLength(10);
            feat.Property(f => f.EnergyConsumption).HasColumnName("features_energy_consumption").HasPrecision(10, 2);
            feat.Property(f => f.EnergyEmissions).HasColumnName("features_energy_emissions").HasPrecision(10, 2);
            feat.Property(f => f.HasPool).HasColumnName("features_has_pool").HasDefaultValue(false);
            feat.Property(f => f.HasGarden).HasColumnName("features_has_garden").HasDefaultValue(false);
            feat.Property(f => f.HasGarage).HasColumnName("features_has_garage").HasDefaultValue(false);
            feat.Property(f => f.HasElevator).HasColumnName("features_has_elevator").HasDefaultValue(false);
            feat.Property(f => f.HasTerrace).HasColumnName("features_has_terrace").HasDefaultValue(false);
            feat.Property(f => f.AirConditioning).HasColumnName("features_air_conditioning").HasDefaultValue(false);
            feat.Property(f => f.Heating).HasColumnName("features_heating").HasDefaultValue(false);
            feat.Property(f => f.Furnished).HasColumnName("features_furnished").HasDefaultValue(false);
            feat.Property(f => f.ParkingSpaces).HasColumnName("features_parking_spaces");
        });

        // Financials as owned entity
        builder.OwnsOne(p => p.Financials, fin =>
        {
            fin.Property(f => f.Price).HasColumnName("financials_price").HasPrecision(14, 2);
            fin.Property(f => f.CommunityFees).HasColumnName("financials_community_fees").HasPrecision(10, 2);
            fin.Property(f => f.IbiTax).HasColumnName("financials_ibi_tax").HasPrecision(10, 2);
            fin.Property(f => f.CatastroReference).HasColumnName("financials_catastro_reference").HasMaxLength(50);
        });

        // Soft-delete query filter
        builder.HasQueryFilter(p => !p.IsDeleted);

        // Indexes
        builder.HasIndex(p => p.TenantId)
            .HasDatabaseName("ix_properties_tenant_id");

        builder.HasIndex(p => p.AgentId)
            .HasDatabaseName("ix_properties_agent_id");

        builder.HasIndex(p => p.Status)
            .HasDatabaseName("ix_properties_status");

        builder.HasIndex(p => p.PropertyType)
            .HasDatabaseName("ix_properties_property_type");

        builder.HasIndex(p => new { p.TenantId, p.Status })
            .HasDatabaseName("ix_properties_tenant_status");

        // Ignore domain events
        builder.Ignore(p => p.DomainEvents);
        builder.Ignore(p => p.PricePerSqm);
    }
}
