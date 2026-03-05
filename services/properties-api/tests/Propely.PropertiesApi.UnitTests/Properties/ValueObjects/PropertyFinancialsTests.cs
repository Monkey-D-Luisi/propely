// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Propely.PropertiesApi.Domain.Common.Exceptions;
using Propely.PropertiesApi.Domain.Properties.ValueObjects;

namespace Propely.PropertiesApi.UnitTests.Properties.ValueObjects;

public class PropertyFinancialsTests
{
    [Fact]
    public void Create_WithValidData_SetsAllFields()
    {
        var financials = PropertyFinancials.Create(
            price: 250000m,
            communityFees: 150m,
            ibiTax: 800m,
            catastroReference: "1234567890ABCDEF");

        financials.Price.Should().Be(250000m);
        financials.CommunityFees.Should().Be(150m);
        financials.IbiTax.Should().Be(800m);
        financials.CatastroReference.Should().Be("1234567890ABCDEF");
    }

    [Fact]
    public void Create_WithNullValues_HasNullFields()
    {
        var financials = PropertyFinancials.Create();
        financials.Price.Should().BeNull();
        financials.CommunityFees.Should().BeNull();
        financials.IbiTax.Should().BeNull();
        financials.CatastroReference.Should().BeNull();
    }

    [Fact]
    public void Create_WithNegativePrice_ThrowsDomainException()
    {
        var act = () => PropertyFinancials.Create(price: -1m);
        act.Should().Throw<DomainException>().WithMessage("*Price*negative*");
    }

    [Fact]
    public void Create_WithNegativeCommunityFees_ThrowsDomainException()
    {
        var act = () => PropertyFinancials.Create(communityFees: -1m);
        act.Should().Throw<DomainException>().WithMessage("*Community fees*negative*");
    }

    [Fact]
    public void Create_WithNegativeIbiTax_ThrowsDomainException()
    {
        var act = () => PropertyFinancials.Create(ibiTax: -1m);
        act.Should().Throw<DomainException>().WithMessage("*IBI tax*negative*");
    }

    [Fact]
    public void Create_WithZeroPrice_Succeeds()
    {
        var financials = PropertyFinancials.Create(price: 0m);
        financials.Price.Should().Be(0m);
    }

    [Fact]
    public void Create_TrimsCatastroReference()
    {
        var financials = PropertyFinancials.Create(catastroReference: "  ABC123  ");
        financials.CatastroReference.Should().Be("ABC123");
    }
}
