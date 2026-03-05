// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.PropertiesApi.Domain.Common.Exceptions;

namespace Propely.PropertiesApi.Domain.Properties.ValueObjects;

public sealed class PropertyFinancials
{
    public decimal? Price { get; private set; }
    public decimal? CommunityFees { get; private set; }
    public decimal? IbiTax { get; private set; }
    public string? CatastroReference { get; private set; }

    private PropertyFinancials() { }

    public static PropertyFinancials Create(
        decimal? price = null,
        decimal? communityFees = null,
        decimal? ibiTax = null,
        string? catastroReference = null)
    {
        if (price.HasValue && price.Value < 0)
            throw new DomainException("Price cannot be negative.");

        if (communityFees.HasValue && communityFees.Value < 0)
            throw new DomainException("Community fees cannot be negative.");

        if (ibiTax.HasValue && ibiTax.Value < 0)
            throw new DomainException("IBI tax cannot be negative.");

        return new PropertyFinancials
        {
            Price = price,
            CommunityFees = communityFees,
            IbiTax = ibiTax,
            CatastroReference = catastroReference?.Trim()
        };
    }
}
