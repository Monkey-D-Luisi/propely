// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Text.RegularExpressions;
using Propely.OrgsApi.Domain.Common.Exceptions;

namespace Propely.OrgsApi.Domain.Agencies;

/// <summary>
/// Value object representing a validated agency slug.
/// Must be lowercase alphanumeric with hyphens, 3-50 characters.
/// </summary>
public sealed partial class AgencySlug
{
    public const int MinLength = 3;
    public const int MaxLength = 50;

    private static readonly Regex SlugPattern = SlugRegex();

    public string Value { get; }

    private AgencySlug(string value)
    {
        Value = value;
    }

    public static AgencySlug Create(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            throw new DomainException("Agency slug is required.");

        var trimmed = slug.Trim();

        if (trimmed.Length < MinLength)
            throw new DomainException($"Agency slug must be at least {MinLength} characters.");

        if (trimmed.Length > MaxLength)
            throw new DomainException($"Agency slug must not exceed {MaxLength} characters.");

        if (!SlugPattern.IsMatch(trimmed))
            throw new DomainException("Agency slug must contain only lowercase letters, numbers, and hyphens, and must start and end with a letter or number.");

        return new AgencySlug(trimmed);
    }

    public override string ToString() => Value;
    public override bool Equals(object? obj) => obj is AgencySlug other && Value == other.Value;
    public override int GetHashCode() => Value.GetHashCode();

    [GeneratedRegex(@"^[a-z0-9][a-z0-9-]{1,48}[a-z0-9]$")]
    private static partial Regex SlugRegex();
}
