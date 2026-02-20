// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Application.FeatureFlags.Interfaces;

public interface IFeatureFlagDefaults
{
    IReadOnlyDictionary<string, bool> GetDefaults();
    bool IsDefinedFlag(string name);
}
