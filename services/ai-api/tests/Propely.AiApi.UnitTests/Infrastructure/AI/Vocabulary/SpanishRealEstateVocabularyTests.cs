// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Propely.AiApi.Infrastructure.AI.Vocabulary;

namespace Propely.AiApi.UnitTests.Infrastructure.AI.Vocabulary;

public sealed class SpanishRealEstateVocabularyTests
{
    // --- Property Type Mappings ---

    [Theory]
    [InlineData("piso", "apartment")]
    [InlineData("pisos", "apartment")]
    [InlineData("apartamento", "apartment")]
    [InlineData("apartamentos", "apartment")]
    [InlineData("apartment", "apartment")]
    [InlineData("bajo", "apartment")]
    [InlineData("bajos", "apartment")]
    public void ResolvePropertyType_ApartmentVariants_ShouldReturnApartment(string term, string expected)
        => SpanishRealEstateVocabulary.ResolvePropertyType(term).Should().Be(expected);

    [Theory]
    [InlineData("atico", "penthouse")]
    [InlineData("ático", "penthouse")]
    [InlineData("aticos", "penthouse")]
    [InlineData("áticos", "penthouse")]
    [InlineData("penthouse", "penthouse")]
    public void ResolvePropertyType_PenthouseVariants_ShouldReturnPenthouse(string term, string expected)
        => SpanishRealEstateVocabulary.ResolvePropertyType(term).Should().Be(expected);

    [Theory]
    [InlineData("duplex", "duplex")]
    [InlineData("dúplex", "duplex")]
    public void ResolvePropertyType_DuplexVariants_ShouldReturnDuplex(string term, string expected)
        => SpanishRealEstateVocabulary.ResolvePropertyType(term).Should().Be(expected);

    [Theory]
    [InlineData("loft", "studio")]
    [InlineData("lofts", "studio")]
    [InlineData("estudio", "studio")]
    [InlineData("estudios", "studio")]
    [InlineData("studio", "studio")]
    public void ResolvePropertyType_StudioVariants_ShouldReturnStudio(string term, string expected)
        => SpanishRealEstateVocabulary.ResolvePropertyType(term).Should().Be(expected);

    [Theory]
    [InlineData("adosado", "house")]
    [InlineData("adosados", "house")]
    [InlineData("pareado", "house")]
    [InlineData("pareados", "house")]
    [InlineData("chalet", "house")]
    [InlineData("chalets", "house")]
    [InlineData("chalé", "house")]
    [InlineData("casa", "house")]
    [InlineData("house", "house")]
    public void ResolvePropertyType_HouseVariants_ShouldReturnHouse(string term, string expected)
        => SpanishRealEstateVocabulary.ResolvePropertyType(term).Should().Be(expected);

    [Theory]
    [InlineData("villa", "villa")]
    [InlineData("villas", "villa")]
    [InlineData("finca", "villa")]
    [InlineData("fincas", "villa")]
    [InlineData("cortijo", "villa")]
    [InlineData("cortijos", "villa")]
    [InlineData("masía", "villa")]
    [InlineData("masia", "villa")]
    public void ResolvePropertyType_VillaVariants_ShouldReturnVilla(string term, string expected)
        => SpanishRealEstateVocabulary.ResolvePropertyType(term).Should().Be(expected);

    [Theory]
    [InlineData("local", "commercial")]
    [InlineData("local comercial", "commercial")]
    [InlineData("oficina", "commercial")]
    [InlineData("oficinas", "commercial")]
    [InlineData("nave", "commercial")]
    [InlineData("nave industrial", "commercial")]
    [InlineData("edificio", "commercial")]
    [InlineData("commercial", "commercial")]
    public void ResolvePropertyType_CommercialVariants_ShouldReturnCommercial(string term, string expected)
        => SpanishRealEstateVocabulary.ResolvePropertyType(term).Should().Be(expected);

    [Theory]
    [InlineData("solar", "land")]
    [InlineData("solares", "land")]
    [InlineData("terreno", "land")]
    [InlineData("parcela", "land")]
    [InlineData("land", "land")]
    public void ResolvePropertyType_LandVariants_ShouldReturnLand(string term, string expected)
        => SpanishRealEstateVocabulary.ResolvePropertyType(term).Should().Be(expected);

    [Theory]
    [InlineData("garaje", "garage")]
    [InlineData("garajes", "garage")]
    [InlineData("plaza de garaje", "garage")]
    [InlineData("garage", "garage")]
    public void ResolvePropertyType_GarageVariants_ShouldReturnGarage(string term, string expected)
        => SpanishRealEstateVocabulary.ResolvePropertyType(term).Should().Be(expected);

    [Theory]
    [InlineData("trastero", "storage")]
    [InlineData("trasteros", "storage")]
    [InlineData("storage", "storage")]
    public void ResolvePropertyType_StorageVariants_ShouldReturnStorage(string term, string expected)
        => SpanishRealEstateVocabulary.ResolvePropertyType(term).Should().Be(expected);

