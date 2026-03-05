// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Propely.PropertiesApi.Domain.Common.Exceptions;
using Propely.PropertiesApi.Domain.Properties.ValueObjects;

namespace Propely.PropertiesApi.UnitTests.Properties.ValueObjects;

public class AddressTests
{
    [Fact]
    public void Create_WithValidData_SetsAllFields()
    {
        var address = Address.Create(
            street: "Calle Mayor 15",
            city: "Malaga",
            province: "M\u00E1laga",
            postalCode: "29001",
            country: "ES",
            provinceCode: "29",
            municipalityCode: "29067",
            latitude: 36.7213,
            longitude: -4.4214);

        address.Street.Should().Be("Calle Mayor 15");
        address.City.Should().Be("Malaga");
        address.Province.Should().Be("M\u00E1laga");
        address.PostalCode.Should().Be("29001");
        address.Country.Should().Be("ES");
        address.ProvinceCode.Should().Be("29");
        address.MunicipalityCode.Should().Be("29067");
        address.Latitude.Should().Be(36.7213);
        address.Longitude.Should().Be(-4.4214);
    }

    [Fact]
    public void Create_WithNullValues_HasNullFields()
    {
        var address = Address.Create();
        address.Street.Should().BeNull();
        address.City.Should().BeNull();
        address.Latitude.Should().BeNull();
        address.Longitude.Should().BeNull();
    }

    [Fact]
    public void Create_TrimsStringValues()
    {
        var address = Address.Create(street: "  Calle Mayor  ", city: "  Malaga  ");
        address.Street.Should().Be("Calle Mayor");
        address.City.Should().Be("Malaga");
    }

    [Theory]
    [InlineData(-91)]
    [InlineData(91)]
    [InlineData(-100)]
    [InlineData(200)]
    public void Create_WithInvalidLatitude_ThrowsDomainException(double latitude)
    {
        var act = () => Address.Create(latitude: latitude);
        act.Should().Throw<DomainException>().WithMessage("*Latitude*-90*90*");
    }

    [Theory]
    [InlineData(-181)]
    [InlineData(181)]
    [InlineData(-200)]
    [InlineData(360)]
    public void Create_WithInvalidLongitude_ThrowsDomainException(double longitude)
    {
        var act = () => Address.Create(longitude: longitude);
        act.Should().Throw<DomainException>().WithMessage("*Longitude*-180*180*");
    }

    [Theory]
    [InlineData(-90)]
    [InlineData(0)]
    [InlineData(90)]
    public void Create_WithBoundaryLatitude_Succeeds(double latitude)
    {
        var address = Address.Create(latitude: latitude);
        address.Latitude.Should().Be(latitude);
    }

    [Theory]
    [InlineData(-180)]
    [InlineData(0)]
    [InlineData(180)]
    public void Create_WithBoundaryLongitude_Succeeds(double longitude)
    {
        var address = Address.Create(longitude: longitude);
        address.Longitude.Should().Be(longitude);
    }
}
