// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Microsoft.Extensions.Options;
using Propely.OrgsApi.Application.FeatureFlags.Interfaces;

namespace Propely.OrgsApi.Infrastructure.Services;

public sealed class FeatureFlagDefaults : IFeatureFlagDefaults
{
    private readonly Dictionary<string, bool> _defaults;

    public FeatureFlagDefaults(IOptions<Dictionary<string, bool>> options)
    {
        _defaults = new Dictionary<string, bool>(options.Value, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyDictionary<string, bool> GetDefaults() => _defaults;

    public bool IsDefinedFlag(string name) =>
        _defaults.ContainsKey(name);
}