    [Theory]
    [InlineData("PISO", "apartment")]
    [InlineData("Chalet", "house")]
    [InlineData("  piso  ", "apartment")]
    public void ResolvePropertyType_ShouldBeCaseInsensitiveAndTrimWhitespace(string term, string expected)
        => SpanishRealEstateVocabulary.ResolvePropertyType(term).Should().Be(expected);

    [Theory]
    [InlineData("atico", "penthouse")]
    [InlineData("ático", "penthouse")]
    public void ResolvePropertyType_ShouldHandleAccentedAndUnaccentedVariants(string term, string expected)
        => SpanishRealEstateVocabulary.ResolvePropertyType(term).Should().Be(expected);

    [Theory]
    [InlineData("unknown_type")]
    [InlineData("castillo")]
    [InlineData("")]
    public void ResolvePropertyType_UnknownTerm_ShouldReturnNull(string term)
        => SpanishRealEstateVocabulary.ResolvePropertyType(term).Should().BeNull();

    // --- Operation Type Mappings ---

    [Theory]
    [InlineData("venta", "sale")]
    [InlineData("ventas", "sale")]
    [InlineData("vender", "sale")]
    [InlineData("comprar", "sale")]
    [InlineData("compra", "sale")]
    [InlineData("sale", "sale")]
    public void ResolveOperationType_SaleVariants_ShouldReturnSale(string term, string expected)
        => SpanishRealEstateVocabulary.ResolveOperationType(term).Should().Be(expected);

    [Theory]
    [InlineData("alquiler", "rent")]
    [InlineData("alquilar", "rent")]
    [InlineData("arrendar", "rent")]
    [InlineData("arriendo", "rent")]
    [InlineData("rent", "rent")]
    [InlineData("alquiler vacacional", "rent")]
    [InlineData("alquiler temporal", "rent")]
    [InlineData("alquiler con opcion a compra", "rent")]
    [InlineData("alquiler con opción a compra", "rent")]
    public void ResolveOperationType_RentVariants_ShouldReturnRent(string term, string expected)
        => SpanishRealEstateVocabulary.ResolveOperationType(term).Should().Be(expected);

    [Theory]
    [InlineData("traspaso", "transfer")]
    [InlineData("traspasar", "transfer")]
    [InlineData("transfer", "transfer")]
    public void ResolveOperationType_TransferVariants_ShouldReturnTransfer(string term, string expected)
        => SpanishRealEstateVocabulary.ResolveOperationType(term).Should().Be(expected);

    [Theory]
    [InlineData("unknown_op")]
    [InlineData("")]
    public void ResolveOperationType_UnknownTerm_ShouldReturnNull(string term)
        => SpanishRealEstateVocabulary.ResolveOperationType(term).Should().BeNull();

    // --- Feature Mappings ---

    [Theory]
    [InlineData("piscina", "pool")]
    [InlineData("jardín", "garden")]
    [InlineData("jardin", "garden")]
    [InlineData("garaje", "garage")]
    [InlineData("trastero", "storage")]
    [InlineData("aire acondicionado", "air_conditioning")]
    [InlineData("calefacción", "heating")]
    [InlineData("calefaccion", "heating")]
    [InlineData("ascensor", "elevator")]
    [InlineData("terraza", "terrace")]
    [InlineData("balcón", "balcony")]
    [InlineData("balcon", "balcony")]
    [InlineData("amueblado", "furnished")]
    [InlineData("luminoso", "bright")]
    [InlineData("exterior", "exterior")]
    [InlineData("interior", "interior")]
    [InlineData("reformado", "renovated")]
    [InlineData("a estrenar", "new_build")]
    public void ResolveFeature_SpanishTerms_ShouldReturnNormalizedValue(string term, string expected)
        => SpanishRealEstateVocabulary.ResolveFeature(term).Should().Be(expected);

    [Theory]
    [InlineData("unknown_feature")]
    [InlineData("")]
    public void ResolveFeature_UnknownTerm_ShouldReturnNull(string term)
        => SpanishRealEstateVocabulary.ResolveFeature(term).Should().BeNull();

    // --- Mapping completeness ---

    [Fact]
    public void GetPropertyTypeMappings_ShouldContainAllExpectedTerms()
    {
        var mappings = SpanishRealEstateVocabulary.GetPropertyTypeMappings();
        mappings.Count.Should().BeGreaterThanOrEqualTo(20, "at least 20 property type terms should be mapped");
    }

    [Fact]
    public void GetOperationTypeMappings_ShouldContainAllExpectedTerms()
    {
        var mappings = SpanishRealEstateVocabulary.GetOperationTypeMappings();
        mappings.Count.Should().BeGreaterThanOrEqualTo(10, "at least 10 operation type terms should be mapped");
    }

    [Fact]
    public void GetFeatureMappings_ShouldContainAllExpectedTerms()
    {
        var mappings = SpanishRealEstateVocabulary.GetFeatureMappings();
        mappings.Count.Should().BeGreaterThanOrEqualTo(15, "at least 15 feature terms should be mapped");
    }
}
