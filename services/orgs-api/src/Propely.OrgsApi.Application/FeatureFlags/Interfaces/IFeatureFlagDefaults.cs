// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Application.FeatureFlags.Interfaces;

public interface IFeatureFlagDefaults
{
    IReadOnlyDictionary<string, bool> GetDefaults();
    bool IsDefinedFlag(string name);
}
