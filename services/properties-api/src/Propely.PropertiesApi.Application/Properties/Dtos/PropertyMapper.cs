// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.PropertiesApi.Domain.Properties;
using Propely.PropertiesApi.Domain.Properties.ValueObjects;

namespace Propely.PropertiesApi.Application.Properties.Dtos;

public static class PropertyMapper
{
    public static PropertyDto ToDto(Property property)
    {
        return new PropertyDto
        {
            Id = property.Id,
            Title = property.Title,
            PropertyType = property.PropertyType,
            OperationType = property.OperationType,
            Status = property.Status,
            TenantId = property.TenantId,
            AgentId = property.AgentId,
            AgencyId = property.AgencyId,
            Description = MapLocalizedText(property.Description),
            Address = MapAddress(property.Address),
            Features = MapFeatures(property.Features),
            Financials = MapFinancials(property.Financials),
            VirtualTourUrl = property.VirtualTourUrl,
            VideoUrl = property.VideoUrl,
            PricePerSqm = property.PricePerSqm,
            CreatedAtUtc = property.CreatedAtUtc,
            UpdatedAtUtc = property.UpdatedAtUtc,
            PublishedAtUtc = property.PublishedAtUtc
        };
    }

    public static PropertyListItemDto ToListItemDto(Property property)
    {
        return new PropertyListItemDto
        {
            Id = property.Id,
            Title = property.Title,
            PropertyType = property.PropertyType,
            OperationType = property.OperationType,
            Status = property.Status,
            Price = property.Financials?.Price,
            City = property.Address?.City,
            BuiltArea = property.Features?.BuiltArea,
            Bedrooms = property.Features?.Bedrooms,
            Bathrooms = property.Features?.Bathrooms,
            AgentId = property.AgentId,
            CreatedAtUtc = property.CreatedAtUtc,
            UpdatedAtUtc = property.UpdatedAtUtc
        };
    }

    private static LocalizedTextDto? MapLocalizedText(LocalizedText? text)
    {
        if (text is null) return null;
        return new LocalizedTextDto
        {
            Es = text.Es,
            Pt = text.Pt,
            En = text.En,
            Fr = text.Fr,
            De = text.De,
            Nl = text.Nl
        };
    }

    private static AddressDto? MapAddress(Address? address)
    {
        if (address is null) return null;
        return new AddressDto
        {
            Street = address.Street,
            City = address.City,
            Province = address.Province,
            PostalCode = address.PostalCode,
            Country = address.Country,
            ProvinceCode = address.ProvinceCode,
            MunicipalityCode = address.MunicipalityCode,
            Latitude = address.Latitude,
            Longitude = address.Longitude
        };
    }

    private static PropertyFeaturesDto? MapFeatures(PropertyFeatures? features)
    {
        if (features is null) return null;
        return new PropertyFeaturesDto
        {
            Bedrooms = features.Bedrooms,
            Bathrooms = features.Bathrooms,
            BuiltArea = features.BuiltArea,
            UsableArea = features.UsableArea,
            PlotArea = features.PlotArea,
            Floor = features.Floor,
            Orientation = features.Orientation,
            YearBuilt = features.YearBuilt,
            EnergyRating = features.EnergyRating,
            EnergyConsumption = features.EnergyConsumption,
            EnergyEmissions = features.EnergyEmissions,
            HasPool = features.HasPool,
            HasGarden = features.HasGarden,
            HasGarage = features.HasGarage,
            HasElevator = features.HasElevator,
            HasTerrace = features.HasTerrace,
            AirConditioning = features.AirConditioning,
            Heating = features.Heating,
            Furnished = features.Furnished,
            ParkingSpaces = features.ParkingSpaces
        };
    }

    private static PropertyFinancialsDto? MapFinancials(PropertyFinancials? financials)
    {
        if (financials is null) return null;
        return new PropertyFinancialsDto
        {
            Price = financials.Price,
            CommunityFees = financials.CommunityFees,
            IbiTax = financials.IbiTax,
            CatastroReference = financials.CatastroReference
        };
    }

    public static LocalizedText? ToLocalizedText(LocalizedTextDto? dto)
    {
        if (dto is null) return null;
        return LocalizedText.Create(dto.Es, dto.Pt, dto.En, dto.Fr, dto.De, dto.Nl);
    }

    public static Address? ToAddress(AddressDto? dto)
    {
        if (dto is null) return null;
        return Address.Create(
            dto.Street, dto.City, dto.Province, dto.PostalCode,
            dto.Country, dto.ProvinceCode, dto.MunicipalityCode,
            dto.Latitude, dto.Longitude);
    }

    public static PropertyFeatures? ToFeatures(PropertyFeaturesDto? dto)
    {
        if (dto is null) return null;
        return PropertyFeatures.Create(
            dto.Bedrooms, dto.Bathrooms, dto.BuiltArea, dto.UsableArea,
            dto.PlotArea, dto.Floor, dto.Orientation, dto.YearBuilt,
            dto.EnergyRating, dto.EnergyConsumption, dto.EnergyEmissions,
            dto.HasPool, dto.HasGarden, dto.HasGarage, dto.HasElevator,
            dto.HasTerrace, dto.AirConditioning, dto.Heating, dto.Furnished,
            dto.ParkingSpaces);
    }

    public static PropertyFinancials? ToFinancials(PropertyFinancialsDto? dto)
    {
        if (dto is null) return null;
        return PropertyFinancials.Create(dto.Price, dto.CommunityFees, dto.IbiTax, dto.CatastroReference);
    }
}
