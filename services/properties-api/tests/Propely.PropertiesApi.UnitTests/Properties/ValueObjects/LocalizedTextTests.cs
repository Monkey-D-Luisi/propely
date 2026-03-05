// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Propely.PropertiesApi.Domain.Common.Exceptions;
using Propely.PropertiesApi.Domain.Properties.ValueObjects;

namespace Propely.PropertiesApi.UnitTests.Properties.ValueObjects;

public class LocalizedTextTests
{
    [Fact]
    public void Create_WithValues_SetsAllFields()
    {
        var text = LocalizedText.Create(
            es: "Apartamento de lujo",
            pt: "Apartamento de luxo",
            en: "Luxury apartment",
            fr: "Appartement de luxe",
            de: "Luxuswohnung",
            nl: "Luxe appartement");

        text.Es.Should().Be("Apartamento de lujo");
        text.Pt.Should().Be("Apartamento de luxo");
        text.En.Should().Be("Luxury apartment");
        text.Fr.Should().Be("Appartement de luxe");
        text.De.Should().Be("Luxuswohnung");
        text.Nl.Should().Be("Luxe appartement");
    }

    [Fact]
    public void Create_WithNullValues_HasNullFields()
    {
        var text = LocalizedText.Create();
        text.Es.Should().BeNull();
        text.Pt.Should().BeNull();
        text.En.Should().BeNull();
    }

    [Fact]
    public void Create_TrimsValues()
    {
        var text = LocalizedText.Create(es: "  Apartamento  ", en: "  Apartment  ");
        text.Es.Should().Be("Apartamento");
        text.En.Should().Be("Apartment");
    }

    [Fact]
    public void HasAnyValue_WithAtLeastOneValue_ReturnsTrue()
    {
        var text = LocalizedText.Create(es: "Test");
        text.HasAnyValue().Should().BeTrue();
    }

    [Fact]
    public void HasAnyValue_WithAllNull_ReturnsFalse()
    {
        var text = LocalizedText.Create();
        text.HasAnyValue().Should().BeFalse();
    }

    [Fact]
    public void HasAnyValue_WithOnlyWhitespace_ReturnsFalse()
    {
        var text = LocalizedText.Create(es: " ", en: "  ");
        text.HasAnyValue().Should().BeFalse();
    }

    [Fact]
    public void RequireAtLeastOne_WithValue_DoesNotThrow()
    {
        var text = LocalizedText.Create(en: "Apartment");
        var act = () => LocalizedText.RequireAtLeastOne(text, "Title");
        act.Should().NotThrow();
    }

    [Fact]
    public void RequireAtLeastOne_WithNull_ThrowsDomainException()
    {
        var act = () => LocalizedText.RequireAtLeastOne(null, "Title");
        act.Should().Throw<DomainException>().WithMessage("*Title*at least one*");
    }

    [Fact]
    public void RequireAtLeastOne_WithEmptyValues_ThrowsDomainException()
    {
        var text = LocalizedText.Create();
        var act = () => LocalizedText.RequireAtLeastOne(text, "Title");
        act.Should().Throw<DomainException>().WithMessage("*Title*at least one*");
    }
}
