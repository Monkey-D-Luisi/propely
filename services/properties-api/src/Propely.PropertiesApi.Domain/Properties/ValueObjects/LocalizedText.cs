// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.PropertiesApi.Domain.Common.Exceptions;

namespace Propely.PropertiesApi.Domain.Properties.ValueObjects;

public sealed class LocalizedText
{
    public string? Es { get; private set; }
    public string? Pt { get; private set; }
    public string? En { get; private set; }
    public string? Fr { get; private set; }
    public string? De { get; private set; }
    public string? Nl { get; private set; }

    private LocalizedText() { }

    public static LocalizedText Create(
        string? es = null,
        string? pt = null,
        string? en = null,
        string? fr = null,
        string? de = null,
        string? nl = null)
    {
        return new LocalizedText
        {
            Es = es?.Trim(),
            Pt = pt?.Trim(),
            En = en?.Trim(),
            Fr = fr?.Trim(),
            De = de?.Trim(),
            Nl = nl?.Trim()
        };
    }

    public bool HasAnyValue()
    {
        return !string.IsNullOrWhiteSpace(Es)
            || !string.IsNullOrWhiteSpace(Pt)
            || !string.IsNullOrWhiteSpace(En)
            || !string.IsNullOrWhiteSpace(Fr)
            || !string.IsNullOrWhiteSpace(De)
            || !string.IsNullOrWhiteSpace(Nl);
    }

    public static void RequireAtLeastOne(LocalizedText? text, string fieldName)
    {
        if (text is null || !text.HasAnyValue())
            throw new DomainException($"{fieldName} must have at least one non-empty localized value.");
    }
}
