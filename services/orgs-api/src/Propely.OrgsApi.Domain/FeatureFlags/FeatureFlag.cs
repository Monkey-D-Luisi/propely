// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Domain.Common;

namespace Propely.OrgsApi.Domain.FeatureFlags;

public sealed class FeatureFlag : Entity
{
    public const int NameMaxLength = 100;
    public const int DescriptionMaxLength = 500;

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsEnabled { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private FeatureFlag() { }

    public static FeatureFlag Create(string name, bool isEnabled, string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var trimmedName = name.Trim();
        if (trimmedName.Length > NameMaxLength)
            throw new ArgumentException($"Name must not exceed {NameMaxLength} characters.", nameof(name));

        return new FeatureFlag
        {
            Id = Guid.NewGuid(),
            Name = trimmedName,
            Description = description?.Trim(),
            IsEnabled = isEnabled,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void SetEnabled(bool value)
    {
        IsEnabled = value;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
