// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.PropertiesApi.Domain.Common.Exceptions;

namespace Propely.PropertiesApi.Domain.Properties.ValueObjects;

public sealed class Address
{
    public string? Street { get; private set; }
    public string? City { get; private set; }
    public string? Province { get; private set; }
    public string? PostalCode { get; private set; }
    public string? Country { get; private set; }
    public string? ProvinceCode { get; private set; }
    public string? MunicipalityCode { get; private set; }
    public double? Latitude { get; private set; }
    public double? Longitude { get; private set; }

    private Address() { }

    public static Address Create(
        string? street = null,
        string? city = null,
        string? province = null,
        string? postalCode = null,
        string? country = null,
        string? provinceCode = null,
        string? municipalityCode = null,
        double? latitude = null,
        double? longitude = null)
    {
        if (latitude.HasValue && (latitude.Value < -90 || latitude.Value > 90))
            throw new DomainException("Latitude must be between -90 and 90.");

        if (longitude.HasValue && (longitude.Value < -180 || longitude.Value > 180))
            throw new DomainException("Longitude must be between -180 and 180.");

        return new Address
        {
            Street = street?.Trim(),
            City = city?.Trim(),
            Province = province?.Trim(),
            PostalCode = postalCode?.Trim(),
            Country = country?.Trim(),
            ProvinceCode = provinceCode?.Trim(),
            MunicipalityCode = municipalityCode?.Trim(),
            Latitude = latitude,
            Longitude = longitude
        };
    }
}
